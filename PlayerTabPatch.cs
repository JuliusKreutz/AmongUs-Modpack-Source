using HarmonyLib;
using SaveManager = BLCGIFOPMIA;

namespace Modpack
{
    public class PlayerTabPatch
    {

        [HarmonyPatch(typeof(PlayerTab), nameof(PlayerTab.OnEnable))]
        public static class OnEnablePatch
        {
            public static void Postfix(PlayerTab __instance)
            {
                for (int i = 0; i < __instance.ColorChips.Count; i++)
                {
                    var chip = __instance.ColorChips.ToArray()[i];
                    chip.transform.localScale *= 0.65f;
                }
            }
        }

    }
}