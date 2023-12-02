using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace SongOrganizer.Patches;

// idk how to create a toggle, so stealing one from the home screen. yoink
[HarmonyPatch(typeof(HomeController), nameof(HomeController.Start))]
public class HomeControllerStartPatch : MonoBehaviour
{
    static void Postfix(HomeController __instance)
    {
        Plugin.Toggle = Instantiate(__instance.set_tog_vsync);
        DontDestroyOnLoad(Plugin.Toggle);

        Plugin.Button = Instantiate(__instance.graphicspanel.GetComponentInChildren<Button>());
        Plugin.Button.onClick.RemoveAllListeners();
        DontDestroyOnLoad(Plugin.Button);
    }
}
