using OutwardModsCommunicatorMenu.Utility.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutwardModsCommunicatorMenu.Utility.Helpers
{
    public static class ChatHelpers
    {
        public static void SendChatLog(ChatPanel panel, string message, ChatLogStatus status = ChatLogStatus.Info)
        {
            panel.ChatMessageReceived("System", ChatLogStatusHelper.GetChatLogText(message, status));
        }

        public static void SendChatLog(Character character, string message, ChatLogStatus status = ChatLogStatus.Info)
        {
            if(character.CharacterUI?.ChatPanel == null)
            {
                OMCM.LogMessage("ChatHelpers@SendChatLog provided character with missing chatPanel!");
                return;
            }

            SendChatLog(character.CharacterUI.ChatPanel, message, status);
        }

        public static void SendChatOrLog(Character character, string message, ChatLogStatus status = ChatLogStatus.Info)
        {
            if(character.CharacterUI?.ChatPanel == null)
            {
                OMCM.LogStatusMessage(message, status);
                return;
            }

            SendChatLog(character.CharacterUI.ChatPanel, message, status);
        }
    }
}
