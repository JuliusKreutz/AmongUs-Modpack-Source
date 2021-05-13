using HarmonyLib;

namespace Modpack
{
    [HarmonyPatch]
    public static class CredentialsPatch
    {
        [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
        private static class VersionShowerPatch
        {
            private static void Postfix(VersionShower __instance)
            {
                var spacer = new string('\n', 8);

                if (__instance.text.text.Contains(spacer))
                    __instance.text.text = __instance.text.text + "\n<color=#FCCE03FF>Mods loaded!</color>";
                else
                    __instance.text.text = __instance.text.text + spacer + "<color=#FCCE03FF>Mods loaded!</color>";
                __instance.text.alignment = TMPro.TextAlignmentOptions.TopLeft;
            }
        }
    }
}