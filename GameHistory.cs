using System.Collections.Generic;
using System;
using UnityEngine;

namespace Modpack
{
    public class DeadPlayer
    {
        public readonly PlayerControl player;
        public DateTime timeOfDeath;
        public readonly PlayerControl killerIfExisting;

        public DeadPlayer(PlayerControl player, DateTime timeOfDeath, DeathReason deathReason,
            PlayerControl killerIfExisting)
        {
            this.player = player;
            this.timeOfDeath = timeOfDeath;
            this.killerIfExisting = killerIfExisting;
        }
    }

    internal static class GameHistory
    {
        public static List<Tuple<Vector3, bool>> localPlayerPositions = new List<Tuple<Vector3, bool>>();
        public static List<DeadPlayer> deadPlayers = new List<DeadPlayer>();

        public static void clearGameHistory()
        {
            localPlayerPositions = new List<Tuple<Vector3, bool>>();
            deadPlayers = new List<DeadPlayer>();
        }
    }
}