using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CoDeco.UI
{
    public class VirtualSpaceEntering : MonoBehaviour
    {
        public void OnCreateClicked()
        {
            VirtualSpaceNetworkManager.Singleton.StartHost();
        }
    }
}