using System;
using System.Collections.Generic;
using BaboonAPI.Hooks.Tracks;
using HarmonyLib;
using SongOrganizer.Data;
using SongOrganizer.Utils;
using SongOrganizer.UI;
using TMPro;
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

    static void Postfix(LevelSelectController __instance)
    {
        Favorites.ShowFavorites(__instance);
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
    static void Prefix()
    {
        GlobalVariables.sortmode = Plugin.Options.SortMode.Value;
        GlobalVariables.levelselect_index = Plugin.Options.LastIndex.Value;
        if (GlobalVariables.levelselect_index >= GlobalVariables.data_tracktitles.Length)
        {
            GlobalVariables.levelselect_index = 0;
        }
        Plugin.FavoriteButtons.Clear();
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

        new SortDropdown().Setup(__instance);
        AddSearchBar(__instance);
        AddStarSlider(__instance);
        new Favorites().Setup(__instance);
    }

    private static void AddStarSlider(LevelSelectController __instance)
    {
        var fullscreenPanel = GameObject.Find(UnityPaths.FULLSCREENPANEL);

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
        var fullscreenPanel = GameObject.Find(UnityPaths.FULLSCREENPANEL);
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
            var selectedTrackref = __instance.alltrackslist[__instance.songindex].trackref;
            Plugin.SearchInput.text = "";
            Plugin.SearchInput.textComponent.text = "";
            Plugin.Options.SearchValue.Value = "";
            RefreshLevelSelect.FilterTracks(__instance);
            /*__instance.songindex = __instance.alltrackslist.FindIndex(track => track.trackref == selectedTrackref);
            __instance.populateSongNames(false);*/
            GlobalVariables.levelselect_index = __instance.alltrackslist.FindIndex(track => track.trackref == selectedTrackref);
            GlobalVariables.levelselect_index = GlobalVariables.levelselect_index == -1 ? 0 : GlobalVariables.levelselect_index;
            __instance.sortTracks(GlobalVariables.sortmode, false);
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
