using OutwardModsCommunicatorMenu.Utility.Enums;
using OutwardModsCommunicatorMenu.Utility.Helpers;
using OutwardModsCommunicator.EventBus;
using System;
using System.Collections.Generic;

namespace OutwardModsCommunicatorMenu.Events.Publishers
{
    public static class MenuCommandPublisher
    {
        public static void SendMenuCommand()
        {
            var payload = new EventPayload
            {
                [ChatCommandsManagerParamsHelper.Get(ChatCommandsManagerParams.CommandName).key] = "MCMenu",
                [ChatCommandsManagerParamsHelper.Get(ChatCommandsManagerParams.CommandDescription).key] = "Toggle the Mods Communicator Menu",
                [ChatCommandsManagerParamsHelper.Get(ChatCommandsManagerParams.CommandAction).key] = (Action<Character, Dictionary<string, string>>)ToggleMenu,
                [ChatCommandsManagerParamsHelper.Get(ChatCommandsManagerParams.CommandRequiresDebugMode).key] = false
            };

            EventBus.Publish(EventBusPublisher.ChatCommands_Listener, EventBusPublisher.Event_AddCommand, payload);
        }

        private static void ToggleMenu(Character caller, Dictionary<string, string> arguments)
        {
            Managers.MenuVisibilityManager.Instance.Toggle();
            
            var panel = caller?.CharacterUI?.ChatPanel;
            if (panel != null)
            {
                ChatHelpers.SendChatLog(panel, "Toggled Mods Communicator Menu!", ChatLogStatus.Success);
            }
        }
    }
}
