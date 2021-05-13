using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Modpack
{
    public class HatsPatch
    {
        private static bool modded;

        protected internal struct HatData
        {
            public bool bounce;
            public string name;
            public bool highUp;
            public Vector2 offset;
        }

        private static readonly List<HatData> _hatDatas = new List<HatData>()
        {
            new HatData
            {
                name = "glitch", bounce = false, highUp = false, offset = new Vector2(0f, 0.1f)
            },
            new HatData
            {
                name = "firegod", bounce = false, highUp = false, offset = new Vector2(0f, 0.1f)
            },
            new HatData
            {
                name = "dad", bounce = false, highUp = false, offset = new Vector2(-0.1f, 0.4f)
            },
            new HatData
            {
                name = "mama", bounce = false, highUp = false, offset = new Vector2(-0.1f, 0.4f)
            },
            new HatData
            {
                name = "pinkee", bounce = false, highUp = false, offset = new Vector2(-0.1f, 0.4f)
            },
            new HatData
            {
                name = "racoon", bounce = false, highUp = false, offset = new Vector2(-0.1f, 0.4f)
            },
            new HatData
            {
                name = "raflp", bounce = true, highUp = true, offset = new Vector2(-0.1f, 0.1f)
            },
            new HatData
            {
                name = "aphex", bounce = false, highUp = false, offset = new Vector2(-0.1f, 0.2f)
            },
            new HatData
            {
                name = "junkyard", bounce = false, highUp = false, offset = new Vector2(-0.1f, 0.2f)
            },
            new HatData
            {
                name = "cheesy", bounce = false, highUp = false, offset = new Vector2(-0.1f, 0.2f)
            },
            new HatData
            {
                name = "shubble", bounce = false, highUp = false, offset = new Vector2(-0.1f, 0.2f)
            },
            new HatData
            {
                name = "aplatypuss", bounce = false, highUp = false, offset = new Vector2(-0.1f, 0.2f)
            },
            new HatData
            {
                name = "ze", bounce = false, highUp = false, offset = new Vector2(-0.1f, 0.2f)
            },
            new HatData
            {
                name = "chilled", bounce = false, highUp = false, offset = new Vector2(-0.1f, 0.2f)
            },

            new HatData
            {
                name = "harrie", bounce = true, highUp = true, offset = new Vector2(-0.1f, 0.1f)
            },
            new HatData
            {
                name = "razz", bounce = true, highUp = true, offset = new Vector2(-0.1f, 0.3f)
            },
            new HatData
            {
                name = "kay", bounce = false, highUp = false, offset = new Vector2(-0.1f, 0.4f)
            },
            new HatData
            {
                name = "zylus", bounce = true, highUp = true, offset = new Vector2(-0.1f, 0.1f)
            },
            new HatData
            {
                name = "annie", bounce = false, highUp = false, offset = new Vector2(-0.1f, 0.4f)
            },
            new HatData
            {
                name = "annamaja", bounce = false, highUp = false, offset = new Vector2(-0.1f, 0.4f)
            },
            new HatData
            {
                name = "bloody", bounce = false, highUp = false, offset = new Vector2(-0.1f, 0.4f)
            },
            new HatData
            {
                name = "ellum", bounce = false, highUp = false, offset = new Vector2(-0.1f, 0.4f)
            },
            new HatData
            {
                name = "stumpy", bounce = false, highUp = true, offset = new Vector2(-0.1f, 0.1f)
            },
            new HatData
            {
                name = "breeh", bounce = false, highUp = true, offset = new Vector2(-0.1f, 0.2f)
            },
            new HatData
            {
                name = "vikram_1", bounce = true, highUp = true, offset = new Vector2(-0.1f, 0.3f)
            },
            new HatData
            {
                name = "vikram_2", bounce = true, highUp = true, offset = new Vector2(-0.1f, 0.3f)
            },
            new HatData
            {
                name = "dizzilulu", bounce = false, highUp = false, offset = new Vector2(-0.1f, 0.3f)
            },
            new HatData
            {
                name = "freya", bounce = true, highUp = false, offset = new Vector2(-0.1f, 0.4f)
            },
            new HatData
            {
                name = "lexie", bounce = false, highUp = false, offset = new Vector2(-0.1f, 0.4f)
            },
            new HatData
            {
                name = "slushie", bounce = false, highUp = true, offset = new Vector2(-0.1f, 0.1f)
            },
            new HatData
            {
                name = "falcone", bounce = true, highUp = false, offset = new Vector2(-0.1f, 0.4f)
            },

            new HatData
            {
                name = "bisexual", bounce = false, highUp = false, offset = new Vector2(-0.1f, 0.1f)
            },
            new HatData
            {
                name = "asexual", bounce = false, highUp = false, offset = new Vector2(-0.1f, 0.1f)
            },
            new HatData
            {
                name = "gay", bounce = false, highUp = false, offset = new Vector2(-0.1f, 0.1f)
            },
            new HatData
            {
                name = "pansexual", bounce = false, highUp = false, offset = new Vector2(-0.1f, 0.1f)
            },
            new HatData
            {
                name = "nonbinary", bounce = false, highUp = false, offset = new Vector2(-0.1f, 0.1f)
            },

            new HatData
            {
                name = "trans_1", bounce = false, highUp = false, offset = new Vector2(-0.1f, 0.1f)
            },
            new HatData
            {
                name = "trans_4", bounce = false, highUp = true, offset = new Vector2(-0.1f, 0.5f)
            },
            new HatData
            {
                name = "trans_3", bounce = false, highUp = false, offset = new Vector2(-0.1f, 0.1f)
            },
            new HatData
            {
                name = "trans_2", bounce = false, highUp = true, offset = new Vector2(-0.1f, 0.1f)
            },
            new HatData
            {
                name = "kiraa", bounce = false, highUp = false, offset = new Vector2(-0.1f, 0.5f)
            },
            new HatData
            {
                name = "oggy", bounce = false, highUp = false, offset = new Vector2(-0.1f, 0.5f)
            },
            new HatData
            {
                name = "werella", bounce = false, highUp = false, offset = new Vector2(-0.1f, 0.5f)
            },
            new HatData
            {
                name = "yuki", bounce = false, highUp = false, offset = new Vector2(-0.1f, 0.5f)
            },
            new HatData
            {
                name = "corpse", bounce = false, highUp = false, offset = new Vector2(-0.1f, 0.2f)
            },
            new HatData
            {
                name = "sykkuno", bounce = false, highUp = false, offset = new Vector2(-0.1f, 0.6f)
            },
        };

        public static readonly List<uint> TallIds = new List<uint>();

        protected internal static readonly Dictionary<uint, HatData> IdToData = new Dictionary<uint, HatData>();

        private static HatBehaviour CreateHat(HatData hat, int id)
        {
            var sprite = Helpers.loadSpriteFromResources($"Modpack.Resources.Hats.hat_{hat.name}.png", 225f, true);
            var newHat = ScriptableObject.CreateInstance<HatBehaviour>();
            newHat.MainImage = sprite;
            newHat.ProductId = hat.name;
            newHat.Order = 99 + id;
            newHat.InFront = true;
            newHat.NoBounce = !hat.bounce;
            newHat.ChipOffset = hat.offset;

            return newHat;
        }

        private static IEnumerable<HatBehaviour> CreateAllHats()
        {
            var i = 0;
            foreach (var hat in _hatDatas)
            {
                yield return CreateHat(hat, ++i);
            }
        }

        [HarmonyPatch(typeof(HatManager), nameof(HatManager.GetHatById))]
        public static class HatManagerPatch
        {
            private static bool Prefix(HatManager __instance)
            {
                if (modded) return true;
                modded = true;
                var id = 0;

                foreach (var hatData in _hatDatas)
                {
                    var hat = CreateHat(hatData, id++);
                    __instance.AllHats.Add(hat);
                    if (hatData.highUp)
                    {
                        TallIds.Add((uint) (__instance.AllHats.Count - 1));
                    }

                    IdToData.Add((uint) __instance.AllHats.Count - 1, hatData);
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetHat))]
        public static class PlayerControl_SetHat
        {
            public static void Postfix(PlayerControl __instance, uint __0, int __1)
            {
                __instance.nameText.transform.localPosition = new Vector3(
                    0f,
                    __0 == 0U ? 0.7f : TallIds.Contains(__0) ? 1.2f : 1.05f,
                    -0.5f
                );
            }
        }
    }
}