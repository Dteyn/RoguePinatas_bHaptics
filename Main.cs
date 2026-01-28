/******************************************************************************
 *          Rogue Piñatas: VRmageddon bHaptics Integration by Dteyn
 *              https://github.com/Dteyn/RoguePinatas_bHaptics
 *              
 * Version: 1.0.0
 * Dated: Jan 28 2026
 * 
 * BepInEx version: 5.4.23.2
 * Game BuildID: 19155164 (July 14 2025)
 * Game Version: 1.1.0:9027
 * 
 * Shout-Outs: Astienth for mentoring and teaching me how to make bHaptics mods, and for being a continuous
 * inspiration to create great things. Also, shoutouts to Florian for his excellent bHaptics mod tutorial 
 * (see: https://github.com/floh-bhaptics/ShadowLegend_bhaptics) and all the great mods released which have
 * provided inspiration and examples to work from. Another huge shout-out to my friend farmertrue, a VR content
 * creator with the best VR content around. And finally, shout-outs to anyone reading this - you rock! :)
 * 
 * Astienth: https://github.com/Astienth
 * Florian: https://github.com/floh-bhaptics
 * farmertrue: https://farmertruevr.com
 * 
 * PLAYER EFFECTS:
 * ===============
 * 
 * Description              Effect ID            Hook (in Patches.cs)                                                      
 * =============            ============         ===========================================                
 * Heart Beat               player_heartbeat     CombatPlayer.OnHealthChanged() (when health is below {x}%, set in .cfg)
 * Player Damage            player_impact        S_PlayerCollision.HandleHit(), CombatPlayer.TakeDamage()
 * Player Damage (Rear)     player_impact_rear   S_PlayerCollision.HandleHit(), CombatPlayer.TakeDamage()
 * Common Explosion         player_explosion     VfxCommandBuffer.SpawnCommonExplosionVfx()
 * PopSweeper Explosion     player_explosion     VfxCommandBuffer.SpawnPopSweeperExplosionVfx()
 * Bomber/Enemy Explosion   player_explosion     VfxCommandBuffer.SpawnBomberExplosionVfx()
 * Vehicle Explosion        player_explosion     VfxCommandBuffer.SpawnVehicleExplosionVfx()
 * Net-Sync Explosion (Com) player_explosion     S_NetSync.Record<CommonExplosionVfxRequest>.ProcessIncomingRequest()
 * Net-Sync Explosion (PS)  player_explosion     S_NetSync.Record<PopSweeperExplosionVfxRequest>.ProcessIncomingRequest()
 * Heal                     player_heal          CombatPlayer.Heal()                                        
 * Revive                   player_revive        CombatPlayer.Heal() w/ bool revive = true                  
 * Death                    player_death         CombatPlayer.OnHealthChanged() w/ .IsDead = true           
 * Candy Pickup             player_candypickup   CombatPlayer.AddCandy()                                    
 * Meta Candy P/U           player_candypickup   CombatPlayer.AddMetaCandy()                                
 * Level Upgrade            player_levelup       UpgradeService.PlayUpgradeSFX()
 * Party Box                player_partibox      UpgradeService.PlayPartiBoxSFX()
 *
 * WEAPON EFFECTS:
 * ===============
 * 
 * Melee Enemy Hits         weapon_melee_r/l     NetSyncService.DamageEnemy()
 * Melee Object Hits        weapon_melee_r/l     ECSNetworkSyncService.CallDamageInteractable()
 * JolliZapper Hits         weapon_zapper_r/l    NetSyncService.DamageEnemy() + ECSNetworkSyncService.CallDamageInteractable()
 * Ranged Hitscan Fire      weapon_ranged_r/l    HitscanProjectileFirer.OnWeaponFire()
 * Projectile Fire          weapon_ranged_r/l    PhysicalProjectileFirer.OnWeaponFire()
 * JawDropper Fire          weapon_ranged_r/l    JawDropperWeapon.OnWeaponFire()
 * BoomBoxer Shockwave      weapon_boomboxer_r/l S_PoolableVfxGenericInit.OnUpdate()
 *****************************************************************************/

using BepInEx;
using HarmonyLib;
using NerdNinjas.Fiesta;
using System;
using System.Collections;
using tact_csharp2;  // bHaptics SDKv2 (https://github.com/bhaptics/tact-csharp2)
using UnityEngine;
using static RoguePinatas_bHaptics.Config;

namespace RoguePinatas_bHaptics
{
   public static class PluginInfo
    {
        public const string PLUGIN_GUID = "com.github.dteyn.RoguePinatas_bHaptics";
        public const string PLUGIN_NAME = "Rogue Pinatas bHaptics Integration";
        public const string PLUGIN_VERSION = "1.0.0";
    }

    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Main : BaseUnityPlugin
    {
        public static TactsuitVR tactsuitVr;

        // Explosion tracking
        private const float ExplosionCooldown = 0.25f;
        private static float lastExplosionTime;

        // Dedupe tracking between HandleHit and TakeDamage (for player impacts)
        private const float ImpactFallbackWindow = 0.2f;
        private static float lastHandleHitTime;

        // Haptic state tracking
        public static bool downedHapticsPlayed;
        public static bool hapticsPaused;

        private void Awake()
        {
            // Logger setup
            dLog.Init(Logger);
            dLog.AddTimestamp = true;

            dLog.Info($"{PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} loaded!");

            // Make sure bHaptics Player is installed
            if (!BhapticsSDK2Wrapper.isPlayerInstalled())
            {
                dLog.Error("[bHaptics] ERROR - bHaptics Player is not installed!\n" +
                    "Visit www.bhaptics.com to download and install the bHaptics Player software, then re-launch the game.");
                return;
            }

            // TactsuitVR setup & init
            tactsuitVr = new TactsuitVR();
            downedHapticsPlayed = false;

            // Apply Harmony patches
            var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll();
        }

        #region Helpers

        /// <summary>
        /// Updates the heartbeat haptic effect based on player health status.
        /// Starts heartbeat when health drops below threshold, stops when dead or health recovers.
        /// </summary>
        /// <param name="player">The CombatPlayer instance to monitor</param>
        public static void UpdateHeartBeat(CombatPlayer player)
        {
            if (tactsuitVr == null || TactsuitVR.suitDisabled || player == null || HeartBeatOnLowHealth == false)
            {
                return;
            }

            if (!IsLocalPlayer(player))
            {
                return;
            }

            if (player.IsDead || IsLocalPlayerDowned())
            {
                TactsuitVR.StopHeartBeat();
                return;
            }

            if (player.CurrentHealthPercent < LowHealthThreshold)
            {
                TactsuitVR.StartHeartBeat();
            }
            else
            {
                TactsuitVR.StopHeartBeat();
            }
        }

        /// <summary>
        /// Central gate for haptics: true when safe to play. Checks if TactsuitVR is set up
        /// and enabled, in gameplay session, not paused, and player is not downed or dead.
        /// </summary>
        /// <remarks>
        /// This gate is used for most of the patches, as a safety guard to prevent haptics
        /// from firing when they shouldn't.
        /// </remarks>
        public static bool CanPlayHaptics()
        {
            var player = XRPlayer.Instance?.CombatPlayer;
            // Haptics are playable if:
            return tactsuitVr != null &&                // tactsuitVr is setup properly,
                   !TactsuitVR.suitDisabled &&          // gear isn't disabled,
                   IsGameplay() &&                      // game is in 'gameplay' mode (not in Garage)
                   !IsLocalPlayerDowned() &&            // player isn't downed
                   !hapticsPaused &&                    // game isn't paused (single player), and
                   (player == null || !player.IsDead);  // player isn't dead.
        }

        /// <summary>
        /// Checks the player list and returns true if single player only, false if multiplayer.
        /// </summary>
        public static bool IsSinglePlayer()
        {
            return Photon.Pun.PhotonNetwork.PlayerList.Length <= 1;
        }

        /// <summary>
        /// Determines if the specified CombatPlayer instance represents the local player.
        /// </summary>
        /// <param name="player">The CombatPlayer instance to check</param>
        /// <returns>True if the player is the local XRPlayer instance, false otherwise</returns>
        public static bool IsLocalPlayer(CombatPlayer player)
        {
            return player != null && XRPlayer.Instance != null && XRPlayer.Instance.CombatPlayer == player;
        }

        /// <summary>
        /// Checks if the game is currently in an active gameplay state.
        /// </summary>
        /// <returns>True if ECSPinataManager instance exists (gameplay active), false otherwise</returns>
        public static bool IsGameplay()
        {
            return ECSPinataManager.instance != null;
        }

        /// <summary>
        /// True when local player is in "downed" state (health == min, but not dead).
        /// </summary>
        public static bool IsLocalPlayerDowned()
        {
            var player = XRPlayer.Instance?.CombatPlayer;
            if (player == null)
            {
                return false;
            }

            bool isDowned = player.CurrentHealth.value == player.CurrentHealth.min;

            return isDowned && !player.IsDead;
        }



        /// <summary>
        /// Attempts to determine which hand is currently equipped with the specified weapon.
        /// </summary>
        /// <param name="weaponId">The weapon identifier string to search for</param>
        /// <param name="isLeftHand">Output parameter indicating if weapon is in left hand (true) or right hand (false)</param>
        /// <returns>True if weapon is found in either hand, false otherwise</returns>
        public static bool TryGetHandForWeapon(string weaponId, out bool isLeftHand)
        {
            isLeftHand = false;
            if (XRPlayer.Instance == null)
            {
                return false;
            }

            string left = XRPlayer.Instance.LeftHandWeaponSlot?.GetEquippedWeaponName();
            if (!string.IsNullOrEmpty(left) && string.Equals(left, weaponId, StringComparison.OrdinalIgnoreCase))
            {
                isLeftHand = true;
                return true;
            }

            string right = XRPlayer.Instance.RightHandWeaponSlot?.GetEquippedWeaponName();
            if (!string.IsNullOrEmpty(right) && string.Equals(right, weaponId, StringComparison.OrdinalIgnoreCase))
            {
                isLeftHand = false;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Plays haptic feedback for ranged weapon firing with weapon-specific intensity adjustments.
        /// Triggers both vest and arm haptics based on which hand fired the weapon.
        /// </summary>
        /// <param name="weapon">The EquippableWeapon that was fired</param>
        /// <param name="isLeftHand">True if weapon was fired from left hand, false for right hand</param>
        /// <param name="classification">Optional projectile classification for additional customization</param>
        public static void PlayRangedFireHaptics(EquippableWeapon weapon, bool isLeftHand, ProjectileWeaponClassification? classification)
        {
            if (tactsuitVr == null || TactsuitVR.suitDisabled || weapon == null)
            {
                return;
            }

            string eventId = isLeftHand ? "weapon_recoil_l" : "weapon_recoil_r";  // vest and arms

            float intensity = WeaponRecoilIntensity; // Default: 0.5f
            bool overridden = false;

            // Per-weapon overrides
            // Weapons can be overridden here by weapon ID, and could include a custom eventId override also
            // see enum WeaponType for list of weapons
            switch (weapon.WeaponID)
            {
                case "PopRocket":
                    intensity = WeaponPopRocketIntensity; // Default: 0.7f
                    overridden = true;
                    break;

                case "PopSweeper":
                    intensity = WeaponPopSweeperIntensity; // Default: 0.8f
                    overridden = true;
                    break;

                case "Funderbuss":
                    intensity = WeaponFunderbussIntensity; // Default: 0.75f
                    overridden = true;
                    break;

                case "FunderDrone":
                    intensity = WeaponFunderDroneIntensity; // Default: 0.8f
                    overridden = true;
                    break;

            }

            // If needed, per-projectile overrides can be implemented via ProjectileWeaponClassification
            // This is currently not used, but example left here in case it's useful for future builds
            /* see enum ProjectileWeaponClassification for list of projectiles
            if (classification == ProjectileWeaponClassification.Wand)
            {
                eventId = isLeftHand ? "weapon_wand_l" : "weapon_wand_r";
            } */

            // Required values for playParam
            int reqId = 0;
            float duration = 1.0f;
            float angleX = 0f;
            float offsetY = 0f;

            dLog.Debug($"[PlayRangedFireHaptics] Playing ranged fire haptics for weapon: {weapon.WeaponID}, " +
                $"eventId: {eventId}, intensity {intensity}, intensity override: {overridden}");

            TactsuitVR.PlayParam(eventId, reqId, intensity, duration, angleX, offsetY);
        }

        /// <summary>
        /// Plays haptic feedback when a melee weapon successfully hits a target.
        /// Triggers effects on the arm and vest corresponding to the attacking hand.
        /// </summary>
        /// <param name="isLeftHand">True if hit was performed with left hand weapon, false for right hand</param>
        /// <param name="weaponId">The identifier of the melee weapon used</param>
        public static void PlayMeleeHitHaptics(bool isLeftHand, string weaponId)
        {
            if (tactsuitVr == null || TactsuitVR.suitDisabled)
            {
                return;
            }

            // Default intensity
            float intensity = WeaponMeleeIntensity; // Default: 1.0f

            // In future builds, can implement per-weapon effects using weaponId.
            // For now, all melee weapons except JolliZapper and BoomBoxer use the same effect
            string eventId = isLeftHand ? "weapon_melee_l" : "weapon_melee_r";  // arms only

            // Use playParam if intensity is less than 1.0f, otherwise use play
            if (intensity < 1.0f)
            {
                // Required values for playParam
                int reqId = 0;
                float duration = 1.0f;
                float angleX = 0f;
                float offsetY = 0f;

                dLog.Debug($"[PlayMeleeHitHaptics] Using playParam: eventId: {eventId}, intensity: {intensity}");
                TactsuitVR.PlayParam(eventId, reqId, intensity, duration, angleX, offsetY);
            }
            else
            {
                dLog.Debug($"[PlayMeleeHitHaptics] Using play: eventId: {eventId}");
                TactsuitVR.Play(eventId);
            }
        }

        /// <summary>
        /// Plays unique haptic feedback for JolliZapper and JolliJam weapon contact damage.
        /// Uses reduced intensity compared to standard melee hits to reflect continuous contact nature.
        /// </summary>
        /// <param name="isLeftHand">True if zapper contact occurred with left hand weapon, false for right hand</param>
        /// <param name="multiTarget">True if zapper is contacting more than one enemy, to ramp intensity</param>
        public static void PlayJolliZapperHitHaptics(bool isLeftHand, bool multiTarget)
        {
            if (tactsuitVr == null || TactsuitVR.suitDisabled)
            {
                return;
            }

            // Default intensity: 0.4f when contacting single enemy, 0.8f for multiple enemies
            float intensity = multiTarget ?
                WeaponJolliZapperMultiIntensity :
                WeaponJolliZapperIntensity;

            string eventId = isLeftHand ? "weapon_zapper_l" : "weapon_zapper_r";  // vest and arms

            // Required values for playParam
            int reqId = 0;
            float duration = 1.0f;
            float angleX = 0f;
            float offsetY = 0f;

            dLog.Debug($"[PlayMeleeHitHaptics] eventId: {eventId}, intensity: {intensity}");
            TactsuitVR.PlayParam(eventId, reqId, intensity, duration, angleX, offsetY);
        }

        /// <summary>
        /// Attempts to determine which hand is currently equipped with the JolliZapper.
        /// </summary>
        /// <param name="isLeftHand">Output parameter indicating if JolliZapper is in left hand (true) or right hand (false)</param>
        /// <returns>True if JolliZapper is found in either hand, false otherwise</returns>
        public static bool TryGetZapperHand(out bool isLeftHand)
        {
            isLeftHand = false;

            if (XRPlayer.Instance == null)
            {
                return false;
            }

            string left = XRPlayer.Instance.LeftHandWeaponSlot?.GetEquippedWeaponName();
            if (!string.IsNullOrEmpty(left) &&
                (left.Equals("JolliZapper", StringComparison.OrdinalIgnoreCase) ||
                 left.Equals("JolliJam", StringComparison.OrdinalIgnoreCase)))
            {
                isLeftHand = true;
                return true;
            }

            string right = XRPlayer.Instance.RightHandWeaponSlot?.GetEquippedWeaponName();
            if (!string.IsNullOrEmpty(right) &&
                (right.Equals("JolliZapper", StringComparison.OrdinalIgnoreCase) ||
                 right.Equals("JolliJam", StringComparison.OrdinalIgnoreCase)))
            {
                isLeftHand = false;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Plays distance-based directional haptic feedback for explosions.
        /// Intensity scales with proximity using quadratic falloff, with left/right bias based on explosion direction.
        /// Rate-limited to prevent feedback saturation from multiple simultaneous explosions.
        /// </summary>
        /// <param name="explosionPos">World position of the explosion</param>
        /// <param name="radius">Explosion radius for distance calculations</param>
        public static void PlayExplosionHaptics(Vector3 explosionPos, float radius)
        {
            if (tactsuitVr == null || TactsuitVR.suitDisabled || XRPlayer.Instance?.CombatPlayer == null)
            {
                return;
            }

            if (!IsGameplay())
            {
                return;
            }

            if (Time.time - lastExplosionTime < ExplosionCooldown)
            {
                return;
            }

            // Calculate distance from explosion
            float maxDist = Mathf.Max(4f, radius * 1.5f);
            float dist = Vector3.Distance(XRPlayer.Instance.CombatPlayer.transform.position, explosionPos);

            // Max distance check - cancel effect if the explosion is too far away
            if (dist > maxDist)
            {
                return;
            }

            // Ramp intensity based on distance from explosion
            float normalized = Mathf.Clamp01(dist / maxDist);
            float intensity = 1.0f - normalized;

            // Minimum intensity clamp
            intensity = Mathf.Max(intensity, 0.25f);

            // Track explosion cooldown time
            lastExplosionTime = Time.time;

            // Required values for playParam
            string eventId = "player_explosion";  // vest and arms
            int reqId = 0;
            float duration = 1.0f;
            float angleX = 0f;
            float offsetY = 0f;

            dLog.Debug($"[PlayExplosionHaptics] distance: {dist}, normalized: {normalized}, intensity: {intensity}");
            TactsuitVR.PlayParam(eventId, reqId, intensity, duration, angleX, offsetY);

        }

        /// <summary>
        /// Returns an impact intensity for haptic effects based on damage amount. Intensity values can be configured in .cfg file.
        /// </summary>
        /// <param name="damage"></param>
        public static float GetImpactIntensityFromDamage(int damage)
        {
            if (damage >= 20) return ImpactIntensityHighDmg; // Default: 1.0f
            if (damage >= 10) return ImpactIntensityMedDmg; // Default: 0.8f
            return ImpactIntensityRegDmg; // Default: 0.6f
        }

        /// <summary>
        /// Helper for dedupe between HandleHit and TakeDamage
        /// </summary>
        public static void MarkHandleHit()
        {
            lastHandleHitTime = Time.time;
        }

        /// <summary>
        /// Helper for fallback to TakeDamage hook
        /// </summary>
        /// <returns></returns>
        public static bool CanFallbackImpact()
        {
            return Time.time - lastHandleHitTime > ImpactFallbackWindow;
        }

        #endregion

    } // end class Plugin

} // end namespace
