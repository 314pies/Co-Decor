using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Michsky.UI.ModernUIPack;


namespace CoDeco.UI
{
    public class VirtualSpaceJoin : MonoBehaviour
    {
        public Transform ServerInfoBarInstancesRoot;
        public GameObject ServerInfoBarPrefab;


        public float UpdateRoomListInterval = 0.3f;
        [ReadOnly]
        [ShowInInspector]
        float lastUpdateRoomListTime = float.MinValue;


        private void OnEnable()
        {
            try
            {
                Debug.Log("StartDiscovery()");
                VirtualSpaceDiscovery.Singleton.StartDiscovery();
            }
            catch (System.Exception exp)
            {
                Debug.Log("SearchLANRoom.OnEnable() exception: " + exp);
            }

        }

        private void OnDisable()
        {
            try
            {

                if (VirtualSpaceDiscovery.Singleton != null)
                {
                    VirtualSpaceDiscovery.Singleton.StopDiscovery();
                }
                VirtualSpaceNetworkManager.OnClientConnectEvent -= OnConnected;
                VirtualSpaceNetworkManager.OnClientDisconnectEvent -= OnConnectedFailed;
            }
            catch (System.Exception exp)
            {
                Debug.Log("SearchLANRoom.OnDisable() exception: " + exp);
            }
        }

        [ShowInInspector]
        public Dictionary<long, DiscoveryResponse> FoundedServers = new Dictionary<long, DiscoveryResponse>();
        [ShowInInspector]
        public Dictionary<long, GameObject> ServerInfoBarInstances = new Dictionary<long, GameObject>();


        // Update is called once per frame
        void Update()
        {
            if (Time.time > lastUpdateRoomListTime + UpdateRoomListInterval)
            {
                UpdateServerListUI();
                lastUpdateRoomListTime = Time.time;
            }
        }

        void UpdateServerListUI()
        {
            VirtualSpaceDiscovery.Singleton.CleanUpTimeoutServer();
            FoundedServers = VirtualSpaceDiscovery.Singleton.FoundedServers;

            //Keep UI sync up with LWPNetworkDiscovery.FoundedServers

            foreach (var _key in FoundedServers.Keys)
            {
                if (!ServerInfoBarInstances.ContainsKey(_key))
                {
                    var newServerInfoBarInstance = Instantiate(ServerInfoBarPrefab);
                    ServerInfoBarInstances.Add(_key, newServerInfoBarInstance);

                    newServerInfoBarInstance.transform.SetParent(ServerInfoBarInstancesRoot);
                    newServerInfoBarInstance.transform.localScale = new Vector3(1, 1, 1);
                    newServerInfoBarInstance.SetActive(true);
                }

                //InitiaInfo
                ServerInfoBarInstances[_key].GetComponent<ServerInfoBar>().SetServerInfo(
                    FoundedServers[_key].serverId,
                    FoundedServers[_key].RoomName,
                    FoundedServers[_key].ModeName,
                    FoundedServers[_key].EndPoint.Address.ToString(),
                    this
                 );
            }

            List<long> removals = new List<long>();
            foreach (var _key in ServerInfoBarInstances.Keys)
            {
                if (!FoundedServers.ContainsKey(_key))
                {
                    removals.Add(_key);
                }
            }
            foreach (var _key in removals)
            {
                var _instance = ServerInfoBarInstances[_key];
                ServerInfoBarInstances.Remove(_key);
                Destroy(_instance);
            }
        }

        //To block all input until we got a response
        [SerializeField]
        GameObject InputBlocker;

        string latestAttempJoinRoomName;
        public void ConnectToRoom(long serverID)
        {
            if (FoundedServers.ContainsKey(serverID))
            {
                DiscoveryResponse _info = FoundedServers[serverID];
                Debug.Log("SearchLANRoom.ConnectToRoom()");
                InputBlocker.SetActive(true);
                VirtualSpaceNetworkManager.OnClientConnectEvent += OnConnected;
                VirtualSpaceNetworkManager.OnClientDisconnectEvent += OnConnectedFailed;
                VirtualSpaceNetworkManager.Singleton.StartClient(_info.uri);

                CenterPopupManager.Singleton.PopupLoading("Joining " + _info.RoomName
                    , "connecting to " + _info.EndPoint.Address.ToString());
                latestAttempJoinRoomName = _info.RoomName;
            }
        }

        void OnConnected()
        {
            Debug.Log("SearchLANRoom. OnConnected(), Disabling SearchLANRoom");
            gameObject.SetActive(false);
            InputBlocker.SetActive(false);
            VirtualSpaceNetworkManager.OnClientConnectEvent -= OnConnected;
            VirtualSpaceNetworkManager.OnClientDisconnectEvent -= OnConnectedFailed;
            CenterPopupManager.Singleton.CloseWindow();
            CenterPopupManager.Singleton.PopupInfo("Success!", "Successfully join '" + latestAttempJoinRoomName + "'");
            
        }

        void OnConnectedFailed()
        {
            Debug.Log("SearchLANRoom. OnConnectedFailed()");
            InputBlocker.SetActive(false);
            Debug.Log("SearchLANRoom. Failed to connect to room.");
            //CenterPopupManager.Singleton.CloseWindow();
            VirtualSpaceNetworkManager.OnClientConnectEvent -= OnConnected;
            VirtualSpaceNetworkManager.OnClientDisconnectEvent -= OnConnectedFailed;
            CenterPopupManager.Singleton.PopupInfo("Connection Failed.", "Cannot connect to target room.");
        }
    }
}
