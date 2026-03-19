using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutwardModsCommunicatorMenu.Utility.Enums
{
    public enum ChatCommandsManagerParams
    {
        CommandName,
        CommandParameters,
        CommandAction,
        IsCheatCommand,
        CommandDescription,
        CommandRequiresDebugMode,
    }

    public static class ChatCommandsManagerParamsHelper
    {
        private static readonly Dictionary<ChatCommandsManagerParams, (string key, Type type)> _registry
            = new()
            {
                [ChatCommandsManagerParams.CommandName] = ("command", typeof(string)),
                [ChatCommandsManagerParams.CommandParameters] = ("parameters", typeof(Dictionary<string, (string, string)>)),
                [ChatCommandsManagerParams.CommandAction] = ("function", typeof(Action<Character, Dictionary<string, string>>)),
                [ChatCommandsManagerParams.IsCheatCommand] = ("isCheatCommand", typeof(bool)),
                [ChatCommandsManagerParams.CommandDescription] = ("description", typeof(string)),
                [ChatCommandsManagerParams.CommandRequiresDebugMode] = ("debugMode", typeof(bool)),
            };

        public static (string key, Type type) Get(ChatCommandsManagerParams param) => _registry[param];
    }
}
