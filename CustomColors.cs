using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HarmonyLib;
using UnhollowerBaseLib;
using Assets.CoreScripts;

namespace Modpack
{
    public static class CustomColors
    {
        private static readonly Dictionary<int, string> ColorStrings = new Dictionary<int, string>();
        public static readonly List<int> lighterColors = new List<int>() {3, 4, 5, 7, 10, 11};
        public static int pickableColors = 12;

        public static void Load()
        {
            var longlist = Palette.ColorNames.ToList();
            var shortlist = Palette.ShortColorNames.ToList();
            var colorlist = Palette.PlayerColors.ToList();
            var shadowlist = Palette.ShadowColors.ToList();

            var colors = new List<CustomColor>
            {
                new CustomColor
                {
                    longname = "Salmon",
                    shortname = "SALMN",
                    color = new Color32(239, 191, 192, byte.MaxValue),
                    shadow = new Color32(182, 119, 114, byte.MaxValue),
                    isLighterColor = true
                },
                new CustomColor
                {
                    longname = "Bordeaux",
                    shortname = "BRDX",
                    color = new Color32(109, 7, 26, byte.MaxValue),
                    shadow = new Color32(54, 2, 11, byte.MaxValue),
                    isLighterColor = false
                },
                new CustomColor
                {
                    longname = "Olive",
                    shortname = "OLIVE",
                    color = new Color32(154, 140, 61, byte.MaxValue),
                    shadow = new Color32(104, 95, 40, byte.MaxValue),
                    isLighterColor = false
                },
                new CustomColor
                {
                    longname = "Turqoise",
                    shortname = "TURQ",
                    color = new Color32(22, 132, 176, byte.MaxValue),
                    shadow = new Color32(15, 89, 117, byte.MaxValue),
                    isLighterColor = false
                },
                new CustomColor
                {
                    longname = "Mint",
                    shortname = "MINT",
                    color = new Color32(111, 192, 156, byte.MaxValue),
                    shadow = new Color32(65, 148, 111, byte.MaxValue),
                    isLighterColor = true
                },
                new CustomColor
                {
                    longname = "Lavender",
                    shortname = "LVNDR",
                    color = new Color32(173, 126, 201, byte.MaxValue),
                    shadow = new Color32(131, 58, 203, byte.MaxValue),
                    isLighterColor = true
                },
                new CustomColor
                {
                    longname = "Nougat",
                    shortname = "NOUGT",
                    color = new Color32(160, 101, 56, byte.MaxValue),
                    shadow = new Color32(115, 15, 78, byte.MaxValue),
                    isLighterColor = false
                },
                new CustomColor
                {
                    longname = "Peach",
                    shortname = "PEACH",
                    color = new Color32(255, 164, 119, byte.MaxValue),
                    shadow = new Color32(238, 128, 100, byte.MaxValue),
                    isLighterColor = true
                },
                new CustomColor
                {
                    longname = "Wasabi",
                    shortname = "WSBI",
                    color = new Color32(112, 143, 46, byte.MaxValue),
                    shadow = new Color32(72, 92, 29, byte.MaxValue),
                    isLighterColor = true
                },
                new CustomColor
                {
                    longname = "Hot Pink",
                    shortname = "HTPNK",
                    color = new Color32(255, 51, 102, byte.MaxValue),
                    shadow = new Color32(232, 0, 58, byte.MaxValue),
                    isLighterColor = true
                },
                new CustomColor
                {
                    longname = "Gray",
                    shortname = "GRAY",
                    color = new Color32(147, 147, 147, byte.MaxValue),
                    shadow = new Color32(120, 120, 120, byte.MaxValue),
                    isLighterColor = false
                },
                new CustomColor
                {
                    longname = "Petrol",
                    shortname = "PTRL",
                    color = new Color32(0, 99, 105, byte.MaxValue),
                    shadow = new Color32(0, 61, 54, byte.MaxValue),
                    isLighterColor = false
                }
            };


            pickableColors += colors.Count; // Colors to show in Tab
            /* Hidden Colors */
            colors.Add(new CustomColor
            {
                longname = "Panda", shortname = "PANDA",
                color = new Color32(255, 255, 255, 0),
                shadow = new Color32(12, 12, 12, 0),
                isLighterColor = true
            });
            colors.Add(new CustomColor
            {
                longname = "Midnight", shortname = "MDNT",
                color = new Color32(64, 8, 71, 0),
                shadow = new Color32(24, 32, 116, 0),
                isLighterColor = false
            });
            /* Add Colors */
            var id = 50000;
            foreach (var cc in colors)
            {
                longlist.Add((StringNames) id);
                ColorStrings[id++] = cc.longname;
                shortlist.Add((StringNames) id);
                ColorStrings[id++] = cc.shortname;
                colorlist.Add(cc.color);
                shadowlist.Add(cc.shadow);
                if (cc.isLighterColor)
                    lighterColors.Add(colorlist.Count - 1);
            }

            Palette.ShortColorNames = shortlist.ToArray();
            Palette.ColorNames = longlist.ToArray();
            Palette.PlayerColors = colorlist.ToArray();
            Palette.ShadowColors = shadowlist.ToArray();
            MedScanMinigame.ColorNames = Palette.ColorNames;
            Telemetry.ColorNames = Palette.ColorNames;
        }

        protected internal struct CustomColor
        {
            public string longname;
            public string shortname;
            public Color32 color;
            public Color32 shadow;
            public bool isLighterColor;
        }

        [HarmonyPatch]
        public static class CustomColorPatches
        {
            [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new[]
            {
                typeof(StringNames),
                typeof(Il2CppReferenceArray<Il2CppSystem.Object>)
            })]
            private class ColorStringPatch
            {
                public static bool Prefix(ref string __result, [HarmonyArgument(0)] StringNames name)
                {
                    if ((int) name < 50000) return true;
                    var text = ColorStrings[(int) name];
                    if (text == null) return true;
                    __result = text;
                    return false;
                }
            }

            [HarmonyPatch(typeof(PlayerTab), nameof(PlayerTab.OnEnable))]
            private static class PlayerTabEnablePatch
            {
                public static void Postfix(PlayerTab __instance)
                {
                    // Replace instead
                    var chips = __instance.ColorChips.ToArray();
                    const int
                        cols = 4; // TODO: Design an algorithm to dynamically position chips to optimally fill space
                    for (var i = 0; i < chips.Length; i++)
                    {
                        var chip = chips[i];
                        int row = i / cols, col = i % cols; // Dynamically do the positioning
                        var transform = chip.transform;
                        transform.localPosition = new Vector3(1.46f + (col * 0.6f), -0.43f - (row * 0.55f),
                            transform.localPosition.z);
                        transform.localScale *= 0.9f;

                        if (i >= pickableColors)
                            chip.transform.localScale *= 0f; // Needs to exist for PlayerTab
                    }
                }
            }
        }
    }
}