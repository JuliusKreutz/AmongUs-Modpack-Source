using System.Security.Cryptography;
using System.Text;
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
                    using var md5 = MD5.Create();
                    var hash = System.BitConverter
                        .ToString(md5.ComputeHash(Encoding.UTF8.GetBytes("tor@" + text.ToLower() + "§eof")))
                        .Replace("-", "").ToLowerInvariant();
                    switch (hash)
                    {
                        case "a4eb05314008537d2832e32fa1f33b2e":
                        {
                            // i am a cheater
                            handled = true;
                            var colorId = (byte) CustomColors.pickableColors;
                            SaveManager.BodyColor = colorId;
                            if (PlayerControl.LocalPlayer)
                                PlayerControl.LocalPlayer.CmdCheckColor(colorId);
                            break;
                        }
                        case "80cc70dc5f21bc321b84ce984abd511b":
                        {
                            // i dont understand hashes
                            handled = true;
                            var colorId = (byte) (CustomColors.pickableColors + 1);
                            SaveManager.BodyColor = colorId;
                            if (PlayerControl.LocalPlayer)
                                PlayerControl.LocalPlayer.CmdCheckColor(colorId);
                            break;
                        }
                        case "3359ffcd0b14ffa39d476a5c96632032":
                        {
                            // Batch 2
                            handled = true;
                            var colorId = (byte) (CustomColors.pickableColors + 2);
                            SaveManager.BodyColor = colorId;
                            if (PlayerControl.LocalPlayer)
                                PlayerControl.LocalPlayer.CmdCheckColor(colorId);
                            break;
                        }
                        case "14056e0b9e53bc91f0c6a8b1fd5ce8b5":
                        {
                            handled = true;
                            var colorId = (byte) (CustomColors.pickableColors + 3);
                            SaveManager.BodyColor = colorId;
                            if (PlayerControl.LocalPlayer)
                                PlayerControl.LocalPlayer.CmdCheckColor(colorId);
                            break;
                        }
                        case "fb00fb81b0be5177af908576e144d788":
                        {
                            handled = true;
                            var colorId = (byte) (CustomColors.pickableColors + 4);
                            SaveManager.BodyColor = colorId;
                            if (PlayerControl.LocalPlayer)
                                PlayerControl.LocalPlayer.CmdCheckColor(colorId);
                            break;
                        }
                        case "a79e2bd7c9cdc723924bd4d7734ae5da":
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