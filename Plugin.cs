using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using BaboonAPI.Hooks.Tracks;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Newtonsoft.Json;
using SongOrganizer.Data;
using SongOrganizer.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static SongOrganizer.Utils.TootTallyWebClient;

namespace SongOrganizer;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("TrombLoader")]
[BepInDependency("ch.offbeatwit.baboonapi.plugin")]
[BepInDependency("TootTallyDiffCalcLibs", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("TootTallyCore", BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BaseUnityPlugin
{
    public static Plugin Instance;
    public static ManualLogSource Log;
    public static Options Options;
    public static RefreshLevelSelect RefreshLevelSelect;

    public static Toggle Toggle;
    public static Button Button;
    public static TMP_InputField InputFieldPrefab;
    public static TMP_InputField SearchInput;
    public static List<Button> FavoriteButtons = new();

    public static ConcurrentDictionary<string, Track> TrackDict = new();
    public static ConcurrentDictionary<string, float> StarDict = new();
    public static ConcurrentBag<string> RatedTrackrefs = new();
    public static List<SearchTrackResult> RatedTracksPaged = new();
    public const int TRACK_SCORE_LENGTH = 5;
    public const int MAX_STARS = 11;
    public const int MAX_PARALLELISM = 8;

    public static readonly string RatedTracksPath = $"{Paths.ConfigPath}/rated.json";
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
            ShowDefault = Config.Bind(FILTER_SECTION, nameof(Options.ShowDefault), false),
            ShowCustom = Config.Bind(FILTER_SECTION, nameof(Options.ShowCustom), false),
            ShowUnplayed = Config.Bind(FILTER_SECTION, nameof(Options.ShowUnplayed), false),
            ShowPlayed = Config.Bind(FILTER_SECTION, nameof(Options.ShowPlayed), false),
            ShowNotSRank = Config.Bind(FILTER_SECTION, nameof(Options.ShowNotSRank), false),
            ShowSRank = Config.Bind(FILTER_SECTION, nameof(Options.ShowSRank), false),
            ShowUnrated = Config.Bind(FILTER_SECTION, nameof(Options.ShowUnrated), false),
            ShowRated = Config.Bind(FILTER_SECTION, nameof(Options.ShowRated), false),
            ShowOnlyFavorites = Config.Bind(FILTER_SECTION, nameof(Options.ShowOnlyFavorites), false),
            Favorites = Config.Bind(FILTER_SECTION, nameof(Options.Favorites), ""),
            SortMode = Config.Bind(SORT_SECTION, nameof(Options.SortMode), "default"),
            LastIndex = Config.Bind(INDEX_SECTION, nameof(Options.LastIndex), 0),
            CollectionIndex = Config.Bind(INDEX_SECTION, nameof(Options.CollectionIndex), 4),
            SearchValue = Config.Bind(SEARCH_SECTION, nameof(Options.SearchValue), ""),
            MinStar = Config.Bind(SEARCH_SECTION, nameof(Options.MinStar), 0f),
            MaxStar = Config.Bind(SEARCH_SECTION, nameof(Options.MaxStar), 11f),
        };
        Options.SetFavorites();
        TrackCalculation.CalculateStars();
        var start = DateTime.Now;
        StartCoroutine(GetRatedTracksRequest(1, PAGE_SIZE, result =>
        {
            StartCoroutine(GetMoreRatedTracks(start, result));
        }));
        SceneManager.sceneUnloaded += UnloadModule;
        new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();
    }
    
    public IEnumerator<Coroutine> GetMoreRatedTracks(DateTime start, SearchResponse result)
    {
        var coroutines = new List<Coroutine>();
        int end = result.count / PAGE_SIZE + 1;
        for (int i = 2; i <= end; i++)
        {
            coroutines.Add(StartCoroutine(GetRatedTracksRequest(i, PAGE_SIZE)));
        }
        foreach (var coroutine in coroutines)
        {
            yield return coroutine;
        }
        var combinedResponse = new SearchResponse { count = RatedTracksPaged.Count, results = RatedTracksPaged };
        var text = JsonConvert.SerializeObject(combinedResponse);
        File.WriteAllText(RatedTracksPath, text);
        Log.LogDebug($"Queried {RatedTracksPaged.Count} rated tracks elapsed: {DateTime.Now - start}");
    }

    public static void UnloadModule(Scene scene) => UnloadModule();

    public static void UnloadModule()
    {
        if (RefreshLevelSelect != null)
        {
            TracksLoadedEvent.EVENT.Unregister(RefreshLevelSelect);
            RefreshLevelSelect = null;
        }
    }
}
