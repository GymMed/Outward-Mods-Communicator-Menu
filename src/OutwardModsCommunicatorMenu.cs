using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using SideLoader;
using OutwardModsCommunicator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.IO;
using OutwardModsCommunicator.EventBus;
using OutwardModsCommunicatorMenu.Events;
using OutwardModsCommunicatorMenu.Utility.Enums;
using OutwardModsCommunicatorMenu.Managers;
using OutwardModsCommunicatorMenu.Tests;
using UnityEngine;
using UniverseLib;
using UniverseLib.Config;

// RENAME 'OutwardModPackTemplate' TO SOMETHING ELSE
namespace OutwardModsCommunicatorMenu
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency(SideLoader.SL.GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(OutwardModsCommunicator.OMC.GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("gymmed.chat_commands_manager", BepInDependency.DependencyFlags.SoftDependency)]
    public class OMCM : BaseUnityPlugin
    {
        // Choose a GUID for your project. Change "myname" and "mymodpack".
        public const string GUID = "gymmed.mods_communicator_menu";
        // Choose a NAME for your project, generally the same as your Assembly Name.
        public const string NAME = "Mods Communicator Menu";
        // Increment the VERSION when you release a new version of your mod.
        public const string VERSION = "0.0.2";

        // Choose prefix for log messages for quicker search and readablity
        public static string prefix = "[Mods-Communicator-Menu]";

        // Will be used as id for accepting events from other mods 
        public const string EVENTS_LISTENER_GUID = GUID + "_*";

        internal static ManualLogSource Log;

        // If you need settings, define them like so:
        //public static ConfigEntry<bool> ExampleConfig;

        // Awake is called when your plugin is created. Use this to set up your mod.
        internal void Awake()
        {
            try
            {
                Log = this.Logger;
                LogMessage($"Hello world from {NAME} {VERSION}!");

                Universe.Init(
                    startupDelay: 1f,
                    onInitialized: () => LogMessage("UniverseLib initialized"),
                    logHandler: (msg, type) => LogMessage($"[UniverseLib] {msg}"),
                    config: new UniverseLibConfig
                    {
                        Force_Unlock_Mouse = true
                    });

                new Harmony(GUID).PatchAll();
                Managers.UIManager.Instance.Initialize(Log);

#if DEBUG
                RunDebugTests();
#endif

                LogMessage("Awake completed successfully");
            }
            catch (Exception ex)
            {
                Logger.LogError($"[OMCM] Error in Awake: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
            }
        }

        // Update is called once per frame. Use this only if needed.
        // You also have all other MonoBehaviour methods available (OnGUI, etc)
        internal void Update()
        {
            Managers.UIManager.Instance.OnUpdate();
        }

        //  Log message with prefix
        public static void LogMessage(string message)
        {
            Log.LogMessage($"{OMCM.prefix} {message}");
        }

        public static void LogStatusMessage(string message, ChatLogStatus status = ChatLogStatus.Info)
        {
            LogMessage($"[{status}] {message}");
        }

        // Log message through side loader, helps to see it
        // if you are using UnityExplorer and want to see live logs
        public static void LogSL(string message)
        {
            SL.Log($"{OMCM.prefix} {message}");
        }

// Gets mod dll location at run time
        public static string GetProjectLocation()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

#if DEBUG
        private void RunDebugTests()
        {
            LogMessage("Running debug tests...");
            var tests = new TypeParserTests();
            tests.RunAllTests();
            LogMessage("Debug tests completed.");
        }
#endif
    }
}
