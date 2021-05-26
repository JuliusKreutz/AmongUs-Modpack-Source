using HarmonyLib;
using static Modpack.Modpack;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Text;

namespace Modpack
{
    internal enum CustomGameOverReason
    {
        LoversWin = 10,
        TeamJackalWin = 11,
        ChildLose = 12,
        JesterWin = 13,
        ArsonistWin = 14
    }

    internal enum WinCondition
    {
        Default,
        LoversTeamWin,
        LoversSoloWin,
        JesterWin,
        JackalWin,
        ChildLose,
        ArsonistWin
    }

    internal static class AdditionalTempData
    {
        // Should be implemented using a proper GameOverReason in the future
        public static WinCondition winCondition = WinCondition.Default;
        public static readonly List<PlayerRoleInfo> playerRoles = new List<PlayerRoleInfo>();

        public static void clear()
        {
            playerRoles.Clear();
            winCondition = WinCondition.Default;
        }

        internal class PlayerRoleInfo
        {
            public string PlayerName { get; set; }
            public List<RoleInfo> Roles { get; set; }
            public int TasksCompleted { get; set; }
            public int TasksTotal { get; set; }
        }
    }


    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    public class OnGameEndPatch
    {
        private static GameOverReason gameOverReason;

        public static void Prefix(AmongUsClient __instance, [HarmonyArgument(0)] ref GameOverReason reason,
            [HarmonyArgument(1)] bool showAd)
        {
            gameOverReason = reason;
            if ((int) reason >= 10) reason = GameOverReason.ImpostorByKill;
        }

        public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ref GameOverReason reason,
            [HarmonyArgument(1)] bool showAd)
        {
            AdditionalTempData.clear();

            foreach (var playerControl in PlayerControl.AllPlayerControls)
            {
                var roles = RoleInfo.getRoleInfoForPlayer(playerControl);
                var (tasksCompleted, tasksTotal) = TasksHandler.taskInfo(playerControl.Data);
                AdditionalTempData.playerRoles.Add(new AdditionalTempData.PlayerRoleInfo()
                {
                    PlayerName = playerControl.Data.PlayerName, Roles = roles, TasksTotal = tasksTotal,
                    TasksCompleted = tasksCompleted
                });
            }

            // Remove Jester, Arsonist, Jackal, former Jackals and Sidekick from winners (if they win, they'll be readded)
            var notWinners = new List<PlayerControl>();
            if (Jester.jester != null) notWinners.Add(Jester.jester);
            if (Sidekick.sidekick != null) notWinners.Add(Sidekick.sidekick);
            if (Jackal.jackal != null) notWinners.Add(Jackal.jackal);
            if (Arsonist.arsonist != null) notWinners.Add(Arsonist.arsonist);
            notWinners.AddRange(Jackal.formerJackals);

            var winnersToRemove = new List<WinningPlayerData>();
            foreach (var winner in TempData.winners)
            {
                if (notWinners.Any(x => x.Data.PlayerName == winner.Name)) winnersToRemove.Add(winner);
            }

            foreach (var winner in winnersToRemove) TempData.winners.Remove(winner);

            var jesterWin = Jester.jester != null && gameOverReason == (GameOverReason) CustomGameOverReason.JesterWin;
            var arsonistWin = Arsonist.arsonist != null &&
                              gameOverReason == (GameOverReason) CustomGameOverReason.ArsonistWin;
            var childLose = Child.child != null && gameOverReason == (GameOverReason) CustomGameOverReason.ChildLose;
            var loversWin = Lovers.existingAndAlive() &&
                            (gameOverReason == (GameOverReason) CustomGameOverReason.LoversWin ||
                             TempData.DidHumansWin(gameOverReason) &&
                             !Lovers
                                 .existingWithKiller()); // Either they win if they are among the last 3 players, or they win if they are both Crewmates and both alive and the Crew wins (Team Imp/Jackal Lovers can only win solo wins)
            var teamJackalWin = gameOverReason == (GameOverReason) CustomGameOverReason.TeamJackalWin &&
                                (Jackal.jackal != null && !Jackal.jackal.Data.IsDead ||
                                 Sidekick.sidekick != null && !Sidekick.sidekick.Data.IsDead);

            // Child lose
            if (childLose)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                var wpd = new WinningPlayerData(Child.child.Data) {IsYou = false};
                // If "no one is the Child", it will display the Child, but also show defeat to everyone
                TempData.winners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.ChildLose;
            }

            // Jester win
            else if (jesterWin)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                var wpd = new WinningPlayerData(Jester.jester.Data);
                TempData.winners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.JesterWin;
            }

            // Arsonist win
            else if (arsonistWin)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                var wpd = new WinningPlayerData(Arsonist.arsonist.Data);
                TempData.winners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.ArsonistWin;
            }

            // Lovers win conditions
            else if (loversWin)
            {
                // Double win for lovers, crewmates also win
                if (!Lovers.existingWithKiller())
                {
                    AdditionalTempData.winCondition = WinCondition.LoversTeamWin;
                    TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                    foreach (var p in PlayerControl.AllPlayerControls)
                    {
                        if (p == null) continue;
                        if (p == Lovers.lover1 || p == Lovers.lover2)
                            TempData.winners.Add(new WinningPlayerData(p.Data));
                        else if (p != Jester.jester && p != Jackal.jackal && p != Sidekick.sidekick &&
                                 p != Arsonist.arsonist && !Jackal.formerJackals.Contains(p) && !p.Data.IsImpostor)
                            TempData.winners.Add(new WinningPlayerData(p.Data));
                    }
                }
                // Lovers solo win
                else
                {
                    AdditionalTempData.winCondition = WinCondition.LoversSoloWin;
                    TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                    TempData.winners.Add(new WinningPlayerData(Lovers.lover1.Data));
                    TempData.winners.Add(new WinningPlayerData(Lovers.lover2.Data));
                }
            }

            // Jackal win condition (should be implemented using a proper GameOverReason in the future)
            else if (teamJackalWin)
            {
                // Jackal wins if nobody except jackal is alive
                AdditionalTempData.winCondition = WinCondition.JackalWin;
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                var wpd = new WinningPlayerData(Jackal.jackal.Data) {IsImpostor = false};
                TempData.winners.Add(wpd);
                // If there is a sidekick. The sidekick also wins
                if (Sidekick.sidekick != null)
                {
                    var wpdSidekick = new WinningPlayerData(Sidekick.sidekick.Data) {IsImpostor = false};
                    TempData.winners.Add(wpdSidekick);
                }

                foreach (var wpdFormerJackal in Jackal.formerJackals.Select(
                    player => new WinningPlayerData(player.Data)))
                {
                    wpdFormerJackal.IsImpostor = false;
                    TempData.winners.Add(wpdFormerJackal);
                }
            }

            // Reset Settings
            RPCProcedure.resetVariables();
        }
    }

    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
    public class EndGameManagerSetUpPatch
    {
        private static readonly int Color1 = Shader.PropertyToID("_Color");

        public static void Postfix(EndGameManager __instance)
        {
            var bonusText = UnityEngine.Object.Instantiate(__instance.WinText.gameObject);
            var position1 = __instance.WinText.transform.position;
            bonusText.transform.position = new Vector3(position1.x,
                position1.y - 0.8f, position1.z);
            bonusText.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
            var textRenderer = bonusText.GetComponent<TMPro.TMP_Text>();
            textRenderer.text = "";

            switch (AdditionalTempData.winCondition)
            {
                case WinCondition.JesterWin:
                    textRenderer.text = "Jester Wins";
                    textRenderer.color = Jester.color;
                    break;
                case WinCondition.ArsonistWin:
                    textRenderer.text = "Arsonist Wins";
                    textRenderer.color = Arsonist.color;
                    break;
                case WinCondition.LoversTeamWin:
                    textRenderer.text = "Lovers And Crewmates Win";
                    textRenderer.color = Lovers.color;
                    __instance.BackgroundBar.material.SetColor(Color1, Lovers.color);
                    break;
                case WinCondition.LoversSoloWin:
                    textRenderer.text = "Lovers Win";
                    textRenderer.color = Lovers.color;
                    __instance.BackgroundBar.material.SetColor(Color1, Lovers.color);
                    break;
                case WinCondition.JackalWin:
                    textRenderer.text = "Team Jackal Wins";
                    textRenderer.color = Jackal.color;
                    break;
                case WinCondition.ChildLose:
                    textRenderer.text = "Child died";
                    textRenderer.color = Child.color;
                    break;
            }

            if (MapOptions.showRoleSummary)
            {
                if (Camera.main is { })
                {
                    var position = Camera.main.ViewportToWorldPoint(new Vector3(0f, 1f, Camera.main.nearClipPlane));
                    var roleSummary = UnityEngine.Object.Instantiate(__instance.WinText.gameObject);
                    roleSummary.transform.position = new Vector3(__instance.ExitButton.transform.position.x + 0.1f,
                        position.y - 0.1f, -14f);
                    roleSummary.transform.localScale = new Vector3(1f, 1f, 1f);

                    var roleSummaryText = new StringBuilder();
                    roleSummaryText.AppendLine("Players and roles at the end of the game:");
                    foreach (var data in AdditionalTempData.playerRoles)
                    {
                        var roles = string.Join(" ", data.Roles.Select(x => Helpers.cs(x.color, x.name)));
                        var taskInfo = data.TasksTotal > 0
                            ? $" - <color=#FAD934FF>({data.TasksCompleted}/{data.TasksTotal})</color>"
                            : "";
                        roleSummaryText.AppendLine($"{data.PlayerName} - {roles}{taskInfo}");
                    }

                    var roleSummaryTextMesh = roleSummary.GetComponent<TMPro.TMP_Text>();
                    roleSummaryTextMesh.alignment = TMPro.TextAlignmentOptions.TopLeft;
                    roleSummaryTextMesh.color = Color.white;
                    roleSummaryTextMesh.fontSizeMin = 1.5f;
                    roleSummaryTextMesh.fontSizeMax = 1.5f;
                    roleSummaryTextMesh.fontSize = 1.5f;

                    var roleSummaryTextMeshRectTransform = roleSummaryTextMesh.GetComponent<RectTransform>();
                    roleSummaryTextMeshRectTransform.anchoredPosition =
                        new Vector2(position.x + 3.5f, position.y - 0.1f);
                    roleSummaryTextMesh.text = roleSummaryText.ToString();
                }
            }

            AdditionalTempData.clear();
        }
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CheckEndCriteria))]
    internal class CheckEndCriteriaPatch
    {
        public static bool Prefix(ShipStatus __instance)
        {
            if (!GameData.Instance) return false;
            if (DestroyableSingleton<TutorialManager>
                .InstanceExists) // InstanceExists | Don't check Custom Criteria when in Tutorial
                return true;
            var statistics = new PlayerStatistics(__instance);
            if (CheckAndEndGameForChildLose(__instance)) return false;
            if (CheckAndEndGameForJesterWin(__instance)) return false;
            if (CheckAndEndGameForArsonistWin(__instance)) return false;
            if (CheckAndEndGameForSabotageWin(__instance)) return false;
            if (CheckAndEndGameForTaskWin(__instance)) return false;
            if (CheckAndEndGameForLoverWin(__instance, statistics)) return false;
            if (CheckAndEndGameForJackalWin(__instance, statistics)) return false;
            if (CheckAndEndGameForImpostorWin(__instance, statistics)) return false;
            return CheckAndEndGameForCrewmateWin(__instance, statistics) && false;
        }

        private static bool CheckAndEndGameForChildLose(ShipStatus __instance)
        {
            if (!Child.triggerChildLose) return false;
            __instance.enabled = false;
            ShipStatus.RpcEndGame((GameOverReason) CustomGameOverReason.ChildLose, false);
            return true;
        }

        private static bool CheckAndEndGameForJesterWin(ShipStatus __instance)
        {
            if (!Jester.triggerJesterWin) return false;
            __instance.enabled = false;
            ShipStatus.RpcEndGame((GameOverReason) CustomGameOverReason.JesterWin, false);
            return true;
        }

        private static bool CheckAndEndGameForArsonistWin(ShipStatus __instance)
        {
            if (!Arsonist.triggerArsonistWin) return false;
            __instance.enabled = false;
            ShipStatus.RpcEndGame((GameOverReason) CustomGameOverReason.ArsonistWin, false);
            return true;
        }

        private static bool CheckAndEndGameForSabotageWin(ShipStatus __instance)
        {
            if (__instance.Systems == null) return false;
            var systemType = __instance.Systems.ContainsKey(SystemTypes.LifeSupp)
                ? __instance.Systems[SystemTypes.LifeSupp]
                : null;
            var lifeSuppSystemType = systemType?.TryCast<LifeSuppSystemType>();
            if (lifeSuppSystemType != null && lifeSuppSystemType.Countdown < 0f)
            {
                EndGameForSabotage(__instance);
                lifeSuppSystemType.Countdown = 10000f;
                return true;
            }

            var systemType2 = (__instance.Systems.ContainsKey(SystemTypes.Reactor)
                ? __instance.Systems[SystemTypes.Reactor]
                : null) ?? (__instance.Systems.ContainsKey(SystemTypes.Laboratory)
                ? __instance.Systems[SystemTypes.Laboratory]
                : null);

            var criticalSystem = systemType2?.TryCast<ICriticalSabotage>();
            if (criticalSystem == null || !(criticalSystem.Countdown < 0f)) return false;
            EndGameForSabotage(__instance);
            criticalSystem.ClearSabotage();
            return true;
        }

        private static bool CheckAndEndGameForTaskWin(ShipStatus __instance)
        {
            if (GameData.Instance.TotalTasks > GameData.Instance.CompletedTasks) return false;
            __instance.enabled = false;
            ShipStatus.RpcEndGame(GameOverReason.HumansByTask, false);
            return true;
        }

        private static bool CheckAndEndGameForLoverWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.TeamLoversAlive != 2 || statistics.TotalAlive > 3) return false;
            __instance.enabled = false;
            ShipStatus.RpcEndGame((GameOverReason) CustomGameOverReason.LoversWin, false);
            return true;
        }

        private static bool CheckAndEndGameForJackalWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.TeamJackalAlive < statistics.TotalAlive - statistics.TeamJackalAlive ||
                statistics.TeamImpostorsAlive != 0 ||
                statistics.TeamJackalHasAliveLover && statistics.TeamLoversAlive == 2) return false;
            __instance.enabled = false;
            ShipStatus.RpcEndGame((GameOverReason) CustomGameOverReason.TeamJackalWin, false);
            return true;
        }

        private static bool CheckAndEndGameForImpostorWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.TeamImpostorsAlive < statistics.TotalAlive - statistics.TeamImpostorsAlive ||
                statistics.TeamJackalAlive != 0 ||
                statistics.TeamImpostorHasAliveLover && statistics.TeamLoversAlive == 2) return false;
            __instance.enabled = false;
            var endReason = TempData.LastDeathReason switch
            {
                DeathReason.Exile => GameOverReason.ImpostorByVote,
                DeathReason.Kill => GameOverReason.ImpostorByKill,
                _ => GameOverReason.ImpostorByVote
            };

            ShipStatus.RpcEndGame(endReason, false);
            return true;
        }

        private static bool CheckAndEndGameForCrewmateWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.TeamImpostorsAlive != 0 || statistics.TeamJackalAlive != 0) return false;
            __instance.enabled = false;
            ShipStatus.RpcEndGame(GameOverReason.HumansByVote, false);
            return true;
        }

        private static void EndGameForSabotage(ShipStatus __instance)
        {
            __instance.enabled = false;
            ShipStatus.RpcEndGame(GameOverReason.ImpostorBySabotage, false);
        }
    }

    internal class PlayerStatistics
    {
        public int TeamImpostorsAlive { get; set; }
        public int TeamJackalAlive { get; set; }
        public int TeamLoversAlive { get; set; }
        public int TotalAlive { get; set; }
        public bool TeamImpostorHasAliveLover { get; set; }
        public bool TeamJackalHasAliveLover { get; set; }

        public PlayerStatistics(ShipStatus __instance)
        {
            GetPlayerCounts();
        }

        private static bool isLover(GameData.PlayerInfo p)
        {
            return Lovers.lover1 != null && Lovers.lover1.PlayerId == p.PlayerId ||
                   Lovers.lover2 != null && Lovers.lover2.PlayerId == p.PlayerId;
        }

        private void GetPlayerCounts()
        {
            var numJackalAlive = 0;
            var numImpostorsAlive = 0;
            var numLoversAlive = 0;
            var numTotalAlive = 0;
            var impLover = false;
            var jackalLover = false;

            for (var i = 0; i < GameData.Instance.PlayerCount; i++)
            {
                GameData.PlayerInfo playerInfo = GameData.Instance.AllPlayers[i];
                if (playerInfo.Disconnected) continue;
                if (playerInfo.IsDead) continue;
                numTotalAlive++;

                var lover = isLover(playerInfo);
                if (lover) numLoversAlive++;

                if (playerInfo.IsImpostor)
                {
                    numImpostorsAlive++;
                    if (lover) impLover = true;
                }

                if (Jackal.jackal != null && Jackal.jackal.PlayerId == playerInfo.PlayerId)
                {
                    numJackalAlive++;
                    if (lover) jackalLover = true;
                }

                if (Sidekick.sidekick == null || Sidekick.sidekick.PlayerId != playerInfo.PlayerId) continue;
                numJackalAlive++;
                if (lover) jackalLover = true;
            }

            TeamJackalAlive = numJackalAlive;
            TeamImpostorsAlive = numImpostorsAlive;
            TeamLoversAlive = numLoversAlive;
            TotalAlive = numTotalAlive;
            TeamImpostorHasAliveLover = impLover;
            TeamJackalHasAliveLover = jackalLover;
        }
    }
}