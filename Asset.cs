using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using BepInEx;
using Dissonance;
using System.Reflection;
using NoTimeOff;

namespace NoTimeOffAssets
{
    [HarmonyPatch]
    public class Assets
    {
        internal static AssetBundle bundle;

        public static bool ReadSettingEarly(string filePath, string settingName)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    // Log the error
                    //Log.LogWarning($"Config file not found: {filePath}");
                    return false;
                }
                else
                {
                    // Log the success
                    //Log.LogInfo($"Config file found: {filePath}");
                }
                // Read the file content
                string fileContent = File.ReadAllText(filePath);

                // Match the setting name and its value
                string pattern = $@"{Regex.Escape(settingName)}\s*=\s*(true|false)";

                // Match the setting
                Match match = Regex.Match(fileContent, pattern, RegexOptions.IgnoreCase);

                // If the setting is found, return its value
                if (match.Success)
                {
                    // Get the value
                    string value = match.Groups[1].Value;
                    //Log.LogInfo($"Setting '{settingName}' found: {value}");
                    return bool.Parse(value);
                }
                else
                {
                    // Log the error
                    //Log.LogWarning($"Setting '{settingName}' not found.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Log the error
                //Log.LogWarning($"An error occurred: {ex.Message}");
                return false;
            }
        }

        internal static void Load()
        {
            bundle = Assets.LoadAssetBundleFile("bundle");
        }

        private static AssetBundle LoadAssetBundleFile(string fileName)
        {
            try
            {
                string dllFolderPath = Path.GetDirectoryName(Plugin.Instance.Info.Location);
                string assetBundleFilePath = Path.Combine(dllFolderPath, fileName);
                return AssetBundle.LoadFromFile(assetBundleFilePath);
            }
            catch (Exception e)
            {
                NoTimeOff.Helpers.Logger.Warn($"Failed to load AssetBundle \"{fileName}\". {e}");
            }

            return null;
        }

        internal static AssetBundle LoadAssetBundle(string fileName)
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();

                using (Stream bundle = assembly.GetManifestResourceStream(fileName))
                {
                    return AssetBundle.LoadFromStream(bundle);
                }
            }
            catch (Exception ex)
            {
                NoTimeOff.Helpers.Logger.Error($"An error occurred while loading the asset bundle: {ex.Message}");
            }
            
            return null;
        }

        private static void RegisterNetworkPrefabs(params GameObject[] objects)
        {
            foreach (GameObject obj in objects)
            {
                NetworkManager.Singleton.AddNetworkPrefab(obj);
            }
        }

    }
} 