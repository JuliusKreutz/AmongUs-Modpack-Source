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

    [HarmonyPatch]
    internal class IntroPatch
    {
        public static void setupIntroTeam(IntroCutscene __instance,
            ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
        {
            // Intro solo teams
            if (PlayerControl.LocalPlayer == Jester.jester || PlayerControl.LocalPlayer == Jackal.jackal ||
                PlayerControl.LocalPlayer == Arsonist.arsonist)
            {
                var soloTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                soloTeam.Add(PlayerControl.LocalPlayer);
                yourTeam = soloTeam;
            }

            // Add the Spy to the Impostor team (for the Impostors)
            if (Spy.spy == null || !PlayerControl.LocalPlayer.Data.IsImpostor) return;
            var players = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
            var fakeImpostorTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
            foreach (var p in players.Where(p => p == Spy.spy || p.Data.IsImpostor))
            {
                fakeImpostorTeam.Add(p);
            }

            yourTeam = fakeImpostorTeam;
        }

        public static void setupIntroRole(IntroCutscene __instance,
            ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
        {
            var infos = RoleInfo.getRoleInfoForPlayer(PlayerControl.LocalPlayer);
            var roleInfo = infos.FirstOrDefault(info => info.roleId != RoleId.Lover);

            if (roleInfo != null)
            {
                __instance.Title.text = roleInfo.name;
                __instance.ImpostorText.gameObject.SetActive(true);
                __instance.ImpostorText.text = roleInfo.introDescription;
                if (roleInfo.roleId != RoleId.Crewmate && roleInfo.roleId != RoleId.Impostor)
                {
                    // For native Crewmate or Impostor do not modify the colors
                    __instance.Title.color = roleInfo.color;
                    __instance.BackgroundBar.material.color = roleInfo.color;
                }
            }

            if (infos.All(info => info.roleId != RoleId.Lover)) return;
            var loversText =
                UnityEngine.Object.Instantiate(__instance.ImpostorText, __instance.ImpostorText.transform.parent);
            loversText.transform.localPosition += Vector3.down * 3f;
            var otherLover = PlayerControl.LocalPlayer == Lovers.lover1 ? Lovers.lover2 : Lovers.lover1;
            loversText.text =
                Helpers.cs(Lovers.color, $"❤ You are in love with {otherLover?.Data?.PlayerName ?? ""} ❤");
        }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
        private class BeginCrewmatePatch
        {
            public static void Prefix(IntroCutscene __instance,
                ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
            {
                setupIntroTeam(__instance, ref yourTeam);
            }

            public static void Postfix(IntroCutscene __instance,
                ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
            {
                setupIntroRole(__instance, ref yourTeam);
            }
        }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
        private class BeginImpostorPatch
        {
            public static void Prefix(IntroCutscene __instance,
                ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
            {
                setupIntroTeam(__instance, ref yourTeam);
            }

            public static void Postfix(IntroCutscene __instance,
                ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
            {
                setupIntroRole(__instance, ref yourTeam);
            }
        }
    }
}