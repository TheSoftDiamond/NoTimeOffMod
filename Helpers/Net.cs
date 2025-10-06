using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using NoTimeOffAssets;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

namespace NoTimeOff.Helpers
{
    [HarmonyPatch]
    internal class Net : NetworkBehaviour
    {
        public static Net Instance;
        private static GameObject netObject;

        public void Awake()
        {
            Instance = this;
        }

        public void Update()
        {

        }

        public override void OnNetworkSpawn()
        {
            Instance = this;

            base.OnNetworkSpawn();
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
        }

        [ServerRpc(RequireOwnership = false)]
        public void TimeOffServerRpc()
        {
            TimeOffClientRpc();
        }

        [HarmonyPostfix]
        [HarmonyPriority(Priority.VeryHigh)]
        [HarmonyPatch(typeof(GameNetworkManager), "Start")]
        private static void InitalizeServerObject()
        {
            if (netObject != null) return;

            netObject = new GameObject("NetObject");
            DontDestroyOnLoad(netObject);

            netObject.AddComponent<Net>();
            netObject.AddComponent<NetworkObject>();
            NetworkManager.Singleton.AddNetworkPrefab(netObject);
        }

        [HarmonyPrefix]
        [HarmonyPriority(Priority.VeryHigh)]
        [HarmonyPatch(typeof(Terminal), "Start")]
        private static void SpawnServerObject()
        {

            if (!FindObjectOfType<NetworkManager>().IsServer) return;

            GameObject net = Instantiate(netObject);
            net.GetComponent<NetworkObject>().Spawn(destroyWithScene: false);

        }

        [ClientRpc]
        public void TimeOffClientRpc()
        {
            if (PatchCompanyMoon.planetName == "71 Gordion")
            {
                {
                    // Decrement the day counter    
                    TimeOfDay.Instance.timeUntilDeadline -= TimeOfDay.Instance.totalTime;

                    // Update the profit quota  
                    TimeOfDay.Instance.UpdateProfitQuotaCurrentTime();

                    // Calculate days left    
                    int daysLeft = (int)(TimeOfDay.Instance.timeUntilDeadline / TimeOfDay.Instance.totalTime);

                    // Display days left    
                    HUDManager.Instance.DisplayDaysLeft(daysLeft);
                }
            }
        }
    }
}
