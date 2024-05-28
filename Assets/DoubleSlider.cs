using System;
using BepInEx.Configuration;
using SongOrganizer.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace SongOrganizer.Assets;

public class DoubleSlider : MonoBehaviour
{
    private Slider minSlider;
    private Slider maxSlider;

    private const string HANDLE_PATH = "Handle Slide Area/Handle";
    private const string FILL_AREA_PATH = "Fill Area";
    private const string FILL_PATH = "Fill Area/Fill";
    private const string BACKGROUND_PATH = "Background";

    public void Setup(LevelSelectController __instance, Transform transform, Vector2 position)
    {
        maxSlider = CreateSlider(__instance, transform, position, Plugin.Options.MaxStar, ChangeMaxSlider);
        maxSlider.name = $"StarSliderMax";

        minSlider = CreateSlider(__instance, transform, position, Plugin.Options.MinStar, ChangeMinSlider);
        minSlider.name = $"StarSliderMin";
        minSlider.transform.Find(FILL_AREA_PATH).gameObject.SetActive(false);
        minSlider.transform.Find(BACKGROUND_PATH).gameObject.SetActive(false);
        EmptyMinSlider();
    }

    private void ChangeMinSlider(float value)
    {
        if (maxSlider.value <= value) maxSlider.value = value + 1;
    }

    private void ChangeMaxSlider(float value)
    {
        if (minSlider.value >= value) minSlider.value = value - 1;
    }

    public Slider CreateSlider(LevelSelectController __instance, Transform transform, Vector2 position, ConfigEntry<float> entry, Action<float> action)
    {
        var starSlider = Instantiate(__instance.gamespeedslider, transform);

        var handle = starSlider.transform.Find(HANDLE_PATH);
        var starSliderLabel = Instantiate(__instance.label_speed_slider, handle.transform);
        starSliderLabel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        starSliderLabel.name = $"StarSliderLabel";
        SetLabel(starSliderLabel, entry.Value);

        starSlider.GetComponent<RectTransform>().anchoredPosition = position;
        starSlider.wholeNumbers = true;
        starSlider.minValue = 0;
        starSlider.maxValue = Plugin.MAX_STARS;
        starSlider.value = entry.Value;
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

    private void SetLabel(Text label, float value)
    {
        label.text = value == Plugin.MAX_STARS ? "∞" : value.ToString();
    }

    private void EmptyMinSlider()
    {
        var fillRect = maxSlider.transform.Find(FILL_PATH).gameObject.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(Plugin.Options.MinStar.Value / Plugin.MAX_STARS, fillRect.anchorMin.y);
    }
}
