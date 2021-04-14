using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using System;
using Assets.CoreScripts;
using UnityEngine;
using Palette = BLMBFIODBKL;

namespace Modpack
{
    [BepInPlugin(Id, "Modpack", Version)]
    [BepInProcess("Among Us.exe")]
    public class ModpackPlugin : BasePlugin
    {
        public const string Id = "de.julius-kreutz.Modpack";
        public const string Version = "1.0";
        public const byte Major = 2;
        public const byte Minor = 2;
        public const byte Patch = 1;

        public Harmony Harmony { get; } = new Harmony(Id);
        public static ModpackPlugin Instance;

        public static int optionsPage = 1;

        public static ConfigEntry<bool> DebugMode { get; private set; }
        public static ConfigEntry<string> Ip { get; set; }
        public static ConfigEntry<ushort> Port { get; set; }

        public override void Load()
        {
            LoadColors();
            
            DebugMode = Config.Bind("Custom", "Enable Debug Mode", false);
            Ip = Config.Bind("Custom", "Custom Server IP", "127.0.0.1");
            Port = Config.Bind("Custom", "Custom Server Port", (ushort) 22023);

            IRegionInfo customRegion =
                new DnsRegionInfo(Ip.Value, "Custom", StringNames.NoTranslation, Ip.Value, Port.Value)
                    .Cast<IRegionInfo>();
            ServerManager serverManager = DestroyableSingleton<ServerManager>.CHNDKKBEIDG;
            IRegionInfo[] regions = ServerManager.DefaultRegions;

            regions = regions.Concat(new IRegionInfo[] {customRegion}).ToArray();
            ServerManager.DefaultRegions = regions;
            serverManager.AGFAPIKFOFF = regions;
            serverManager.SaveServers();

            CEIOGGEDKAN.LMADJLEGIMH =
                CEIOGGEDKAN.IJGNCMMDGDI = Enumerable.Repeat(3, 16).ToArray(); // Max Imp = Recommended Imp = 3
            CEIOGGEDKAN.JMEMPINECJN = Enumerable.Repeat(4, 15).ToArray(); // Min Players = 4

            DebugMode = Config.Bind("Custom", "Enable Debug Mode", false);
            Instance = this;
            CustomOptionHolder.Load();

            Harmony.PatchAll();
        }
        
        private void LoadColors()
        {
            Color32[] playerColors =
            {
                new Color32(240, 19, 19, byte.MaxValue),
                new Color32(19, 46, 210, byte.MaxValue),
                new Color32(0, 112, 13, byte.MaxValue),
                new Color32(238, 84, 187, byte.MaxValue),
                new Color32(250, 167, 0, byte.MaxValue),
                new Color32(246, 246, 87, byte.MaxValue),
                new Color32(63, 71, 78, byte.MaxValue),
                new Color32(215, 225, 241, byte.MaxValue),
                new Color32(107, 47, 188, byte.MaxValue),
                new Color32(77, 50, 21, byte.MaxValue),
                new Color32(56, 254, 220, byte.MaxValue),
                new Color32(80, 240, 57, byte.MaxValue),
                //NEW COLORS
                new Color32(75, 0, 0, byte.MaxValue),
                new Color32(0, 0, 113, byte.MaxValue),
                new Color32(250, 50, 145, byte.MaxValue),
                new Color32(37, 0, 56, byte.MaxValue),
                new Color32(159, 132, 69, byte.MaxValue),
                new Color32(255, 149, 141, byte.MaxValue),
                new Color32(105, 135, 250, byte.MaxValue),
                new Color32(135, 50, 85, 255),
                new Color32(0, 161, 164, byte.MaxValue),
            };
            
            Color32[] shadowColors =
            {
                new Color32(122, 8, 56, byte.MaxValue),
                new Color32(9, 21, 142, byte.MaxValue),
                new Color32(0, 78, 5, byte.MaxValue),
                new Color32(172, 43, 174, byte.MaxValue),
                new Color32(180, 62, 21, byte.MaxValue),
                new Color32(195, 136, 34, byte.MaxValue),
                new Color32(30, 31, 38, byte.MaxValue),
                new Color32(132, 149, 192, byte.MaxValue),
                new Color32(59, 23, 124, byte.MaxValue),
                new Color32(56, 37, 16, byte.MaxValue),
                new Color32(36, 168, 190, byte.MaxValue),
                new Color32(21, 168, 66, byte.MaxValue),
                //NEW COLORS
                new Color32(35, 0, 0, byte.MaxValue),
                new Color32(0, 0, 78, byte.MaxValue),
                new Color32(200, 40, 115, byte.MaxValue),
                new Color32(17, 0, 39, byte.MaxValue),
                new Color32(76, 63, 30, byte.MaxValue),
                new Color32(214, 119, 127, byte.MaxValue),
                new Color32(60, 60, 250, byte.MaxValue),
                new Color32(70, 30, 45, byte.MaxValue),
                new Color32(0, 82, 83, byte.MaxValue),
            };
            
            StringNames[] colorNames =
            {
                StringNames.ColorRed,
                StringNames.ColorBlue,
                StringNames.ColorGreen,
                StringNames.ColorPink,
                StringNames.ColorOrange,
                StringNames.ColorYellow,
                StringNames.ColorBlack,
                StringNames.ColorWhite,
                StringNames.ColorPurple,
                StringNames.ColorBrown,
                StringNames.ColorCyan,
                StringNames.ColorLime,
                //NEW COLORS
                StringNames.ColorRed,
                StringNames.ColorBlue,
                StringNames.ColorPink,
                StringNames.ColorPurple,
                StringNames.ColorBrown,
                StringNames.ColorPink,
                StringNames.ColorBlue,
                StringNames.ColorRed,
                StringNames.ColorCyan,
            };
            
            StringNames[] shortColorNames =
            {
                StringNames.VitalsRED,
                StringNames.VitalsBLUE,
                StringNames.VitalsGRN,
                StringNames.VitalsPINK,
                StringNames.VitalsORGN,
                StringNames.VitalsYLOW,
                StringNames.VitalsBLAK,
                StringNames.VitalsWHTE,
                StringNames.VitalsPURP,
                StringNames.VitalsBRWN,
                StringNames.VitalsCYAN,
                StringNames.VitalsLIME,
                //NEW COLORS
                StringNames.VitalsRED,
                StringNames.VitalsBLUE,
                StringNames.VitalsPINK,
                StringNames.VitalsPURP,
                StringNames.VitalsBRWN,
                StringNames.VitalsPINK,
                StringNames.VitalsBLUE,
                StringNames.VitalsRED,
                StringNames.VitalsCYAN,
            };
            
            Palette.AEDCMKGJKAG = playerColors;
            Palette.PHFOPNDOEMD = shadowColors;
            
            MedScanMinigame.ALHCFFDHINL = colorNames;
            Telemetry.ALHCFFDHINL = colorNames;

            Palette.MBDDHJCCLBP = shortColorNames;
        }
    }

    // Deactivate bans, since I always leave my local testing game and ban myself
    [HarmonyPatch(typeof(IAJICOPDKHA), nameof(IAJICOPDKHA.LEHPLHFNDLM), MethodType.Getter)]
    public static class AmBannedPatch
    {
        public static void Postfix(out bool __result)
        {
            __result = false;
        }
    }

    // Debugging tools
    [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
    public static class DebugManager
    {
        private static readonly System.Random random = new System.Random((int) DateTime.Now.Ticks);
        private static List<PlayerControl> bots = new List<PlayerControl>();

        public static void Postfix(KeyboardJoystick __instance)
        {
            if (!ModpackPlugin.DebugMode.Value) return;

            // Spawn dummys
            if (Input.GetKeyDown(KeyCode.F))
            {
                var playerControl = UnityEngine.Object.Instantiate(AmongUsClient.Instance.PlayerPrefab);
                var i = playerControl.PlayerId = (byte) GameData.Instance.GetAvailableId();

                bots.Add(playerControl);
                GameData.Instance.AddPlayer(playerControl);
                AmongUsClient.Instance.Spawn(playerControl, -2, IDCDPDDALNM.None);

                playerControl.transform.position = PlayerControl.LocalPlayer.transform.position;
                playerControl.GetComponent<DummyBehaviour>().enabled = true;
                playerControl.NetTransform.enabled = false;
                playerControl.SetName(RandomString(10));
                playerControl.SetColor((byte) random.Next(BLMBFIODBKL.AEDCMKGJKAG.Length));
                playerControl.SetHat((uint) random.Next(HatManager.CHNDKKBEIDG.AllHats.Count),
                    playerControl.PPMOEEPBHJO.IMMNCAGJJJC);
                playerControl.SetPet((uint) random.Next(HatManager.CHNDKKBEIDG.AllPets.Count));
                playerControl.SetSkin((uint) random.Next(HatManager.CHNDKKBEIDG.AllSkins.Count));
                GameData.Instance.RpcSetTasks(playerControl.PlayerId, new byte[0]);
            }

            // Terminate round
            if (Input.GetKeyDown(KeyCode.L))
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                    (byte) CustomRPC.ForceEnd, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.forceEnd();
            }
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}