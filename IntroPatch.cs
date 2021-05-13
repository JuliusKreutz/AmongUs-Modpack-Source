using HarmonyLib;
using System;
using static Modpack.Modpack;
using UnityEngine;
using System.Linq;

namespace Modpack
{
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
    internal class IntroCutsceneOnDestroyPatch
    {
        public static void Prefix(IntroCutscene __instance)
        {
            // Arsonist generate player icons
            if (PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer != Arsonist.arsonist ||
                HudManager.Instance == null) return;
            var playerCounter = 0;
            var localPosition = HudManager.Instance.UseButton.transform.localPosition;
            var bottomLeft = new Vector3(-localPosition.x, localPosition.y, localPosition.z);
            bottomLeft += new Vector3(-0.25f, -0.25f, 0);
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player == PlayerControl.LocalPlayer) continue;
                var data = player.Data;
                var poolablePlayer =
                    UnityEngine.Object.Instantiate(__instance.PlayerPrefab, HudManager.Instance.transform);
                var transform = poolablePlayer.transform;
                transform.localPosition = bottomLeft + Vector3.right * playerCounter * 0.35f;
                transform.localScale = Vector3.one * 0.3f;
                PlayerControl.SetPlayerMaterialColors(data.ColorId, poolablePlayer.Body);
                DestroyableSingleton<HatManager>.Instance.SetSkin(poolablePlayer.SkinSlot, data.SkinId);
                poolablePlayer.HatSlot.SetHat(data.HatId, data.ColorId);
                PlayerControl.SetPetImage(data.PetId, data.ColorId, poolablePlayer.PetSlot);
                poolablePlayer.NameText.text = data.PlayerName;
                poolablePlayer.SetFlipX(true);
                poolablePlayer.setSemiTransparent(true);
                Arsonist.dousedIcons[player.PlayerId] = poolablePlayer;
                playerCounter++;
            }
        }
    }

    [HarmonyPatch(typeof(IntroCutscene._CoBegin_d__11), nameof(IntroCutscene._CoBegin_d__11.MoveNext))]
    internal class IntroPatch
    {
        private static void Prefix(IntroCutscene._CoBegin_d__11 __instance)
        {
            // Intro solo teams
            if (PlayerControl.LocalPlayer == Jester.jester || PlayerControl.LocalPlayer == Jackal.jackal ||
                PlayerControl.LocalPlayer == Arsonist.arsonist)
            {
                var soloTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                soloTeam.Add(PlayerControl.LocalPlayer);
                __instance.yourTeam = soloTeam;
            }

            // Add the Spy to the Impostor team (for the Impostors)
            if (Spy.spy == null || !PlayerControl.LocalPlayer.Data.IsImpostor) return;
            var players = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
            var fakeImpostorTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
            foreach (var p in players.Where(p => p == Spy.spy || p.Data.IsImpostor))
            {
                fakeImpostorTeam.Add(p);
            }

            __instance.yourTeam = fakeImpostorTeam;
        }

        // Intro display role
        private static void Postfix(IntroCutscene._CoBegin_d__11 __instance)
        {
            var infos = RoleInfo.getRoleInfoForPlayer(PlayerControl.LocalPlayer);
            if (infos.Count == 0) return;
            var roleInfo = infos[0];

            if (PlayerControl.LocalPlayer == Lovers.lover1 || PlayerControl.LocalPlayer == Lovers.lover2)
            {
                var otherLover = PlayerControl.LocalPlayer == Lovers.lover1 ? Lovers.lover2 : Lovers.lover1;
                __instance.__4__this.Title.text = PlayerControl.LocalPlayer.Data.IsImpostor
                    ? "<color=#FF1919FF>Imp</color><color=#FC03BEFF>Lover</color>"
                    : "<color=#FC03BEFF>Lover</color>";
                __instance.__4__this.Title.color =
                    PlayerControl.LocalPlayer.Data.IsImpostor ? Color.white : Lovers.color;
                __instance.__4__this.ImpostorText.text =
                    "You are in <color=#FC03BEFF>Love</color><color=#FFFFFFFF> with </color><color=#FC03BEFF>" +
                    (otherLover?.Data?.PlayerName ?? "") + "</color>";
                __instance.__4__this.ImpostorText.gameObject.SetActive(true);
                __instance.__4__this.BackgroundBar.material.color = Lovers.color;
            }
            else if (roleInfo.name == "Crewmate" || roleInfo.name == "Impostor")
            {
            }
            else
            {
                __instance.__4__this.Title.text = roleInfo.name;
                __instance.__4__this.Title.color = roleInfo.color;
                __instance.__4__this.ImpostorText.gameObject.SetActive(true);
                __instance.__4__this.ImpostorText.text = roleInfo.introDescription;
                __instance.__4__this.BackgroundBar.material.color = roleInfo.color;
            }
        }
    }
}