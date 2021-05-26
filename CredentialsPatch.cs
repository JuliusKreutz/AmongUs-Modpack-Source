using HarmonyLib;
using UnityEngine;

namespace Modpack
{
    [HarmonyPatch]
    public static class CredentialsPatch
    {
        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
        private static class LogoPatch
        {
            private static void Postfix(PingTracker __instance)
            {
                var amongUsLogo = GameObject.Find("bannerLogo_AmongUs");
                if (amongUsLogo != null)
                {
                    amongUsLogo.transform.localScale *= 0.6f;
                    amongUsLogo.transform.position += Vector3.up * 0.25f;
                }

                var torLogo = new GameObject("bannerLogo_TOR");
                torLogo.transform.position = Vector3.up;
                var renderer = torLogo.AddComponent<SpriteRenderer>();
                renderer.sprite = Helpers.loadSpriteFromResources("Modpack.Resources.Banner.png", 70f);
            }
        }
    }
}