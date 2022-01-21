using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Mirror;
using System.Linq;

namespace CoDeco
{
    public class FloorPlanePlacer : MonoBehaviour
    {
        // public GameObject prefab;

        public ARRaycastManager aRRaycastManager;
        public ARPlaneManager aRPlaneManager;


        public GameObject floorPlaneInstance;
        public ARSession aRSession;

        public GameObject PlaceFloorPlaneUIs;

        private Vector2 touchPosition;

        static List<ARRaycastHit> hits = new List<ARRaycastHit>();

        bool IsPlacingFloorPlane
        {
            get { return this.enabled; }
            set
            {
                Debug.Log("FloorPlanePlacer.IsPlacingFloorPlane: " + value);

                if (value == true)
                {
                    aRSession.Reset();
                    floorPlaneInstance.transform.position = new Vector3(0, 999, 0);
                }


                aRPlaneManager.enabled = value;
                var arPlanes = GameObject.FindGameObjectsWithTag("ARPlane");
                foreach (var p in arPlanes)
                {
                    p.SetActive(value);
                }

                this.enabled = value;
                PlaceFloorPlaneUIs.SetActive(value);
            }
        }

        public GameObject FloorPlaneSwitchButton;
        // Start is called before the first frame update
        void Awake()
        {
           
        }

        // Update is called once per frame
        void Update()
        {
            if (IsPlacingFloorPlane)
            {
                //var raycastPoint = Input.GetTouch(0).position;
                var raycastPoint = Camera.main.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
                //Debug.Log(raycastPoint);
                if (aRRaycastManager.Raycast(raycastPoint, hits, TrackableType.PlaneWithinPolygon))
                {
                    var hitPose = hits[0].pose;

                    var cameraForward = Camera.main.transform.forward;
                    var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;

                    floorPlaneInstance.transform.position = hitPose.position;
                    floorPlaneInstance.transform.rotation = Quaternion.LookRotation(cameraBearing);
                }
            }
        }

        public GameSessionManager gameSessionManager;

        public void EnableFloorPlanePlacing()
        {
            IsPlacingFloorPlane = true;
            FloorPlaneSwitchButton.SetActive(VirtualSpaceNetworkManager.Singleton.mode == NetworkManagerMode.Host);
            //
            gameSessionManager.enabled = false;
        }

        public void PlaceFloorPlane()
        {
            IsPlacingFloorPlane = false;
            //
            gameSessionManager.enabled = true;

#if UNITY_EDITOR
            Camera.main.GetComponent<FreeFlyCamera>().enabled = true;
#endif
        }

        [Header("Floor Plane")]
        public GameObject[] FloorPlanes;
        public int FloorPlaneIndex = 0;

        public void ServerOnFloorPlaneSwitchButtonClick()
        {

            FloorPlaneIndex++;
            if (FloorPlaneIndex >= FloorPlanes.Length)
            {
                FloorPlaneIndex = 0;
            }
            ServerChangeFloorPlane(FloorPlaneIndex);
        }

        //Should only be called on server
        public void ServerChangeFloorPlane(int index)
        {
            //Clear all networkObject on scene
            var allNetworkObj = NetworkServer.spawned.Values.ToArray();
            for(int i=0;i< allNetworkObj.Length; i++)
            {
                if (allNetworkObj[i].GetComponent<CoDecoNetworkTransform>() != null)
                {
                    NetworkServer.Destroy(allNetworkObj[i].gameObject);
                }
            }

            gameSessionManager.FloorPlaneID = index;
        }

        public void ClientChangeFloorPlane(int index)
        {
            Debug.Log(index);
            foreach(var f in FloorPlanes) { f.SetActive(false); }
            FloorPlanes[index].SetActive(true);
        }
    }

}
