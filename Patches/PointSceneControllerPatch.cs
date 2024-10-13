using HarmonyLib;
using SongOrganizer.UI;

namespace SongOrganizer.Patches;

[HarmonyPatch(typeof(PointSceneController), nameof(PointSceneController.Start))]
public class PointSceneControllerPatch
{
    static void Postfix(PointSceneController __instance)
    {
        Favorites.Setup(__instance);
    }
}
