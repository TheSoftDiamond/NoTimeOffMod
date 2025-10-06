using HarmonyLib;
using Unity.Netcode;
using NoTimeOff.Helpers;

namespace NoTimeOff
{
    [HarmonyPatch]
    internal class PatchCompanyMoon
    {
        public static string planetName = "";
        public static void SetPreviousPlanet()
        {
            planetName = StartOfRound.Instance.currentLevel.PlanetName;
            Logger.Info("Planet Name set to: " + planetName);
        }

        // Grab the planet name when you leave the planet

        [HarmonyPrefix]
        [HarmonyPriority(Priority.VeryHigh)]
        [HarmonyPatch(typeof(StartOfRound), "ShipLeave")]
        public static void ShipLeave()
        {
            // Grab the current moon name  
            SelectableLevel currentExpectedLevel = StartOfRound.Instance.currentLevel;
            SetPreviousPlanet();
        }

        
        // You get charged a day 

        [HarmonyPostfix]
        [HarmonyPriority(Priority.VeryHigh)]
        [HarmonyPatch(typeof(StartOfRound), "ShipHasLeft")]
        public static void ShipHasLeft()
        {
            {
                Logger.Info("Current Planet : " + StartOfRound.Instance.currentLevel.PlanetName);
                if (NetworkManager.Singleton.IsHost)
                {
                    Logger.Info("Server Detected");
                    Net.Instance.TimeOffServerRpc();
                }
            }
        }
    }


}