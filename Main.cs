using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

namespace Modpack
{
    [BepInPlugin(Id, "Modpack", VersionString)]
    [BepInProcess("Among Us.exe")]
    public class ModpackPlugin : BasePlugin
    {
        public const string Id = "de.julius-kreutz.Modpack";
        public const string VersionString = "1.0.0";
        public static readonly Version Version = Version.Parse(VersionString);

        public Harmony Harmony { get; } = new Harmony(Id);
        public static ModpackPlugin Instance;

        public static int optionsPage = 1;

        public static ConfigEntry<bool> DebugMode { get; private set; }
        public static ConfigEntry<bool> StreamerMode { get; set; }
        public static ConfigEntry<bool> GhostsSeeTasks { get; set; }
        public static ConfigEntry<bool> GhostsSeeRoles { get; set; }
        public static ConfigEntry<bool> GhostsSeeVotes { get; set; }
        public static ConfigEntry<bool> ShowRoleSummary { get; set; }
        public static ConfigEntry<string> StreamerModeReplacementText { get; set; }
        public static ConfigEntry<string> StreamerModeReplacementColor { get; set; }
        public static ConfigEntry<string> Ip { get; set; }
        public static ConfigEntry<ushort> Port { get; set; }

        public static IRegionInfo[] defaultRegions;

        public static void UpdateRegions()
        {
            var serverManager = DestroyableSingleton<ServerManager>.Instance;
            var regions = defaultRegions;

            var CustomRegion = new DnsRegionInfo(Ip.Value, "Custom", StringNames.NoTranslation, Ip.Value, Port.Value);
            regions = regions.Concat(new[] {CustomRegion.Cast<IRegionInfo>()}).ToArray();
            ServerManager.DefaultRegions = regions;
            serverManager.AvailableRegions = regions;
        }

        public override void Load()
        {
            DebugMode = Config.Bind("Custom", "Enable Debug Mode", false);
            StreamerMode = Config.Bind("Custom", "Enable Streamer Mode", false);
            GhostsSeeTasks = Config.Bind("Custom", "Ghosts See Remaining Tasks", true);
            GhostsSeeRoles = Config.Bind("Custom", "Ghosts See Roles", true);
            GhostsSeeVotes = Config.Bind("Custom", "Ghosts See Votes", true);
            ShowRoleSummary = Config.Bind("Custom", "Show Role Summary", true);
            StreamerModeReplacementText =
                Config.Bind("Custom", "Streamer Mode Replacement Text", "\n\nThe Other Roles");
            StreamerModeReplacementColor =
                Config.Bind("Custom", "Streamer Mode Replacement Text Hex Color", "#87AAF5FF");


            Ip = Config.Bind("Custom", "Custom Server IP", "julius-kreutz.de");
            Port = Config.Bind("Custom", "Custom Server Port", (ushort) 22023);
            defaultRegions = ServerManager.DefaultRegions;

            UpdateRegions();

            GameOptionsData.RecommendedImpostors =
                GameOptionsData.MaxImpostors = Enumerable.Repeat(3, 16).ToArray(); // Max Imp = Recommended Imp = 3
            GameOptionsData.MinPlayers = Enumerable.Repeat(4, 15).ToArray(); // Min Players = 4

            DebugMode = Config.Bind("Custom", "Enable Debug Mode", false);
            Instance = this;
            CustomOptionHolder.Load();
            CustomColors.Load();

            Harmony.PatchAll();
        }
    }

    // Deactivate bans, since I always leave my local testing game and ban myself
    [HarmonyPatch(typeof(StatsManager), nameof(StatsManager.AmBanned), MethodType.Getter)]
    public static class AmBannedPatch
    {
        public static void Postfix(out bool __result)
        {
            __result = false;
        }
    }

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.Awake))]
    public static class ChatControllerAwakePatch
    {
        private static void Prefix()
        {
            if (EOSManager.Instance.IsMinor()) return;
            SaveManager.chatModeType = 1;
            SaveManager.isGuest = false;
        }
    }

    // Debugging tools
    [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
    public static class DebugManager
    {
        private static readonly System.Random random = new System.Random((int) DateTime.Now.Ticks);
        private static readonly List<PlayerControl> bots = new List<PlayerControl>();

        public static void Postfix(KeyboardJoystick __instance)
        {
            if (!ModpackPlugin.DebugMode.Value) return;

            // Spawn dummys
            if (Input.GetKeyDown(KeyCode.F))
            {
                var playerControl = UnityEngine.Object.Instantiate(AmongUsClient.Instance.PlayerPrefab);
                playerControl.PlayerId = (byte) GameData.Instance.GetAvailableId();

                bots.Add(playerControl);
                GameData.Instance.AddPlayer(playerControl);
                AmongUsClient.Instance.Spawn(playerControl, -2, InnerNet.SpawnFlags.None);

                playerControl.transform.position = PlayerControl.LocalPlayer.transform.position;
                playerControl.GetComponent<DummyBehaviour>().enabled = true;
                playerControl.NetTransform.enabled = false;
                playerControl.SetName(RandomString(10));
                playerControl.SetColor((byte) random.Next(Palette.PlayerColors.Length));
                playerControl.SetHat((uint) random.Next(HatManager.Instance.AllHats.Count), playerControl.Data.ColorId);
                playerControl.SetPet((uint) random.Next(HatManager.Instance.AllPets.Count));
                playerControl.SetSkin((uint) random.Next(HatManager.Instance.AllSkins.Count));
                GameData.Instance.RpcSetTasks(playerControl.PlayerId, new byte[0]);
            }

            // Terminate round
            if (!Input.GetKeyDown(KeyCode.L)) return;
            var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                (byte) CustomRPC.ForceEnd, Hazel.SendOption.Reliable, -1);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.forceEnd();
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}