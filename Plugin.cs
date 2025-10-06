using System;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using NoTimeOff.Helpers;
using NoTimeOffAssets;
using Unity.Netcode;
using UnityEngine;

namespace NoTimeOff
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]

    internal class Plugin : BaseUnityPlugin
    {
        public static GameObject netObject;

        internal static Plugin Instance { get; private set; }

        internal static Harmony? Harmony { get; set; }


        private void Awake()
        {
            Helpers.Logger.SetLogger(Logger);

            if (Instance == null) Instance = this;

            Patch();

            Helpers.Logger.Info($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");

            NetcodePatcherAwake();
        }

        internal static void Patch()
        {
            Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

            Helpers.Logger.Debug("Patching...");

            Harmony.PatchAll();

            Helpers.Logger.Debug("Finished patching!");
        }

        internal static void Unpatch()
        {
            Helpers.Logger.Debug("Unpatching...");

            Harmony?.UnpatchSelf();

            Helpers.Logger.Debug("Finished unpatching!");
        }

        private void NetcodePatcherAwake()
        {
            try
            {
                var currentAssembly = Assembly.GetExecutingAssembly();
                var types = currentAssembly.GetTypes();

                foreach (var type in types)
                {
                    var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

                    foreach (var method in methods)
                    {
                        try
                        {
                            // Safely attempt to retrieve custom attributes
                            var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);

                            if (attributes.Length > 0)
                            {
                                try
                                {
                                    // Safely attempt to invoke the method
                                    method.Invoke(null, null);
                                }
                                catch (TargetInvocationException ex)
                                {
                                    // Log and continue if method invocation fails (e.g., due to missing dependencies)
                                    Logger.LogWarning($"Failed to invoke method {method.Name}: {ex.Message}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Handle errors when fetching custom attributes, due to missing types or dependencies
                            Logger.LogWarning($"Error processing method {method.Name} in type {type.Name}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Catch any general exceptions that occur in the process
                Logger.LogError($"An error occurred in NetcodePatcherAwake: {ex.Message}");
            }
        }

    }
}