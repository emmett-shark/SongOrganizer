using System.Collections.Generic;
using BaboonAPI.Hooks.Tracks;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using SongOrganizer.Data;
using SongOrganizer.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace SongOrganizer;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("TrombLoader")]
[BepInDependency("ch.offbeatwit.baboonapi.plugin")]
public class Plugin : BaseUnityPlugin
{
    public static Plugin Instance;
    public static ManualLogSource Log;
    public static Options Options;
    public static TrackLoaded TrackLoaded;

    public static Toggle Toggle;
    public static Button Button;
    public static InputField SearchInput;
    public static Image Star;

    public static Dictionary<string, Track> TrackDict = new Dictionary<string, Track>();
    public const int TRACK_SCORE_LENGTH = 5;
    public static Button[] DeleteButtons = new Button[TRACK_SCORE_LENGTH + 1];

    private const string FILTER_SECTION = "Filter";
    private const string SORT_SECTION = "Sort";
    private const string INDEX_SECTION = "Index";
    private const string SEARCH_SECTION = "Search";

    private void Awake()
    {
        Instance = this;
        Log = Logger;
        Options = new Options
        {
            ShowDefault = base.Config.Bind(FILTER_SECTION, nameof(Options.ShowDefault), true),
            ShowCustom = base.Config.Bind(FILTER_SECTION, nameof(Options.ShowCustom), true),
            ShowUnplayed = base.Config.Bind(FILTER_SECTION, nameof(Options.ShowUnplayed), true),
            ShowPlayed = base.Config.Bind(FILTER_SECTION, nameof(Options.ShowPlayed), true),
            ShowNotSRank = base.Config.Bind(FILTER_SECTION, nameof(Options.ShowNotSRank), true),
            ShowSRank = base.Config.Bind(FILTER_SECTION, nameof(Options.ShowSRank), true),
            ShowUnrated = base.Config.Bind(FILTER_SECTION, nameof(Options.ShowUnrated), true),
            ShowRated = base.Config.Bind(FILTER_SECTION, nameof(Options.ShowRated), true),
            SortMode = base.Config.Bind(SORT_SECTION, nameof(Options.SortMode), "default"),
            LastIndex = base.Config.Bind(INDEX_SECTION, nameof(Options.LastIndex), 0),
            SearchValue = base.Config.Bind(SEARCH_SECTION, nameof(Options.SearchValue), ""),
        };
        StartCoroutine(TootTally.GetRatedTracks());
        new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();
    }

    public void UnloadModule()
    {
        if (TrackLoaded != null)
        {
            TracksLoadedEvent.EVENT.Unregister(TrackLoaded);
            TrackLoaded = null;
        }
    }
}