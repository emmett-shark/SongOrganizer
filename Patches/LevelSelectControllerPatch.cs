using System;
using System.Collections.Generic;
using System.Linq;
using BaboonAPI.Hooks.Tracks;
using BepInEx.Configuration;
using HarmonyLib;
using SongOrganizer.Data;
using SongOrganizer.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace SongOrganizer.Patches;

[HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.sortTracks))]
public class LevelSelectControllerSortTracksPatch : MonoBehaviour
{
    static bool Prefix(LevelSelectController __instance, string sortcriteria, bool anim)
    {
        Plugin.Options.SortMode.Value = sortcriteria;
        GlobalVariables.sortmode = sortcriteria;
        __instance.sortlabel.text = "Sort: " + sortcriteria;
        if (anim)
        {
            __instance.clipPlayer.cancelCrossfades();
            __instance.doSfx(__instance.sfx_click);
            __instance.closeSortDropdown();
            __instance.btnspanel.transform.localScale = new Vector3(1f / 1000f, 1f, 1f);
            LeanTween.scaleX(__instance.btnspanel, 1f, 0.2f).setEaseOutQuart();
        }
        if (sortcriteria == "default")
            __instance.alltrackslist.Sort((t1, t2) => t1.trackindex.CompareTo(t2.trackindex));
        else if (sortcriteria == "difficulty")
            __instance.alltrackslist.Sort((t1, t2) => t1.difficulty.CompareTo(t2.difficulty));
        else if (sortcriteria == "alpha")
            __instance.alltrackslist.Sort((t1, t2) => t1.trackname_short == null ? -1 : t1.trackname_short.Trim().CompareTo(t2.trackname_short.Trim()));
        else if (sortcriteria == "long name")
            __instance.alltrackslist.Sort((t1, t2) => t1.trackname_long == null ? -1 : t1.trackname_long.Trim().CompareTo(t2.trackname_long.Trim()));
        else if (sortcriteria == "length")
            __instance.alltrackslist.Sort((t1, t2) => t1.length.CompareTo(t2.length));
        else if (sortcriteria == "artist")
            __instance.alltrackslist.Sort((t1, t2) => t1.artist == null ? -1 : t1.artist.Trim().CompareTo(t2.artist.Trim()));
        __instance.songindex = !anim ? GlobalVariables.levelselect_index : 0;
        __instance.populateSongNames(true);
        return false;
    }
}

[HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.updateGraph))]
public class LevelSelectControllerUpdateGraphPatch : MonoBehaviour
{
    static bool Prefix(LevelSelectController __instance)
    {
        for (int i = 0; i < 5; i++)
        {
            __instance.graphline.SetPosition(i, __instance.getGraphVector(i, Mathf.FloorToInt(UnityEngine.Random.value * 100f)));
        }
        return false;
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
public class LevelSelectControllerSearchFirstLetterPatch : MonoBehaviour
{
    static bool Prefix(string startingletter) => false;
}

[HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Update))]
public class LevelSelectControllerUpdatePatch : MonoBehaviour
{
    static void Postfix(LevelSelectController __instance, ref int ___songindex, ref List<SingleTrackData> ___alltrackslist)
    {
        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.F))
        {
            Plugin.SearchInput.Select();
        }
        else if (!Plugin.SearchInput.isFocused && Input.anyKeyDown && Input.inputString.Length > 0)
        {
            char key = Input.inputString.ToLower()[0];
            int increment = 1;
            for (int i = ___songindex + 1; i < ___alltrackslist.Count; i++, increment++)
            {
                if (___alltrackslist[i].trackname_short.ToLower().Trim()[0] == key)
                {
                    __instance.advanceSongs(increment, true);
                    return;
                }
            }
            for (int i = 0; i < ___songindex; i++, increment++)
            {
                if (___alltrackslist[i].trackname_short.ToLower().Trim()[0] == key)
                {
                    __instance.advanceSongs(increment, true);
                    return;
                }
            }
        }
    }
}

[HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.clickBack))]
public class LevelSelectControllerBackPatch : MonoBehaviour
{
    static void Postfix() => Plugin.UnloadModule();
}

[HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.clickPlay))]
public class LevelSelectControllerPlayPatch : MonoBehaviour
{
    static void Postfix() => Plugin.UnloadModule();
}

[HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Start))]
public class LevelSelectControllerStartPatch : MonoBehaviour
{
    private const string FULLSCREENPANEL = "MainCanvas/FullScreenPanel/";
    private const string LEADERBOARD_PATH = $"{FULLSCREENPANEL}Leaderboard";
    private const string SORT_DROPDROPDOWN_PATH = $"{FULLSCREENPANEL}sort-dropdown/face";
    private const string SORT_BUTTON_PATH = $"{SORT_DROPDROPDOWN_PATH}/btn_sort_length";
    private const string COMPOSER_NAME_PATH = $"{FULLSCREENPANEL}capsules/composername";
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

    static void Postfix(LevelSelectController __instance, List<SingleTrackData> ___alltrackslist)
    {
        var ratedTracks = TootTally.ReadRatedTracks();
        if (Plugin.TrackLoaded == null)
        {
            Plugin.TrackLoaded = new TrackLoaded(__instance, ratedTracks);
            TracksLoadedEvent.EVENT.Register(Plugin.TrackLoaded);
            Plugin.TrackLoaded.OnTracksLoaded(null);
        }

        GameObject leaderboard = GameObject.Find(LEADERBOARD_PATH);

        AddOptions(__instance);
        AddSearchBar(__instance);
        AddDeleteButtons(__instance, ___alltrackslist, leaderboard);
        AddStars(__instance, leaderboard);
    }

    #region AddStars
    private static void AddStars(LevelSelectController __instance, GameObject leaderboard)
    {
        var oldStar = leaderboard.GetComponentsInChildren<Image>()?.Where(i => i.name == "#1 star").FirstOrDefault();
        GameObject face = GameObject.Find(SORT_DROPDROPDOWN_PATH);
        RectTransform sortDropRectTransform = __instance.sortdrop.GetComponent<RectTransform>();
        //var star = Instantiate(oldStar, sortDropRectTransform);
        Destroy(oldStar);
    }
    #endregion

    #region AddOptions
    // idk how these numbers work
    private static void AddOptions(LevelSelectController __instance)
    {
        GameObject face = GameObject.Find(SORT_DROPDROPDOWN_PATH);
        RectTransform sortDropRectTransform = __instance.sortdrop.GetComponent<RectTransform>();
        RectTransform faceRectTransform = face.GetComponent<RectTransform>();
        int length = 250;
        faceRectTransform.sizeDelta = new Vector2(180, length);
        sortDropRectTransform.sizeDelta = new Vector2(180, length);
        CreateSortOption(__instance, face, "artist", -75);
        CreateSortOption(__instance, face, "long name", -105);

        int filterOffset = -155;
        foreach (FilterOption filterOption in Enum.GetValues(typeof(FilterOption)))
        {
            Toggle filter = CreateFilterOption(face, filterOption, new Vector2(0, filterOffset -= 30));
            ConfigEntry<bool> configEntry = GetConfigEntry(filterOption);
            if (configEntry == null) continue;
            filter.isOn = configEntry.Value;
            filter.onValueChanged.AddListener(b => ToggleListener(configEntry, b, __instance));
        }
        foreach (var button in face.GetComponentsInChildren<Button>())
        {
            RectTransform t = button.GetComponent<RectTransform>();
            t.anchoredPosition = new Vector2(t.anchoredPosition.x, t.anchoredPosition.y + 60);
        }
    }

    private static void ToggleListener(ConfigEntry<bool> configEntry, bool b, LevelSelectController __instance)
    {
        configEntry.Value = b;
        TrackLoaded.FilterTracks(__instance);
    }

    private static ConfigEntry<bool> GetConfigEntry(FilterOption filterOption)
    {
        switch (filterOption)
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

    private static void CreateSortOption(LevelSelectController __instance, GameObject face, string sortOption, float y)
    {
        GameObject sortObject = GameObject.Find(SORT_BUTTON_PATH);
        var sort = Instantiate(sortObject.GetComponent<Button>(), face.transform.transform);
        sort.name = sortOption;
        var rectTransform = sort.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(0, y);

        Text text = sort.GetComponentInChildren<Text>();
        text.text = sortOption;
        Button sortButton = sort.GetComponent<Button>();
        sortButton.onClick.RemoveAllListeners();
        sortButton.onClick.AddListener(() => __instance.sortTracks(sortOption, true));
    }

    private static Toggle CreateFilterOption(GameObject face, FilterOption filter, Vector2 position)
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

    #region AddDeleteButtons
    private static void AddDeleteButtons(LevelSelectController __instance, List<SingleTrackData> ___alltrackslist, GameObject leaderboard)
    {
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
        deleteButton.onClick.AddListener(() => Delete(__instance, ___alltrackslist));
        return deleteButton;
    }

    private static Button DeleteSingleScoreButton(LevelSelectController __instance, Text scoreText, List<SingleTrackData> ___alltrackslist)
    {
        Button deleteButton = AddDeleteButton(scoreText);
        int index = int.Parse(scoreText.name);
        deleteButton.name = $"delete score {index}";
        deleteButton.onClick.AddListener(() => Delete(__instance, index, ___alltrackslist));
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

    #region AddSearchBar
    private static void AddSearchBar(LevelSelectController __instance)
    {
        var fullscreenPanel = GameObject.Find(FULLSCREENPANEL);

        var titleBar = Instantiate(GameObject.Find(TITLE_BAR), fullscreenPanel.transform);
        titleBar.name = "search underline";
        var titleRectTransform = titleBar.GetComponent<RectTransform>();
        titleRectTransform.anchoredPosition = new Vector2(145, -30);
        titleRectTransform.sizeDelta = new Vector2(275, 200);

        var searchBar = Instantiate(GameObject.Find(COMPOSER_NAME_PATH), fullscreenPanel.transform);
        var searchRectTransform = searchBar.GetComponent<RectTransform>();
        searchRectTransform.anchoredPosition = new Vector2(-130, 200);
        searchRectTransform.sizeDelta = new Vector2(250, 14);
        searchRectTransform.rotation = Quaternion.identity;

        var searchText = searchBar.transform.GetComponent<Text>();
        searchText.text = Plugin.Options.SearchValue.Value;
        searchText.alignment = TextAnchor.MiddleLeft;

        Plugin.SearchInput = searchBar.AddComponent<InputField>();
        Plugin.SearchInput.textComponent = searchText;
        Plugin.SearchInput.name = "search";
        Plugin.SearchInput.onValueChanged.AddListener(val => SearchListener(val, __instance));
        ClearSearchButton(__instance, searchText);

        Destroy(__instance.scenetitle);
    }

    private static Button ClearSearchButton(LevelSelectController __instance, Text scoreText)
    {
        Button deleteButton = AddDeleteButton(scoreText);
        deleteButton.name = $"clear search";
        deleteButton.onClick.AddListener(() => {
            Plugin.SearchInput.text = "";
            scoreText.text = "";
            SearchListener("", __instance);
        });
        var deleteRectTransform = deleteButton.GetComponent<RectTransform>();

        deleteRectTransform.sizeDelta = new Vector2(15, 15);
        deleteRectTransform.anchoredPosition = new Vector2(-25, 0);

        var deleteText = deleteButton.GetComponentInChildren<Text>();
        deleteText.text = "X";
        deleteText.fontSize = 12;

        return deleteButton;
    }

    private static void SearchListener(string val, LevelSelectController __instance)
    {
        Plugin.Log.LogDebug($"search: {val}");
        Plugin.Options.SearchValue.Value = val;
        TrackLoaded.FilterTracks(__instance);
    }
    #endregion
}
