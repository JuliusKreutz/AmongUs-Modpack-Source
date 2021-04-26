using HarmonyLib;
using System;
using static Modpack.Modpack;
using UnityEngine;
using System.Linq;

namespace Modpack
{
    [HarmonyPatch(typeof(IntroCutscene.Nested_0), nameof(IntroCutscene.Nested_0.MoveNext))]
    internal class IntroPatch
    {
        // Intro special teams
        private static void Prefix(IntroCutscene.Nested_0 __instance)
        {
            if (PlayerControl.LocalPlayer == Jester.jester)
            {
                var jesterTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                jesterTeam.Add(PlayerControl.LocalPlayer);
                __instance.yourTeam = jesterTeam;
            }
            else if (PlayerControl.LocalPlayer == Jackal.jackal)
            {
                var jackalTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                jackalTeam.Add(PlayerControl.LocalPlayer);
                __instance.yourTeam = jackalTeam;
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
        private static void Postfix(IntroCutscene.Nested_0 __instance)
        {
            var infos = RoleInfo.getRoleInfoForPlayer(PlayerControl.LocalPlayer);
            if (infos.Count == 0) return;
            var roleInfo = infos[0];

            if (PlayerControl.LocalPlayer == Lovers.lover1 || PlayerControl.LocalPlayer == Lovers.lover2)
            {
                var otherLover = PlayerControl.LocalPlayer == Lovers.lover1 ? Lovers.lover2 : Lovers.lover1;
                __instance.__this.Title.text = PlayerControl.LocalPlayer.Data.IsImpostor
                    ? "<color=#FF1919FF>Imp</color><color=#FC03BEFF>Lover</color>"
                    : "<color=#FC03BEFF>Lover</color>";
                __instance.__this.Title.color = PlayerControl.LocalPlayer.Data.IsImpostor ? Color.white : Lovers.color;
                __instance.__this.ImpostorText.text =
                    "You are in <color=#FC03BEFF>Love</color><color=#FFFFFFFF> with </color><color=#FC03BEFF>" +
                    (otherLover?.Data?.PlayerName ?? "") + "</color>";
                __instance.__this.ImpostorText.gameObject.SetActive(true);
                __instance.__this.BackgroundBar.material.color = Lovers.color;
            }
            else if (roleInfo.name == "Crewmate" || roleInfo.name == "Impostor")
            {
            }
            else
            {
                __instance.__this.Title.text = roleInfo.name;
                __instance.__this.Title.color = roleInfo.color;
                __instance.__this.ImpostorText.gameObject.SetActive(true);
                __instance.__this.ImpostorText.text = roleInfo.introDescription;
                __instance.__this.BackgroundBar.material.color = roleInfo.color;
            }
        }
    }
}