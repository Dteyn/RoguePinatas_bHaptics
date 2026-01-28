/******************************************************************************
 * File: Config.cs
 * 
 * Purpose:
 *  Provides 'Config' class which can be used to easily manage BepInEx
 *  plugin config settings. Settings can then be used throughout project as
 *  'Config.Setting', or by direct setting name if added as a 'using static'
 *******************************************************************************/

using BepInEx;
using BepInEx.Configuration;
using System.IO;

namespace RoguePinatas_bHaptics
{
    public static class Config
    {
        internal static string ConfigFilename = "RoguePinatas_bHaptics.cfg";
        static Config()
        {
            string text = Path.Combine(Paths.ConfigPath, ConfigFilename);
            ConfigFile configFile = new ConfigFile(text, true);

            // SECTION: Effects
            #region Effects

            heartBeatOnLowHealth = configFile.Bind(
                "Effects",
                "HeartBeatOnLowHealth",
                true,
                "Heartbeat effect on low health (less than threshold defined below)");

            lowHealthThreshold = configFile.Bind(
                "Effects",
                "LowHealthThreshold",
                0.2f,
                "Threshold of health (%), below which the heartbeat effect is started (if enabled above)");

            impactIntensityRegDmg = configFile.Bind(
                "Effects",
                "ImpactIntensityRegDmg",
                0.6f,
                "Impact intensity when receiving damage of 1-10 HP");

            impactIntensityMedDmg = configFile.Bind(
                "Effects",
                "ImpactIntensityMedDmg",
                0.8f,
                "Impact intensity when receiving damage of 10-20 HP");

            impactIntensityHighDmg = configFile.Bind(
                "Effects",
                "ImpactIntensityHighDmg",
                1.0f,
                "Impact intensity when receiving damage of 20+ HP");

            candyPickupSmall = configFile.Bind(
                "Effects",
                "CandyPickupSmall",
                0.35f,
                "Candy pickups, small (regular candies)");

            candyPickupLarge = configFile.Bind(
                "Effects",
                "CandyPickupLarge",
                0.7f,
                "Candy pickups, larger candies");

            metaCandyPickupSmall = configFile.Bind(
                "Effects",
                "MetaCandyPickupSmall",
                0.7f,
                "Meta Candy pickups, small (regular meta candies)");

            metaCandyPickupLarge = configFile.Bind(
                "Effects",
                "MetaCandyPickupLarge",
                1.0f,
                "Meta Candy pickups, larger candies");

            levelUpIntensity = configFile.Bind(
                "Effects",
                "LevelUpIntensity",
                0.75f,
                "Level Up effect intensity");

            partiBoxIntensity = configFile.Bind(
                "Effects",
                "PartiBoxIntensity",
                1.0f,
                "PartiBox upgrade effect intensity");


            #endregion

            // SECTION: Weapons
            #region Weapons

            weaponMeleeIntensity = configFile.Bind(
                "Weapons",
                "WeaponMeleeIntensity",
                1.0f,
                "Default intensity for melee weapons (except JolliZapper)");

            weaponJolliZapperIntensity = configFile.Bind(
                "Weapons",
                "WeaponJolliZapperIntensity",
                0.5f,
                "JolliZapper intensity when contacting a single enemy or object");

            weaponJolliZapperMultiIntensity = configFile.Bind(
                "Weapons",
                "WeaponJolliZapperMultiIntensity",
                0.5f,
                "JolliZapper intensity when contacting more than one enemy or object");

            weaponRecoilIntensity = configFile.Bind(
                "Weapons",
                "WeaponRecoilIntensity",
                0.5f,
                "Default recoil intensity for most ranged weapons");

            weaponPopRocketIntensity = configFile.Bind(
                "Weapons",
                "WeaponPopRocketIntensity",
                0.7f,
                "PopRocket recoil intensity");

            weaponPopSweeperIntensity = configFile.Bind(
                "Weapons",
                "WeaponPopSweeperIntensity",
                0.8f,
                "PopSweeper recoil intensity");

            weaponFunderbussIntensity = configFile.Bind(
                "Weapons",
                "WeaponFunderbussIntensity",
                0.75f,
                "Funderbuss recoil intensity");

            weaponFunderDroneIntensity = configFile.Bind(
                "Weapons",
                "WeaponFunderDroneIntensity",
                0.8f,
                "FunderDrone recoil intensity");

            weaponBoomBoxerIntensity = configFile.Bind(
                "Weapons",
                "WeaponBoomBoxerIntensity",
                0.8f,
                "BoomBoxer shockwave intensity");

            weaponBoomBlasterIntensity = configFile.Bind(
                "Weapons",
                "WeaponBoomBlasterIntensity",
                1.0f,
                "BoomBlaster shockwave intensity");

            #endregion

        }
        #region Getters

        // Effects
        public static bool HeartBeatOnLowHealth
        {
            get { return heartBeatOnLowHealth.Value; }
        }
        private static readonly ConfigEntry<bool> heartBeatOnLowHealth;

        public static float LowHealthThreshold
        {
            get { return lowHealthThreshold.Value; }
        }
        private static readonly ConfigEntry<float> lowHealthThreshold;

        public static float ImpactIntensityRegDmg
        {
            get { return impactIntensityRegDmg.Value; }
        }
        private static readonly ConfigEntry<float> impactIntensityRegDmg;

        public static float ImpactIntensityMedDmg
        {
            get { return impactIntensityMedDmg.Value; }
        }
        private static readonly ConfigEntry<float> impactIntensityMedDmg;

        public static float ImpactIntensityHighDmg
        {
            get { return impactIntensityHighDmg.Value; }
        }
        private static readonly ConfigEntry<float> impactIntensityHighDmg;

        public static float CandyPickupSmall
        {
            get { return candyPickupSmall.Value; }
        }
        private static readonly ConfigEntry<float> candyPickupSmall;
        public static float CandyPickupLarge
        {
            get { return candyPickupLarge.Value; }
        }
        private static readonly ConfigEntry<float> candyPickupLarge;
        public static float MetaCandyPickupSmall
        {
            get { return metaCandyPickupSmall.Value; }
        }
        private static readonly ConfigEntry<float> metaCandyPickupSmall;
        public static float MetaCandyPickupLarge
        {
            get { return metaCandyPickupLarge.Value; }
        }
        private static readonly ConfigEntry<float> metaCandyPickupLarge;

        public static float LevelUpIntensity
        {
            get { return levelUpIntensity.Value; }
        }
        private static readonly ConfigEntry<float> levelUpIntensity;

        public static float PartiBoxIntensity
        {
            get { return partiBoxIntensity.Value; }
        }
        private static readonly ConfigEntry<float> partiBoxIntensity;

        // Weapons
        public static float WeaponMeleeIntensity
        {
            get { return weaponMeleeIntensity.Value; }
        }
        private static readonly ConfigEntry<float> weaponMeleeIntensity;

        public static float WeaponJolliZapperIntensity
        {
            get { return weaponJolliZapperIntensity.Value; }
        }
        private static readonly ConfigEntry<float> weaponJolliZapperIntensity;

        public static float WeaponJolliZapperMultiIntensity
        {
            get { return weaponJolliZapperMultiIntensity.Value; }
        }
        private static readonly ConfigEntry<float> weaponJolliZapperMultiIntensity;

        public static float WeaponRecoilIntensity
        {
            get { return weaponRecoilIntensity.Value; }
        }
        private static readonly ConfigEntry<float> weaponRecoilIntensity;

        public static float WeaponPopRocketIntensity
        {
            get { return weaponPopRocketIntensity.Value; }
        }
        private static readonly ConfigEntry<float> weaponPopRocketIntensity;

        public static float WeaponPopSweeperIntensity
        {
            get { return weaponPopSweeperIntensity.Value; }
        }
        private static readonly ConfigEntry<float> weaponPopSweeperIntensity;

        public static float WeaponFunderbussIntensity
        {
            get { return weaponFunderbussIntensity.Value; }
        }
        private static readonly ConfigEntry<float> weaponFunderbussIntensity;

        public static float WeaponFunderDroneIntensity
        {
            get { return weaponFunderDroneIntensity.Value; }
        }
        private static readonly ConfigEntry<float> weaponFunderDroneIntensity;

        public static float WeaponBoomBoxerIntensity
        {
            get { return weaponBoomBoxerIntensity.Value; }
        }
        private static readonly ConfigEntry<float> weaponBoomBoxerIntensity;

        public static float WeaponBoomBlasterIntensity
        {
            get { return weaponBoomBlasterIntensity.Value; }
        }
        private static readonly ConfigEntry<float> weaponBoomBlasterIntensity;

        #endregion
    }
}