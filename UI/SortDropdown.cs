using System;
using BepInEx.Configuration;
using SongOrganizer.Data;
using SongOrganizer.Utils;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace SongOrganizer.UI;

public class SortDropdown : MonoBehaviour
{
    public void Setup(LevelSelectController __instance)
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

        GameObject.Find(UnityPaths.SORT_BUTTON_SHADOW_PATH).GetComponent<Image>().color = OptionalTheme.colors.playButton.shadow;

        GameObject sortDropdown = GameObject.Find(UnityPaths.SORT_DROPDOWN_PATH);
        sortDropdown.transform.SetAsLastSibling();
        sortDropdown.GetComponent<Image>().color = OptionalTheme.colors.playButton.shadow; // dropdown outline color
        GameObject face = GameObject.Find(UnityPaths.SORT_DROPDOWN_FACE_PATH);
        face.GetComponent<Image>().color = mainColor; // dropdown color
        foreach (var text in face.GetComponentsInChildren<Text>()) text.color = __instance.sortlabel.color;
        RectTransform sortDropRectTransform = __instance.sortdrop.GetComponent<RectTransform>();
        RectTransform faceRectTransform = face.GetComponent<RectTransform>();
        CreateSortOption(__instance, face, "artist", -75);
        CreateSortOption(__instance, face, "long name", -105);
        int length = 460;
        faceRectTransform.sizeDelta = new Vector2(180, length);
        sortDropRectTransform.sizeDelta = new Vector2(180, length);
        foreach (var filterOption in face.GetComponentsInChildren<Button>())
        {
            var filterOptionRect = filterOption.GetComponent<RectTransform>();
            filterOptionRect.anchoredPosition = new Vector2(filterOptionRect.anchoredPosition.x, filterOptionRect.anchoredPosition.y + 105);
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
            FilterOption.ONLY_FAVORITES => Plugin.Options.ShowOnlyFavorites,
            _ => null,
        };

    private static void CreateSortOption(LevelSelectController __instance, GameObject face, string sortOption, float y)
    {
        GameObject sortObject = GameObject.Find(UnityPaths.SORT_LENGTH_BUTTON_PATH);
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
}
