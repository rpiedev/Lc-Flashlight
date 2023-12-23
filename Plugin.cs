using BepInEx;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine.InputSystem;
using UnityEngine;
using System.IO;
using BepInEx.Logging;
using LethalCompanyInputUtils.Api;
using System.Xml.Linq;

namespace Flashlight
{
    public class FlashButton : LcInputActions
    {
        [InputAction("<Keyboard>/f", Name = "Flashlight")]
        public InputAction FlashKey { get; set; }
    }
    [BepInPlugin("rr.Flashlight", "Flashlight", "1.5.0")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource logSource;
        internal static FlashButton InputActionInstance = new FlashButton();
        private Harmony _harmony = new Harmony("Flashlight");
        private void Awake()
        {
            this._harmony.PatchAll(typeof(Plugin));
            this.Logger.LogInfo("------Flashlight done.------");
            Plugin.logSource = base.Logger;
        }
        [HarmonyPatch(typeof(PlayerControllerB), "KillPlayer")]
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
            if (__instance.inTerminalMenu || __instance.isTypingChat) return;
            if (!Application.isFocused) return;
            if (__instance.currentlyHeldObjectServer is FlashlightItem && __instance.currentlyHeldObjectServer != __instance.pocketedFlashlight)
                __instance.pocketedFlashlight = __instance.currentlyHeldObjectServer;
            if (__instance.pocketedFlashlight == null) return;

            if (Plugin.InputActionInstance.FlashKey.triggered && __instance.pocketedFlashlight is FlashlightItem && __instance.pocketedFlashlight.isHeld)
            {
                try 
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
                } catch 
                { 
                    
                }
            }
        }
    }
}
