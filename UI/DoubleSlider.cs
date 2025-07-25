﻿using System;
using BepInEx.Configuration;
using SongOrganizer.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace SongOrganizer.UI;

public class DoubleSlider : MonoBehaviour
{
    public static Slider minSlider;
    public static Slider maxSlider;

    private const string HANDLE_PATH = "Handle Slide Area/Handle";
    private const string FILL_AREA_PATH = "Fill Area";
    private const string FILL_PATH = "Fill Area/Fill";
    private const string BACKGROUND_PATH = "Background";

    public static void Setup(LevelSelectController __instance, Transform transform, Vector2 position)
    {
        Plugin.Options.MaxStar.Value = Math.Min(Plugin.Options.MaxStar.Value, Plugin.Options.MaxStarSlider.Value + 1);
        Plugin.Options.MinStar.Value = Math.Min(Plugin.Options.MinStar.Value, Plugin.Options.MaxStarSlider.Value + 1);

        maxSlider = CreateSlider(__instance, transform, position, Plugin.Options.MaxStar, ChangeMaxSlider);
        maxSlider.transform.Find(FILL_PATH).GetComponent<Image>().color = OptionalTheme.colors.scrollSpeedSlider.fill;
        maxSlider.transform.Find(BACKGROUND_PATH).GetComponent<Image>().color = OptionalTheme.colors.scrollSpeedSlider.background;
        maxSlider.name = $"StarSliderMax";

        minSlider = CreateSlider(__instance, transform, position, Plugin.Options.MinStar, ChangeMinSlider);
        minSlider.name = $"StarSliderMin";
        minSlider.transform.Find(FILL_AREA_PATH).gameObject.SetActive(false);
        minSlider.transform.Find(BACKGROUND_PATH).gameObject.SetActive(false);
        EmptyMinSlider();
    }

    private static void ChangeMinSlider(float value)
    {
        if (maxSlider.value <= value) maxSlider.value = value + 1;
    }

    private static void ChangeMaxSlider(float value)
    {
        if (minSlider.value >= value) minSlider.value = value - 1;
    }

    public static Slider CreateSlider(LevelSelectController __instance, Transform transform, Vector2 position, ConfigEntry<float> entry, Action<float> action)
    {
        var starSlider = Instantiate(__instance.gamespeedslider, transform);

        var handle = starSlider.transform.Find(HANDLE_PATH);
        var starSliderLabel = Instantiate(__instance.label_speed_slider, handle.transform);
        starSliderLabel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        starSliderLabel.name = $"StarSliderLabel";
        starSliderLabel.color = OptionalTheme.colors.scrollSpeedSlider.text;
        SetLabel(starSliderLabel, entry.Value);

        starSlider.GetComponent<RectTransform>().anchoredPosition = position;
        starSlider.wholeNumbers = true;
        starSlider.minValue = 0;
        starSlider.maxValue = Plugin.Options.MaxStarSlider.Value + 1;
        starSlider.value = entry.Value;
        handle.gameObject.GetComponent<Image>().color = OptionalTheme.colors.scrollSpeedSlider.handle;
        handle.GetComponent<RectTransform>().sizeDelta = new Vector2(18, 3);
        starSlider.transform.Find(FILL_AREA_PATH).GetComponent<RectTransform>().sizeDelta = new Vector2(-10, 0);
        starSlider.transform.Find(BACKGROUND_PATH).GetComponent<RectTransform>().sizeDelta = new Vector2(2, 2);

        starSlider.onValueChanged.AddListener(value =>
        {
            entry.Value = value;
            SetLabel(starSliderLabel, value);
            EmptyMinSlider();
            RefreshLevelSelect.FilterTracks(__instance);
            action(value);
        });

        return starSlider;
    }

    private static void SetLabel(Text label, float value)
    {
        label.text = value == Plugin.Options.MaxStarSlider.Value + 1 ? "∞" : value.ToString();
    }

    private static void EmptyMinSlider()
    {
        var fillRect = maxSlider.transform.Find(FILL_PATH).gameObject.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(Plugin.Options.MinStar.Value / (Plugin.Options.MaxStarSlider.Value + 1), fillRect.anchorMin.y);
    }
}
