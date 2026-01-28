/******************************************************************************
 * File: TactsuitVR.cs
 * 
 * Purpose: Adds bHaptics SDK support, using bHaptics tact_csharp2 to interface
 * with bhaptics Player and provide haptic feedback. This file contains the
 * setup methods and credentials for the project to initialize and use
 * throughout the project.
 * 
 * Credit: Florian & Astien for the After the Fall bHaptics mod (SDKv2 implementation).
 * Source: https://github.com/floh-bhaptics/AfterTheFall_bhaptics/blob/master/MyBhapticsTactsuit.cs
 *****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using tact_csharp2;

namespace RoguePinatas_bHaptics
{
    public class TactsuitVR
    {
        // Credentials from bHaptics Developer Portal
        public string sdkAPIKey = "vHVMRYI9GZgTut5ayU0J";  // API Key
        public string workspaceId = "695ad07551ec50d087ca7069";  // Workspace ID

        // State tracking
        public static bool suitDisabled = true;
        public static bool systemInitialized = false;

        // Heartbeat settings
        public static int heartBeatRate = 1000;
        public static string heartBeatEvent = "player_heartbeat";
        public static bool heartBeatRunning;
        private static ManualResetEvent HeartBeatMrse = new ManualResetEvent(false);

        public TactsuitVR()
        {
            dLog.Info("[bHaptics] Initializing bHaptics SDK...");

            // Start Player if it's not running
            if (!BhapticsSDK2Wrapper.isPlayerRunning())
            {
                dLog.Info("[bHaptics] Player is not running. Launching it...");
                BhapticsSDK2Wrapper.launchPlayer(true);
            }

            // Default config exported from the bHaptics Developer Portal, used in case the PC is not online
            // config.json is placed in /Properties and added as a Resource
            var config = Encoding.UTF8.GetString(Properties.Resources.config);

            // Initialize with apiKey, workspaceId, and default initData (config.json) in case API is unreachable
            var init = BhapticsSDK2Wrapper.registryAndInit(sdkAPIKey, workspaceId, config);

            // If it worked, enable the suit
            suitDisabled = !init;

            if (!init)
            {
                dLog.Error("[bHaptics] Init failed; bHaptics is disabled for this session. " +
                    "Check your bHaptics Player settings and ensure your devices are paired and working, then try again.");
                return;
            }

            // Start heartbeat thread
            Thread HeartBeatThread = new Thread(HeartBeatFunc);
            HeartBeatThread.IsBackground = true;
            HeartBeatThread.Start();
        }

        /// <summary>
        /// Play bHaptics event. Includes debug logging.
        /// </summary>
        public static void Play(string eventId)
        {
            dLog.Debug($"[BhapticsSDK2Wrapper.play] Playing event '{eventId}'");

            BhapticsSDK2Wrapper.play(eventId);
        }

        /// <summary>
        /// Play bHaptics event with parameters. Includes debug logging.
        /// </summary>
        public static void PlayParam(string eventId, int requestId, float intensity, float duration, float angleX, float offsetY)
        {
            dLog.Debug($"[BhapticsSDK2Wrapper.playParam] Playing event '{eventId}' with params: requestId: {requestId}, " +
                $"intensity: {intensity}, duration: {duration}, angleX: {angleX}, offsetY: {offsetY}");

            BhapticsSDK2Wrapper.playParam(eventId, requestId, intensity, duration, angleX, offsetY);
        }

        public static void HeartBeatFunc()
        {
            while (true)
            {
                HeartBeatMrse.WaitOne();
                BhapticsSDK2Wrapper.play(heartBeatEvent);
                Thread.Sleep(heartBeatRate);
            }
        }

        public static void StartHeartBeat()
        {
            if (heartBeatRunning) return;
            heartBeatRunning = true;
            HeartBeatMrse.Set();
            dLog.Debug("[bHaptics] Started heartbeat.");
        }

        public static void StopHeartBeat()
        {
            if (!heartBeatRunning) return;
            heartBeatRunning = false;
            HeartBeatMrse.Reset();
            dLog.Debug("[bHaptics] Stopped heartbeat.");
        }

        public static void StopThreads()
        {
            StopHeartBeat();
        }
    } // end class TactsuitVR
}
