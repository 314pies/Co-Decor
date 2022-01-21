using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using UnityEngine.SceneManagement;
using CoDeco.UI;

namespace CoDeco
{
    public class VirtualSpaceNetworkManager : NetworkManager
    {
        public static VirtualSpaceNetworkManager Singleton { get; private set; }

        private void Start()
        {
            Singleton = this;
            foreach (var p in NetworkObjectToRegister)
                NetworkClient.RegisterPrefab(p);
        }


        public static event Action OnClientConnectEvent;
        public static event Action OnClientDisconnectEvent;

        public string RoomName = "Leo's Virtual Space";
        public string GameModeName;

        public VirtualSpaceDiscovery networkDiscovery;

        public override void OnStartHost()
        {
            Debug.Log("VirtualSpaceNetworkManager.OnStartHost()");
            networkDiscovery.AdvertiseServer();
            Debug.Log(" networkDiscovery.AdvertiseServer()");
            EnterVirtualSpaceInitialization();
            CenterPopupManager.Singleton.PopupInfo("Virtual Space Created", "Enjoy!");
        }

        public override void OnStopHost()
        {
            Debug.Log("VirtualSpaceNetworkManager.OnStopHost()");
            networkDiscovery.StopDiscovery();
            Debug.Log(" networkDiscovery..StopDiscovery()");
        }

        public override void OnClientConnect(NetworkConnection conn)
        {
            base.OnClientConnect(conn);
            OnClientConnectEvent?.Invoke();
            EnterVirtualSpaceInitialization();
        }

        public override void OnClientDisconnect(NetworkConnection conn)
        {
            base.OnClientDisconnect(conn);
            OnClientDisconnectEvent?.Invoke();
        }




        //----------------------------------------

        public FloorPlanePlacer floorPlanePlacer;
        public GameObject EnterVirtualSpaceRootUI;
        /// <summary>
        /// After successful connect to a server (Create or Join one)
        /// </summary>
        public void EnterVirtualSpaceInitialization()
        {
            Debug.Log("EnterVirtualSpaceInitialization()");

            //Enable Floor Plane Placing First
            floorPlanePlacer.EnableFloorPlanePlacing();
            EnterVirtualSpaceRootUI.SetActive(false);

#if UNITY_EDITOR
            CamTransform.position = CameraInitialPos;
            CamTransform.eulerAngles = CameraInitialRot;

            FloorTransform.position = FloorPlaneInitialPos;
            FloorTransform.eulerAngles = FloorPlaneInitialRot;
#endif

        }

        public GameObject[] NetworkObjectToRegister;

        [Header("Editor Debug/Dev")]
        public Vector3 FloorPlaneInitialPos;
        public Vector3 FloorPlaneInitialRot;
        public Vector3 CameraInitialPos;
        public Vector3 CameraInitialRot;
        public Transform CamTransform;
        public Transform FloorTransform;

    }

}