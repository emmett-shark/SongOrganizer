using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using SongOrganizer.Data;
using SongOrganizer.Utils;
using TrombLoader.Helpers;
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

[HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.searchForSongName))]
public class LevelSelectControllerUpdatePatch : MonoBehaviour
{
    static bool Prefix(string startingletter, LevelSelectController __instance, ref int ___songindex, ref List<SingleTrackData> ___alltrackslist)
    {
        if (Plugin.SearchInput.isFocused) return false;
        char key = startingletter.ToLower()[0];
        int increment = 1;
        for (int i = ___songindex + 1; i < ___alltrackslist.Count; i++, increment++)
        {
            if (___alltrackslist[i].trackname_short.ToLower()[0] == key)
            {
                __instance.advanceSongs(increment, true);
                return false;
            }
        }
        for (int i = 0; i < ___songindex; i++, increment++)
        {
            if (___alltrackslist[i].trackname_short.ToLower()[0] == key)
            {
                __instance.advanceSongs(increment, true);
                return false;
            }
        }
        return false;
    }
}

[HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Start))]
public class LevelSelectControllerStartPatch : MonoBehaviour
{
    private const string FULLSCREENPANEL = "MainCanvas/FullScreenPanel/";
    private const string LEADERBOARD_PATH = $"{FULLSCREENPANEL}Leaderboard";
    private const string SORT_DROPDROPDOWN_PATH = $"{FULLSCREENPANEL}sort-dropdown/face";
    private const string SCROLL_SPEED_PATH = $"{FULLSCREENPANEL}ScrollSpeedShad/ScrollSpeed";
    private const string TITLE_BAR = $"{FULLSCREENPANEL}title bar";

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
        AddOptions(__instance, ___alltrackslist);
        AddTracks(___alltrackslist);
        AddSearchBar(__instance, ___alltrackslist);
        FilterTracks(__instance, ref ___alltrackslist);
        AddDeleteButtons(__instance, ___alltrackslist);
        InitializeSongGraphs(ref ___songgraphs);
    }

    private static void SearchListener(string val, LevelSelectController __instance, ref List<SingleTrackData> ___alltrackslist)
    {
        Plugin.Log.LogDebug($"search: {val}");
        Plugin.Options.SearchValue.Value = val;
        FilterTracks(__instance, ref ___alltrackslist);
    }

    private static void ToggleListener(ConfigEntry<bool> configEntry, bool b, LevelSelectController __instance, ref List<SingleTrackData> ___alltrackslist)
    {
        configEntry.Value = b;
        FilterTracks(__instance,  ref ___alltrackslist);
    }

    #region AddOptions
    // idk how these numbers work
    private static void AddOptions(LevelSelectController __instance, List<SingleTrackData> ___alltrackslist)
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
            toggle.onValueChanged.AddListener(b => ToggleListener(configEntry, b, __instance, ref ___alltrackslist));
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
            case FilterOption.UNRATED:
                return Plugin.Options.ShowUnrated;
            case FilterOption.RATED:
                return Plugin.Options.ShowRated;
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
    #endregion

    #region AddTracks
    private static void AddTracks(List<SingleTrackData> alltrackslist)
    {
        Plugin.Log.LogDebug($"Add tracks: {Plugin.TrackDict.Count} in dict, {alltrackslist.Count} total");
        var ratedTracks = Helpers.GetRatedTracks();
        var missingRatedTrackNames = new HashSet<string>(ratedTracks.Values);
        var trackScores = GlobalVariables.localsave.data_trackscores
            .Where(i => i != null && i[0] != null)
            .GroupBy(i => i[0])
            .ToDictionary(i => i.Key, i => i.First());
        if (Plugin.TrackDict.Count == 0)
        { // add tracks to the dict
            foreach (var track in alltrackslist)
            {
                Track newTrack = new Track(track);
                newTrack.custom = Globals.IsCustomTrack(track.trackref);
                newTrack.rated = ratedTracks.ContainsKey(track.trackref);
                SetScores(newTrack, track.trackref, trackScores);
                Plugin.TrackDict.TryAdd(track.trackref, newTrack);
            }
        }
        else
        { // update the track scores in the dict
            foreach (var track in Plugin.TrackDict.Values)
            {
                SetScores(track, track.trackref, trackScores);
            }
        }
        foreach (var track in Plugin.TrackDict.Values)
        {
            track.rated = ratedTracks.ContainsKey(track.trackref);
            missingRatedTrackNames.Remove(track.trackname_short);
        }
        Plugin.Log.LogInfo($"Rated tracks: {ratedTracks.Count} total. {missingRatedTrackNames.Count} missing: [{string.Join(", ", missingRatedTrackNames)}]");
    }

    private static void SetScores(Track newTrack, string trackref, Dictionary<string, string[]> trackScores)
    {
        bool scoreFound = trackScores.TryGetValue(trackref, out string[] trackScore);
        newTrack.letterScore = scoreFound ? trackScore[1] : "-";
        newTrack.scores = scoreFound ? trackScore.Skip(2).Select(int.Parse).ToArray() : new int[Plugin.TRACK_SCORE_LENGTH];
    }
    #endregion

    #region FilterTracks
    private static void FilterTracks(LevelSelectController __instance, ref List<SingleTrackData> ___alltrackslist)
    {
        List<string[]> newTracktitles = new List<string[]>();
        List<SingleTrackData> newTrackData = new List<SingleTrackData>();
        int newTrackIndex = 0;
        foreach (Track track in Plugin.TrackDict.Values)
        {
            if (!ShowTrack(track)) continue;
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

    private static bool ShowTrack(Track track)
    {
        return ShowTrack(Plugin.Options.ShowCustom.Value, Plugin.Options.ShowDefault.Value, track.custom)
            && ShowTrack(Plugin.Options.ShowPlayed.Value, Plugin.Options.ShowUnplayed.Value, track.letterScore != "-")
            && ShowTrack(Plugin.Options.ShowSRank.Value, Plugin.Options.ShowNotSRank.Value, track.letterScore == "S")
            && ShowTrack(Plugin.Options.ShowRated.Value, Plugin.Options.ShowUnrated.Value, track.rated)
            && ShowTrack(Plugin.Options.SearchValue.Value, track);
    }

    private static bool ShowTrack(bool optionToggle, bool oppositeOptionToggle, bool option) =>
        optionToggle == oppositeOptionToggle ? true : optionToggle == option;

    private static bool ShowTrack(string searchVal, Track track)
    {
        if (searchVal.IsNullOrWhiteSpace()) return true;
        string search = searchVal.ToLower().Trim();
        return track.trackname_long.ToLower().Contains(search) 
            || track.trackname_short.ToLower().Contains(search)
            || track.artist.ToLower().Contains(search)
            || track.desc.ToLower().Contains(search);
    }
    #endregion

    #region AddDeleteButtons
    private static void AddDeleteButtons(LevelSelectController __instance, List<SingleTrackData> ___alltrackslist)
    {
        GameObject leaderboard = GameObject.Find(LEADERBOARD_PATH);
        Destroy(leaderboard.GetComponentsInChildren<Image>()?.Where(i => i.name == "#1 star").FirstOrDefault());

        var nums = Enumerable.Range(1, Plugin.TRACK_SCORE_LENGTH).Select(i => i.ToString());
        Text[] leaderboardText = leaderboard.GetComponentsInChildren<Text>();
        Text[] scoreNums = leaderboardText.Where(x => nums.Contains(x.name))
            .OrderBy(x => x.name).ToArray();
        for (int i = 1; i <= Plugin.TRACK_SCORE_LENGTH; i++)
        {
            Plugin.DeleteButtons[i] = DeleteSingleScoreButton(__instance, scoreNums[i - 1], ___alltrackslist);
        }
        Plugin.DeleteButtons[0] = DeleteTrackScoresButton(__instance, scoreNums[0], ___alltrackslist);
    }

    private static Button DeleteTrackScoresButton(LevelSelectController __instance, Text scoreText, List<SingleTrackData> ___alltrackslist)
    {
        Button deleteButton = AddDeleteButton(scoreText);
        var deleteRectTransform = deleteButton.GetComponent<RectTransform>();
        deleteRectTransform.localPosition = new Vector2(-25, 30);
        deleteButton.name = $"delete track scores";
        deleteButton.onClick.AddListener(delegate { Delete(__instance, ___alltrackslist); });
        return deleteButton;
    }

    private static Button DeleteSingleScoreButton(LevelSelectController __instance, Text scoreText, List<SingleTrackData> ___alltrackslist)
    {
        Button deleteButton = AddDeleteButton(scoreText);
        int index = int.Parse(scoreText.name);
        deleteButton.name = $"delete score {index}";
        deleteButton.onClick.AddListener(delegate { Delete(__instance, index, ___alltrackslist); });
        return deleteButton;
    }

    private static Button AddDeleteButton(Text scoreText)
    {
        var scoreRectTransform = scoreText.GetComponent<RectTransform>();
        var deleteButton = Instantiate(Plugin.Button, scoreRectTransform);
        var deleteRectTransform = deleteButton.GetComponent<RectTransform>();
        deleteButton.onClick.RemoveAllListeners();

        deleteRectTransform.sizeDelta = new Vector2(18, 18);
        deleteRectTransform.position = scoreRectTransform.position;
        deleteRectTransform.anchoredPosition = new Vector2(-25, 5);

        var deleteText = deleteButton.GetComponentInChildren<Text>();
        deleteText.text = "X";
        deleteText.fontSize = 15;
        return deleteButton;
    }

    static void Delete(LevelSelectController __instance, List<SingleTrackData> ___alltrackslist)
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

    static void Delete(LevelSelectController __instance, int index, List<SingleTrackData> ___alltrackslist)
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
        trackscores[1] = Helpers.GetBestLetterScore(trackref, int.Parse(trackscores[2]));

        SaverLoader.updateSavedGame();
        __instance.populateSongNames(true);
    }
    #endregion

    #region InitializeSongGraphs
    private static void InitializeSongGraphs(ref int[][] ___songgraphs)
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
    #endregion

    #region AddSearchBar
    private static void AddSearchBar(LevelSelectController __instance, List<SingleTrackData> ___alltrackslist)
    {
        var fullscreenPanel = GameObject.Find(FULLSCREENPANEL);

        var titleBar = Instantiate(GameObject.Find(TITLE_BAR), fullscreenPanel.transform);
        titleBar.name = "search underline";
        var titleRectTransform = titleBar.GetComponent<RectTransform>();
        titleRectTransform.anchoredPosition = new Vector2(145, -30);
        titleRectTransform.sizeDelta = new Vector2(275, 200);

        var searchBar = Instantiate(GameObject.Find(SCROLL_SPEED_PATH), fullscreenPanel.transform);
        var searchRectTransform = searchBar.GetComponent<RectTransform>();
        searchRectTransform.anchoredPosition = new Vector2(-130, 200);
        searchRectTransform.sizeDelta = new Vector2(250, 14);

        var searchText = searchBar.transform.GetComponent<Text>();
        searchText.text = Plugin.Options.SearchValue.Value;

        Plugin.SearchInput = searchBar.AddComponent<InputField>();
        Plugin.SearchInput.textComponent = searchText;
        Plugin.SearchInput.name = "search";
        Plugin.SearchInput.onValueChanged.AddListener(val => SearchListener(val, __instance, ref ___alltrackslist));

        Destroy(__instance.scenetitle);
    }
    #endregion
}
