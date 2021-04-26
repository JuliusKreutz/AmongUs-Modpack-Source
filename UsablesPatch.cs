using HarmonyLib;
using System;
using UnityEngine;
using System.Linq;
using static Modpack.Modpack;
using static Modpack.GameHistory;
using static Modpack.MapOptions;
using System.Collections.Generic;


namespace Modpack
{
    [HarmonyPatch(typeof(Vent), "CanUse")]
    public static class VentCanUsePatch
    {
        public static bool Prefix(Vent __instance, ref float __result, [HarmonyArgument(0)] GameData.PlayerInfo pc,
            [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
        {
            var num = float.MaxValue;
            var @object = pc.Object;


            var roleCouldUse = false;

            if (Engineer.engineer != null && Engineer.engineer == @object)
                roleCouldUse = true;
            else if (Jackal.canUseVents && Jackal.jackal != null && Jackal.jackal == @object)
                roleCouldUse = true;
            else if (Sidekick.canUseVents && Sidekick.sidekick != null && Sidekick.sidekick == @object)
                roleCouldUse = true;
            else if (pc.IsImpostor)
            {
                if (Janitor.janitor == null || Janitor.janitor != PlayerControl.LocalPlayer)
                {
                    if (Morphling.morphling == null || Morphling.morphling != @object)
                    {
                        if (Mafioso.mafioso == null || Mafioso.mafioso != PlayerControl.LocalPlayer ||
                            Godfather.godfather == null || Godfather.godfather.Data.IsDead)
                            roleCouldUse = true;
                    }
                }
            }

            var usableDistance = __instance.UsableDistance;
            if (__instance.name.StartsWith("JackInTheBoxVent_"))
            {
                if (Trickster.trickster != PlayerControl.LocalPlayer)
                {
                    // Only the Trickster can use the Jack-In-The-Boxes!
                    canUse = false;
                    couldUse = false;
                    __result = num;
                    return false;
                }

                // Reduce the usable distance to reduce the risk of gettings stuck while trying to jump into the box if it's placed near objects
                usableDistance = 0.4f;
            }

            couldUse = ((@object.inVent || roleCouldUse) && !pc.IsDead && (@object.CanMove || @object.inVent));
            canUse = couldUse;
            if (canUse)
            {
                var truePosition = @object.GetTruePosition();
                var position = __instance.transform.position;
                num = Vector2.Distance(truePosition, position);

                canUse &= (num <= usableDistance &&
                           !PhysicsHelpers.AnythingBetween(truePosition, position, Constants.ShipOnlyMask, false));
            }

            __result = num;
            return false;
        }
    }

    [HarmonyPatch(typeof(Vent), "Use")]
    public static class VentUsePatch
    {
        public static bool Prefix(Vent __instance)
        {
            __instance.CanUse(PlayerControl.LocalPlayer.Data, out var flag, out _);

            if (!flag || !__instance.name.StartsWith("JackInTheBoxVent_")) return true; // Continue with default method

            var isEnter = !PlayerControl.LocalPlayer.inVent;

            __instance.SetButtons(isEnter);
            var writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId,
                (byte) CustomRPC.UseUncheckedVent, Hazel.SendOption.Reliable);
            writer.WritePacked(__instance.Id);
            writer.Write(PlayerControl.LocalPlayer.PlayerId);
            writer.Write(isEnter ? byte.MaxValue : (byte) 0);
            writer.EndMessage();
            RPCProcedure.useUncheckedVent(__instance.Id, PlayerControl.LocalPlayer.PlayerId,
                isEnter ? byte.MaxValue : (byte) 0);

            return false;
        }
    }

    [HarmonyPatch(typeof(UseButtonManager), nameof(UseButtonManager.SetTarget))]
    internal class UseButtonSetTargetPatch
    {
        private static void Postfix(UseButtonManager __instance)
        {
            // Trickster render special vent button
            if (__instance.currentTarget != null && Trickster.trickster != null &&
                Trickster.trickster == PlayerControl.LocalPlayer)
            {
                var possibleVent = __instance.currentTarget.TryCast<Vent>();
                if (possibleVent != null && possibleVent.gameObject != null &&
                    possibleVent.gameObject.name.StartsWith("JackInTheBoxVent_"))
                {
                    __instance.UseButton.sprite = Trickster.getTricksterVentButtonSprite();
                }
            }

            // Mafia sabotage button render patch
            var blockSabotageJanitor = (Janitor.janitor != null && Janitor.janitor == PlayerControl.LocalPlayer);
            var blockSabotageMafioso = (Mafioso.mafioso != null && Mafioso.mafioso == PlayerControl.LocalPlayer &&
                                        Godfather.godfather != null && !Godfather.godfather.Data.IsDead);
            if (__instance.currentTarget != null || (!blockSabotageJanitor && !blockSabotageMafioso)) return;
            __instance.UseButton.sprite =
                DestroyableSingleton<TranslationController>.Instance.GetImage(ImageNames.UseButton);
            __instance.UseButton.color = new Color(1f, 1f, 1f, 0.3f);
        }
    }

    [HarmonyPatch(typeof(UseButtonManager), nameof(UseButtonManager.DoClick))]
    internal class UseButtonDoClickPatch
    {
        private static bool Prefix(UseButtonManager __instance)
        {
            if (__instance.currentTarget != null) return true;

            // Mafia sabotage button click patch
            var blockSabotageJanitor = (Janitor.janitor != null && Janitor.janitor == PlayerControl.LocalPlayer);
            var blockSabotageMafioso = (Mafioso.mafioso != null && Mafioso.mafioso == PlayerControl.LocalPlayer &&
                                        Godfather.godfather != null && !Godfather.godfather.Data.IsDead);
            if (blockSabotageJanitor || blockSabotageMafioso) return false;

            return true;
        }
    }

    [HarmonyPatch(typeof(EmergencyMinigame), nameof(EmergencyMinigame.Update))]
    internal class EmergencyMinigameUpdatePatch
    {
        private static void Postfix(EmergencyMinigame __instance)
        {
            var roleCanCallEmergency = true;
            var statusText = "";

            // Deactivate emergency button for Swapper
            if (Swapper.swapper != null && Swapper.swapper == PlayerControl.LocalPlayer && !Swapper.canCallEmergency)
            {
                roleCanCallEmergency = false;
                statusText = "The Swapper can't start an emergency meeting";
            }

            // Potentially deactivate emergency button for Jester
            if (Jester.jester != null && Jester.jester == PlayerControl.LocalPlayer && !Jester.canCallEmergency)
            {
                roleCanCallEmergency = false;
                statusText = "The Jester can't start an emergency meeting";
            }

            if (!roleCanCallEmergency)
            {
                __instance.StatusText.text = statusText;
                __instance.NumberText.text = string.Empty;
                __instance.ClosedLid.gameObject.SetActive(true);
                __instance.OpenLid.gameObject.SetActive(false);
                __instance.ButtonActive = false;
                return;
            }

            // Handle max number of meetings
            if (__instance.state != 1) return;
            var localRemaining = PlayerControl.LocalPlayer.RemainingEmergencies;
            var teamRemaining = Mathf.Max(0, maxNumberOfMeetings - meetingsCount);
            var remaining = Mathf.Min(localRemaining,
                (Mayor.mayor != null && Mayor.mayor == PlayerControl.LocalPlayer) ? 1 : teamRemaining);
            __instance.NumberText.text = $"{localRemaining.ToString()} and the ship has {teamRemaining.ToString()}";
            __instance.ButtonActive = remaining > 0;
            __instance.ClosedLid.gameObject.SetActive(!__instance.ButtonActive);
            __instance.OpenLid.gameObject.SetActive(__instance.ButtonActive);
        }
    }


    [HarmonyPatch(typeof(Console), nameof(Console.CanUse))]
    public static class ConsoleCanUsePatch
    {
        public static bool Prefix(ref float __result, Console __instance, [HarmonyArgument(0)] GameData.PlayerInfo pc,
            [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
        {
            canUse = couldUse = false;
            if (Swapper.swapper != null && Swapper.swapper == PlayerControl.LocalPlayer)
                return !__instance.TaskTypes.Any(x => x == TaskTypes.FixLights || x == TaskTypes.FixComms);
            if (__instance.AllowImpostor) return true;
            if (!pc.Object.hasFakeTasks()) return true;
            __result = float.MaxValue;
            return false;
        }
    }


    [HarmonyPatch(typeof(TuneRadioMinigame), nameof(TuneRadioMinigame.Begin))]
    internal class CommsMinigameBeginPatch
    {
        private static void Postfix(TuneRadioMinigame __instance)
        {
            // Block Swapper from fixing comms. Still looking for a better way to do this, but deleting the task doesn't seem like a viable option since then the camera, admin table, ... work while comms are out
            if (Swapper.swapper != null && Swapper.swapper == PlayerControl.LocalPlayer)
            {
                __instance.Close();
            }
        }
    }

    [HarmonyPatch(typeof(SwitchMinigame), nameof(SwitchMinigame.Begin))]
    internal class LightsMinigameBeginPatch
    {
        private static void Postfix(SwitchMinigame __instance)
        {
            // Block Swapper from fixing lights. One could also just delete the PlayerTask, but I wanted to do it the same way as with coms for now.
            if (Swapper.swapper != null && Swapper.swapper == PlayerControl.LocalPlayer)
            {
                __instance.Close();
            }
        }
    }

    [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Begin))]
    internal class VitalsMinigameBeginPatch
    {
        private static void Postfix(VitalsMinigame __instance)
        {
            if (__instance.vitals.Length <= 10) return;
            for (var i = 0; i < __instance.vitals.Length; i++)
            {
                var vitalsPanel = __instance.vitals[i];
                var player = GameData.Instance.AllPlayers[i];
                vitalsPanel.Text.text = player.PlayerName.Length >= 4
                    ? player.PlayerName.Substring(0, 4).ToUpper()
                    : player.PlayerName.ToUpper();
            }
        }
    }


    [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Update))]
    internal class VitalsMinigameUpdatePatch
    {
        private static void Postfix(VitalsMinigame __instance)
        {
            // Hacker show time since death
            var showHackerInfo = Hacker.hacker != null && Hacker.hacker == PlayerControl.LocalPlayer &&
                                 Hacker.hackerTimer > 0;
            for (var k = 0; k < __instance.vitals.Length; k++)
            {
                var vitalsPanel = __instance.vitals[k];
                GameData.PlayerInfo player = GameData.Instance.AllPlayers[k];

                // Crowded scaling
                var scale = 10f / Mathf.Max(10, __instance.vitals.Length);
                var transform = vitalsPanel.transform;
                transform.localPosition = new Vector3(k * 0.6f * scale + -2.7f, 0.2f, -1f);
                transform.localScale = new Vector3(scale, scale, transform.localScale.z);

                // Hacker update
                if (!vitalsPanel.IsDead) continue;
                var deadPlayer = deadPlayers?.Where(x => x.player?.PlayerId == player?.PlayerId)
                    .FirstOrDefault();
                if (deadPlayer?.timeOfDeath == null) continue;
                var timeSinceDeath = ((float) (DateTime.UtcNow - deadPlayer.timeOfDeath).TotalMilliseconds);

                if (showHackerInfo)
                    vitalsPanel.Text.text = Math.Round(timeSinceDeath / 1000) + "s";
                else if (__instance.vitals.Length > 10)
                    vitalsPanel.Text.text = player.PlayerName.Length >= 4
                        ? player.PlayerName.Substring(0, 4).ToUpper()
                        : player.PlayerName.ToUpper();
                else
                    vitalsPanel.Text.text =
                        DestroyableSingleton<TranslationController>.Instance.GetString(
                            Palette.ShortColorNames[player.ColorId],
                            new UnhollowerBaseLib.Il2CppReferenceArray<Il2CppSystem.Object>(0));
            }
        }
    }

    [HarmonyPatch]
    internal class AdminPanelPatch
    {
        private static Dictionary<SystemTypes, List<Color>> players = new Dictionary<SystemTypes, List<Color>>();
        private static readonly int BodyColor = Shader.PropertyToID("_BodyColor");

        [HarmonyPatch(typeof(MapCountOverlay), nameof(MapCountOverlay.Update))]
        private class MapCountOverlayUpdatePatch
        {
            private static bool Prefix(MapCountOverlay __instance)
            {
                // Save colors for the Hacker
                __instance.timer += Time.deltaTime;
                if (__instance.timer < 0.1f)
                {
                    return false;
                }

                __instance.timer = 0f;
                players = new Dictionary<SystemTypes, List<Color>>();
                var commsActive = false;
                foreach (var task in PlayerControl.LocalPlayer.myTasks)
                    if (task.TaskType == TaskTypes.FixComms)
                        commsActive = true;


                switch (__instance.isSab)
                {
                    case false when commsActive:
                        __instance.isSab = true;
                        __instance.BackgroundColor.SetColor(Palette.DisabledGrey);
                        __instance.SabotageText.gameObject.SetActive(true);
                        return false;
                    case true when !commsActive:
                        __instance.isSab = false;
                        __instance.BackgroundColor.SetColor(Color.green);
                        __instance.SabotageText.gameObject.SetActive(false);
                        break;
                }

                foreach (var counterArea in __instance.CountAreas)
                {
                    var roomColors = new List<Color>();
                    players.Add(counterArea.RoomType, roomColors);

                    if (!commsActive)
                    {
                        var plainShipRoom = ShipStatus.Instance.FastRooms[counterArea.RoomType];

                        if (plainShipRoom != null && plainShipRoom.roomArea)
                        {
                            var num = plainShipRoom.roomArea.OverlapCollider(__instance.filter, __instance.buffer);
                            var num2 = num;
                            for (var j = 0; j < num; j++)
                            {
                                var collider2D = __instance.buffer[j];
                                if (collider2D.tag != "DeadBody")
                                {
                                    var component = collider2D.GetComponent<PlayerControl>();
                                    if (!component || component.Data == null || component.Data.Disconnected ||
                                        component.Data.IsDead)
                                    {
                                        num2--;
                                    }
                                    else if (component.myRend?.material != null)
                                    {
                                        var color = component.myRend.material.GetColor(BodyColor);
                                        if (Hacker.onlyColorType)
                                        {
                                            var id = Mathf.Max(0, Palette.PlayerColors.IndexOf(color));
                                            color = Helpers.isLighterColor((byte) id)
                                                ? Palette.PlayerColors[7]
                                                : Palette.PlayerColors[6];
                                        }

                                        roomColors.Add(color);
                                    }
                                }
                                else
                                {
                                    var component = collider2D.GetComponent<DeadBody>();
                                    if (!component) continue;
                                    var playerInfo =
                                        GameData.Instance.GetPlayerById(component.ParentId);
                                    if (playerInfo == null) continue;
                                    var color = Palette.PlayerColors[playerInfo.ColorId];
                                    if (Hacker.onlyColorType)
                                        color = Helpers.isLighterColor(playerInfo.ColorId)
                                            ? Palette.PlayerColors[7]
                                            : Palette.PlayerColors[6];
                                    roomColors.Add(color);
                                }
                            }

                            counterArea.UpdateCount(num2);
                        }
                        else
                        {
                            Debug.LogWarning("Couldn't find counter for:" + counterArea.RoomType);
                        }
                    }
                    else
                    {
                        counterArea.UpdateCount(0);
                    }
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(CounterArea), nameof(CounterArea.UpdateCount))]
        private class CounterAreaUpdateCountPatch
        {
            private static Sprite defaultIcon;

            private static void Postfix(CounterArea __instance)
            {
                // Hacker display saved colors on the admin panel
                var showHackerInfo = Hacker.hacker != null && Hacker.hacker == PlayerControl.LocalPlayer &&
                                     Hacker.hackerTimer > 0;
                if (!players.ContainsKey(__instance.RoomType)) return;
                var colors = players[__instance.RoomType];

                for (var i = 0; i < __instance.myIcons.Count; i++)
                {
                    PoolableBehavior icon = __instance.myIcons[i];
                    var renderer = icon.GetComponent<SpriteRenderer>();

                    if (renderer == null) continue;
                    if (defaultIcon == null) defaultIcon = renderer.sprite;
                    if (showHackerInfo && colors.Count > i && Hacker.getAdminTableIconSprite() != null)
                    {
                        renderer.sprite = Hacker.getAdminTableIconSprite();
                        renderer.color = colors[i];
                    }
                    else
                    {
                        renderer.sprite = defaultIcon;
                        renderer.color = Color.white;
                    }
                }
            }
        }
    }
}