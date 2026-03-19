using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutwardModsCommunicatorMenu.Utility.Enums
{
    public enum ChatLogStatus
    {
        Info,
        Success,
        Warning, 
        Error
    }

    public static class ChatLogStatusHelper
    {
        public static string GetChatLogText(string message, ChatLogStatus status = ChatLogStatus.Info)
        {
            switch(status)
            {
                case ChatLogStatus.Success:
                    {
                        return Global.SetTextColor(message, Global.LIGHT_GREEN);
                    }
                case ChatLogStatus.Warning:
                    {
                        return Global.SetTextColor(message, Global.LIGHT_ORANGE);
                    }
                case ChatLogStatus.Error:
                    {
                        return $"<color=#{Global.LIGHT_RED.ToHex()}>{message}</color>";
                    }
                case ChatLogStatus.Info:
                default:
                    {
                        return message;
                    }
            }
        }
    }
}
