using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using HarmonyLib;
using SongOrganizer.Data;
using UnityEngine;
using UnityEngine.UI;

namespace SongOrganizer.Patches;

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
    static void Prefix(ref int ___lastindex, ref int[][] ___songgraphs)
    {
        ___lastindex = ___lastindex >= ___songgraphs.Length ? 0 : ___lastindex;
    }
}

[HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.populateSongNames))]
public class LevelSelectControllerPopulateSongNamesPatch : MonoBehaviour
{
    static void Prefix(ref int ___songindex)
    {
        GlobalVariables.levelselect_index = ___songindex;
        Plugin.Options.LastIndex.Value = ___songindex;
    }
}

[HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.clickRandomTrack))]
public class LevelSelectControllerClickRandomTrackPatch : MonoBehaviour
{
    static void Prefix()
    {
        Array.ForEach(Plugin.DeleteButtons, i => i.interactable = false);
    }
}

[HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.doneRandomizing))]
public class LevelSelectControllerDoneRandomizingPatch : MonoBehaviour
{
    static void Postfix()
    {
        Array.ForEach(Plugin.DeleteButtons, i => i.interactable = true);
    }
}

[HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Update))]
public class LevelSelectControllerUpdatePatch : MonoBehaviour
{
    static void Postfix(LevelSelectController __instance, ref int ___songindex, ref List<SingleTrackData> ___alltrackslist)
    {
        foreach (var keyCode in Plugin.KeyCodes)
        {
            if (Input.GetKeyDown(keyCode))
            {
                int increment = findSong((char)keyCode, ___songindex, ___alltrackslist);
                if (increment >= 0)
                {
                    __instance.advanceSongs(increment, true);
                }
            }
        }
    }

    private static int findSong(char key, int ___songindex, List<SingleTrackData> ___alltrackslist)
    {
        int increment = 1;
        for (int i = ___songindex + 1; i < ___alltrackslist.Count; i++, increment++)
        {
            if (___alltrackslist[i].trackname_short.ToLower()[0] == key)
            {
                return increment;
            }
        }
        for (int i = 0; i < ___songindex; i++, increment++)
        {
            if (___alltrackslist[i].trackname_short.ToLower()[0] == key)
            {
                return increment;
            }
        }
        return -1;
    }
}

[HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Start))]
public class LevelSelectControllerStartPatch : MonoBehaviour
{
    private const string LEADERBOARD_PATH = "MainCanvas/FullScreenPanel/Leaderboard";
    private const string SORT_DROPDROPDOWN_PATH = "MainCanvas/FullScreenPanel/sort-dropdown/face";

    static void Prefix()
    {
        GlobalVariables.sortmode = Plugin.Options.SortMode.Value;
        GlobalVariables.levelselect_index = Plugin.Options.LastIndex.Value;
        if (GlobalVariables.levelselect_index >= GlobalVariables.data_tracktitles.Length)
        {
            GlobalVariables.levelselect_index = 0;
        }
    }

    static void Postfix(LevelSelectController __instance, List<SingleTrackData> ___alltrackslist, ref int[][] ___songgraphs)
    {
        addOptions(__instance, ___alltrackslist);
        addTracks(___alltrackslist);
        filterTracks(__instance, ref ___alltrackslist);
        addDeleteButtons(__instance, ___alltrackslist);
        initializeSongGraphs(ref ___songgraphs);
    }

    private static void initializeSongGraphs(ref int[][] ___songgraphs)
    {
        int totalTracks = Math.Max(GlobalVariables.data_tracktitles.Length, Plugin.TrackDict.Count);
        ___songgraphs = new int[totalTracks][];
        for (int i = 0; i < ___songgraphs.Length; i++)
        {
            ___songgraphs[i] = new int[Plugin.TRACK_SCORE_LENGTH];
            for (int j = 0; j < ___songgraphs[i].Length; j++)
            {
                ___songgraphs[i][j] = Mathf.FloorToInt(UnityEngine.Random.value * 100f);
            }
        }
    }

    private static void filterTracks(LevelSelectController __instance, ref List<SingleTrackData> ___alltrackslist)
    {
        List<string[]> newTracktitles = new List<string[]>();
        List<SingleTrackData> newTrackData = new List<SingleTrackData>();
        int newTrackIndex = 0;
        foreach (Track track in Plugin.TrackDict.Values)
        {
            if (!showTrack(track)) continue;
            track.trackindex = newTrackIndex;
            newTracktitles.Add(new string[] {
                track.trackname_long,
                track.trackname_short,
                track.trackref,
                track.year,
                track.artist,
                track.genre,
                track.desc,
                track.difficulty.ToString(),
                track.length.ToString(),
                track.tempo.ToString()
            });
            newTrackData.Add(track);
            newTrackIndex++;
        }
        if (newTracktitles.Count > 0)
        {
            GlobalVariables.data_tracktitles = newTracktitles.ToArray();
            ___alltrackslist.Clear();
            ___alltrackslist.AddRange(newTrackData);
        }
        if (GlobalVariables.levelselect_index >= GlobalVariables.data_tracktitles.Length)
        {
            GlobalVariables.levelselect_index = 0;
        }
        Plugin.Log.LogDebug($"Filter result: {___alltrackslist.Count} found of {Plugin.TrackDict.Count}");

        __instance.sortTracks(Plugin.Options.SortMode.Value.ToLower(), false);
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
                setScores(newTrack, track.trackref, trackScores);
                Plugin.TrackDict.TryAdd(track.trackref, newTrack);
            }
        }
        else
        { // update the track scores in the dict
            foreach (var track in Plugin.TrackDict.Values)
            {
                setScores(track, track.trackref, trackScores);
            }
        }
    }

    private static void setScores(Track newTrack, string trackref, Dictionary<string, string[]> trackScores)
    {
        bool scoreFound = trackScores.TryGetValue(trackref, out string[] trackScore);
        newTrack.letterScore = scoreFound ? trackScore[1] : "-";
        newTrack.scores = scoreFound ? trackScore.Skip(2).Select(int.Parse).ToArray() : new int[Plugin.TRACK_SCORE_LENGTH];
    }

    private static void addDeleteButtons(LevelSelectController __instance, List<SingleTrackData> ___alltrackslist)
    {
        GameObject leaderboard = GameObject.Find(LEADERBOARD_PATH);
        var nums = Enumerable.Range(1, Plugin.TRACK_SCORE_LENGTH).Select(i => i.ToString());
        Text[] leaderboardText = leaderboard.GetComponentsInChildren<Text>();
        Text[] scoreNums = leaderboardText.Where(x => nums.Contains(x.name))
            .OrderBy(x => x.name).ToArray();
        for (int i = 1; i <= Plugin.TRACK_SCORE_LENGTH; i++)
        {
            Plugin.DeleteButtons[i] = deleteSingleScoreButton(__instance, scoreNums[i - 1], ___alltrackslist);
        }
        Plugin.DeleteButtons[0] = deleteTrackScoresButton(__instance, scoreNums[0], ___alltrackslist);
    }

    private static Button deleteTrackScoresButton(LevelSelectController __instance, Text scoreText, List<SingleTrackData> ___alltrackslist)
    {
        Button deleteButton = addDeleteButton(scoreText);
        var deleteRectTransform = deleteButton.GetComponent<RectTransform>();
        deleteRectTransform.localPosition = new Vector2(-10, 20);
        deleteButton.name = $"delete track scores";
        deleteButton.onClick.AddListener(delegate { delete(__instance, ___alltrackslist); });
        return deleteButton;
    }

    private static Button deleteSingleScoreButton(LevelSelectController __instance, Text scoreText, List<SingleTrackData> ___alltrackslist)
    {
        Button deleteButton = addDeleteButton(scoreText);
        int index = int.Parse(scoreText.name);
        deleteButton.name = $"delete score {index}";
        deleteButton.onClick.AddListener(delegate { delete(__instance, index, ___alltrackslist); });
        return deleteButton;
    }

    private static Button addDeleteButton(Text scoreText)
    {
        var scoreRectTransform = scoreText.GetComponent<RectTransform>();
        var deleteButton = Instantiate(Plugin.Button, scoreRectTransform);
        var deleteRectTransform = deleteButton.GetComponent<RectTransform>();
        deleteButton.onClick.RemoveAllListeners();

        deleteRectTransform.sizeDelta = new Vector2(12, 12);
        deleteRectTransform.position = scoreRectTransform.position;
        deleteRectTransform.anchoredPosition = new Vector2(-20, 10);

        var deleteText = deleteButton.GetComponentInChildren<Text>();
        deleteText.text = "X";
        deleteText.fontSize = 8;
        return deleteButton;
    }

    static void delete(LevelSelectController __instance, List<SingleTrackData> ___alltrackslist)
    {
        string trackref = ___alltrackslist[GlobalVariables.levelselect_index].trackref;
        string shortname = ___alltrackslist[GlobalVariables.levelselect_index].trackname_short;
        Plugin.Log.LogInfo($"Deleting all scores for {shortname}");

        string[] trackscores = GlobalVariables.localsave.data_trackscores
            .Where(i => i != null && i[0] == trackref).FirstOrDefault();
        if (trackscores == null || trackscores[1] == "-")
        {
            Plugin.Log.LogDebug($"No score to delete for {shortname}");
            return;
        }
        trackscores[1] = "-";
        for (int i = 2; i < trackscores.Length; i++)
        {
            trackscores[i] = "0";
        }

        SaverLoader.updateSavedGame();
        __instance.populateSongNames(true);
    }

    static void delete(LevelSelectController __instance, int index, List<SingleTrackData> ___alltrackslist)
    {
        string trackref = ___alltrackslist[GlobalVariables.levelselect_index].trackref;
        string shortname = ___alltrackslist[GlobalVariables.levelselect_index].trackname_short;
        Plugin.Log.LogInfo($"Deleting {shortname} - {index}");

        string[] trackscores = GlobalVariables.localsave.data_trackscores
            .Where(i => i != null && i[0] == trackref).FirstOrDefault();
        if (trackscores == null || int.Parse(trackscores[index + 1]) == 0)
        {
            Plugin.Log.LogDebug($"No score to delete for {shortname} - {index}");
            return;
        }

        var newTrackScores = new List<string>();
        for (int i = 2; i < trackscores.Length; i++)
        {
            if (i != index + 1)
            {
                newTrackScores.Add(trackscores[i]);
            }
        }
        newTrackScores.Add("0");
        for (int i = 2; i < trackscores.Length; i++)
        {
            trackscores[i] = newTrackScores[i - 2];
        }
        trackscores[1] = Utils.getBestLetterScore(trackref, int.Parse(trackscores[2]));

        SaverLoader.updateSavedGame();
        __instance.populateSongNames(true);
    }

    // idk how these numbers work
    private static void addOptions(LevelSelectController __instance, List<SingleTrackData> ___alltrackslist)
    {
        GameObject face = GameObject.Find(SORT_DROPDROPDOWN_PATH);
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
