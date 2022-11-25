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
        Plugin.Toggle = Instantiate(__instance.set_tog_accessb_jumpscare);
        Plugin.Button = Instantiate(__instance.graphicspanel.GetComponentInChildren<Button>());
        DontDestroyOnLoad(Plugin.Toggle);
        DontDestroyOnLoad(Plugin.Button);
    }
}
