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
            public bool adaptive;
            public Vector2 offset;
        }

        private static readonly List<HatData> _hatDatas = new List<HatData>()
        {
            new HatData
            {
                name = "voku", offset = new Vector2(-0.1f, 0.4f)
            },
            new HatData
            {
                name = "tutu", offset = new Vector2(-0.1f, 0.5f)
            },
            new HatData
            {
                name = "trex", adaptive = true, offset = new Vector2(-0.1f, 0.4f)
            },
            new HatData
            {
                name = "cone", bounce = true, offset = new Vector2(-0.1f, 0f)
            },
            new HatData
            {
                name = "towel", bounce = true, adaptive = true, offset = new Vector2(-0.1f, 0.6f)
            },
            new HatData
            {
                name = "turban", adaptive = true, offset = new Vector2(-0.1f, 0.2f)
            },
            new HatData
            {
                name = "tinkercat", offset = new Vector2(-0.1f, 0.5f)
            },
            new HatData
            {
                name = "othercat", offset = new Vector2(-0.1f, 0.4f)
            },
            new HatData
            {
                name = "tentacle", adaptive = true, offset = new Vector2(-0.1f, 0.5f)
            },
            new HatData
            {
                name = "teletubby", adaptive = true, offset = new Vector2(-0.1f, 0.1f)
            },
            new HatData
            {
                name = "tanuki", adaptive = true, offset = new Vector2(-0.1f, 0.5f)
            },
            new HatData
            {
                name = "sunhat", bounce = true, adaptive = true, offset = new Vector2(-0.1f, 0.3f)
            },
            new HatData
            {
                name = "spartan", bounce = true, adaptive = true, offset = new Vector2(-0.1f, 0.2f)
            },
            new HatData
            {
                name = "sombrero", offset = new Vector2(-0.1f, 0.2f)
            },
            new HatData
            {
                name = "snail", adaptive = true, offset = new Vector2(-0.1f, 0.1f)
            },
            new HatData
            {
                name = "shiba", bounce = true, offset = new Vector2(-0.1f, 0.1f)
            },
            new HatData
            {
                name = "samurai", offset = new Vector2(-0.1f, 0.3f)
            },
            new HatData
            {
                name = "rose", offset = new Vector2(-0.1f, 0.3f)
            },
            new HatData
            {
                name = "propeller", offset = new Vector2(-0.1f, 0.1f)
            },
            new HatData
            {
                name = "pony", adaptive = true, offset = new Vector2(-0.1f, 0.1f)
            },
            new HatData
            {
                name = "phone", adaptive = true, offset = new Vector2(-0.1f, 0.6f)
            },
            new HatData
            {
                name = "penguin", bounce = true, offset = new Vector2(-0.1f, 0.1f)
            },
            new HatData
            {
                name = "panda", offset = new Vector2(-0.1f, 0.1f)
            },
            new HatData
            {
                name = "oldman", offset = new Vector2(-0.1f, 0.5f)
            },
            new HatData
            {
                name = "octopus", bounce = true, offset = new Vector2(-0.1f, 0.2f)
            },
            new HatData
            {
                name = "news", adaptive = true, offset = new Vector2(-0.1f, 0.6f)
            },
            new HatData
            {
                name = "newhoodie", adaptive = true, offset = new Vector2(-0.1f, 0.4f)
            },
            new HatData
            {
                name = "monster", offset = new Vector2(-0.1f, 0.5f)
            },
            new HatData
            {
                name = "mental", offset = new Vector2(-0.1f, 0.4f)
            },
            new HatData
            {
                name = "megaphone", bounce = true, offset = new Vector2(0f, 0.1f)
            },
            new HatData
            {
                name = "mech", adaptive = true, offset = new Vector2(-0.1f, 0.5f)
            },
            new HatData
            {
                name = "love", adaptive = true, offset = new Vector2(-0.1f, 0.5f)
            },
            new HatData
            {
                name = "longbrown", offset = new Vector2(-0.1f, 0.5f)
            },
            new HatData
            {
                name = "leaf", offset = new Vector2(-0.1f, 1f)
            },
            new HatData
            {
                name = "lawyer", offset = new Vector2(0f, 0.4f)
            },
            new HatData
            {
                name = "knight", adaptive = true, offset = new Vector2(-0.1f, 0.4f)
            },
            new HatData
            {
                name = "juli", offset = new Vector2(-0.1f, 0.4f)
            },
            new HatData
            {
                name = "horns", offset = new Vector2(-0.1f, 0.1f)
            },
            new HatData
            {
                name = "hoodie",  adaptive = true, offset = new Vector2(-0.1f, 0.5f)
            },
            new HatData
            {
                name = "holla",  offset = new Vector2(-0.1f, 0.4f)
            },
            new HatData
            {
                name = "harley",  offset = new Vector2(-0.1f, 0.4f)
            },
            new HatData
            {
                name = "hairband", adaptive = true, offset = new Vector2(-0.1f, 0.5f)
            },
            new HatData
            {
                name = "goatee", offset = new Vector2(-0.1f, 0.6f)
            },
            new HatData
            {
                name = "ghostking", bounce = true, offset = new Vector2(-0.1f, 0.1f)
            },
            new HatData
            {
                name = "ghost", bounce = true, adaptive = true, offset = new Vector2(-0.1f, 0.1f)
            },
            new HatData
            {
                name = "frog", bounce = true, offset = new Vector2(-0.1f, 0.3f)
            },
            new HatData
            {
                name = "fisherman", adaptive = true, offset = new Vector2(-0.1f, 0.4f)
            },
            new HatData
            {
                name = "fish", adaptive = true, offset = new Vector2(-0.1f, 0.5f)
            },
            new HatData
            {
                name = "firemage", offset = new Vector2(-0.1f, 0.3f)
            },
            new HatData
            {
                name = "firefighter", adaptive = true, offset = new Vector2(-0.1f, 0.3f)
            },
            new HatData
            {
                name = "katze", offset = new Vector2(-0.1f, 0.2f)
            },
            new HatData
            {
                name = "eof", bounce = true, offset = new Vector2(-0.1f, 0.4f)
            },
            new HatData
            {
                name = "eisbison", offset = new Vector2(-0.1f, 0.1f)
            },
            new HatData
            {
                name = "dumb", offset = new Vector2(-0.1f, 0.1f)
            },
            new HatData
            {
                name = "duck", bounce = true, offset = new Vector2(-0.1f, 0.1f)
            },
            new HatData
            {
                name = "dragon", bounce = true, offset = new Vector2(-0.1f, 0.4f)
            },
            new HatData
            {
                name = "donut", bounce = true, offset = new Vector2(0f, 0.6f)
            },
            new HatData
            {
                name = "dj", offset = new Vector2(-0.1f, 0.4f)
            },
            new HatData
            {
                name = "dino", adaptive = true, offset = new Vector2(-0.1f, 0f)
            },
            new HatData
            {
                name = "demon", adaptive = true, offset = new Vector2(0f, 0.4f)
            },
            new HatData
            {
                name = "darkcat", offset = new Vector2(-0.1f, 0.3f)
            },
            new HatData
            {
                name = "dadhat", offset = new Vector2(-0.1f, 0.2f)
            },
            new HatData
            {
                name = "cook", bounce = true, offset = new Vector2(-0.1f, 0.1f)
            },
            new HatData
            {
                name = "sloth", offset = new Vector2(-0.1f, 0.4f)
            },
            new HatData
            {
                name = "watch", offset = new Vector2(-0.1f, 0.3f)
            },
            new HatData
            {
                name = "catears", adaptive = true, offset = new Vector2(-0.1f, 0.4f)
            },
            new HatData
            {
                name = "cat", offset = new Vector2(-0.1f, 0.4f)
            },
            new HatData
            {
                name = "bushi", offset = new Vector2(-0.1f, 0.4f)
            },
            new HatData
            {
                name = "bulb", adaptive = true, offset = new Vector2(-0.1f, 0.3f)
            },
            new HatData
            {
                name = "bucket", bounce = true, offset = new Vector2(-0.1f, 0.3f)
            },
            new HatData
            {
                name = "brain", adaptive = true, offset = new Vector2(-0.1f, 0.2f)
            },
            new HatData
            {
                name = "boxer", offset = new Vector2(-0.1f, 0.7f)
            },
            new HatData
            {
                name = "viking", offset = new Vector2(-0.1f, 0.5f)
            },
            new HatData
            {
                name = "beard", offset = new Vector2(-0.1f, 0.7f)
            },
            new HatData
            {
                name = "barbarian", adaptive = true, offset = new Vector2(-0.1f, 0.3f)
            },
            new HatData
            {
                name = "bandana", bounce = true, offset = new Vector2(-0.1f, 0.3f)
            },
            new HatData
            {
                name = "axolotl", adaptive = true, offset = new Vector2(-0.1f, 0.4f)
            },
            new HatData
            {
                name = "wings", offset = new Vector2(0f, 0.5f)
            },
            new HatData
            {
                name = "angel", offset = new Vector2(0f, 0.4f)
            },
            new HatData
            {
                name = "afro", bounce = true, offset = new Vector2(-0.1f, 0.3f)
            },
            new HatData
            {
                name = "3d", bounce = true, offset = new Vector2(-0.1f, 0.4f)
            },
            new HatData
            {
                name = "straw", bounce = true, offset = new Vector2(0f, 0.2f)
            },

            new HatData
            {
                name = "glitch", offset = new Vector2(-0.1f, 0.2f)
            },
            new HatData
            {
                name = "firegod", offset = new Vector2(-0.1f, 0.1f)
            },
            new HatData
            {
                name = "dad", offset = new Vector2(-0.1f, 0.4f)
            },
            new HatData
            {
                name = "mama", offset = new Vector2(-0.1f, 0.4f)
            },
            new HatData
            {
                name = "pinkee", offset = new Vector2(-0.1f, 0.4f)
            },
            new HatData
            {
                name = "racoon", offset = new Vector2(-0.1f, 0.4f)
            },
            new HatData
            {
                name = "raflp", bounce = true, highUp = true, offset = new Vector2(-0.1f, 0.1f)
            },
            new HatData
            {
                name = "aphex", offset = new Vector2(-0.1f, 0.2f)
            },
            new HatData
            {
                name = "junkyard", offset = new Vector2(-0.1f, 0.2f)
            },
            new HatData
            {
                name = "cheesy", offset = new Vector2(-0.1f, 0.2f)
            },
            new HatData
            {
                name = "shubble", offset = new Vector2(-0.1f, 0.2f)
            },
            new HatData
            {
                name = "aplatypuss", offset = new Vector2(-0.1f, 0.2f)
            },
            new HatData
            {
                name = "ze", offset = new Vector2(-0.1f, 0.2f)
            },
            new HatData
            {
                name = "chilled", offset = new Vector2(-0.1f, 0.2f)
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
                name = "kay", offset = new Vector2(-0.1f, 0.4f)
            },
            new HatData
            {
                name = "zylus", bounce = true, highUp = true, offset = new Vector2(-0.1f, 0.1f)
            },
            new HatData
            {
                name = "annie", offset = new Vector2(-0.1f, 0.4f)
            },
            new HatData
            {
                name = "annamaja", offset = new Vector2(-0.1f, 0.4f)
            },
            new HatData
            {
                name = "bloody", offset = new Vector2(-0.1f, 0.4f)
            },
            new HatData
            {
                name = "ellum", offset = new Vector2(-0.1f, 0.4f)
            },
            new HatData
            {
                name = "stumpy", highUp = true, offset = new Vector2(-0.1f, 0.1f)
            },
            new HatData
            {
                name = "breeh", highUp = true, offset = new Vector2(-0.1f, 0.2f)
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
                name = "dizzilulu", offset = new Vector2(-0.1f, 0.3f)
            },
            new HatData
            {
                name = "freya", bounce = true, offset = new Vector2(-0.1f, 0.4f)
            },
            new HatData
            {
                name = "lexie", offset = new Vector2(-0.1f, 0.4f)
            },
            new HatData
            {
                name = "slushie", highUp = true, offset = new Vector2(-0.1f, 0.1f)
            },
            new HatData
            {
                name = "falcone", bounce = true, offset = new Vector2(-0.1f, 0.4f)
            },

            new HatData
            {
                name = "bisexual", offset = new Vector2(-0.1f, 0.1f)
            },
            new HatData
            {
                name = "asexual", offset = new Vector2(-0.1f, 0.1f)
            },
            new HatData
            {
                name = "gay", offset = new Vector2(-0.1f, 0.1f)
            },
            new HatData
            {
                name = "pansexual", offset = new Vector2(-0.1f, 0.1f)
            },
            new HatData
            {
                name = "nonbinary", offset = new Vector2(-0.1f, 0.1f)
            },

            new HatData
            {
                name = "trans_1", offset = new Vector2(-0.1f, 0.1f)
            },
            new HatData
            {
                name = "trans_4", highUp = true, offset = new Vector2(-0.1f, 0.5f)
            },
            new HatData
            {
                name = "trans_3", offset = new Vector2(-0.1f, 0.1f)
            },
            new HatData
            {
                name = "trans_2", highUp = true, offset = new Vector2(-0.1f, 0.1f)
            },
            new HatData
            {
                name = "kiraa", offset = new Vector2(-0.1f, 0.5f)
            },
            new HatData
            {
                name = "oggy", offset = new Vector2(-0.1f, 0.5f)
            },
            new HatData
            {
                name = "werella", offset = new Vector2(-0.1f, 0.5f)
            },
            new HatData
            {
                name = "yuki", offset = new Vector2(-0.1f, 0.5f)
            },
            new HatData
            {
                name = "corpse", offset = new Vector2(-0.1f, 0.2f)
            },
            new HatData
            {
                name = "sykkuno", offset = new Vector2(-0.1f, 0.6f)
            },
        };

        public static readonly List<uint> TallIds = new List<uint>();

        protected internal static readonly Dictionary<uint, HatData> IdToData = new Dictionary<uint, HatData>();

        private static Material hatShader;

        private static HatBehaviour CreateHat(HatData hat, int id)
        {
            if (hatShader == null && DestroyableSingleton<HatManager>.InstanceExists)
            {
                foreach (var h in DestroyableSingleton<HatManager>.Instance.AllHats)
                {
                    if (h.AltShader == null) continue;
                    hatShader = h.AltShader;
                    break;
                }
            }

            var sprite = Helpers.loadSpriteFromResources($"Modpack.Resources.Hats.hat_{hat.name}.png", 225f, true);
            var newHat = ScriptableObject.CreateInstance<HatBehaviour>();
            newHat.MainImage = sprite;
            newHat.ProductId = hat.name;
            newHat.Order = 99 + id;
            newHat.InFront = true;
            newHat.NoBounce = !hat.bounce;
            newHat.ChipOffset = hat.offset;

            if (hat.adaptive && hatShader != null)
                newHat.AltShader = hatShader;

            sprite = Helpers.loadSpriteFromResources($"Modpack.Resources.Hats.back_{hat.name}.png", 225f, true);
            if (sprite != null)
            {
                newHat.BackImage = sprite;
                newHat.InFront = false;
            }
                

            sprite = Helpers.loadSpriteFromResources($"Modpack.Resources.Hats.climb_{hat.name}.png", 225f, true);
            if (sprite != null)
                newHat.ClimbImage = sprite;

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