using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using UnhollowerBaseLib;
using static Modpack.Modpack;
using static Modpack.MapOptions;
using System;
using UnityEngine;

namespace Modpack
{
    [HarmonyPatch]
    internal class MeetingHudPatch
    {
        private static bool[] selections;
        private static SpriteRenderer[] renderers;
        private static GameData.PlayerInfo target;
        private const float scale = 0.65f;

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting))]
        private class MeetingCalculateVotesPatch
        {
            private static byte[] calculateVotes(MeetingHud __instance)
            {
                var array = new byte[16];
                foreach (var playerVoteArea in __instance.playerStates)
                {
                    if (!playerVoteArea.didVote) continue;
                    var num = playerVoteArea.votedFor + 1;
                    if (num < 0 || num >= array.Length) continue;
                    // Mayor count vote twice
                    if (Mayor.mayor != null && playerVoteArea.TargetPlayerId == (sbyte) Mayor.mayor.PlayerId)
                        array[num] += 2;
                    else
                        array[num] += 1;
                }

                // Swapper swap votes
                PlayerVoteArea swapped1 = null;
                PlayerVoteArea swapped2 = null;

                foreach (var playerVoteArea in __instance.playerStates)
                {
                    if (playerVoteArea.TargetPlayerId == Swapper.playerId1) swapped1 = playerVoteArea;
                    if (playerVoteArea.TargetPlayerId == Swapper.playerId2) swapped2 = playerVoteArea;
                }

                if (swapped1 == null || swapped2 == null || swapped1.TargetPlayerId + 1 < 0 ||
                    swapped1.TargetPlayerId + 1 >= array.Length || swapped2.TargetPlayerId + 1 < 0 ||
                    swapped2.TargetPlayerId + 1 >= array.Length) return array;
                var tmp = array[swapped1.TargetPlayerId + 1];
                array[swapped1.TargetPlayerId + 1] = array[swapped2.TargetPlayerId + 1];
                array[swapped2.TargetPlayerId + 1] = tmp;
                return array;
            }

            private static int IndexOfMax(byte[] self, Func<byte, int> comparer, out bool tie)
            {
                tie = false;
                var num = int.MinValue;
                var result = -1;
                for (var i = 0; i < self.Length; i++)
                {
                    var num2 = comparer(self[i]);
                    if (num2 > num)
                    {
                        result = i;
                        num = num2;
                        tie = false;
                    }
                    else if (num2 == num)
                    {
                        tie = true;
                        result = -1;
                    }
                }

                return result;
            }

            private static bool Prefix(MeetingHud __instance)
            {
                if (!__instance.playerStates.All(ps => ps.isDead || ps.didVote)) return false;
                // If skipping is disabled, replace skipps/no-votes with self vote
                if (target == null && blockSkippingInEmergencyMeetings && noVoteIsSelfVote)
                {
                    foreach (var playerVoteArea in __instance.playerStates)
                    {
                        if (playerVoteArea.votedFor < 0)
                            playerVoteArea.votedFor = playerVoteArea.TargetPlayerId; // TargetPlayerId
                    }
                }

                var self = calculateVotes(__instance);
                var maxIdx = IndexOfMax(self, p => p, out var tie) - 1;
                GameData.PlayerInfo exiled = null;
                foreach (var pi in GameData.Instance.AllPlayers)
                {
                    if (pi.PlayerId != maxIdx) continue;
                    exiled = pi;
                    break;
                }

                var array = new byte[15];
                foreach (var playerVoteArea in __instance.playerStates)
                {
                    array[playerVoteArea.TargetPlayerId] = playerVoteArea.GetState();
                }

                // RPCVotingComplete
                if (AmongUsClient.Instance.AmClient)
                    __instance.VotingComplete(array, exiled, tie);
                var messageWriter = AmongUsClient.Instance.StartRpc(__instance.NetId, 23, SendOption.Reliable);
                messageWriter.WriteBytesAndSize(array);
                messageWriter.Write(exiled?.PlayerId ?? byte.MaxValue);
                messageWriter.Write(tie);
                messageWriter.EndMessage();
                return false;
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.PopulateResults))]
        private class MeetingPopulateVotesPatch
        {
            private static bool Prefix(MeetingHud __instance, [HarmonyArgument(0)] Il2CppStructArray<byte> states)
            {
                // Swapper swap votes
                PlayerVoteArea swapped1 = null;
                PlayerVoteArea swapped2 = null;

                foreach (var playerVoteArea in __instance.playerStates)
                {
                    if (playerVoteArea.TargetPlayerId == Swapper.playerId1) swapped1 = playerVoteArea;
                    if (playerVoteArea.TargetPlayerId == Swapper.playerId2) swapped2 = playerVoteArea;
                }

                var doSwap = swapped1 != null && swapped2 != null;
                var delay = 0f;
                if (doSwap)
                {
                    delay = 2f;
                    var transform = swapped1.transform;
                    __instance.StartCoroutine(Effects.Slide3D(transform, transform.localPosition,
                        swapped2.transform.localPosition, 2f));
                    var transform1 = swapped2.transform;
                    __instance.StartCoroutine(Effects.Slide3D(transform1, transform1.localPosition,
                        swapped1.transform.localPosition, 2f));
                }

                var votesXOffset = 0f;
                var votesFinalSize = 1f;
                if (__instance.playerStates.Length > 10)
                {
                    votesXOffset = 0.1f;
                    votesFinalSize = scale;
                }

                // Mayor display vote twice
                __instance.TitleText.text =
                    DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MeetingVotingResults,
                        new Il2CppReferenceArray<Il2CppSystem.Object>(0));
                var num = 0;
                for (var i = 0; i < __instance.playerStates.Length; i++)
                {
                    var playerVoteArea = __instance.playerStates[i];
                    playerVoteArea.ClearForResults();
                    var num2 = 0;
                    var mayorFirstVoteDisplayed = false;

                    for (var j = 0; j < __instance.playerStates.Length; j++)
                    {
                        var playerVoteArea2 = __instance.playerStates[j];
                        var self = states[playerVoteArea2.TargetPlayerId];

                        if ((self & 128) > 0) continue;
                        var playerById = GameData.Instance.GetPlayerById((byte) playerVoteArea2.TargetPlayerId);
                        var votedFor = (int) PlayerVoteArea.GetVotedFor(self);
                        if (votedFor == playerVoteArea.TargetPlayerId)
                        {
                            var spriteRenderer = UnityEngine.Object.Instantiate(__instance.PlayerVotePrefab,
                                playerVoteArea.transform, true);
                            if (!PlayerControl.GameOptions.AnonymousVotes ||
                                PlayerControl.LocalPlayer.Data.IsDead && ghostsSeeVotes)
                                PlayerControl.SetPlayerMaterialColors(playerById.ColorId, spriteRenderer);
                            else
                                PlayerControl.SetPlayerMaterialColors(Palette.DisabledGrey, spriteRenderer);

                            var transform = spriteRenderer.transform;
                            transform.localPosition = __instance.CounterOrigin +
                                                      new Vector3(votesXOffset + __instance.CounterOffsets.x * num2, 0f,
                                                          0f);
                            transform.localScale = Vector3.zero;
                            Transform transform1;
                            (transform1 = spriteRenderer.transform).SetParent(playerVoteArea.transform
                                .parent); // Reparent votes so they don't move with their playerVoteArea
                            __instance.StartCoroutine(Effects.Bloop(num2 * 0.5f + delay, transform1, votesFinalSize,
                                0.5f));
                            num2++;
                        }
                        else if (i == 0 && votedFor == -1)
                        {
                            var spriteRenderer2 = UnityEngine.Object.Instantiate(__instance.PlayerVotePrefab,
                                __instance.SkippedVoting.transform, true);
                            if (!PlayerControl.GameOptions.AnonymousVotes ||
                                PlayerControl.LocalPlayer.Data.IsDead && ghostsSeeVotes)
                                PlayerControl.SetPlayerMaterialColors(playerById.ColorId, spriteRenderer2);
                            else
                                PlayerControl.SetPlayerMaterialColors(Palette.DisabledGrey, spriteRenderer2);
                            var transform = spriteRenderer2.transform;
                            transform.localPosition = __instance.CounterOrigin +
                                                      new Vector3(votesXOffset + __instance.CounterOffsets.x * num, 0f,
                                                          0f);
                            transform.localScale = Vector3.zero;
                            Transform transform1;
                            (transform1 = spriteRenderer2.transform).SetParent(playerVoteArea.transform
                                .parent); // Reparent votes so they don't move with their playerVoteArea
                            __instance.StartCoroutine(Effects.Bloop(num * 0.5f + delay, transform1, votesFinalSize,
                                0.5f));
                            num++;
                        }

                        // Major vote, redo this iteration to place a second vote
                        if (Mayor.mayor == null || playerVoteArea2.TargetPlayerId != (sbyte) Mayor.mayor.PlayerId ||
                            mayorFirstVoteDisplayed) continue;
                        mayorFirstVoteDisplayed = true;
                        j--;
                    }
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
        private class MeetingHudVotingCompletedPatch
        {
            private static void Postfix(MeetingHud __instance, [HarmonyArgument(0)] byte[] states,
                [HarmonyArgument(1)] GameData.PlayerInfo exiled, [HarmonyArgument(2)] bool tie)
            {
                // Reset swapper values
                Swapper.playerId1 = byte.MaxValue;
                Swapper.playerId2 = byte.MaxValue;

                // Lovers save next to be exiled, because RPC of ending game comes before RPC of exiled
                Lovers.notAckedExiledIsLover = false;
                if (exiled != null)
                    Lovers.notAckedExiledIsLover = Lovers.lover1 != null && Lovers.lover1.PlayerId == exiled.PlayerId ||
                                                   Lovers.lover2 != null && Lovers.lover2.PlayerId == exiled.PlayerId;
            }
        }


        private static void onClick(int i, MeetingHud __instance)
        {
            if (__instance.state == MeetingHud.VoteStates.Results) return;
            if (__instance.playerStates[i].isDead) return;

            var selectedCount = selections.Count(b => b);
            var renderer = renderers[i];

            switch (selectedCount)
            {
                case 0:
                    renderer.color = Color.green;
                    selections[i] = true;
                    break;
                case 1 when selections[i]:
                    renderer.color = Color.red;
                    selections[i] = false;
                    break;
                case 1:
                {
                    selections[i] = true;
                    renderer.color = Color.green;

                    PlayerVoteArea firstPlayer = null;
                    PlayerVoteArea secondPlayer = null;
                    for (var A = 0; A < selections.Length; A++)
                    {
                        if (!selections[A]) continue;
                        if (firstPlayer != null)
                        {
                            secondPlayer = __instance.playerStates[A];
                            break;
                        }

                        firstPlayer = __instance.playerStates[A];
                    }

                    if (firstPlayer != null && secondPlayer != null)
                    {
                        var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                            (byte) CustomRPC.SwapperSwap, SendOption.Reliable, -1);
                        writer.Write((byte) firstPlayer.TargetPlayerId);
                        writer.Write((byte) secondPlayer.TargetPlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);

                        RPCProcedure.swapperSwap((byte) firstPlayer.TargetPlayerId, (byte) secondPlayer.TargetPlayerId);
                    }

                    break;
                }
            }
        }

        private static void populateButtonsPostfix(MeetingHud __instance)
        {
            // Change buttons if there are more than 10 players
            if (__instance.playerStates != null && __instance.playerStates.Length > 10)
            {
                var playerStates = __instance.playerStates.OrderBy(p => p.isDead ? 50 : 0)
                    .ThenBy(p => p.TargetPlayerId)
                    .ToArray();
                for (var i = 0; i < playerStates.Length; i++)
                {
                    var area = playerStates[i];

                    int row = i / 3, col = i % 3;

                    // Update scalings
                    area.Overlay.transform.localScale =
                        area.PlayerButton.transform.localScale = new Vector3(1, 1 / scale, 1);
                    area.NameText.transform.localScale = new Vector3(1 / scale, 1 / scale, 1);
                    area.gameObject.transform.localScale = new Vector3(scale, scale, 1);
                    var megaphoneWrapper = new GameObject();
                    megaphoneWrapper.transform.SetParent(area.transform);
                    megaphoneWrapper.transform.localScale = Vector3.one * 1 / scale;
                    Transform transform;
                    (transform = area.Megaphone.transform).SetParent(megaphoneWrapper.transform);

                    // Update positions
                    transform.localPosition += Vector3.left * 0.1f;
                    area.NameText.transform.localPosition += new Vector3(0.25f, 0.043f, 0f);
                    area.transform.localPosition =
                        new Vector3(-3.63f + 2.43f * col, 1.5f - 0.76f * row, -0.9f - 0.01f * row);
                }
            }

            // Add Swapper Buttons
            if (Swapper.swapper == null || PlayerControl.LocalPlayer != Swapper.swapper ||
                Swapper.swapper.Data.IsDead) return;
            if (__instance.playerStates == null) return;
            {
                selections = new bool[__instance.playerStates.Length];
                renderers = new SpriteRenderer[__instance.playerStates.Length];

                for (var i = 0; i < __instance.playerStates.Length; i++)
                {
                    var playerVoteArea = __instance.playerStates[i];
                    if (playerVoteArea.isDead || playerVoteArea.TargetPlayerId == Swapper.swapper.PlayerId &&
                        Swapper.canOnlySwapOthers) continue;

                    var template = playerVoteArea.Buttons.transform.Find("CancelButton").gameObject;
                    var checkbox = UnityEngine.Object.Instantiate(template, playerVoteArea.transform, true);
                    checkbox.transform.position = template.transform.position;
                    checkbox.transform.localPosition = new Vector3(0f, 0.03f, template.transform.localPosition.z);
                    var renderer = checkbox.GetComponent<SpriteRenderer>();
                    renderer.sprite = Swapper.getCheckSprite();
                    renderer.color = Color.red;

                    var button = checkbox.GetComponent<PassiveButton>();
                    button.OnClick.RemoveAllListeners();
                    var copiedIndex = i;
                    button.OnClick.AddListener(
                        (UnityEngine.Events.UnityAction) (() => onClick(copiedIndex, __instance)));


                    selections[i] = false;
                    renderers[i] = renderer;
                }
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.ServerStart))]
        private class MeetingServerStartPatch
        {
            private static void Postfix(MeetingHud __instance)
            {
                populateButtonsPostfix(__instance);
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Deserialize))]
        private class MeetingDeserializePatch
        {
            private static void Postfix(MeetingHud __instance, [HarmonyArgument(0)] MessageReader reader,
                [HarmonyArgument(1)] bool initialState)
            {
                // Add swapper buttons
                if (initialState)
                {
                    populateButtonsPostfix(__instance);
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CoStartMeeting))]
        private class StartMeetingPatch
        {
            public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)] GameData.PlayerInfo meetingTarget)
            {
                // Reset vampire bitten
                Vampire.bitten = null;
                // Count meetings
                if (meetingTarget == null) meetingsCount++;
                // Save the meeting target
                target = meetingTarget;
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
        private class MeetingHudUpdatePatch
        {
            private static void Postfix(MeetingHud __instance)
            {
                // Deactivate skip Button if skipping on emergency meetings is disabled
                if (target == null && blockSkippingInEmergencyMeetings)
                    __instance.SkipVoteButton.gameObject.SetActive(false);
            }
        }
    }

    [HarmonyPatch(typeof(ExileController), "Begin")]
    internal class ExileBeginPatch
    {
        public static void Prefix(ExileController __instance, [HarmonyArgument(0)] ref GameData.PlayerInfo exiled,
            [HarmonyArgument(1)] bool tie)
        {
            // Shifter shift
            if (Shifter.shifter != null && AmongUsClient.Instance.AmHost && Shifter.futureShift != null)
            {
                // We need to send the RPC from the host here, to make sure that the order of shifting and erasing is correct (for that reason the futureShifted and futureErased are being synced)
                var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                    (byte) CustomRPC.ShifterShift, SendOption.Reliable, -1);
                writer.Write(Shifter.futureShift.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.shifterShift(Shifter.futureShift.PlayerId);
            }

            Shifter.futureShift = null;

            // Eraser erase
            if (Eraser.eraser != null && AmongUsClient.Instance.AmHost && Eraser.futureErased != null)
            {
                // We need to send the RPC from the host here, to make sure that the order of shifting and erasing is correct (for that reason the futureShifted and futureErased are being synced)
                foreach (var target in Eraser.futureErased)
                {
                    if (target == null) continue;
                    var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                        (byte) CustomRPC.ErasePlayerRoles, SendOption.Reliable, -1);
                    writer.Write(target.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.erasePlayerRoles(target.PlayerId);
                }
            }

            Eraser.futureErased = new List<PlayerControl>();

            // Trickster boxes
            if (Trickster.trickster != null && JackInTheBox.hasJackInTheBoxLimitReached())
            {
                JackInTheBox.convertToVents();
            }

            // SecurityGuard vents and cameras
            var allCameras = ShipStatus.Instance.AllCameras.ToList();
            camerasToAdd.ForEach(camera =>
            {
                camera.gameObject.SetActive(true);
                camera.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
                allCameras.Add(camera);
            });
            ShipStatus.Instance.AllCameras = allCameras.ToArray();
            camerasToAdd = new List<SurvCamera>();

            foreach (var vent in ventsToSeal)
            {
                var animator = vent.GetComponent<PowerTools.SpriteAnim>();
                animator?.Stop();
                vent.EnterVentAnim = vent.ExitVentAnim = null;
                vent.myRend.sprite = animator == null
                    ? SecurityGuard.getStaticVentSealedSprite()
                    : SecurityGuard.getAnimatedVentSealedSprite();
                vent.myRend.color = Color.white;
                vent.name = "SealedVent_" + vent.name;
            }

            ventsToSeal = new List<Vent>();
        }
    }


    [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Destroy), typeof(UnityEngine.Object))]
    internal class MeetingExiledEndPatch
    {
        private static void Prefix(UnityEngine.Object obj)
        {
            if (ExileController.Instance == null || obj != ExileController.Instance.gameObject) return;
            // Reset custom button timers where necessary
            CustomButton.MeetingEndedUpdate();
            // Child set adapted cooldown
            if (Child.child != null && PlayerControl.LocalPlayer == Child.child && Child.child.Data.IsImpostor)
            {
                var multiplier = Child.isGrownUp() ? 0.66f : 2f;
                Child.child.SetKillTimer(PlayerControl.GameOptions.KillCooldown * multiplier);
            }

            // Seer spawn souls
            if (Seer.deadBodyPositions != null && Seer.seer != null && PlayerControl.LocalPlayer == Seer.seer &&
                (Seer.mode == 0 || Seer.mode == 2))
            {
                foreach (var pos in Seer.deadBodyPositions)
                {
                    var soul = new GameObject();
                    soul.transform.position = pos;
                    soul.layer = 5;
                    var rend = soul.AddComponent<SpriteRenderer>();
                    rend.sprite = Seer.getSoulSprite();

                    if (Seer.limitSoulDuration)
                    {
                        HudManager.Instance.StartCoroutine(Effects.Lerp(Seer.soulDuration, new Action<float>((p) =>
                        {
                            if (rend != null)
                            {
                                var tmp = rend.color;
                                tmp.a = Mathf.Clamp01(1 - p);
                                rend.color = tmp;
                            }

                            if (p == 1f && rend != null && rend.gameObject != null)
                                UnityEngine.Object.Destroy(rend.gameObject);
                        })));
                    }
                }

                Seer.deadBodyPositions = new List<Vector3>();
            }

            // Arsonist deactivate dead poolable players
            if (Arsonist.arsonist == null || Arsonist.arsonist != PlayerControl.LocalPlayer) return;
            {
                var visibleCounter = 0;
                var transform = HudManager.Instance.UseButton.transform;
                var localPosition = transform.localPosition;
                var bottomLeft = new Vector3(-localPosition.x, localPosition.y, localPosition.z);
                bottomLeft += new Vector3(-0.25f, -0.25f, 0);
                foreach (var p in PlayerControl.AllPlayerControls)
                {
                    if (!Arsonist.dousedIcons.ContainsKey(p.PlayerId)) continue;
                    if (p.Data.IsDead || p.Data.Disconnected)
                    {
                        Arsonist.dousedIcons[p.PlayerId].gameObject.SetActive(false);
                    }
                    else
                    {
                        Arsonist.dousedIcons[p.PlayerId].transform.localPosition =
                            bottomLeft + Vector3.right * visibleCounter * 0.35f;
                        visibleCounter++;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), typeof(StringNames),
        typeof(Il2CppReferenceArray<Il2CppSystem.Object>))]
    internal class ExileControllerMessagePatch
    {
        private static void Postfix(ref string __result, [HarmonyArgument(0)] StringNames id,
            [HarmonyArgument(1)] Il2CppReferenceArray<Il2CppSystem.Object> parts)
        {
            if (ExileController.Instance == null || ExileController.Instance.exiled == null) return;
            var player = Helpers.playerById(ExileController.Instance.exiled.Object.PlayerId);
            if (player == null) return;
            switch (id)
            {
                // Exile role text
                case StringNames.ExileTextPN:
                case StringNames.ExileTextSN:
                case StringNames.ExileTextPP:
                case StringNames.ExileTextSP:
                    __result = player.Data.PlayerName + " was The " + string.Join(" ",
                        RoleInfo.getRoleInfoForPlayer(player).Select(x => x.name).ToArray());
                    break;
                // Hide number of remaining impostors on Jester win
                case StringNames.ImpostorsRemainP:
                case StringNames.ImpostorsRemainS:
                {
                    if (Jester.jester != null && player.PlayerId == Jester.jester.PlayerId) __result = "";
                    break;
                }
            }
        }
    }
}