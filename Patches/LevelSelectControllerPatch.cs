using System.Collections.Generic;
using HarmonyLib;

namespace SongOrganizer.Patches
{
    [HarmonyPatch(typeof(LevelSelectController))]
    public class LevelSelectControllerPatch
    {
        // Filter to show only custom songs
        [HarmonyPatch(nameof(LevelSelectController.Start))]
        static void Prefix(LevelSelectController __instance)
        {
            List<string> newTrackrefs = new List<string>();
            List<string[]> newTrackTitles = new List<string[]>();
            for (int i = 0; i < GlobalVariables.data_tracktitles.Length; i++)
            {
                var track = GlobalVariables.data_tracktitles[i];
                string trackref = GlobalVariables.data_trackrefs[i];
                if (Plugin.IsCustomTrack(trackref))
                {
                    newTrackTitles.Add(track);
                    newTrackrefs.Add(trackref);
                }
            }
            if (newTrackrefs.Count > 0 && newTrackTitles.Count > 0)
            {
                GlobalVariables.data_trackrefs = newTrackrefs.ToArray();
                GlobalVariables.data_tracktitles = newTrackTitles.ToArray();
            }
        }
    }
}