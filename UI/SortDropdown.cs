using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using SongOrganizer.Data;
using SongOrganizer.Utils;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace SongOrganizer.UI;

public class SortDropdown : MonoBehaviour
{
    public static List<Toggle> Toggles = new();

    public const string SORT_BUTTON_SHADOW_PATH = $"{UnityPaths.FULLSCREENPANEL}/sort_button/btn-shadow";
    public const string SORT_DROPDOWN_PATH = $"{UnityPaths.FULLSCREENPANEL}/sort-dropdown";
    public const string SORT_DROPDOWN_FACE_PATH = $"{SORT_DROPDOWN_PATH}/face";
    public const string SORT_DROPDOWN_SHADOW_PATH = $"{SORT_DROPDOWN_PATH}/shadow";
    public const string SORT_LENGTH_BUTTON_PATH = $"{SORT_DROPDOWN_FACE_PATH}/btn_sort_length";

    public static void Setup(LevelSelectController __instance)
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
        CreateSortOption(__instance, face, "artist", -86);
        CreateSortOption(__instance, face, "long name", -108);
        foreach (var sortOption in face.GetComponentsInChildren<Button>())
        {
            sortOption.colors = new ColorBlock
            {
                normalColor = mainColor,
                highlightedColor = new Color(mainColor.r + 0.3f, mainColor.g + 0.3f, mainColor.b + 0.3f),
                pressedColor = mainColor,
                colorMultiplier = 1
            };
        }

        int length = 240;
        var faceRectTransform = face.GetComponent<RectTransform>();
        var shadowRectTransform = GameObject.Find(SORT_DROPDOWN_SHADOW_PATH).GetComponent<RectTransform>();
        faceRectTransform.sizeDelta = new Vector2(0, length);
        shadowRectTransform.sizeDelta = new Vector2(0, length);
        faceRectTransform.anchoredPosition = new Vector2(0, -length / 2);
        shadowRectTransform.anchoredPosition = new Vector2(5, -length / 2 - 5);

        int filterOffset = -128;
        foreach (FilterOption filterOption in Enum.GetValues(typeof(FilterOption)))
        {
            Toggle filter = CreateFilterOption(face, filterOption, new Vector2(234, filterOffset -= 24));
            ConfigEntry<bool> configEntry = GetConfigEntry(filterOption);
            if (configEntry == null) continue;
            filter.isOn = configEntry.Value;
            filter.onValueChanged.AddListener(b =>
            {
                configEntry.Value = b;
                RefreshLevelSelect.FilterTracks(__instance);
            });
            Toggles.Add(filter);
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
            FilterOption.ONLY_FAVORITES => Plugin.Options.ShowOnlyFavorites,
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
        text.fontSize = 11;
        text.color = OptionalTheme.colors.playButton.text;

        var background = toggle.transform.Find("Background").GetComponent<Image>();
        background.rectTransform.sizeDelta = new Vector2(0, 0);

        var checkmark = toggle.transform.Find("Background/Checkmark").GetComponent<Image>();
        checkmark.rectTransform.anchoredPosition = new Vector2(-85, 20);
        checkmark.rectTransform.sizeDelta = new Vector2(15, 15);

        var label = toggle.GetComponentInChildren<Text>();
        label.rectTransform.sizeDelta = new Vector2(180, label.rectTransform.sizeDelta.y);
        return toggle;
    }
}
