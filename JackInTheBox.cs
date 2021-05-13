using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Modpack
{
    public class JackInTheBox
    {
        public static List<JackInTheBox> AllJackInTheBoxes = new List<JackInTheBox>();
        public const int JackInTheBoxLimit = 3;
        public static bool boxesConvertedToVents;
        public static readonly Sprite[] boxAnimationSprites = new Sprite[18];

        public static Sprite getBoxAnimationSprite(int index)
        {
            if (boxAnimationSprites == null || boxAnimationSprites.Length == 0) return null;
            index = Mathf.Clamp(index, 0, boxAnimationSprites.Length - 1);
            if (boxAnimationSprites[index] == null)
                boxAnimationSprites[index] =
                    Helpers.loadSpriteFromResources(
                        $"Modpack.Resources.TricksterAnimation.trickster_box_00{index + 1:00}.png", 175f);
            return boxAnimationSprites[index];
        }

        public static void startAnimation(int ventId)
        {
            var box = AllJackInTheBoxes.FirstOrDefault((x) => x?.vent != null && x.vent.Id == ventId);
            if (box == null) return;
            var vent = box.vent;

            HudManager.Instance.StartCoroutine(Effects.Lerp(0.6f, new Action<float>((p) =>
            {
                if (vent == null || vent.myRend == null) return;
                vent.myRend.sprite = getBoxAnimationSprite((int) (p * boxAnimationSprites.Length));
                if (p == 1f) vent.myRend.sprite = getBoxAnimationSprite(0);
            })));
        }

        private readonly GameObject gameObject;
        public readonly Vent vent;

        public JackInTheBox(Vector2 p)
        {
            gameObject = new GameObject("JackInTheBox");
            var position = new Vector3(p.x, p.y, PlayerControl.LocalPlayer.transform.position.z + 1f);
            position += (Vector3) PlayerControl.LocalPlayer.Collider
                .offset; // Add collider offset that DoMove moves the player up at a valid position
            // Create the marker
            gameObject.transform.position = position;
            var boxRenderer = gameObject.AddComponent<SpriteRenderer>();
            boxRenderer.sprite = getBoxAnimationSprite(0);

            // Create the vent
            var referenceVent = UnityEngine.Object.FindObjectOfType<Vent>();
            vent = UnityEngine.Object.Instantiate(referenceVent);
            vent.transform.position = gameObject.transform.position;
            vent.Left = null;
            vent.Right = null;
            vent.Center = null;
            vent.EnterVentAnim = null;
            vent.ExitVentAnim = null;
            vent.Offset = new Vector3(0f, 0.25f, 0f);
            vent.GetComponent<PowerTools.SpriteAnim>()?.Stop();
            vent.Id = ShipStatus.Instance.AllVents.Select(x => x.Id).Max() + 1; // Make sure we have a unique id
            var ventRenderer = vent.GetComponent<SpriteRenderer>();
            ventRenderer.sprite = getBoxAnimationSprite(0);
            vent.myRend = ventRenderer;
            var allVentsList = ShipStatus.Instance.AllVents.ToList();
            allVentsList.Add(vent);
            ShipStatus.Instance.AllVents = allVentsList.ToArray();
            vent.gameObject.SetActive(false);
            vent.name = "JackInTheBoxVent_" + vent.Id;

            // Only render the box for the Trickster
            var playerIsTrickster = PlayerControl.LocalPlayer == Trickster.trickster;
            gameObject.SetActive(playerIsTrickster);

            AllJackInTheBoxes.Add(this);
        }

        public static void UpdateStates()
        {
            if (boxesConvertedToVents) return;
            foreach (var box in AllJackInTheBoxes)
            {
                var playerIsTrickster = PlayerControl.LocalPlayer == Trickster.trickster;
                box.gameObject.SetActive(playerIsTrickster);
            }
        }

        public void convertToVent()
        {
            gameObject.SetActive(false);
            vent.gameObject.SetActive(true);
        }

        public static void convertToVents()
        {
            foreach (var box in AllJackInTheBoxes)
            {
                box.convertToVent();
            }

            connectVents();
            boxesConvertedToVents = true;
        }

        public static bool hasJackInTheBoxLimitReached()
        {
            return AllJackInTheBoxes.Count >= JackInTheBoxLimit;
        }

        private static void connectVents()
        {
            for (var i = 0; i < AllJackInTheBoxes.Count - 1; i++)
            {
                var a = AllJackInTheBoxes[i];
                var b = AllJackInTheBoxes[i + 1];
                a.vent.Right = b.vent;
                b.vent.Left = a.vent;
            }

            // Connect first with last
            AllJackInTheBoxes.First().vent.Left = AllJackInTheBoxes.Last().vent;
            AllJackInTheBoxes.Last().vent.Right = AllJackInTheBoxes.First().vent;
        }

        public static void clearJackInTheBoxes()
        {
            boxesConvertedToVents = false;
            AllJackInTheBoxes = new List<JackInTheBox>();
        }
    }
}