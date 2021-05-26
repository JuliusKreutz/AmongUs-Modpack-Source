using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace Modpack
{
    [HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Start))]
    public class OptionsMenuBehaviourStartPatch
    {
        private static Vector3? origin;
        private static ToggleButtonBehaviour streamerModeButton;
        private static ToggleButtonBehaviour ghostsSeeTasksButton;
        private static ToggleButtonBehaviour ghostsSeeRolesButton;
        private static ToggleButtonBehaviour ghostsSeeVotesButton;
        private static ToggleButtonBehaviour showRoleSummaryButton;

        public const float xOffset = 1.75f;
        public const float yOffset = -0.5f;

        private static void updateToggle(ToggleButtonBehaviour button, string text, bool on)
        {
            if (button == null || button.gameObject == null) return;

            var color = on ? new Color(0f, 1f, 0.16470589f, 1f) : Color.white;
            button.Background.color = color;
            button.Text.text = $"{text}{(on ? "On" : "Off")}";
            if (button.Rollover) button.Rollover.ChangeOutColor(color);
        }

        private static ToggleButtonBehaviour createCustomToggle(string text, bool on, Vector3 offset,
            UnityEngine.Events.UnityAction onClick, OptionsMenuBehaviour __instance)
        {
            if (__instance.CensorChatButton == null) return null;
            var button = Object.Instantiate(__instance.CensorChatButton, __instance.CensorChatButton.transform.parent);
            button.transform.localPosition = (origin ?? Vector3.zero) + offset;
            var passiveButton = button.GetComponent<PassiveButton>();
            passiveButton.OnClick = new Button.ButtonClickedEvent();
            passiveButton.OnClick.AddListener(onClick);
            updateToggle(button, text, @on);

            return button;
        }

        public static void Postfix(OptionsMenuBehaviour __instance)
        {
            if (__instance.CensorChatButton != null)
            {
                var transform = __instance.CensorChatButton.transform;
                origin ??= transform.localPosition + Vector3.up * 0.25f;
                transform.localPosition = origin.Value + Vector3.left * xOffset;
                transform.localScale = Vector3.one * 2f / 3f;
            }

            if (streamerModeButton == null || streamerModeButton.gameObject == null)
            {
                streamerModeButton = createCustomToggle("Streamer Mode: ", ModpackPlugin.StreamerMode.Value,
                    Vector3.zero, (UnityEngine.Events.UnityAction) streamerModeToggle, __instance);

                static void streamerModeToggle()
                {
                    ModpackPlugin.StreamerMode.Value = !ModpackPlugin.StreamerMode.Value;
                    updateToggle(streamerModeButton, "Streamer Mode: ", ModpackPlugin.StreamerMode.Value);
                }
            }

            if (ghostsSeeTasksButton == null || ghostsSeeTasksButton.gameObject == null)
            {
                ghostsSeeTasksButton = createCustomToggle("Ghosts See Remaining Tasks: ",
                    ModpackPlugin.GhostsSeeTasks.Value, Vector3.right * xOffset,
                    (UnityEngine.Events.UnityAction) ghostsSeeTaskToggle, __instance);

                static void ghostsSeeTaskToggle()
                {
                    ModpackPlugin.GhostsSeeTasks.Value = !ModpackPlugin.GhostsSeeTasks.Value;
                    MapOptions.ghostsSeeTasks = ModpackPlugin.GhostsSeeTasks.Value;
                    updateToggle(ghostsSeeTasksButton, "Ghosts See Remaining Tasks: ",
                        ModpackPlugin.GhostsSeeTasks.Value);
                }
            }

            if (ghostsSeeRolesButton == null || ghostsSeeRolesButton.gameObject == null)
            {
                ghostsSeeRolesButton = createCustomToggle("Ghosts See Roles: ", ModpackPlugin.GhostsSeeRoles.Value,
                    new Vector2(-xOffset, yOffset), (UnityEngine.Events.UnityAction) ghostsSeeRolesToggle, __instance);

                static void ghostsSeeRolesToggle()
                {
                    ModpackPlugin.GhostsSeeRoles.Value = !ModpackPlugin.GhostsSeeRoles.Value;
                    MapOptions.ghostsSeeRoles = ModpackPlugin.GhostsSeeRoles.Value;
                    updateToggle(ghostsSeeRolesButton, "Ghosts See Roles: ", ModpackPlugin.GhostsSeeRoles.Value);
                }
            }

            if (ghostsSeeVotesButton == null || ghostsSeeVotesButton.gameObject == null)
            {
                ghostsSeeVotesButton = createCustomToggle("Ghosts See Votes: ", ModpackPlugin.GhostsSeeVotes.Value,
                    new Vector2(0, yOffset), (UnityEngine.Events.UnityAction) ghostsSeeVotesToggle, __instance);

                static void ghostsSeeVotesToggle()
                {
                    ModpackPlugin.GhostsSeeVotes.Value = !ModpackPlugin.GhostsSeeVotes.Value;
                    MapOptions.ghostsSeeVotes = ModpackPlugin.GhostsSeeVotes.Value;
                    updateToggle(ghostsSeeVotesButton, "Ghosts See Votes: ", ModpackPlugin.GhostsSeeVotes.Value);
                }
            }

            if (showRoleSummaryButton != null && showRoleSummaryButton.gameObject != null) return;
            showRoleSummaryButton = createCustomToggle("Role Summary: ", ModpackPlugin.ShowRoleSummary.Value,
                new Vector2(xOffset, yOffset), (UnityEngine.Events.UnityAction) showRoleSummaryToggle, __instance);

            static void showRoleSummaryToggle()
            {
                ModpackPlugin.ShowRoleSummary.Value = !ModpackPlugin.ShowRoleSummary.Value;
                MapOptions.showRoleSummary = ModpackPlugin.ShowRoleSummary.Value;
                updateToggle(showRoleSummaryButton, "Role Summary: ", ModpackPlugin.ShowRoleSummary.Value);
            }
        }
    }

    [HarmonyPatch(typeof(TextBoxTMP), nameof(TextBoxTMP.SetText))]
    public static class HiddenTextPatch
    {
        private static void Postfix(TextBoxTMP __instance)
        {
            var flag = ModpackPlugin.StreamerMode.Value && (__instance.name == "GameIdText" ||
                                                            __instance.name == "IpTextBox" ||
                                                            __instance.name == "PortTextBox");
            if (flag) __instance.outputText.text = new string('*', __instance.text.Length);
        }
    }
}