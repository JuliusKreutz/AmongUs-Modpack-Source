using HarmonyLib;
using System.Linq;

namespace Modpack
{
    [HarmonyPatch]
    public static class ChatCommands
    {
        [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
        private static class SendChatPatch
        {
            private static bool Prefix(ChatController __instance)
            {
                var text = __instance.TextArea.text;
                var handled = false;
                if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started)
                {
                    switch (text)
                    {
                        case "1":
                        {
                            // i am a cheater
                            handled = true;
                            var colorId = (byte) CustomColors.pickableColors;
                            SaveManager.BodyColor = colorId;
                            if (PlayerControl.LocalPlayer)
                                PlayerControl.LocalPlayer.CmdCheckColor(colorId);
                            break;
                        }
                        case "2":
                        {
                            // i dont understand hashes
                            handled = true;
                            var colorId = (byte) (CustomColors.pickableColors + 1);
                            SaveManager.BodyColor = colorId;
                            if (PlayerControl.LocalPlayer)
                                PlayerControl.LocalPlayer.CmdCheckColor(colorId);
                            break;
                        }
                        case "3":
                        {
                            // Batch 2
                            handled = true;
                            var colorId = (byte) (CustomColors.pickableColors + 2);
                            SaveManager.BodyColor = colorId;
                            if (PlayerControl.LocalPlayer)
                                PlayerControl.LocalPlayer.CmdCheckColor(colorId);
                            break;
                        }
                        case "4":
                        {
                            handled = true;
                            var colorId = (byte) (CustomColors.pickableColors + 3);
                            SaveManager.BodyColor = colorId;
                            if (PlayerControl.LocalPlayer)
                                PlayerControl.LocalPlayer.CmdCheckColor(colorId);
                            break;
                        }
                        case "5":
                        {
                            handled = true;
                            var colorId = (byte) (CustomColors.pickableColors + 4);
                            SaveManager.BodyColor = colorId;
                            if (PlayerControl.LocalPlayer)
                                PlayerControl.LocalPlayer.CmdCheckColor(colorId);
                            break;
                        }
                        case "6":
                        {
                            // Eisbison Color
                            handled = true;
                            var colorId = (byte) (CustomColors.pickableColors + 5);
                            SaveManager.BodyColor = colorId;
                            if (PlayerControl.LocalPlayer)
                                PlayerControl.LocalPlayer.CmdCheckColor(colorId);
                            break;
                        }
                        default:
                        {
                            if (text.ToLower().StartsWith("/kick "))
                            {
                                var playerName = text[6..];
                                var target = PlayerControl.AllPlayerControls.ToArray().ToList()
                                    .FirstOrDefault(x => x.Data.PlayerName.Equals(playerName));
                                if (target != null && AmongUsClient.Instance != null && AmongUsClient.Instance.CanBan())
                                {
                                    var client = AmongUsClient.Instance.GetClient(target.OwnerId);
                                    if (client != null)
                                    {
                                        AmongUsClient.Instance.KickPlayer(client.Id, false);
                                        handled = true;
                                    }
                                }
                            }

                            break;
                        }
                    }
                }

                if (!handled) return !handled;
                __instance.TextArea.Clear();
                __instance.quickChatMenu.ResetGlyphs();

                return !handled;
            }
        }
    }
}