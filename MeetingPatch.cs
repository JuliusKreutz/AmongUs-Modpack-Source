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

            private static int IndexOfMax(IReadOnlyList<byte> self, Func<byte, int> comparer, out bool tie)
            {
                tie = false;
                var num = int.MinValue;
                var result = -1;
                for (var i = 0; i < self.Count; i++)
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
                    if (pi.PlayerId != maxIdx || pi.IsDead) continue;
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
            private static bool Prefix(MeetingHud __instance, [HarmonyArgument(0)] IList<byte> states)
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


        private static void swapperOnClick(int i, MeetingHud __instance)
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

        private static GameObject guesserUI;

        private static void guesserOnClick(int buttonTarget, MeetingHud __instance)
        {
            if (guesserUI != null || !(__instance.state == MeetingHud.VoteStates.Voted ||
                                       __instance.state == MeetingHud.VoteStates.NotVoted)) return;

            Transform transform;
            var container =
                UnityEngine.Object.Instantiate((transform = __instance.transform).FindChild("Background"), transform);
            container.transform.localPosition = new Vector3(0, 0, -5f);
            guesserUI = container.gameObject;

            var i = 0;
            var buttonTemplate = __instance.playerStates[0].transform.FindChild("votePlayerBase");
            var smallButtonTemplate = __instance.playerStates[0].Buttons.transform.Find("CancelButton");
            var textTemplate = __instance.playerStates[0].NameText;


            var exitButton = UnityEngine.Object.Instantiate(smallButtonTemplate.transform, container);
            exitButton.transform.localPosition = new Vector3(2.5f, 2.25f, -5);
            exitButton.GetComponent<PassiveButton>().OnClick.RemoveAllListeners();
            exitButton.GetComponent<PassiveButton>().OnClick.AddListener((UnityEngine.Events.UnityAction) (() =>
            {
                UnityEngine.Object.Destroy(container.gameObject);
            }));

            var confirmButtons = new List<Transform>();

            foreach (var roleInfo in RoleInfo.allRoleInfos)
            {
                if (roleInfo.roleId == RoleId.Lover || roleInfo.roleId == RoleId.Guesser ||
                    roleInfo == RoleInfo.niceMini) continue; // Not guessable roles

                var button = UnityEngine.Object.Instantiate(buttonTemplate.transform, container);
                var confirm = UnityEngine.Object.Instantiate(smallButtonTemplate.transform, button);
                confirmButtons.Add(confirm);
                var label = UnityEngine.Object.Instantiate(textTemplate, button);
                int row = i / 4, col = i % 4;
                button.localPosition = new Vector3(-3 + 1.83f * col, 1.5f - 0.4f * row, -5);
                button.localScale = new Vector3(0.4f, 0.4f, 1f);
                confirm.localScale = new Vector3(1.5f, 1.5f, 1f);
                confirm.localPosition = new Vector3(0, 0, confirm.localPosition.z);
                confirm.GetComponent<SpriteRenderer>().sprite = Guesser.getTargetSprite();
                confirm.GetComponent<SpriteRenderer>().color = Color.black;
                confirm.gameObject.SetActive(false);
                label.text = Helpers.cs(roleInfo.color, roleInfo.name);
                label.alignment = TMPro.TextAlignmentOptions.Center;
                label.transform.localPosition = new Vector3(0, 0, label.transform.localPosition.z);
                label.transform.localScale *= 2;

                button.GetComponent<PassiveButton>().OnClick.RemoveAllListeners();
                button.GetComponent<PassiveButton>().OnClick.AddListener((UnityEngine.Events.UnityAction) (() =>
                {
                    confirmButtons.ForEach(x => x.gameObject.SetActive(false));
                    confirm.gameObject.SetActive(true);
                }));


                confirm.GetComponent<PassiveButton>().OnClick.RemoveAllListeners();
                confirm.GetComponent<PassiveButton>().OnClick.AddListener((UnityEngine.Events.UnityAction) (() =>
                {
                    var id = Helpers.playerById((byte) __instance.playerStates[buttonTarget].TargetPlayerId);
                    if (!(__instance.state == MeetingHud.VoteStates.Voted ||
                          __instance.state == MeetingHud.VoteStates.NotVoted) || id == null ||
                        Guesser.remainingShots <= 0) return;

                    var mainRoleInfo = RoleInfo.getRoleInfoForPlayer(id).FirstOrDefault();
                    if (mainRoleInfo == null) return;

                    id = mainRoleInfo == roleInfo ? id : PlayerControl.LocalPlayer;

                    var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                        (byte) CustomRPC.GuesserShoot, SendOption.Reliable, -1);
                    writer.Write(id.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.guesserShoot(id.PlayerId);

                    UnityEngine.Object.Destroy(container.gameObject);
                    __instance.playerStates.ToList().ForEach(x =>
                    {
                        if (x.transform.FindChild("ShootButton") != null)
                            UnityEngine.Object.Destroy(x.transform.FindChild("ShootButton").gameObject);
                    });
                }));

                i++;
            }

            container.transform.localScale *= 0.75f;
        }

        [HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.Select))]
        private class PlayerVoteAreaSelectPatch
        {
            private static bool Prefix(MeetingHud __instance)
            {
                return !(PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer == Guesser.guesser &&
                         guesserUI != null);
            }
        }


        private static void populateButtonsPostfix(MeetingHud __instance)
        {
            // Add Swapper Buttons
            if (Swapper.swapper != null && PlayerControl.LocalPlayer == Swapper.swapper && !Swapper.swapper.Data.IsDead)
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
                        (UnityEngine.Events.UnityAction) (() => swapperOnClick(copiedIndex, __instance)));

                    selections[i] = false;
                    renderers[i] = renderer;
                }
            }

            // Add Guesser Buttons
            if (Guesser.guesser != null && PlayerControl.LocalPlayer == Guesser.guesser &&
                !Guesser.guesser.Data.IsDead && Guesser.remainingShots >= 0)
            {
                for (var i = 0; i < __instance.playerStates.Length; i++)
                {
                    var playerVoteArea = __instance.playerStates[i];
                    if (playerVoteArea.isDead || playerVoteArea.TargetPlayerId == Guesser.guesser.PlayerId) continue;

                    var template = playerVoteArea.Buttons.transform.Find("CancelButton").gameObject;
                    var targetBox = UnityEngine.Object.Instantiate(template, playerVoteArea.transform);
                    targetBox.name = "ShootButton";
                    targetBox.transform.localPosition = new Vector3(0f, 0.03f, template.transform.localPosition.z);
                    var renderer = targetBox.GetComponent<SpriteRenderer>();
                    renderer.sprite = Guesser.getTargetSprite();
                    var button = targetBox.GetComponent<PassiveButton>();
                    button.OnClick.RemoveAllListeners();
                    var copiedIndex = i;
                    button.OnClick.AddListener(
                        (UnityEngine.Events.UnityAction) (() => guesserOnClick(copiedIndex, __instance)));
                }
            }

            // Change buttons if there are more than 10 players
            if (__instance.playerStates == null || __instance.playerStates.Length <= 10) return;
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
}