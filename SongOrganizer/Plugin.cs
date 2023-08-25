using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using SongOrganizer.Data;
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

    public static Toggle Toggle;
    public static Button Button;
    public static InputField SearchInput;
    public static Image Star;

    public static Dictionary<string, Track> TrackDict = new();
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
            ShowDefault = Config.Bind(FILTER_SECTION, nameof(Options.ShowDefault), true),
            ShowCustom = Config.Bind(FILTER_SECTION, nameof(Options.ShowCustom), true),
            ShowUnplayed = Config.Bind(FILTER_SECTION, nameof(Options.ShowUnplayed), true),
            ShowPlayed = Config.Bind(FILTER_SECTION, nameof(Options.ShowPlayed), true),
            ShowNotSRank = Config.Bind(FILTER_SECTION, nameof(Options.ShowNotSRank), true),
            ShowSRank = Config.Bind(FILTER_SECTION, nameof(Options.ShowSRank), true),
            ShowUnrated = Config.Bind(FILTER_SECTION, nameof(Options.ShowUnrated), true),
            ShowRated = Config.Bind(FILTER_SECTION, nameof(Options.ShowRated), true),
            SortMode = Config.Bind(SORT_SECTION, nameof(Options.SortMode), "default"),
            LastIndex = Config.Bind(INDEX_SECTION, nameof(Options.LastIndex), 0),
            SearchValue = Config.Bind(SEARCH_SECTION, nameof(Options.SearchValue), ""),
        };
        StartCoroutine(TootTally.GetRatedTracks());
        new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();
    }
}