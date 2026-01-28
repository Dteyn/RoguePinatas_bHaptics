/******************************************************************************
 * File: Logger.cs
 * 
 * Purpose: Adds universal 'dLog' logging utility, as described below.
  *******************************************************************************/

/*
 ======================================================================================
               Logger.cs - dLog() - Logging Helper for BepInEx plugins
 ======================================================================================
                          Version: 1.1-BIE   Author: Dteyn
                              Last Updated: 12/10/25
 
Provides a simple interface for logging using BepInEx logging methods.
Allows selecting timestamps on or off.

 SETUP:
   - Change the namespace below to match your plugin
   - Drop Logger.cs into your plugin folder
 
 USAGE:
  1. Initialize in Load() or Awake():
       dLog.Init(Log);    // Log is the property from BasePlugin

  2. Use anywhere in your plugin or Harmony patches:
       dLog.Info("Plugin started!");
       dLog.Warn("Warning: something looks odd");
       dLog.Error("Error: something broke");
       dLog.Debug("Debugging: value=" + someValue);  // NOTE: dLog.Debug statements only compile in DEBUG release

  3. Options: Toggle timestamp output:
       dLog.AddTimestamp = false;            // Disable timestamp

 ======================================================================================
*/

using BepInEx.Logging;
using System;
using System.Diagnostics;

namespace RoguePinatas_bHaptics
{
    internal static class dLog
    {
        private static ManualLogSource bieLogger;

        /// <summary>Controls whether a timestamp is set.</summary>
        internal static bool AddTimestamp { get; set; } = true;             // Add timestamp option     Default: true
        internal static bool UTCTimestamp { get; set; } = true;             // UTC timestamps, if false uses local timestamps

        /// <summary>Initialize the logger system.</summary>
        internal static void Init(ManualLogSource logger)
        {
            bieLogger = logger;
        }


        // Call-site helpers for each log level

        /// <summary>
        /// INFO log level.
        /// </summary>
        /// <param name="logMsg">Log message (string)</param>
        internal static void Info(string logMsg) => Log(LogLevel.Info, logMsg);

        /// <summary>
        /// WARNING log level.
        /// </summary>
        /// <param name="logMsg">Log message (string)</param>
        internal static void Warn(string logMsg) => Log(LogLevel.Warning, logMsg);

        /// <summary>
        /// ERROR log level.
        /// </summary>
        /// <param name="logMsg">Log message (string)</param>
        internal static void Error(string logMsg) => Log(LogLevel.Error, logMsg);

        /// <summary>
        /// DEBUG log level. Only compiled in DEBUG releases and omitted entirely from Release builds.
        /// </summary>
        /// <remarks>
        /// In Release builds, any calls to 'dLog.Debug()' won't be compiled.
        /// </remarks>
        /// <param name="logMsg">Log message (string)</param>
        [Conditional("DEBUG")]
        internal static void Debug(string logMsg) => Log(LogLevel.Debug, logMsg);

        // Meat and potatoes
        internal static void Log(LogLevel lvl, string msg)
        {
            string timeStr = UTCTimestamp                       // select UTC or local time stamps
                ? DateTime.UtcNow.ToString("HH:mm:ss.fff")
                : DateTime.Now.ToString("HH:mm:ss.fff");
            string finalMsg = AddTimestamp ? $"[{timeStr}] {msg}" : msg;  // add timestamp if enabled

            // BepInEx output
            if (bieLogger != null)
                bieLogger.Log(lvl, finalMsg);

        }
    }
}
