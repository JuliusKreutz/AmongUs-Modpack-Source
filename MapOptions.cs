using Il2CppSystem.Collections.Generic;
using UnityEngine;

namespace Modpack
{
    internal static class MapOptions
    {
        // Set values
        public static int maxNumberOfMeetings = 10;
        public static bool blockSkippingInEmergencyMeetings;
        public static bool noVoteIsSelfVote;
        public static bool hidePlayerNames;
        public static bool ghostsSeeRoles = true;
        public static bool ghostsSeeTasks = true;

        // Updating values
        public static int meetingsCount;
        public static List<SurvCamera> camerasToAdd = new List<SurvCamera>();
        public static List<Vent> ventsToSeal = new List<Vent>();

        public static void clearAndReloadMapOptions()
        {
            meetingsCount = 0;
            camerasToAdd = new List<SurvCamera>();
            ventsToSeal = new List<Vent>();

            maxNumberOfMeetings = Mathf.RoundToInt(CustomOptionHolder.maxNumberOfMeetings.getSelection());
            blockSkippingInEmergencyMeetings = CustomOptionHolder.blockSkippingInEmergencyMeetings.getBool();
            noVoteIsSelfVote = CustomOptionHolder.noVoteIsSelfVote.getBool();
            hidePlayerNames = CustomOptionHolder.hidePlayerNames.getBool();
            ghostsSeeRoles = ModpackPlugin.GhostsSeeRoles.Value;
            ghostsSeeTasks = ModpackPlugin.GhostsSeeTasks.Value;
        }
    }
}