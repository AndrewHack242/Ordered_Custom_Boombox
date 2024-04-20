using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

//todo make into not infinite sprint

namespace Ordered_Custom_Boombox
{
    [BepInPlugin(mod_GUID, mod_name, mod_version)]
    public class Ordered_custom_boombox_base : BaseUnityPlugin
    {
        private const string mod_GUID = "Hackattack242.Ordered_Custom_Boombox";
        private const string mod_name = "Ordered Custom Boombox";
        private const string mod_version = "0.0.1";

        private readonly Harmony harmony = new Harmony(mod_GUID);

        private static Ordered_custom_boombox_base Instance;

        private ManualLogSource logger;


        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            logger = BepInEx.Logging.Logger.CreateLogSource(mod_name);

            logger.LogInfo("Sample mod has awakened! :D");

            harmony.PatchAll(typeof(Ordered_custom_boombox_base));
            harmony.PatchAll(typeof(Patches.Boombox_start_music_patch));
            harmony.PatchAll(typeof(Netcode.Sync_track_num)); // todo remove the specifics here
        }

        internal static void LogDebug(string message)
        {
            Instance.Log(message, (LogLevel)32);
        }

        internal static void LogInfo(string message)
        {
            Instance.Log(message, (LogLevel)16);
        }

        internal static void LogWarning(string message)
        {
            Instance.Log(message, (LogLevel)4);
        }

        internal static void LogError(string message)
        {
            Instance.Log(message, (LogLevel)2);
        }

        internal static void LogError(Exception ex)
        {
            Instance.Log(ex.Message + "\n" + ex.StackTrace, (LogLevel)2);
        }

        private void Log(string message, LogLevel logLevel)
        {
            ((Ordered_custom_boombox_base)this).logger.Log(logLevel, (object)message);
        }

}
}
