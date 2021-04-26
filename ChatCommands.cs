using HarmonyLib;

namespace Modpack
{
    [HarmonyPatch]
    public static class ChatCommands
    {
        [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
        private static class SendChatPatch
        {
            public static bool Prefix(ChatController __instance)
            {
                var text = __instance.TextArea.text;
                var handled = false;
                if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started)
                {
                    switch (text.ToLower())
                    {
                        case "i'm a panda":
                        {
                            handled = true;
                            var colorId = (byte) CustomColors.pickableColors;
                            SaveManager.BodyColor = colorId;
                            if (PlayerControl.LocalPlayer)
                                PlayerControl.LocalPlayer.CmdCheckColor(colorId);
                            break;
                        }
                        case "nightsky":
                        {
                            handled = true;
                            var colorId = (byte) (CustomColors.pickableColors + 1);
                            SaveManager.BodyColor = colorId;
                            if (PlayerControl.LocalPlayer)
                                PlayerControl.LocalPlayer.CmdCheckColor(colorId);
                            break;
                        }
                    }
                }

                if (!handled) return true;
                __instance.TextArea.Clear();
                __instance.quickChatMenu.ResetGlyphs();
                System.Console.WriteLine("Chat Clear");

                return false;
            }
        }
    }
}