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
using UnityEngine;

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
        public const string VERSION = "0.0.1";

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
            // You can find BepInEx logs in directory "BepInEx\LogOutput.log"
            Log = this.Logger;
            LogMessage($"Hello world from {NAME} {VERSION}!");

            // Harmony is for patching methods. If you're not patching anything, you can comment-out or delete this line.
            new Harmony(GUID).PatchAll();

            //EventBusSubscriber.AddSubscribers();
            EventBusPublisher.SendCommands();

            Managers.UIManager.Instance.Initialize(Log);
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
    }
}
