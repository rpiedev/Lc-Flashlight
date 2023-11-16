using BepInEx;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine.InputSystem;

namespace Flashlight
{
    [BepInPlugin("rr.Flashlight", "Flashlight", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private Harmony _harmony = new Harmony("Flashlight");
        private void Awake()
        {
            this._harmony.PatchAll(typeof(Plugin));
            this.Logger.LogInfo("------Flashlight done.------");
        }

        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPostfix]
        public static void ReadInput(PlayerControllerB __instance)
        {
            if ((!__instance.IsOwner || !__instance.isPlayerControlled || __instance.IsServer && !__instance.isHostPlayerObject) && !__instance.isTestingPlayer)
                return;
            if(__instance.currentlyHeldObjectServer is FlashlightItem && __instance.currentlyHeldObjectServer != __instance.pocketedFlashlight)
                __instance.pocketedFlashlight = __instance.currentlyHeldObjectServer;
            if(Keyboard.current.fKey.wasPressedThisFrame && __instance.pocketedFlashlight is FlashlightItem && __instance.pocketedFlashlight.isHeld)
            {
                if(__instance.pocketedFlashlight.isBeingUsed)
                {
                    __instance.pocketedFlashlight.ItemActivate(false);
                    __instance.helmetLight.enabled = false;
                } else
                {
                    __instance.pocketedFlashlight.ItemActivate(true);
                    if (!(__instance.currentlyHeldObjectServer is FlashlightItem))
                    {
                        __instance.helmetLight.enabled = true;
                        (__instance.pocketedFlashlight as FlashlightItem).flashlightBulb.enabled = false;
                        (__instance.pocketedFlashlight as FlashlightItem).flashlightBulbGlow.enabled = false;
                    }
                }
            }
        }
    }
}
