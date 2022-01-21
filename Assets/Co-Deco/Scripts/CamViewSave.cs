
using CoDeco.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoDeco
{
    public class CamViewSave : MonoBehaviour
    {

        public Camera ARCamera;

        public Camera ViewCamera;
        public Vector3 savedPos;
        public Quaternion saveRot;

        public void SaveCamView()
        {
            savedPos = ARCamera.transform.position;
            saveRot = ARCamera.transform.rotation;
            CenterPopupManager.Singleton.PopupInfo("View Saved", "You can switch back to this view later!");
        }

        public GameObject ViewCamUI;
        public void SwitchToView()
        {
            ViewCamera.transform.position = savedPos;
            ViewCamera.transform.rotation = saveRot;

            //ARCamera.enabled = false;
            ViewCamera.enabled = true;
            ViewCamUI.SetActive(true);
            GetComponent<GameSessionManager>().CurrentActionStatus = ActionStatus.View;
            
        }

        public void SwitchBackToARCam()
        {
            //ARCamera.enabled = true;
            ViewCamera.enabled = false;
            ViewCamUI.SetActive(false);
            GetComponent<GameSessionManager>().CurrentActionStatus = ActionStatus.Default;
        }
    }
}