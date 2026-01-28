/******************************************************************************
 *          Rogue Piñatas: VRmageddon bHaptics Integration by Dteyn
  *              https://github.com/Dteyn/RoguePinatas_bHaptics
 *****************************************************************************/

using HarmonyLib;
using NerdNinjas.Fiesta;
using NerdNinjas.PlayFabAPI;
using NerdNinjas.Services;
using System;
using System.Collections;
using tact_csharp2;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static RoguePinatas_bHaptics.Config;

namespace RoguePinatas_bHaptics
{
    // ============ STARTUP HEARTBEAT ============

    /// <summary>
    /// Play a startup heartbeat during the splash screen load.
    /// </summary>
    [HarmonyPatch(typeof(LoadingScreenManager), "StartInitialSplashScreen")]
    public class Patch_OnSplashLogo
    {
        private static bool playedHeartbeat;

        [HarmonyPostfix]
        public static void Postfix()
        {
            if (Main.tactsuitVr == null || TactsuitVR.suitDisabled)
            {
                return;
            }

            if (playedHeartbeat)
            {
                return;
            }

            dLog.Debug("[LoadingScreenManager] Playing startup heartbeat.");
            TactsuitVR.Play(TactsuitVR.heartBeatEvent);
            playedHeartbeat = true;
        }
    }


    // ============ GAME STATE / MENU PAUSE ============

    /// <summary>
    /// Hook when the game is paused, in order to pause heartbeat (if active) when game is paused.
    /// </summary>
    [HarmonyPatch(typeof(PauseService), "SetPause")]
    public class Patch_OnPause
    {
        [HarmonyPostfix]
        public static void Postfix(bool isPaused)
        {
            if (Main.tactsuitVr == null || TactsuitVR.suitDisabled)
            {
                return;
            }

            if (Main.IsSinglePlayer())  // in multiplayer gameplay continues so only do this in single player
            {
                Main.hapticsPaused = isPaused;
                if (isPaused)
                {
                    dLog.Debug("[SetPause] Game paused, stopping haptic feedback.");
                    BhapticsSDK2Wrapper.stopAll();
                    TactsuitVR.StopHeartBeat();
                }
                else
                {
                    dLog.Debug("[SetPause] Game unpaused, restarting heartbeat if needed.");
                    Main.UpdateHeartBeat(XRPlayer.Instance?.CombatPlayer);
                }
            }
        }

    }


    // ============ EXPLOSIONS ============

    /// <summary>
    /// Triggers proximity-based explosion haptics for common explosions (most projectile explosions).
    /// </summary>
    /// <remarks>
    /// Target: NerdNinjas.Fiesta.VfxCommandBuffer.SpawnCommonExplosionVfx()
    /// </remarks>
    [HarmonyPatch(typeof(VfxCommandBuffer), "SpawnCommonExplosionVfx")]
    public class Patch_OnCommonExplosionVfx
    {
        [HarmonyPostfix]
        public static void Postfix(float3 targetPosWS, float radius)
        {
            if (radius <= 0f || !Main.CanPlayHaptics())
            {
                return;
            }

            Vector3 pos = new Vector3(targetPosWS.x, targetPosWS.y, targetPosWS.z);

            dLog.Debug($"[SpawnCommonExplosionVfx] position: {pos}, radius: {radius}");

            Main.PlayExplosionHaptics(pos, radius);
        }
    }

    /// <summary>
    /// Triggers proximity-based explosion haptics for local player PopSweeper explosions.
    /// </summary>
    /// <remarks>
    /// Target: NerdNinjas.Fiesta.VfxCommandBuffer.SpawnPopSweeperExplosionVfx()
    /// </remarks>
    [HarmonyPatch(typeof(VfxCommandBuffer), "SpawnPopSweeperExplosionVfx")]
    public class Patch_OnPopSweeperExplosionVfx
    {
        [HarmonyPostfix]
        public static void Postfix(float3 targetPosWS, float radius)
        {
            if (radius <= 0f || !Main.CanPlayHaptics())
            {
                return;
            }

            Vector3 pos = new Vector3(targetPosWS.x, targetPosWS.y, targetPosWS.z);

            dLog.Debug($"[SpawnPopSweeperExplosionVfx] position: {pos}, radius: {radius}");

            Main.PlayExplosionHaptics(pos, radius);
        }
    }

    /// <summary>
    /// Triggers proximity-based explosion haptics for bomber/enemy explosions.
    /// </summary>
    /// <remarks>
    /// Target: NerdNinjas.Fiesta.VfxCommandBuffer.SpawnBomberExplosionVfx()
    /// </remarks>
    [HarmonyPatch(typeof(VfxCommandBuffer), "SpawnBomberExplosionVfx")]
    public class Patch_OnBomberExplosionVfx
    {
        [HarmonyPostfix]
        public static void Postfix(float3 targetPosWS, float radius)
        {
            if (radius <= 0f || !Main.CanPlayHaptics())
            {
                return;
            }

            Vector3 pos = new Vector3(targetPosWS.x, targetPosWS.y, targetPosWS.z);

            dLog.Debug($"[SpawnBomberExplosionVfx] position: {pos}, radius: {radius}");

            Main.PlayExplosionHaptics(pos, radius);
        }
    }

    /// <summary>
    /// Triggers proximity-based explosion haptics for vehicle explosions.
    /// </summary>
    /// <remarks>
    /// Target: NerdNinjas.Fiesta.VfxCommandBuffer.SpawnVehicleExplosionVfx()
    /// </remarks>
    [HarmonyPatch(typeof(VfxCommandBuffer), "SpawnVehicleExplosionVfx")]
    public class Patch_OnVehicleExplosionVfx
    {
        [HarmonyPostfix]
        public static void Postfix(float3 positionWS, float damage, float radius)
        {
            if (damage <= 0f || !Main.CanPlayHaptics())
            {
                return;
            }

            Vector3 pos = new Vector3(positionWS.x, positionWS.y, positionWS.z);

            dLog.Debug($"[SpawnVehicleExplosionVfx] position: {pos}, radius: {radius}");

            Main.PlayExplosionHaptics(pos, radius);
        }
    }

    /// <summary>
    /// Incoming net-sync for common explosions (most projectile explosions).
    /// Target: NerdNinjas.Fiesta.S_NetSync.Record<CommonExplosionVfxRequest>.ProcessIncomingRequest()
    /// </summary>
    [HarmonyPatch(typeof(S_NetSync.Record<CommonExplosionVfxRequest>), "ProcessIncomingRequest")]
    public class Patch_OnCommonExplosionNetSyncIncoming
    {
        [HarmonyPrefix]
        public static void Prefix(EntityManager entityManager, Entity incomingRequestsEntity, object[] data)
        {
            if (data == null || data.Length < 2 || !(data[1] is string json) || !Main.CanPlayHaptics())
            {
                return;
            }

            CommonExplosionVfxRequest req = JsonUtility.FromJson<CommonExplosionVfxRequest>(json);
            Vector3 pos = new Vector3(req.TargetPosWS.x, req.TargetPosWS.y, req.TargetPosWS.z);

            dLog.Debug($"[CommonExplosionVfxRequest] position: {pos}, radius: {req.Radius}");

            Main.PlayExplosionHaptics(pos, req.Radius);
        }
    }

    /// <summary>
    /// Incoming net-sync for PopSweeper explosions.
    /// Target: NerdNinjas.Fiesta.S_NetSync.Record<PopSweeperExplosionVfxRequest>.ProcessIncomingRequest()
    /// </summary>
    [HarmonyPatch(typeof(S_NetSync.Record<PopSweeperExplosionVfxRequest>), "ProcessIncomingRequest")]
    public class Patch_OnPopSweeperExplosionNetSyncIncoming
    {
        [HarmonyPrefix]
        public static void Prefix(EntityManager entityManager, Entity incomingRequestsEntity, object[] data)
        {
            if (data == null || data.Length < 2 || !(data[1] is string json) || !Main.CanPlayHaptics())
            {
                return;
            }

            PopSweeperExplosionVfxRequest req = JsonUtility.FromJson<PopSweeperExplosionVfxRequest>(json);
            Vector3 pos = new Vector3(req.TargetPosWS.x, req.TargetPosWS.y, req.TargetPosWS.z);

            dLog.Debug($"[PopSweeperExplosionVfxRequest] position: {pos}, radius: {req.Radius}");

            Main.PlayExplosionHaptics(pos, req.Radius);
        }
    }

    // ============ HEALING AND DEATH HAPTICS ============

    /// <summary>
    /// Triggers healing or revival haptic feedback. Plays resurrection effects for revives, or standard healing
    /// effects for health restoration. Updates heartbeat state after healing completes.
    /// </summary>
    /// <remarks>
    /// Target: NerdNinjas.Fiesta.CombatPlayer.Heal()
    /// </remarks>
    [HarmonyPatch(typeof(CombatPlayer), "Heal")]
    public class Patch_OnHealAndRevive
    {
        [HarmonyPostfix]
        public static void Postfix(CombatPlayer __instance, int healAmount, bool revive, bool __result)
        {
            if (Main.tactsuitVr == null || TactsuitVR.suitDisabled || !Main.IsLocalPlayer(__instance))
            {
                return;
            }

            // Guard to prevent haptics if returned to garage
            if (!Main.IsGameplay())
            {
                BhapticsSDK2Wrapper.stopAll();
                TactsuitVR.StopHeartBeat();
                Main.downedHapticsPlayed = false;
                return;
            }

            // Revive haptics - triggered if we get to Heal and revive = true
            if (revive)
            {
                if (Main.downedHapticsPlayed)
                {
                    string eventId = "player_revive";  // vest and arms
                    float intensity = 1.0f;
                    float duration = 0.75f;

                    // Required values for playParam
                    int reqId = 0;
                    float angleX = 0f;
                    float offsetY = 0f;

                    dLog.Debug("[Heal] Player revived; playing revive haptics");

                    TactsuitVR.PlayParam(eventId, reqId, intensity, duration, angleX, offsetY);

                    Main.downedHapticsPlayed = false;  // Reset downed haptics so they'll play if downed again
                }
                Main.UpdateHeartBeat(__instance);
                return;  // skip Heal haptics when revived
            }

            if (healAmount <= 0)
            {
                return;
            }

            // Healing haptics
            if (__result && healAmount >= 5)  // prevent heal haptics on 1-4 HP gain (ie: from health regen)
            {
                string eventId = "player_heal";  // vest

                dLog.Debug($"[Heal] Player healed (healAmount: {healAmount}); playing Heal haptics");
                TactsuitVR.Play(eventId);
            }

            Main.UpdateHeartBeat(__instance);
        }
    }

    /// <summary>
    /// Monitors health changes to trigger death haptics and manage heartbeat state based on current health.
    /// </summary>
    /// <remarks>
    /// Target: NerdNinjas.Fiesta.CombatPlayer.OnHealthChanged()
    /// </remarks>
    [HarmonyPatch(typeof(CombatPlayer), "OnHealthChanged")]
    public class Patch_OnHealthChanged
    {
        [HarmonyPostfix]
        public static void Postfix(CombatPlayer __instance)
        {
            if (Main.tactsuitVr == null || TactsuitVR.suitDisabled || !Main.IsLocalPlayer(__instance))
            {
                return;
            }

            // Guard to prevent haptics if returned to garage
            if (!Main.IsGameplay())
            {
                BhapticsSDK2Wrapper.stopAll();
                TactsuitVR.StopHeartBeat();
                Main.downedHapticsPlayed = false;
                return;
            }

            if (__instance.IsDead || Main.IsLocalPlayerDowned())
            {
                if (!Main.downedHapticsPlayed)
                {
                    dLog.Debug("[OnHealthChanged] Player downed; stopping all haptics and heartbeat and playing death haptics.");
                    BhapticsSDK2Wrapper.stopAll();      // Stop any playing haptic effects
                    TactsuitVR.StopHeartBeat();  // Stop heartbeat
                    TactsuitVR.Play("player_death");    // Play death haptics
                    Main.downedHapticsPlayed = true;  // Only fire once between revives
                }
                return;
            }

            // Revive haptics: if we were downed and are now up, play revive and reset
            if (Main.downedHapticsPlayed && !Main.IsLocalPlayerDowned() && !__instance.IsDead)
            {
                string eventId = "player_revive";  // vest and arms
                float intensity = 1.0f;
                float duration = 0.75f;

                // Required values for playParam
                int reqId = 0;
                float angleX = 0f;
                float offsetY = 0f;

                dLog.Debug("[OnHealthChanged] Player revived; playing revive haptics");

                TactsuitVR.PlayParam(eventId, reqId, intensity, duration, angleX, offsetY);

                // Reset the downed haptics flag so they'll play on next downed event
                Main.downedHapticsPlayed = false;
            }


            Main.UpdateHeartBeat(__instance);

        }
    }

    // ============ WEAPON HAPTICS =============

    /// <summary>
    /// Triggers recoil haptics for hitscan weapon firing with hand-specific feedback.
    /// </summary>
    /// <remarks>
    /// Target: NerdNinjas.Fiesta.HitscanProjectileFirer.OnWeaponFire()
    /// </remarks>
    [HarmonyPatch(typeof(HitscanProjectileFirer), "OnWeaponFire")]
    public class Patch_OnHitscanWeaponFire
    {
        [HarmonyPostfix]
        public static void Postfix(HitscanProjectileFirer __instance, bool isMine)
        {
            if (!isMine || !Main.CanPlayHaptics())
            {
                return;
            }

            EquippableWeapon weapon = __instance?.EquippableWeapon;
            if (weapon?.EquippingWeaponSlot == null)
            {
                return;
            }

            dLog.Debug($"[OnWeaponFire] Hitscan weapon fired, weaponId: {weapon.WeaponID}, " +
                $"isLeftHand: {weapon.EquippingWeaponSlot.IsLeftHand}");

            Main.PlayRangedFireHaptics(weapon, weapon.EquippingWeaponSlot.IsLeftHand, null);
        }
    }

    /// <summary>
    /// Triggers recoil haptics for physical projectile weapon firing, passing projectile classification for customization.
    /// </summary>
    /// <remarks>
    /// Target: NerdNinjas.Fiesta.PhysicalProjectileFirer.OnWeaponFire()
    /// </remarks>
    [HarmonyPatch(typeof(PhysicalProjectileFirer), "OnWeaponFire")]
    public class Patch_OnPhysicalWeaponFire
    {
        [HarmonyPostfix]
        public static void Postfix(PhysicalProjectileFirer __instance, bool isMine)
        {
            if (!isMine || !Main.CanPlayHaptics())
            {
                return;
            }

            EquippableWeapon weapon = __instance?.EquippableWeapon;
            if (weapon?.EquippingWeaponSlot == null)
            {
                return;
            }

            dLog.Debug($"[OnWeaponFire] Physical projectile fired, weaponId: {weapon.WeaponID}, " +
                $"isLeftHand: {weapon.EquippingWeaponSlot.IsLeftHand}");

            Main.PlayRangedFireHaptics(weapon, weapon.EquippingWeaponSlot.IsLeftHand, __instance.WeaponClass);
        }
    }

    /// <summary>
    /// Triggers recoil haptics for the Jaw Dropper weapon.
    /// </summary>
    /// <remarks>
    /// Target: NerdNinjas.Fiesta.JawDropperWeapon.OnWeaponFire()
    /// </remarks>
    [HarmonyPatch(typeof(JawDropperWeapon), "OnWeaponFire")]
    public class Patch_OnJawDropperFire
    {
        [HarmonyPostfix]
        public static void Postfix(JawDropperWeapon __instance, bool isMine)
        {
            if (!isMine || !Main.CanPlayHaptics())
            {
                return;
            }

            EquippableWeapon weapon = __instance?.EquippableWeapon;
            if (weapon?.EquippingWeaponSlot == null)
            {
                return;
            }

            dLog.Debug($"[OnWeaponFire] JawDropper fired, isLeftHand: {weapon.EquippingWeaponSlot.IsLeftHand}");

            Main.PlayRangedFireHaptics(weapon, weapon.EquippingWeaponSlot.IsLeftHand, null);
        }
    }

    /// <summary>
    /// Triggers melee hit haptics when damaging enemies, with special handling for JolliZapper contact.
    /// </summary>
    /// <remarks>
    /// Target: NerdNinjas.Fiesta.NetSyncService.DamageEnemy()
    /// </remarks>
    [HarmonyPatch(typeof(NetSyncService), "DamageEnemy")]
    public class Patch_OnMeleeHitEnemy
    {
        // Tracking for JolliZapper multiple targets
        private static int lastZapperFrame = -1;
        private static int zapperHitCountThisFrame = 0;

        [HarmonyPostfix]
        public static void Postfix(NetSyncService.DamageParameters damageParameters)
        {
            if (damageParameters.damage <= 0f || !Main.CanPlayHaptics())
            {
                return;
            }

            string weaponId = damageParameters.damageSource.ToString();

            // BoomBoxer/BoomBlaster are handled separately
            if (weaponId.StartsWith("Boom"))
            {
                return;
            }    

            // JolliZapper contact
            // Uses 50% intensity for single enemy, 100% intensity for multiple enemies
            if (damageParameters.damageSource == LocalizationKeys.JolliZapper)
            {
                int frame = Time.frameCount;
                if (frame != lastZapperFrame)
                {
                    lastZapperFrame = frame;
                    zapperHitCountThisFrame = 0;
                }

                // If we called 2 or more times in same tick, the zapper is connected to multiple enemies
                zapperHitCountThisFrame++;

                // Pass 'isMultiTarget' as true for multi-target for higher intensity feedback
                bool isMultiTarget = zapperHitCountThisFrame > 1;

                if (Main.TryGetZapperHand(out bool isLeftHand))
                {
                    Main.PlayJolliZapperHitHaptics(isLeftHand, isMultiTarget);
                }

                return;
            }

            var locator = ServiceLocator.Instance;
            if (locator == null || !locator.Has<PlayerService>())
            {
                return;
            }
            PlayerService playerService = locator.Get<PlayerService>();
            if (playerService?.LocalPlayer == null || damageParameters.playerID != playerService.LocalPlayer.playerID)
            {
                return;
            }

            // Regular melee hit
            if (!LocalizationKeys.MeleeWeapons.Contains(damageParameters.damageSource))
            {
                return;
            }

            
            if (!Main.TryGetHandForWeapon(weaponId, out bool isLeftHand2))
            {
                return;
            }

            dLog.Debug($"[DamageEnemy] Melee hit, weaponId: {weaponId}, isLeftHand: {isLeftHand2}");

            Main.PlayMeleeHitHaptics(isLeftHand2, weaponId);
        }
    }

    /// <summary>
    /// Triggers melee hit haptics when damaging destructible objects and interactable items.
    /// </summary>
    /// <remarks>
    /// Target: NerdNinjas.Fiesta.ECSNetworkSyncService.CallDamageInteractable()
    /// </remarks>
    [HarmonyPatch(typeof(ECSNetworkSyncService), "CallDamageInteractable")]
    public class Patch_OnMeleeHitInteractable
    {
        [HarmonyPostfix]
        public static void Postfix(int index, float damageDelta, bool isHit, FixedString64Bytes damageSource)
        {
            if (!isHit || !Main.CanPlayHaptics())
            {
                return;
            }

            string weaponId = damageSource.ToString();

            // BoomBoxer/BoomBlaster are handled separately
            if (weaponId.StartsWith("Boom"))
            {
                return;
            }

            // JolliZapper contact on interactables
            if (damageSource == LocalizationKeys.JolliZapper)
            {
                if (Main.TryGetZapperHand(out bool isZapperLeftHand))
                {
                    Main.PlayJolliZapperHitHaptics(isZapperLeftHand, false);
                }
                return;
            }

            // Melee weapon contact on interactables
            if (damageDelta <= 0f || !LocalizationKeys.MeleeWeapons.Contains(damageSource))
            {
                return;
            }
            
            if (!Main.TryGetHandForWeapon(weaponId, out bool isLeftHand))
            {
                return;
            }

            dLog.Debug($"[CallDamageInteractable] index (unused): {index}, damageDelta (unused): {damageDelta}, " +
                $"isHit: {isHit}, weaponId: {weaponId}, isLeftHand: {isLeftHand}");

            Main.PlayMeleeHitHaptics(isLeftHand, weaponId);
        }
    }

    /// <summary>
    /// BoomBoxer haptics - fire on shockwave
    /// </summary>
    [HarmonyPatch(typeof(S_PoolableVfxGenericInit), "OnUpdate")]
    public static class Patch_BoomBoxerRingVfxInit
    {
        private static readonly PoolableVfxId BoomBoxerVfxId = PoolableVfxId.FromType<UEC_BoomBoxerShockwaveVfx>();

        public static void Prefix(ref SystemState state)
        {
            if (!Main.CanPlayHaptics())
            {
                return;
            }


            // Avoid duplicate worlds
            var defaultWorld = World.DefaultGameObjectInjectionWorld;
            if (defaultWorld == null ||
                state.WorldUnmanaged.SequenceNumber != defaultWorld.Unmanaged.SequenceNumber)
            {
                return;
            }

            var em = state.EntityManager;

            var query = em.CreateEntityQuery(new EntityQueryDesc
            {
                All = new[]
                {
                ComponentType.ReadOnly<C_PoolableVfxInstance>(),
                ComponentType.ReadOnly<C_NeedsDataInitialized>()
            }
            });

            using var entities = query.ToEntityArray(Allocator.Temp);

            foreach (var e in entities)
            {
                var instance = em.GetComponentData<C_PoolableVfxInstance>(e);
                if (instance.PoolableVfxId != BoomBoxerVfxId)
                {
                    continue;
                }

                // Follow parent -> shockwave entity
                if (!em.HasComponent<Parent>(e))
                {
                    continue;
                }

                var parent = em.GetComponentData<Parent>(e).Value;
                if (!em.HasComponent<C_Shockwave>(parent))
                {
                    continue;
                }

                var shockwave = em.GetComponentData<C_Shockwave>(parent);

                // Filter to local player
                if (em.HasComponent<C_PhysicalProjectileNetworkData>(parent))
                {
                    var net = em.GetComponentData<C_PhysicalProjectileNetworkData>(parent);
                    var playerService = ServiceLocator.Instance?.Get<PlayerService>();
                    if (playerService?.LocalPlayer != null &&
                        net.EntityIndex != playerService.LocalPlayer.playerID)
                    {
                        continue;
                    }
                }

                // Apply to left/right hand
                bool isLeftHand = shockwave.Weapon.HandIndex == 0;

                // BoomBoxer specific haptics
                string eventId = isLeftHand ? "weapon_boomboxer_l" : "weapon_boomboxer_r";

                // Config values for BoomBoxer/BoomBlaster intensity
                float intensity = (shockwave.Weapon.WeaponType == WeaponType.BoomBlaster) 
                    ? WeaponBoomBlasterIntensity  // Default: 1.0f
                    : WeaponBoomBoxerIntensity;   // Default: 0.8f

                // Required values for playParam
                int reqId = 0;
                float duration = 1.0f;
                float angleX = 0f;
                float offsetY = 0f;

                dLog.Debug($"[S_PoolableVfxGenericInit.OnUpdate] Shockwave weaponType: {shockwave.Weapon.WeaponType}, eventId: {eventId}, " +
                    $"intensity: {intensity}, isLeftHand: {isLeftHand}");

                TactsuitVR.PlayParam(eventId, reqId, intensity, duration, angleX, offsetY);
            }
        }
    }

    // ============ CANDY PICKUPS AND LEVEL UPGRADES ============

    /// <summary>
    /// Triggers pickup haptics when collecting candy.
    /// </summary>
    /// <remarks>
    /// Target: NerdNinjas.Fiesta.CombatPlayer.AddCandy()
    /// </remarks>
    [HarmonyPatch(typeof(CombatPlayer), "AddCandy")]
    public class Patch_OnAddCandy
    {
        [HarmonyPostfix]
        public static void Postfix(CombatPlayer __instance, float candyDelta, bool __result)
        {
            if (!Main.CanPlayHaptics() || !Main.IsLocalPlayer(__instance))
            {
                return;
            }

            if (__result && candyDelta > 0f)
            {
                string eventId = "player_candypickup";  // vest
                float duration = 0.5f;                  // duration 0.5f for normal candy pickups
                float intensity = CandyPickupSmall; // Default: 0.35f

                // Use a higher intensity for larger candy pickups
                if (candyDelta >= 2)
                {
                    intensity = CandyPickupLarge; // Default: 0.7f
                }

                // Required values for playParam
                int reqId = 0;
                float angleX = 0f;
                float offsetY = 0f;

                dLog.Debug($"[AddCandy] candyDelta: {candyDelta}, intensity: {intensity}, duration: {duration}");
                TactsuitVR.PlayParam(eventId, reqId, intensity, duration, angleX, offsetY);

            }
        }
    }

    /// <summary>
    /// Triggers pickup haptics when collecting meta candy.
    /// </summary>
    /// <remarks>
    /// Target: NerdNinjas.Fiesta.CombatPlayer.AddMetaCandy()
    /// </remarks>
    [HarmonyPatch(typeof(CombatPlayer), "AddMetaCandy")]
    public class Patch_OnAddMetaCandy
    {
        [HarmonyPostfix]
        public static void Postfix(CombatPlayer __instance, int candyDelta, bool __result)
        {
            if (!Main.CanPlayHaptics() || !Main.IsLocalPlayer(__instance))
            {
                return;
            }

            if (__result && candyDelta > 0)
            {
                string eventId = "player_candypickup";  // vest
                float duration = 1.0f;                  // duration 0.5f for meta candy pickups
                float intensity = MetaCandyPickupSmall; // Default: 0.7f

                // Use a higher intensity for larger meta candy pickups
                if (candyDelta >= 2)
                {
                    intensity = MetaCandyPickupLarge; // Default: 1.0f
                }

                // Required values for playParam
                int reqId = 0;
                float angleX = 0f;
                float offsetY = 0f;

                dLog.Debug($"[AddMetaCandy] candyDelta: {candyDelta}, intensity: {intensity}, duration: {duration}");
                TactsuitVR.PlayParam(eventId, reqId, intensity, duration, angleX, offsetY);
            }
        }
    }

    /// <summary>
    /// Triggers haptics at each level upgrade.
    /// </summary>
    /// <remarks>
    /// Target: NerdNinjas.Fiesta.UpgradeService.PlayUpgradeSFX()
    /// </remarks>
    [HarmonyPatch(typeof(UpgradeService), "PlayUpgradeSFX")]
    public class Patch_OnLevelUp
    {
        public static void Postfix()
        {
            if (!Main.CanPlayHaptics())
            {
                return;
            }

            float intensity = LevelUpIntensity; // Default: 0.75f;
            float duration = 0.4f;

            // Required values for playParam, not used here, but since we are using intensity, we need these.
            string eventId = "player_levelup";  // vest
            int reqId = 0;
            float angleX = 0f;
            float offsetY = 0f;

            dLog.Debug($"[PlayUpgradeSFX] Level upgrade!");
            TactsuitVR.PlayParam(eventId, reqId, intensity, duration, angleX, offsetY);

        }

    }

    /// <summary>
    /// Triggers haptics at each parti box opening.
    /// </summary>
    /// <remarks>
    /// Target: NerdNinjas.Fiesta.UpgradeService.PlayPartiBoxSFX()
    /// </remarks>
    [HarmonyPatch(typeof(UpgradeService), "PlayPartiBoxSFX")]
    public class Patch_OnPartiBox
    {
        public static void Postfix()
        {
            if (!Main.CanPlayHaptics())
            {
                return;
            }

            float intensity = PartiBoxIntensity; // Default: 1.0f;
            float duration = 0.8f;

            // Required values for playParam, not used here, but since we are using intensity, we need these.
            string eventId = "player_partibox";  // vest
            int reqId = 0;
            float angleX = 0f;
            float offsetY = 0f;

            dLog.Debug($"[PlayPartiBoxSFX] PartiBox upgrade! Yaaaaaay!");
            TactsuitVR.PlayParam(eventId, reqId, intensity, duration, angleX, offsetY);
        }

    }

    // ============ PLAYER IMPACTS / TAKING DAMAGE ============

    /// <summary>
    /// Front/rear impact based on hit position from player collision.
    /// </summary>
    /// <remarks>
    /// Target: NerdNinjas.Fiesta.S_PlayerCollision.HandleHit()
    /// NOTE: This is a tad over-engineered, but it works. :)
    /// </remarks>
    [HarmonyPatch(typeof(S_PlayerCollision), "HandleHit")]
    public class Patch_OnPlayerCollision_HandleHit
    {
        private static readonly string impactEventFront = "player_impact";      // vest front
        private static readonly string impactEventRear = "player_impact_rear";  // vest rear

        [HarmonyPrefix]
        public static void Prefix(
            ref SystemState state,
            ref EntityCommandBuffer ecb,
            ref VfxCommandBuffer vfxcb,
            ref C_PlayerDamager playerDamager,
            ref DynamicBuffer<C_LocalPlayerDamageSourceRegistration> damageSourceRegister,
            ref C_DamageNumberVfxCommandBuffer damageNumberVfx,
            C_PlayerTag playerTag,
            Entity sourceEntity,
            Entity actionTargetEntity,
            float3 position)
        {
            // Early exit: Check if haptics system is available and player isn't downed
            if (!Main.CanPlayHaptics())
            {
                return;
            }

            // Early exit: Check if ServiceLocator is available
            var locator = ServiceLocator.Instance;
            if (locator == null || !locator.Has<PlayerService>())
            {
                return;
            }

            PlayerService playerService = locator.Get<PlayerService>();
            if (playerService == null || playerService.LocalPlayer == null)
            {
                return;
            }

            // Early exit: Check if damage is valid and enabled
            if (!playerDamager.IsEnabled || playerDamager.DamageAmount <= 0f)
            {
                return;
            }

            // Early exit: Check if this damage is for the local player
            if (playerService.LocalPlayer.playerID != playerTag.TargetPlayerId)
            {
                return;
            }

            // Calculate intensity based on damage amount
            float intensity = Main.GetImpactIntensityFromDamage((int)playerDamager.DamageAmount);

            // Use HMD camera position as reference point for accurate directional feedback
            Transform basis = XRPlayer.Instance.XRCameraTransform ?? XRPlayer.Instance.transform;

            if (XRPlayer.Instance.XRCameraTransform == null)
            {
                dLog.Debug("[HandleHit] XRCameraTransform is NULL; falling back to XRPlayer transform.");
            }

            Vector3 playerPos = basis.position;
            Vector3 playerForward = basis.forward;
            Vector3 playerRight = basis.right;

            // Get the position of the damage source (enemy/projectile)
            Vector3 hitPos = Vector3.zero;
            bool hasValidHitPos = false;

            if (sourceEntity != Entity.Null && state.EntityManager.Exists(sourceEntity))
            {
                if (state.EntityManager.HasComponent<LocalTransform>(sourceEntity))
                {
                    var sourceTransform = state.EntityManager.GetComponentData<LocalTransform>(sourceEntity);
                    hitPos = new Vector3(sourceTransform.Position.x, sourceTransform.Position.y, sourceTransform.Position.z);
                    hasValidHitPos = true;
                    // dLog.Debug($"[HandleHit] Got source position from LocalTransform: {hitPos}");
                }
            }

            // Fallback: If we can't determine hit position, use centered front impact
            if (!hasValidHitPos || hitPos == Vector3.zero)
            {
                dLog.Warn($"[HandleHit] Could not determine hit position. Using centered front impact.");
                Main.MarkHandleHit();
                TactsuitVR.PlayParam(impactEventFront, 0, intensity, 1.0f, 0f, 0f);
                return;
            }

            Vector3 toHit = hitPos - playerPos;
            dLog.Debug($"[HandleHit] playerPos: {playerPos}, hitPos: {hitPos}, toHit: {toHit}");

            // Calculate vertical offset (Y) based on hit height relative to player
            // offsetY range is typically -0.5 to 0.5 for bHaptics
            float hitHeightRelative = toHit.y;
            float offsetY = Mathf.Clamp(hitHeightRelative * 0.7f, -0.5f, 0.5f);

            // Project to horizontal plane for directional calculations
            Vector3 toHitHorizontal = new Vector3(toHit.x, 0f, toHit.z);
            Vector3 forwardHorizontal = new Vector3(playerForward.x, 0f, playerForward.z);
            Vector3 rightHorizontal = new Vector3(playerRight.x, 0f, playerRight.z);

            // Guard against zero-length vectors (enemy too close or on top of player)
            if (toHitHorizontal.sqrMagnitude < 0.0001f || forwardHorizontal.sqrMagnitude < 0.0001f)
            {
                dLog.Warn("[HandleHit] Hit direction too close to player. Using default front impact at center.");
                Main.MarkHandleHit();
                TactsuitVR.PlayParam(impactEventFront, 0, intensity, 1.0f, 0f, offsetY);
                return;
            }

            // Normalize for calculations
            Vector3 toHitNorm = toHitHorizontal.normalized;
            Vector3 forwardNorm = forwardHorizontal.normalized;
            Vector3 rightNorm = rightHorizontal.normalized;

            // Determine front vs rear impact using dot product
            // Positive = front, Negative = rear
            float dotForward = Vector3.Dot(forwardNorm, toHitNorm);
            bool isRear = dotForward < 0f;
            string eventId = isRear ? impactEventRear : impactEventFront;

            // Calculate horizontal offset (X) - left/right position on vest
            // For bHaptics, angleX typically ranges from -1.0 (left) to 1.0 (right)
            float dotRight = Vector3.Dot(rightNorm, toHitNorm);

            // Map to angleX range based on the angle
            // Scale by how directly we're facing the hit to prevent extreme angles for glancing blows
            float angleX = dotRight * Mathf.Clamp01(Mathf.Abs(dotForward) + 0.3f);
            angleX = Mathf.Clamp(angleX, -0.8f, 0.8f); // Limit to reasonable vest width

            // Mark that we've handled hit haptics here in HandleHit to prevent TakeDamage from firing
            Main.MarkHandleHit();

            dLog.Debug($"[HandleHit] Playing '{eventId}' | angleX: {angleX:F2}, offsetY: {offsetY:F2} | " +
                       $"dotForward: {dotForward:F2}, dotRight: {dotRight:F2}, hitHeight: {hitHeightRelative:F2} | " +
                       $"damage: {playerDamager.DamageAmount}, intensity: {intensity:F2}");

            // Play directional impact haptics
            // reqId: 0 to auto-generate request ID
            // duration: 1.0 for default duration multiplier
            int reqId = 0;
            float duration = 1.0f;
            TactsuitVR.PlayParam(eventId, reqId, intensity, duration, angleX, offsetY);
        }
    }

    /// <summary>
    /// Fallback trigger for impact haptics when the player receives damage, with intensity scaled by damage amount.
    /// This only fires if 'S_PlayerCollision.HandleHit' above HAS NOT fired, to prevent duplicated effects.
    /// Feedback from this is front-only.
    /// </summary>
    /// <remarks>
    /// Target: NerdNinjas.Fiesta.CombatPlayer.TakeDamage()
    /// </remarks>
    [HarmonyPatch(typeof(CombatPlayer), "TakeDamage")]
    public class Patch_OnTakeDamage
    {
        [HarmonyPostfix]
        public static void Postfix(CombatPlayer __instance, int damageAmount)
        {
            if (!Main.CanPlayHaptics() || !Main.IsLocalPlayer(__instance))
            {
                return;
            }

            if (damageAmount <= 0)
            {
                Main.UpdateHeartBeat(__instance);
                return;
            }

            // If HandleHit ran recently, CanFallbackImpact will be FALSE to prevent duplicated effects
            if (!Main.CanFallbackImpact())
            {
                Main.UpdateHeartBeat(__instance);
                return;
            }

            // Scale intensity by damage amount
            float intensity = Main.GetImpactIntensityFromDamage(damageAmount);

            dLog.Debug($"[TakeDamage] Using fallback TakeDamage for player damage (HandleHit did not fire); damageAmount: {damageAmount}, intensity: {intensity}");

            string eventId = "player_impact";  // vest front
            int reqId = 0;
            float duration = 1.0f;
            float angleX = 0f;
            float offsetY = 0f;

            TactsuitVR.PlayParam(eventId, reqId, intensity, duration, angleX, offsetY);

            Main.UpdateHeartBeat(__instance);
        }

    }

} // end namespace
