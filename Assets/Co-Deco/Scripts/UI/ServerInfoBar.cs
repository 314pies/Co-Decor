using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace CoDeco.UI
{
    public class ServerInfoBar : MonoBehaviour
    {
        public VirtualSpaceJoin Manager;
        public TMP_Text RoomName, ModeName, Address;
        public long ServerID;

        public void SetServerInfo(long serverID, string roomName, string modeName, string address, VirtualSpaceJoin manager)
        {
            this.ServerID = serverID;
            RoomName.text = roomName;
            this.Manager = manager;
            return;
            ModeName.text = modeName;
            Address.text = address;
            
        }

        public void OnJoinPressed()
        {
            Manager.ConnectToRoom(ServerID);
        }
    }
}