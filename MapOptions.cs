using UnityEngine;

namespace Modpack
{
    static class MapOptions
    {
        // Set values
        public static int maxNumberOfMeetings = 10;
        public static bool allowSkipOnEmergencyMeetings = true;

        // Updating values
        public static int meetingsCount = 0;

        public static void clearAndReloadMapOptions()
        {
            meetingsCount = 0;

            maxNumberOfMeetings = Mathf.RoundToInt(CustomOptionHolder.maxNumberOfMeetings.getSelection());
            allowSkipOnEmergencyMeetings = CustomOptionHolder.allowSkipOnEmergencyMeetings.getBool();
        }
    }
}