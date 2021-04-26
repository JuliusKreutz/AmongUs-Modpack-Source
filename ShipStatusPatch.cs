using HarmonyLib;
using static Modpack.Modpack;
using UnityEngine;

namespace Modpack
{
    [HarmonyPatch(typeof(ShipStatus))]
    public class ShipStatusPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
        public static bool Prefix(ref float __result, ShipStatus __instance,
            [HarmonyArgument(0)] GameData.PlayerInfo player)
        {
            var systemType = __instance.Systems.ContainsKey(SystemTypes.Electrical)
                ? __instance.Systems[SystemTypes.Electrical]
                : null;
            var switchSystem = systemType?.TryCast<SwitchSystem>();
            if (switchSystem == null) return true;

            var num = switchSystem.Value / 255f;

            if (player == null || player.IsDead) // IsDead
                __result = __instance.MaxLightRadius;
            else if (player.IsImpostor) // IsImpostor
                __result = __instance.MaxLightRadius * PlayerControl.GameOptions.ImpostorLightMod;
            else if (Lighter.lighter != null && Lighter.lighter.PlayerId == player.PlayerId &&
                     Lighter.lighterTimer > 0f) // if player is Lighter and Lighter has his ability active
                __result = Mathf.Lerp(__instance.MaxLightRadius * Lighter.lighterModeLightsOffVision,
                    __instance.MaxLightRadius * Lighter.lighterModeLightsOnVision, num);
            else if (Trickster.trickster != null && Trickster.lightsOutTimer > 0f)
            {
                var lerpValue = 1f;
                if (Trickster.lightsOutDuration - Trickster.lightsOutTimer < 0.5f)
                    lerpValue = Mathf.Clamp01((Trickster.lightsOutDuration - Trickster.lightsOutTimer) * 2);
                else if (Trickster.lightsOutTimer < 0.5) lerpValue = Mathf.Clamp01(Trickster.lightsOutTimer * 2);
                __result = Mathf.Lerp(__instance.MinLightRadius, __instance.MaxLightRadius, 1 - lerpValue) *
                           PlayerControl.GameOptions.CrewLightMod; // Instant lights out? Maybe add a smooth transition?
            }
            else
                __result = Mathf.Lerp(__instance.MinLightRadius, __instance.MaxLightRadius, num) *
                           PlayerControl.GameOptions.CrewLightMod;

            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.IsGameOverDueToDeath))]
        public static void Postfix2(ShipStatus __instance, ref bool __result)
        {
            __result = false;
        }
    }
}