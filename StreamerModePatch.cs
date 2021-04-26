  
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace Modpack {
    [HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Start))]
    public class OptionsMenuBehaviourStartPatch {
        public static ToggleButtonBehaviour streamerModeButton;

        private static void updateStreamerModeButton() {
            if (streamerModeButton == null || streamerModeButton.gameObject == null) return;

            var on = ModpackPlugin.StreamerMode.Value;
            var color = on ? new Color(0f, 1f, 0.16470589f, 1f) : Color.white;
            streamerModeButton.Background.color = color;
            streamerModeButton.Text.text = $"Streamer Mode: {(on ? "On" : "Off")}";
            if (streamerModeButton.Rollover) streamerModeButton.Rollover.ChangeOutColor(color);
        }

        public static void Postfix(OptionsMenuBehaviour __instance) {
            if ((streamerModeButton == null || streamerModeButton.gameObject == null) && __instance.CensorChatButton != null) {
                streamerModeButton = Object.Instantiate(__instance.CensorChatButton, __instance.CensorChatButton.transform.parent);
                streamerModeButton.transform.localPosition += Vector3.down * 0.25f;
                __instance.CensorChatButton.transform.localPosition += Vector3.up * 0.25f;
                var button = streamerModeButton.GetComponent<PassiveButton>();
                button.OnClick = new Button.ButtonClickedEvent();
                button.OnClick.AddListener((UnityEngine.Events.UnityAction)onClick);
                updateStreamerModeButton();
            }

            void onClick() {
                ModpackPlugin.StreamerMode.Value = !ModpackPlugin.StreamerMode.Value;
                updateStreamerModeButton();
            }
        }
    }

    [HarmonyPatch(typeof(TextBoxTMP), nameof(TextBoxTMP.SetText))]
	public static class HiddenTextPatch
	{
		private static void Postfix(TextBoxTMP __instance)
		{
			var flag = ModpackPlugin.StreamerMode.Value && (__instance.name == "GameIdText" || __instance.name == "IpTextBox" || __instance.name == "PortTextBox");
			if (flag) __instance.outputText.text = new string('*', __instance.text.Length);
		}
	}
}