using System;
using System.Collections.Generic;
using UnityEngine;

namespace Modpack
{
    internal class Footprint
    {
        private static readonly List<Footprint> footprints = new List<Footprint>();
        private static Sprite sprite;

        public static Sprite getFootprintSprite()
        {
            if (sprite) return sprite;
            sprite = Helpers.loadSpriteFromResources("Modpack.Resources.Footprint.png", 600f);
            return sprite;
        }

        public Footprint(float footprintDuration, bool anonymousFootprints, PlayerControl player)
        {
            var owner = player;
            Color color = anonymousFootprints ? Palette.PlayerColors[6] : Palette.PlayerColors[player.Data.ColorId];

            var footprint = new GameObject("Footprint");
            var transform = player.transform;
            var position1 = transform.position;
            var position = new Vector3(position1.x, position1.y,
                position1.z + 1f);
            footprint.transform.position = position;
            footprint.transform.localPosition = position;
            footprint.transform.SetParent(transform.parent);

            footprint.transform.Rotate(0.0f, 0.0f, UnityEngine.Random.Range(0.0f, 360.0f));


            var spriteRenderer = footprint.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = getFootprintSprite();
            spriteRenderer.color = color;

            footprint.SetActive(true);
            footprints.Add(this);

            HudManager.Instance.StartCoroutine(Effects.Lerp(footprintDuration, new Action<float>(p =>
            {
                var c = color;
                if (!anonymousFootprints && owner != null)
                {
                    if (owner == Morphling.morphling && Morphling.morphTimer > 0 && Morphling.morphTarget?.Data != null)
                        c = Palette.ShadowColors[Morphling.morphTarget.Data.ColorId];
                    else if (Camouflager.camouflageTimer > 0)
                        c = Palette.PlayerColors[6];
                }

                if (spriteRenderer) spriteRenderer.color = new Color(c.r, c.g, c.b, Mathf.Clamp01(1 - p));

                if (p != 1f || footprint == null) return;
                UnityEngine.Object.Destroy(footprint);
                footprints.Remove(this);
            })));
        }
    }
}