using OutwardModsCommunicatorMenu.Utility.Enums;
using OutwardModsCommunicatorMenu.Utility.Helpers;
using OutwardModsCommunicator.EventBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutwardModsCommunicatorMenu.Events.Publishers
{
    public static class ExamplePublisher
    {
        public static void SendExampleCommand()
        {
            Dictionary<string, (string, string)> parameters = new()
            {
                {
                    "yourVar",
                    ("Optional. Some dynamic data that user can provide in chat with --yourVar='value'.", null)
                }
            };

            Action<Character, Dictionary<string, string>> function = TriggerFunction;

            var payload = new EventPayload
            {
                //typing command in chat /example
                [ChatCommandsManagerParamsHelper.Get(ChatCommandsManagerParams.CommandName).key] = "example",
                [ChatCommandsManagerParamsHelper.Get(ChatCommandsManagerParams.CommandParameters).key] = parameters,
                [ChatCommandsManagerParamsHelper.Get(ChatCommandsManagerParams.CommandAction).key] = function,
                [ChatCommandsManagerParamsHelper.Get(ChatCommandsManagerParams.CommandDescription).key] = "Example method for user to test. This description can be seen with chat command:/help example"
            };

            EventBus.Publish(EventBusPublisher.ChatCommands_Listener, EventBusPublisher.Event_AddCommand, payload);
        }

        public static void TriggerFunction(Character caller, Dictionary<string, string> arguments)
        {
            ChatPanel panel = caller?.CharacterUI?.ChatPanel;

            if(panel == null)
            {
                OMCM.LogMessage("ExamplePublisher@TriggerFunction Tried to use missing chatPanel.");
                return;
            }

            arguments.TryGetValue("yourVar", out string myVariable);

            if(string.IsNullOrWhiteSpace(myVariable))
            {
                ChatHelpers.SendChatLog(panel, $"You didn't provide yourVar variable!", ChatLogStatus.Warning);
            }

            ChatHelpers.SendChatLog(panel, $"Finished executing trigger method!", ChatLogStatus.Success);
        }
    }
}
