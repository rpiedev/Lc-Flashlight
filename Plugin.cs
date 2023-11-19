using BepInEx;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine.InputSystem;

namespace Flashlight
{
    [BepInPlugin("rr.Flashlight", "Flashlight", "1.2.0")]
    public class Plugin : BaseUnityPlugin
    {
        private Harmony _harmony = new Harmony("Flashlight");
        private void Awake()
        {
            this._harmony.PatchAll(typeof(Plugin));
            this.Logger.LogInfo("------Flashlight done.------");
        }

        [HarmonyPatch(typeof(PlayerControllerB), "SpawnPlayerAnimation")]
        [HarmonyPostfix]
        public static void ClearFlashlight(PlayerControllerB __instance)
        {
            
            __instance.pocketedFlashlight = null;
        }

        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPostfix]
        public static void ReadInput(PlayerControllerB __instance)
        {
            if ((!__instance.IsOwner || !__instance.isPlayerControlled || __instance.IsServer && !__instance.isHostPlayerObject) && !__instance.isTestingPlayer)
                return;
            if(__instance.currentlyHeldObjectServer is FlashlightItem && __instance.currentlyHeldObjectServer != __instance.pocketedFlashlight)
                __instance.pocketedFlashlight = __instance.currentlyHeldObjectServer;
            if (__instance.pocketedFlashlight == null) return;
            if (Keyboard.current.fKey.wasPressedThisFrame && __instance.pocketedFlashlight is FlashlightItem && __instance.pocketedFlashlight.isHeld)
            {
                __instance.pocketedFlashlight.UseItemOnClient();
                if (!(__instance.currentlyHeldObjectServer is FlashlightItem))
                {
                    (__instance.pocketedFlashlight as FlashlightItem).flashlightBulbGlow.enabled = false;
                    (__instance.pocketedFlashlight as FlashlightItem).flashlightBulb.enabled = false;
                    if ((__instance.pocketedFlashlight as FlashlightItem).isBeingUsed)
                    {
                        __instance.helmetLight.enabled = true;
                        (__instance.pocketedFlashlight as FlashlightItem).usingPlayerHelmetLight = true;
                        (__instance.pocketedFlashlight as FlashlightItem).PocketFlashlightServerRpc(true);
                    } else
                    {
                        __instance.helmetLight.enabled = false;
                        (__instance.pocketedFlashlight as FlashlightItem).usingPlayerHelmetLight = false;
                        (__instance.pocketedFlashlight as FlashlightItem).PocketFlashlightServerRpc(false);
                    }
                }
            }
        }
    }
}
