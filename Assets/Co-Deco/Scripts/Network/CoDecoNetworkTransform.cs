// ʻOumuamua's light curve, assuming little systematic error, presents its
// motion as tumbling, rather than smoothly rotating, and moving sufficiently
// fast relative to the Sun.
//
// A small number of astronomers suggested that ʻOumuamua could be a product of
// alien technology, but evidence in support of this hypothesis is weak.
using UnityEngine;
using Mirror;
namespace CoDeco
{
    [DisallowMultipleComponent]
    public class CoDecoNetworkTransform : NetworkBehaviour
    {
        

        [Command(/*requiresAuthority = false*/)]
        public void CmdRequerSpawnGameObject(Vector3 funitureID, Vector3 Rotation)
        {

        }

        Transform floorPlane_dontUseThis;
        public Transform FloorPlane
        {
            get
            {                
                if (floorPlane_dontUseThis == null)
                {
                    var _f = GameObject.FindGameObjectWithTag("FloorPlane");
                    if (_f != null)
                    {
                        floorPlane_dontUseThis = _f.transform;
                    }
                }
                return floorPlane_dontUseThis;
            }
        }

        private void Start()
        {
            transform.parent = FloorPlane;
            syncInterval = 0.05f;
        }

        /// <summary>
        /// It will give the client who req authority, and put the obj into "edit" mode
        /// </summary>
        /// <param name="sender"></param>
        [Command(requiresAuthority = false)]
        public void CmdStartEdit(NetworkConnectionToClient sender = null)
        {
            netIdentity.AssignClientAuthority(sender);
            currentOwnerID = sender.connectionId;
            IsEditing = true;
            OnIsEditingUpdate(false, IsEditing);
        }

        [SyncVar]
        public int currentOwnerID;

        [Command(requiresAuthority = false)]
        public void CmdEndEdit(NetworkConnectionToClient sender = null)
        {
            netIdentity.RemoveClientAuthority();
            currentOwnerID = -1;
            IsEditing = false;
            OnIsEditingUpdate(false, IsEditing);
        }

        [Command(requiresAuthority = false)]
        public void CmdDestroy()
        {
            NetworkServer.Destroy(gameObject);
        }

        [Command]
        public void CmdUpdatePosition(Vector3 localPos, Vector3 localRot)
        {
            latestLocalPos = localPos;
            latestLocalRot = localRot;
        }

#if UNITY_EDITOR
        [Header("Debug")]
        public Vector3 FloorPlaneAngle;
#endif

        Vector3 lastSentPos, lastSenttRot;
        void FixedUpdate()
        {
            if (hasAuthority)
            {
                if (lastSentPos != transform.localPosition || lastSenttRot != transform.localEulerAngles)
                {
                    lastSentPos = transform.localPosition;
                    lastSenttRot = transform.localEulerAngles;
                    CmdUpdatePosition(lastSentPos, lastSenttRot);
                }
            }
            else
            {
                if (!IsGroup 
                    || (IsGroup && groupingParent == netIdentity)
                )
                {
                    transform.localPosition = latestLocalPos;
                    transform.localEulerAngles = latestLocalRot;
                }
                //If it's group, just follow the parent
            }
        }

        [SyncVar]
        public Vector3 latestLocalPos;
        [SyncVar]
        public Vector3 latestLocalRot;

        [SyncVar(hook = nameof(OnIsEditingUpdate))]
        public bool IsEditing;

        public void OnIsEditingUpdate(bool _, bool _isEditing)
        {
            if (_isEditing)
            {
                if (IsGroup)
                {
                    SetHlighLightStatus(HighLightStatus.Grouping);
                }
                else
                {
                    SetHlighLightStatus(HighLightStatus.SingleSelected);
                }
            }
            else
            {
                SetHlighLightStatus(HighLightStatus.None);                
            }

        }

        public enum HighLightStatus
        {
            None,
            SingleSelected,
            Grouping
        }

        [ShowInInspector]
        HighLightStatus LatestHighLightStatus;
        public void SetHlighLightStatus(HighLightStatus highLightStatus)
        {
            var outLiner = gameObject.GetComponent<Outline>();
            if (outLiner == null)
            {
                outLiner = gameObject.AddComponent<Outline>();
            }

            switch (highLightStatus)
            {
                case HighLightStatus.None:
                    outLiner.enabled = false;
                    gameObject.ChangeEveryLayers("Funiture");
                    break;
                case HighLightStatus.SingleSelected:
                    outLiner.enabled = true;
                    outLiner.OutlineColor = Color.yellow;
                    outLiner.OutlineWidth = 3.6f;
                    gameObject.ChangeEveryLayers("ObjPreview");
                    break;
                case HighLightStatus.Grouping:
                    outLiner.enabled = true;
                    outLiner.OutlineColor = Color.green;
                    outLiner.OutlineWidth = 4.5f;
                    gameObject.ChangeEveryLayers("ObjPreview");
                    break;
            }
            LatestHighLightStatus = highLightStatus;
        }

        [SyncVar(hook = nameof(OnGroupingStatusUpdated))]
        public NetworkIdentity groupingParent;
        public void OnGroupingStatusUpdated(NetworkIdentity _, NetworkIdentity _newValue)
        {
            //may not need this?
            if (_newValue != null && LatestHighLightStatus == HighLightStatus.SingleSelected)
            {
                SetHlighLightStatus(HighLightStatus.Grouping);
            }
            Debug.Log(_newValue);
            if (_newValue != null)
            {
                //return;
                gameObject.AddParentContraint(_newValue.gameObject);
            }
            else//Remove group constraint
            {
                Debug.Log("Removing Group");                
                gameObject.RemoveParentConstraint();
                SetHlighLightStatus(HighLightStatus.None);
            }
        }

        public bool IsGroup { get { return groupingParent != null; } }

        [Command(requiresAuthority = false)]
        public void CmdMakeGroup(NetworkIdentity parent)
        {
            groupingParent = parent;
        }

        [Command(requiresAuthority = false)]
        public void CmdRemoveGroup()
        {
            groupingParent = null;
            latestLocalPos = transform.localPosition;
            latestLocalRot = transform.localEulerAngles;
        }
#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            var _str = "Editor ID: ";
            if (currentOwnerID == -1)
            {
                GUI.contentColor = Color.gray;
                _str += "None";
            }
            else
            {
                GUI.contentColor = Color.green;
                _str += currentOwnerID;
            }
               


            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.1f, _str);
            GUI.contentColor = Color.white;
        }
#endif
    }
}
