using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;
using HarmonyLib;
using SongOrganizer.Data;
using UnityEngine;
using UnityEngine.UI;

namespace SongOrganizer.Patches
{
    [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.sortTracks))]
    public class LevelSelectControllerSortTracksPatch : MonoBehaviour
    {
        static void Prefix(string sortcriteria)
        {
            Plugin.Options.SortMode.Value = sortcriteria;
        }
    }

    [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.updateGraph))]
    public class LevelSelectControllerUpdateGraphPatch : MonoBehaviour
    {
        static void Prefix(ref int ___lastindex)
        {
            ___lastindex = 0;
        }
    }

    [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Start))]
    public class LevelSelectControllerStartPatch : MonoBehaviour
    {
        static void Prefix(List<SingleTrackData> ___alltrackslist)
        {
            GlobalVariables.sortmode = Plugin.Options.SortMode.Value;
            if (GlobalVariables.levelselect_index >= ___alltrackslist.Count)
            {
                GlobalVariables.levelselect_index = 0;
            }
        }

        static void Postfix(LevelSelectController __instance, List<SingleTrackData> ___alltrackslist)
        {
            //__instance.sortdrop.SetActive(true);
            addOptions(__instance, ___alltrackslist);
            addTracks(___alltrackslist);
            filterTracks(__instance, ref ___alltrackslist);
        }

        private static void filterTracks(LevelSelectController __instance, ref List<SingleTrackData> ___alltrackslist)
        {
            List<string> newTrackrefs = new List<string>();
            List<string[]> newTracktitles = new List<string[]>();
            List<SingleTrackData> newTrackData = new List<SingleTrackData>();
            int newTrackIndex = 0;
            foreach (Track track in Plugin.TrackDict.Values)
            {
                if (!showTrack(track)) continue;
                track.trackindex = newTrackIndex;
                newTrackrefs.Add(track.trackref);
                newTracktitles.Add(new string[] { track.trackname_long, track.trackname_short, track.year, track.artist, track.genre, track.desc, track.difficulty.ToString(), track.length.ToString(), track.tempo.ToString() });
                newTrackData.Add(track);
                newTrackIndex++;
            }
            if (newTrackrefs.Count > 0)
            {
                GlobalVariables.data_trackrefs = newTrackrefs.ToArray();
                GlobalVariables.data_tracktitles = newTracktitles.ToArray();
                ___alltrackslist.Clear();
                ___alltrackslist.AddRange(newTrackData);
            }
            if (GlobalVariables.levelselect_index >= ___alltrackslist.Count)
            {
                GlobalVariables.levelselect_index = 0;
            }
            
            Plugin.Log.LogDebug($"Filter result: {___alltrackslist.Count} found of {Plugin.TrackDict.Count}");
            MethodInfo method = __instance.GetType().GetMethod("sortTracks",
    BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(__instance, new object[] { Plugin.Options.SortMode.Value.ToLower(), false });
        }

        private static bool showTrack(Track track)
        {
            return showDefaultCustom(track.custom)
                && showPlayedUnplayed(track.letterScore != "-")
                && showSRanks(track.letterScore == "S");
        }

        private static bool showDefaultCustom(bool custom)
        {
            if (Plugin.Options.ShowDefault.Value == Plugin.Options.ShowCustom.Value) return true;
            return Plugin.Options.ShowCustom.Value == custom;
        }

        private static bool showPlayedUnplayed(bool played)
        {
            if (Plugin.Options.ShowPlayed.Value == Plugin.Options.ShowUnplayed.Value) return true;
            return Plugin.Options.ShowPlayed.Value == played;
        }

        private static bool showSRanks(bool sRank)
        {
            if (Plugin.Options.ShowNotSRank.Value == Plugin.Options.ShowSRank.Value) return true;
            return Plugin.Options.ShowSRank.Value == sRank;
        }

        private static void addTracks(List<SingleTrackData> alltrackslist)
        {
            Plugin.Log.LogDebug($"Add tracks: {Plugin.TrackDict.Count} in dict, {alltrackslist.Count} total");
            var trackScores = GlobalVariables.localsave.data_trackscores
                .Where(i => i != null && i[0] != null)
                .GroupBy(i => i[0])
                .ToDictionary(i => i.Key, i => i.First());
            if (Plugin.TrackDict.Count == 0)
            { // add tracks to the dict
                foreach (var track in alltrackslist)
                {
                    Track newTrack = new Track(track);
                    newTrack.custom = Plugin.IsCustomTrack(track.trackref);
                    bool scoreFound = trackScores.TryGetValue(track.trackref, out string[] trackScore);
                    newTrack.letterScore = scoreFound ? trackScore[1] : "-";
                    newTrack.score = scoreFound ? int.Parse(trackScore[2]) : 0;
                    Plugin.TrackDict.TryAdd(track.trackref, newTrack);
                }
            }
            else
            { // update the track scores in the dict
                foreach (var track in Plugin.TrackDict.Values)
                {
                    bool scoreFound = trackScores.TryGetValue(track.trackref, out string[] trackScore);
                    track.letterScore = scoreFound ? trackScore[1] : "-";
                    track.score = scoreFound ? int.Parse(trackScore[2]) : 0;
                }
            }
        }

        // idk how these numbers work
        private static void addOptions(LevelSelectController __instance, List<SingleTrackData> ___alltrackslist)
        {
            GameObject face = GameObject.Find("MainCanvas/FullScreenPanel/sort-dropdown/face");
            RectTransform sortDropRectTransform = __instance.sortdrop.GetComponent<RectTransform>();
            RectTransform faceRectTransform = face.GetComponent<RectTransform>();
            int length = 250, y = -95;
            faceRectTransform.sizeDelta = new Vector2(180, length);
            sortDropRectTransform.sizeDelta = new Vector2(180, length);
            foreach (FilterOption filterOption in Enum.GetValues(typeof(FilterOption)))
            {
                Toggle toggle = CreateToggle(face, filterOption, new Vector2(0, y -= 30));
                ConfigEntry<bool> configEntry = GetConfigEntry(filterOption);
                if (configEntry == null) continue;
                toggle.isOn = configEntry.Value;
                toggle.onValueChanged.AddListener(b => {
                    configEntry.Value = b;
                    filterTracks(__instance, ref ___alltrackslist);
                });
            }
            foreach (var button in face.GetComponentsInChildren<Button>())
            {
                RectTransform t = button.GetComponent<RectTransform>();
                t.anchoredPosition = new Vector2(t.anchoredPosition.x, t.anchoredPosition.y + 60);
            }
        }

        private static ConfigEntry<bool> GetConfigEntry(FilterOption filterOption)
        {
            switch(filterOption)
            {
                case FilterOption.DEFAULT:
                    return Plugin.Options.ShowDefault;
                case FilterOption.CUSTOM:
                    return Plugin.Options.ShowCustom;
                case FilterOption.PLAYED:
                    return Plugin.Options.ShowPlayed;
                case FilterOption.UNPLAYED:
                    return Plugin.Options.ShowUnplayed;
                case FilterOption.NOT_S_RANK:
                    return Plugin.Options.ShowNotSRank;
                case FilterOption.S_RANK:
                    return Plugin.Options.ShowSRank;
            }
            return null;
        }

        private static Toggle CreateToggle(GameObject face, FilterOption filter, Vector2 position)
        {
            Toggle toggle = Instantiate(Plugin.Toggle, face.transform.transform);
            string name = Enums.GetDescription(filter);
            toggle.name = name;
            var rectTransform = toggle.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = new Vector2(180, 30);

            Text text = toggle.GetComponentInChildren<Text>();
            text.text = $"{name} tracks";
            text.fontSize = 13;
            text.color = new Color(0.024f, 0.294f, 0.302f);

            var images = toggle.GetComponentsInChildren<Image>();
            foreach (var image in images)
            {
                if (image.name == "Background")
                {
                    image.rectTransform.sizeDelta = new Vector2(0, 30);
                    image.color = new Color(.15294117647058823529411764705882f, 1, 1);
                }
                else if (image.name == "Checkmark")
                {
                    image.rectTransform.anchoredPosition = new Vector2(-5, 0);
                    image.rectTransform.sizeDelta = new Vector2(20, 20);
                }
            }
            return toggle;
        }
    }
}