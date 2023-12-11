using BepInEx;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine.InputSystem;
using UnityEngine;
using System.IO;
using BepInEx.Logging;

namespace Flashlight
{
    [BepInPlugin("rr.Flashlight", "Flashlight", "1.4.0")]
    public class Plugin : BaseUnityPlugin
    {
        static string path = Application.persistentDataPath + "/flashlightbutton.txt";
        internal static ManualLogSource logSource;

        static InputActionAsset asset;
        static string defaultkey = "/Keyboard/f";
        private Harmony _harmony = new Harmony("Flashlight");
        private void Awake()
        {
            this._harmony.PatchAll(typeof(Plugin));
            this.Logger.LogInfo("------Flashlight done.------");
            Plugin.logSource = base.Logger;
        }

        public static void setAsset(string thing)
        {
            asset = InputActionAsset.FromJson(@"
                {
                    ""maps"" : [
                        {
                            ""name"" : ""Flashlight"",
                            ""actions"": [
                                {""name"": ""togglef"", ""type"" : ""button""}
                            ],
                            ""bindings"" : [
                                {""path"" : """ + thing + @""", ""action"": ""togglef""}
                            ]
                        }
                    ]
                }");
        }
        [HarmonyPatch(typeof(PlayerControllerB), "KillPlayer")]
        [HarmonyPostfix]
        public static void ClearFlashlight(PlayerControllerB __instance)
        {
            
            __instance.pocketedFlashlight = null;
        }

        [HarmonyPatch(typeof(IngamePlayerSettings), "CompleteRebind")]
        [HarmonyPrefix]
        public static void SavingToFile(IngamePlayerSettings __instance)
        {
            if (__instance.rebindingOperation.action.name != "togglef") return;
            File.WriteAllText(path, __instance.rebindingOperation.action.controls[0].path);
            string thing = defaultkey;
            if (File.Exists(path))
            {
                thing = File.ReadAllText(path);
            }
            setAsset(thing);
        }

        [HarmonyPatch(typeof(KepRemapPanel), "LoadKeybindsUI")]
        [HarmonyPrefix]
        public static void Testing(KepRemapPanel __instance)
        {
            string thing = defaultkey;
            if (!File.Exists(path))
            {
                File.WriteAllText(path, defaultkey);
            } else
            {
                thing = File.ReadAllText(path);
            }
            
            for (int index1 = 0; index1 < __instance.remappableKeys.Count; ++index1)
            {
                if (__instance.remappableKeys[index1].ControlName == "Flashlight") return;
            }
            RemappableKey fl = new RemappableKey();
            setAsset(thing);
            InputActionReference inp = InputActionReference.Create(asset.FindAction("Flashlight/togglef"));
            fl.ControlName = "Flashlight";
            fl.currentInput = inp;

            __instance.remappableKeys.Add(fl);
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
            string thing = defaultkey;
            if (!File.Exists(path))
            {
                File.WriteAllText(path, defaultkey);
            } else
            {
                thing = File.ReadAllText(path);
            }
            if (!asset || !asset.enabled) { setAsset(thing); asset.Enable(); }
            if (asset.FindAction("Flashlight/togglef").triggered && __instance.pocketedFlashlight is FlashlightItem && __instance.pocketedFlashlight.isHeld)
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
