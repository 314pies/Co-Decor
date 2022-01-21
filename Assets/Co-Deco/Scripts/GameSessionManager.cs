using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using TMPro;
using static CoDeco.FurnitureDB;
using Mirror;
using CoDeco.UI;

namespace CoDeco
{

    public enum ActionStatus
    {
        PlacingFloorPlane,
        Default,
        Edit,
        View,
        RealsizeScale
    }

    public class GameSessionManager : NetworkBehaviour
    {

        ActionStatus _currentActionStatus = ActionStatus.Default;
        public ActionStatus CurrentActionStatus
        {
            get { return _currentActionStatus; }
            set
            {
                _currentActionStatus = value;
                //Update UIs
                foreach (var u in AllUIs)
                {
                    if (u != null)
                        u.SetActive(false);
                }
                switch (_currentActionStatus)
                {
                    case ActionStatus.Default:
                        DefaultUIRoot.SetActive(true);
                        break;
                    case ActionStatus.Edit:
                        EditRoot.SetActive(true);
                        //Note: CurrentEditingInstance need to be set first
                        if (CurrentEditingInstance != null)
                        {
                            UngroupButton.SetActive(CurrentEditingInstance.GetComponent<CoDecoNetworkTransform>().IsGroup);
                        }
                        break;
                    case ActionStatus.View:
                        ViewUIRoot.SetActive(true);
                        break;
                    case ActionStatus.RealsizeScale:
                        ToRealWordSizeUIRoot.SetActive(true);
                        break;
                }
            }
        }

        /// <summary>
        /// Root of all UI
        /// </summary>
        public GameObject UIs;
        public void OnEnable()
        {
            UIs.SetActive(true);
        }

        public void OnDisable()
        {
            UIs.SetActive(false);
        }

        List<GameObject> AllUIs;

        [Header("Default")]
        public GameObject DefaultUIRoot;    
        [ReadOnly]
        public FurnitureResource CurrentSelectedFurnitureResource;

        //Can use this to determind wheather are we editing
        [ReadOnly]
        public GameObject CurrentEditingInstance
        {
            get { return _currentEditingInstance; }
            set
            {
                _currentEditingInstance = value;
                if (_currentEditingInstance != null)
                {
                    CurrentActionStatus = ActionStatus.Edit;
                }
                else
                {
                    CurrentActionStatus = ActionStatus.Default;
                }
            }
        }
        private GameObject _currentEditingInstance = null;

        float EditingRotateDelta = 0.0f;
        public float EditingRotateSpeed = 15.0f;

        [Header("Default/FunituresSelection")]
        public GameObject FunituresSelectionRoot;
        public Transform FunituresGridsRoot;
        public GameObject GridTemplate;

        [Header("Default/Group")]
        public GameObject GroupConfirmRoot;

        [Header("Edit")]
        public GameObject EditRoot;
        public GameObject PlaceButton;
        public GameObject DeleteButton, UngroupButton;

        [Header("View")]
        public GameObject ViewUIRoot;
        public GameObject ViewSelectionPanel;

        [Header("Unclassified")]
        public LayerMask PlacingRaycastLayer, ClickToSelectRaycastLayer;

        public FurnitureDB furnitureDB;




        Dictionary<int, GameObject> GridInstances = new Dictionary<int, GameObject>();


        private void Start()
        {
            InitialFunitureUIGrids();

            //Add all UI to this list
            AllUIs = new List<GameObject>()
            {
                DefaultUIRoot,
                EditRoot,
                ViewUIRoot,
                ToRealWordSizeUIRoot
            };
            CurrentActionStatus = ActionStatus.Default;
        }

        public void OpenFurnitureSelection()
        {
            FunituresSelectionRoot.SetActive(true);
        }

        public void CloseFurnitureSelection()
        {
            FunituresSelectionRoot.SetActive(false);
        }

        void InitialFunitureUIGrids()
        {
            for (int i = 0; i < furnitureDB.FurnitureResources.Length; i++)
            {
                var _newInstance = Instantiate(GridTemplate);
                _newInstance.SetActive(true);
                _newInstance.transform.SetParent(FunituresGridsRoot);
                _newInstance.transform.localScale = Vector3.one;
                var f = furnitureDB.FurnitureResources[i];
                _newInstance.GetComponent<FunitureUIGrid>().Initialize(i, f.Icon, f.Prefab.name, this);
                GridInstances.Add(i, _newInstance);
            }
        }

        public List<int> searchFurniture(string keyword)
        {
            List<int> matchFuni = new List<int>();
            var listOfFuni = furnitureDB.FurnitureResources;
            for (int i = 0; i < listOfFuni.Length; i++)
            {
                if (listOfFuni[i].Prefab.name.ToLower().Contains(keyword.ToLower()))
                {
                    matchFuni.Add(i);
                }
            }
            return matchFuni;
        }

        public void OnSearchKeywordUpdated(TMP_InputField inputField)
        {
            Debug.Log("FurniturePlacer.OnSearchKeywordUpdated(), keyword: " + inputField.text);
            var result = searchFurniture(inputField.text);
            foreach (var g in GridInstances)
            {
                g.Value.SetActive(result.Contains(g.Key));
            }
        }

        public void OnFurnitureGridClicked(int funitureID)
        {
            Debug.Log("OnFurnitureGridClicked: " + funitureID);
            CmdRequerSpawnGameObject(funitureID);
            FunituresSelectionRoot.SetActive(false);
        }

        //

        [Command(requiresAuthority = false)]
        public void CmdRequerSpawnGameObject(int funitureID, NetworkConnectionToClient sender = null)
        {
            Debug.Log("RequerSpawnGameObject() Called, funitureID: " + funitureID + ", sender: " + sender);
            CurrentSelectedFurnitureResource = furnitureDB.FurnitureResources[funitureID];

            //var newFunitureInstance = Instantiate(CurrentSelectedFurnitureResource.Prefab,
            //   Camera.main.transform.position + Camera.main.transform.forward * 0.7f,
            //   CurrentSelectedFurnitureResource.Prefab.transform.rotation);
            var newFunitureInstance = Instantiate(CurrentSelectedFurnitureResource.Prefab);

            NetworkServer.Spawn(newFunitureInstance);
            //Debug.Log(newFunitureInstance);
            TargetSpawnGameObhectResponse(sender, newFunitureInstance.GetComponent<NetworkIdentity>().netId);
        }

        [TargetRpc]
        public void TargetSpawnGameObhectResponse(NetworkConnection target, uint spawnedNetID)
        {

            // This will appear on the opponent's client, not the attacking player's
            Debug.Log($"TargetSpawnGameObhectResponse, spawnedNetID: {spawnedNetID}");


            if (CurrentEditingInstance != null)
            {
                //GameObject.Destroy(CurrentPlacingInstance);

                //To-do, send destroy request to server
                CurrentEditingInstance.GetComponent<CoDecoNetworkTransform>().CmdDestroy();
            }
            StartCoroutine(assignSpawnedNetID(spawnedNetID));
        }

        IEnumerator assignSpawnedNetID(uint spawnedNetID)
        {
            float _startTime = Time.time;
            float _maxRetryTime = 3.0f;
            do
            {
                if (NetworkClient.spawned.ContainsKey(spawnedNetID))
                {
                    CurrentEditingInstance = NetworkClient.spawned[spawnedNetID].gameObject;
                    CurrentEditingInstance.GetComponent<CoDecoNetworkTransform>().CmdStartEdit();
                    EditingRotateDelta = 0.0f;

                    CurrentEditingInstance.ChangeEveryLayers("ObjPreview");
                    yield return null;
                    break;
                }
                yield return new WaitForSeconds(0.05f);
            } while (Time.time < _startTime + _maxRetryTime);
        }

        //

        [Header("Group Selection")]
        public RectTransform GroupSelectionBoxVisual;
        public int groupSelectingFingerIndex = -1;
        public Vector2 dragStartPos = Vector2.zero, endDragPos = Vector2.zero;

        List<GameObject> groupSelectedCache = new List<GameObject>();
        private void Update()
        {

            //Which mean CurrentActionStatus should be Edit
            if (CurrentEditingInstance != null)
            {
                RaycastHit hitInfo;
                //Debug.Log(raycastPoint);
                if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hitInfo,
                    100, PlacingRaycastLayer))
                {
                    //Debug.Log("Hit: " + hitInfo.transform.name);
                    var hitPose = hitInfo.point;

                    var cameraForward = Camera.main.transform.forward;
                    var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;

                    CurrentEditingInstance.transform.position = hitInfo.point;

                    //Old obj "look foward" implementation
                    //CurrentEditingInstance.transform.rotation = Quaternion.LookRotation(cameraBearing);
                    //CurrentEditingInstance.transform.localEulerAngles += new Vector3(0, EditingRotateDelta, 0);
                }

                foreach (var t in TouchScreenInputWrapper.touches)
                {

                    if (t.fingerId == 0)
                    {
                        //Old obj "look foward" implementation
                        //EditingRotateDelta += t.deltaPosition.x * Time.deltaTime * EditingRotateSpeed;
                        CurrentEditingInstance.transform.Rotate(new Vector3(0, t.deltaPosition.x * Time.deltaTime * EditingRotateSpeed, 0));
                    }
                }
            }

            if (CurrentActionStatus == ActionStatus.Default
            && GroupConfirmRoot.activeInHierarchy == false
            && FunituresSelectionRoot.activeInHierarchy == false
            && ViewSelectionPanel.activeInHierarchy == false
            )
            {
                if (CurrentEditingInstance == null)
                {
                    foreach (var t in TouchScreenInputWrapper.touches)
                    {
                        if (t.phase == TouchPhase.Began)
                        {
                            //If clicked something, edit, else drag 
                            Ray ray = Camera.main.ScreenPointToRay(t.position);
                            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance: 100, layerMask: ClickToSelectRaycastLayer))
                            {
                                Debug.Log(hit.transform.name);
                                CurrentEditingInstance = hit.transform.gameObject;
                                CoDecoNetworkTransform _decoNetworkInstance = CurrentEditingInstance.GetComponent<CoDecoNetworkTransform>();
                                if (!_decoNetworkInstance.IsGroup)
                                {
                                    _decoNetworkInstance.CmdStartEdit();
                                }
                                else//Is a group, edit its parent
                                {
                                    var _p = _decoNetworkInstance.groupingParent.gameObject;
                                    _decoNetworkInstance = _p.GetComponent<CoDecoNetworkTransform>();

                                    CurrentEditingInstance = _p;
                                    _decoNetworkInstance.CmdStartEdit();

                                    //High light all group
                                    var _allGroup = Utilities.LookForSameGroup(_p);
                                    foreach (var _g in _allGroup)
                                    {
                                        _g.GetComponent<CoDecoNetworkTransform>().SetHlighLightStatus(CoDecoNetworkTransform.HighLightStatus.Grouping);
                                    }
                                }

                            }
                            // drag Select
                            else
                            {
                                groupSelectingFingerIndex = t.fingerId;
                                dragStartPos = t.position;
                            }
                        }

                        if (groupSelectingFingerIndex == t.fingerId)
                        {
                            endDragPos = t.position;
                            //Draw visual
                            Vector2 boxStart = dragStartPos;
                            Vector2 boxEnd = endDragPos;
                            Vector2 boxCenter = (boxStart + boxEnd) / 2;
                            GroupSelectionBoxVisual.position = boxCenter;
                            Vector2 boxSize = new Vector2(Mathf.Abs(dragStartPos.x - endDragPos.x), Mathf.Abs(dragStartPos.y - endDragPos.y));
                            GroupSelectionBoxVisual.sizeDelta = boxSize;


                            //Logical selection
                            var extents = boxSize / 2;
                            Rect selectionBox = new Rect();
                            selectionBox.min = boxCenter - extents;
                            selectionBox.max = boxCenter + extents;

                            //Check what is in range
                            var _objList = NetworkClient.spawned.Values;
                            groupSelectedCache.Clear();
                            foreach (var s in _objList)
                            {
                                var _c = s.gameObject.GetComponent<CoDecoNetworkTransform>();
                                if (_c != null)
                                {
                                    if (selectionBox.Contains(Camera.main.WorldToScreenPoint(s.transform.position)))
                                    {
                                        Debug.Log(s.name);
                                        _c.SetHlighLightStatus(CoDecoNetworkTransform.HighLightStatus.Grouping);
                                        groupSelectedCache.Add(_c.gameObject);
                                    }
                                    else
                                    {
                                        _c.SetHlighLightStatus(CoDecoNetworkTransform.HighLightStatus.None);
                                    }
                                }
                            }

                            if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                            {
                                groupSelectingFingerIndex = -1;
                                GroupSelectionBoxVisual.sizeDelta = Vector2.zero;
                                Debug.Log("Group Selection Ended");
                                if (groupSelectedCache.Count >= 2)
                                    GroupConfirmRoot.SetActive(true);
                                else
                                {
                                    foreach (var _obj in groupSelectedCache)
                                    {
                                        _obj.GetComponent<CoDecoNetworkTransform>().SetHlighLightStatus(CoDecoNetworkTransform.HighLightStatus.None);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void OnPlaceFunitureClicked()
        {
            Debug.Log("OnPlaceFunitureClicked()");
            if (CurrentEditingInstance != null)
            {
                var _coDecoTrans = CurrentEditingInstance.GetComponent<CoDecoNetworkTransform>();
                _coDecoTrans.CmdEndEdit();
                if (_coDecoTrans.IsGroup)
                {
                    //Cancel all highlight in the group
                    var _sameGroup = Utilities.LookForSameGroup(_coDecoTrans.groupingParent.gameObject);
                    foreach (var g_obj in _sameGroup)
                    {
                        g_obj.GetComponent<CoDecoNetworkTransform>().SetHlighLightStatus(CoDecoNetworkTransform.HighLightStatus.None);
                    }
                }

                CurrentEditingInstance = null;

            }
        }

        public void DeleteCurrentSelected()
        {
            if (CurrentEditingInstance != null)
            {
                var _netInstance = CurrentEditingInstance.GetComponent<CoDecoNetworkTransform>();
                if (_netInstance.IsGroup)
                {
                    var _sameGroup = Utilities.LookForSameGroup(_netInstance.groupingParent.gameObject);

                    foreach (var g_obj in _sameGroup)
                    {
                        g_obj.GetComponent<CoDecoNetworkTransform>().CmdDestroy();
                    }
                }
                else
                {
                    _netInstance.CmdDestroy();
                }
            }

            CurrentEditingInstance = null;
        }

        public void UngroupCurrentSelected()
        {

            if (CurrentEditingInstance != null)
            {
                var _netInstance = CurrentEditingInstance.GetComponent<CoDecoNetworkTransform>();
                if (_netInstance.IsGroup)
                {
                    var _sameGroup = Utilities.LookForSameGroup(_netInstance.groupingParent.gameObject);

                    foreach (var g_obj in _sameGroup)
                    {
                        g_obj.GetComponent<CoDecoNetworkTransform>().CmdRemoveGroup();
                    }
                    CurrentEditingInstance.GetComponent<CoDecoNetworkTransform>().SetHlighLightStatus(CoDecoNetworkTransform.HighLightStatus.SingleSelected);
                }
                else//should not need it
                {
                    _netInstance.CmdRemoveGroup();
                }
            }
        }

        public void OnGetCreateGroupResponse(bool resposne)
        {
            if (resposne == true)
            {
                //Start processing group selection
                //Select the one with lowest position as parent
                float lowestYPos = float.MaxValue;
                GameObject selectedParent = null;
                foreach (var g in groupSelectedCache)
                {
                    if (g.transform.position.y < lowestYPos)
                    {
                        lowestYPos = g.transform.position.y;
                        selectedParent = g;
                    }
                }
                foreach (var g in groupSelectedCache)
                {
                    g.GetComponent<CoDecoNetworkTransform>().CmdMakeGroup(selectedParent.GetComponent<NetworkIdentity>());
                }
            }
            else
            {
                foreach (var _obj in groupSelectedCache)
                {
                    _obj.GetComponent<CoDecoNetworkTransform>().SetHlighLightStatus(CoDecoNetworkTransform.HighLightStatus.None);
                }
            }
            GroupConfirmRoot.SetActive(false);
        }

        [Header("Real word size")]
        public GameObject ToRealWordSizeUIRoot;
        public GameObject FloorPlaneRoot;
        public float RealWordScale = 5.5f;
        public float scaleEffectSpeed = 3.0f;
        Coroutine scaleEffectCor = null;
        public void ToRealWordSize(bool isToRealWord)
        {
            if(scaleEffectCor != null) { return; }
            if (isToRealWord)
            {
                CurrentActionStatus = ActionStatus.RealsizeScale;
                scaleEffectCor = StartCoroutine(ScaleEffect(FloorPlaneRoot.transform.localScale.x, RealWordScale));
            }
            else
            {
                CurrentActionStatus = ActionStatus.Default;
                scaleEffectCor = StartCoroutine(ScaleEffect(FloorPlaneRoot.transform.localScale.x, 1));
            }            
        }

        IEnumerator ScaleEffect(float originValue,float targetValue)
        {
            float _p = 0;
            var _scaleAddUp = scaleEffectSpeed;
            //if (targetValue < originValue)
            //    _scaleAddUp = -scaleEffectSpeed;

            while (_p < 0.99)
            {
                var _newScale = Mathf.Lerp(originValue, targetValue, _p);
                yield return null;
                _p += Time.deltaTime * _scaleAddUp;
                FloorPlaneRoot.transform.localScale = new Vector3(_newScale, _newScale, _newScale);
            }

            FloorPlaneRoot.transform.localScale = new Vector3(targetValue, targetValue, targetValue);
            scaleEffectCor = null;
        }

        [SyncVar(hook = nameof(OnFloorPlaneUpdate))]
        public int FloorPlaneID;
        public FloorPlanePlacer floorPlanePlacer;

        public void OnFloorPlaneUpdate(int _, int newFloorPlane)
        {
            floorPlanePlacer.ClientChangeFloorPlane(newFloorPlane);
        }
    }

    public static partial class Utilities
    {

        public static List<GameObject> LookForSameGroup(GameObject Parent)
        {
            List<GameObject> _group = new List<GameObject>();
            //linear search, can be more optimized
            var _objList = NetworkClient.spawned.Values;
            foreach (var _obj in _objList)
            {
                var funiInstance = _obj.GetComponent<CoDecoNetworkTransform>();
                if (funiInstance != null)
                {
                    if (funiInstance.groupingParent != null && funiInstance.groupingParent.gameObject == Parent)
                    {
                        _group.Add(funiInstance.gameObject);
                    }
                }
            }
            return _group;
        }
    }
}