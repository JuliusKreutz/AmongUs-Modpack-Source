using HarmonyLib;
using Hazel;
using UnityEngine;

namespace Modpack
{
    [HarmonyPatch(typeof(OpenDoorConsole), nameof(OpenDoorConsole.Use))]
    internal class ToiletDoorFix
    {
        // Synchronize opening toilet doors among clients
        public static void Prefix(OpenDoorConsole __instance)
        {
            var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                (byte) CustomRPC.OpenToiletDoor, SendOption.None, -1);
            writer.Write(__instance.MyDoor.Id);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
    }

    [HarmonyPatch(typeof(NotificationPopper), nameof(NotificationPopper.Update))]
    public static class NotificationPopperUpdatePatch
    {
        // Fix position of notifications (e.g. player disconnected)
        public static void Postfix(NotificationPopper __instance)
        {
            if (!(__instance.alphaTimer > 0f)) return;
            var transform = __instance.transform;
            var localPosition = transform.localPosition;
            localPosition += new Vector3(0.5f, 0f, 0f);
            transform.localPosition = localPosition;
        }
    }
}