using UnityEngine;

namespace Modpack{
    internal static class MapOptions {
        // Set values
        public static int maxNumberOfMeetings = 10;
        public static bool blockSkippingInEmergencyMeetings;
        public static bool noVoteIsSelfVote;
        public static bool hidePlayerNames;
        public static bool showGhostInfo = true;

        // Updating values
        public static int meetingsCount;

        public static void clearAndReloadMapOptions() {
            meetingsCount = 0;

            maxNumberOfMeetings = Mathf.RoundToInt(CustomOptionHolder.maxNumberOfMeetings.getSelection());
            blockSkippingInEmergencyMeetings = CustomOptionHolder.blockSkippingInEmergencyMeetings.getBool();
            noVoteIsSelfVote = CustomOptionHolder.noVoteIsSelfVote.getBool();
            hidePlayerNames = CustomOptionHolder.hidePlayerNames.getBool();
            showGhostInfo = CustomOptionHolder.showGhostInfo.getBool();
        }
    }
} 