using System;
using System.Collections.Generic;
using BaboonAPI.Hooks.Tracks;
using BepInEx.Configuration;
using HarmonyLib;
using SongOrganizer.Assets;
using SongOrganizer.Data;
using SongOrganizer.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
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
            __instance.alltrackslist.Sort((t1, t2) => t1.sort_order.CompareTo(t2.sort_order));
        else if (sortcriteria == "difficulty" && __instance.alltrackslist.TrueForAll(track => track is Track))
            __instance.alltrackslist.Sort((t1, t2) => ((Track)t1).stars.CompareTo(((Track)t2).stars));
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
    private const string FULLSCREENPANEL = "MainCanvas/FullScreenPanel";
    private const string SORT_BUTTON_PATH = $"{FULLSCREENPANEL}/sort_button";
    private const string SORT_BUTTON_SHADOW_PATH = $"{FULLSCREENPANEL}/sort_button/btn-shadow";
    private const string SORT_DROPDOWN_PATH = $"{FULLSCREENPANEL}/sort-dropdown";
    private const string SORT_DROPDOWN_FACE_PATH = $"{SORT_DROPDOWN_PATH}/face";
    private const string SORT_LENGTH_BUTTON_PATH = $"{SORT_DROPDOWN_FACE_PATH}/btn_sort_length";
    private const string TITLE_BAR = $"{FULLSCREENPANEL}/title bar";

    static void Prefix()
    {
        GlobalVariables.sortmode = Plugin.Options.SortMode.Value;
        GlobalVariables.levelselect_index = Plugin.Options.LastIndex.Value;
        if (GlobalVariables.levelselect_index >= GlobalVariables.data_tracktitles.Length)
        {
            GlobalVariables.levelselect_index = 0;
        }
    }

    static void Postfix(LevelSelectController __instance)
    {
        OptionalTheme.Setup();
        if (Plugin.RefreshLevelSelect == null)
        {
            Plugin.RefreshLevelSelect = new RefreshLevelSelect(__instance);
            TracksLoadedEvent.EVENT.Register(Plugin.RefreshLevelSelect);
            Plugin.RefreshLevelSelect.OnTracksLoaded(null);
        }

        AddOptions(__instance);
        AddSearchBar(__instance);
        AddStarSlider(__instance);
    }

    #region AddOptions
    // idk how these numbers work
    private static void AddOptions(LevelSelectController __instance)
    {
        var sortButton = __instance.sortbutton.GetComponent<Button>();
        var mainColor = OptionalTheme.colors.playButton.background;
        sortButton.colors = new ColorBlock
        {
            normalColor = mainColor,
            highlightedColor = new Color(mainColor.r + 0.3f, mainColor.g + 0.3f, mainColor.b + 0.3f),
            pressedColor = new Color(mainColor.r + 0.6f, mainColor.g + 0.6f, mainColor.b + 0.6f),
            colorMultiplier = 1
        };

        __instance.sortlabel.color = OptionalTheme.colors.playButton.text;
        sortButton.transform.Find("arrow").GetComponent<Image>().color = __instance.sortlabel.color;

        GameObject.Find(SORT_BUTTON_SHADOW_PATH).GetComponent<Image>().color = OptionalTheme.colors.playButton.shadow;

        GameObject sortDropdown = GameObject.Find(SORT_DROPDOWN_PATH);
        sortDropdown.transform.SetAsLastSibling();
        sortDropdown.GetComponent<Image>().color = OptionalTheme.colors.playButton.shadow; // dropdown outline color
        GameObject face = GameObject.Find(SORT_DROPDOWN_FACE_PATH);
        face.GetComponent<Image>().color = mainColor; // dropdown color
        foreach (var text in face.GetComponentsInChildren<Text>()) text.color = __instance.sortlabel.color;
        RectTransform sortDropRectTransform = __instance.sortdrop.GetComponent<RectTransform>();
        RectTransform faceRectTransform = face.GetComponent<RectTransform>();
        CreateSortOption(__instance, face, "artist", -75);
        CreateSortOption(__instance, face, "long name", -105);
        int length = 430;
        faceRectTransform.sizeDelta = new Vector2(180, length);
        sortDropRectTransform.sizeDelta = new Vector2(180, length);
        foreach (var filterOption in face.GetComponentsInChildren<Button>())
        {
            var filterOptionRect = filterOption.GetComponent<RectTransform>();
            filterOptionRect.anchoredPosition = new Vector2(filterOptionRect.anchoredPosition.x, filterOptionRect.anchoredPosition.y + 90);
        }

        int filterOffset = -180;
        foreach (FilterOption filterOption in Enum.GetValues(typeof(FilterOption)))
        {
            Toggle filter = CreateFilterOption(face, filterOption, new Vector2(242, filterOffset -= 30));
            ConfigEntry<bool> configEntry = GetConfigEntry(filterOption);
            if (configEntry == null) continue;
            filter.isOn = configEntry.Value;
            filter.onValueChanged.AddListener(b =>
            {
                configEntry.Value = b;
                RefreshLevelSelect.FilterTracks(__instance);
            });
        }
        foreach (var button in face.GetComponentsInChildren<Button>())
        {
            RectTransform t = button.GetComponent<RectTransform>();
            t.anchoredPosition = new Vector2(t.anchoredPosition.x, t.anchoredPosition.y + 60);
        }
    }

    private static ConfigEntry<bool> GetConfigEntry(FilterOption filterOption) =>
        filterOption switch
        {
            FilterOption.DEFAULT => Plugin.Options.ShowDefault,
            FilterOption.CUSTOM => Plugin.Options.ShowCustom,
            FilterOption.PLAYED => Plugin.Options.ShowPlayed,
            FilterOption.UNPLAYED => Plugin.Options.ShowUnplayed,
            FilterOption.NOT_S_RANK => Plugin.Options.ShowNotSRank,
            FilterOption.S_RANK => Plugin.Options.ShowSRank,
            FilterOption.UNRATED => Plugin.Options.ShowUnrated,
            FilterOption.RATED => Plugin.Options.ShowRated,
            _ => null,
        };

    private static void CreateSortOption(LevelSelectController __instance, GameObject face, string sortOption, float y)
    {
        GameObject sortObject = GameObject.Find(SORT_LENGTH_BUTTON_PATH);
        var sort = Instantiate(sortObject.GetComponent<Button>(), face.transform.transform);
        Destroy(sort.GetComponentInChildren<LocalizeStringEvent>());
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
        text.color = OptionalTheme.colors.playButton.text;

        var background = toggle.transform.Find("Background").GetComponent<Image>();
        background.rectTransform.sizeDelta = new Vector2(0, 0);

        var checkmark = toggle.transform.Find("Background/Checkmark").GetComponent<Image>();
        checkmark.rectTransform.anchoredPosition = new Vector2(-70, 20);
        checkmark.rectTransform.sizeDelta = new Vector2(20, 20);

        var label = toggle.GetComponentInChildren<Text>();
        label.rectTransform.sizeDelta = new Vector2(180, label.rectTransform.sizeDelta.y);
        return toggle;
    }
    #endregion

    private static void AddStarSlider(LevelSelectController __instance)
    {
        var fullscreenPanel = GameObject.Find(FULLSCREENPANEL);

        var description = Instantiate(__instance.label_speed_slider, fullscreenPanel.transform);
        description.name = "StarSliderDescription";
        description.text = "Difficulty range:";
        description.color = OptionalTheme.colors.songName;
        description.GetComponent<RectTransform>().anchoredPosition = new Vector2(-207, 175);

        var doubleSlider = new DoubleSlider();
        doubleSlider.Setup(__instance, fullscreenPanel.transform, new Vector2(-115, 176));
    }

    private static void AddSearchBar(LevelSelectController __instance)
    {
        var fullscreenPanel = GameObject.Find(FULLSCREENPANEL);
        __instance.scenetitle.SetActive(false);

        var color = OptionalTheme.colors.leaderboard.text;
        var textOutlineColor = OptionalTheme.colors.leaderboard.textOutline;
        Plugin.SearchInput = Instantiate(Plugin.InputFieldPrefab, fullscreenPanel.transform);
        Plugin.SearchInput.name = "SearchInput";
        Plugin.SearchInput.GetComponent<RectTransform>().anchoredPosition = new Vector2(190, 420);
        Plugin.SearchInput.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 14);
        Plugin.SearchInput.image.color = color;
        Plugin.SearchInput.textComponent.color = color;
        Plugin.SearchInput.textComponent.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, textOutlineColor);
        Plugin.SearchInput.text = Plugin.Options.SearchValue.Value;
        Plugin.SearchInput.onEndEdit.AddListener(text => Plugin.SearchInput.text = text);
        Plugin.SearchInput.onValueChanged.AddListener(text =>
        {
            Plugin.Options.SearchValue.Value = text;
            RefreshLevelSelect.FilterTracks(__instance);
        });

        Button deleteButton = AddDeleteButton(Plugin.SearchInput.textComponent);
        deleteButton.name = "clear search";
        deleteButton.onClick.AddListener(() => {
            Plugin.SearchInput.text = "";
            Plugin.SearchInput.textComponent.text = "";
            Plugin.Options.SearchValue.Value = "";
            RefreshLevelSelect.FilterTracks(__instance);
        });
    }

    private static Button AddDeleteButton(TMP_Text scoreText)
    {
        var scoreRectTransform = scoreText.GetComponent<RectTransform>();
        var deleteButton = Instantiate(Plugin.Button, scoreRectTransform);
        var deleteRectTransform = deleteButton.GetComponent<RectTransform>();
        deleteButton.onClick.RemoveAllListeners();

        deleteRectTransform.sizeDelta = new Vector2(15, 15);
        deleteRectTransform.position = scoreRectTransform.position;
        deleteRectTransform.anchoredPosition = new Vector2(-20, 15);
        deleteButton.colors = OptionalTheme.colors.replayButton.colors;

        var deleteText = deleteButton.GetComponentInChildren<Text>();
        deleteText.text = "X";
        deleteText.fontSize = 12;
        deleteText.color = OptionalTheme.colors.replayButton.text;
        return deleteButton;
    }
}
