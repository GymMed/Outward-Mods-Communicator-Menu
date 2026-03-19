using SideLoader;

namespace OutwardModsCommunicatorMenu.Utility
{
    public static class KeybindingHelper
    {
        private const string CATEGORY = "Mods Communicator Menu";

        public static void Register(string actionName, string displayName)
        {
            CustomKeybindings.AddAction(
                actionName,
                KeybindingsCategory.CustomKeybindings,
                ControlType.Both,
                InputType.Button
            );
        }

        public static bool IsKeyDown(string actionName)
        {
            return CustomKeybindings.GetKeyDown(actionName);
        }

        public static bool IsKeyHeld(string actionName)
        {
            return CustomKeybindings.GetKey(actionName);
        }
    }
}
