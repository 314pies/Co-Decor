using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

namespace CoDeco
{
    public static class ParentContraintAdder
    {
        public static void AddParentContraint(this GameObject childToAdd, GameObject TargetParent)
        {
            var _oldConstraint = childToAdd.GetComponent<Child>();
            if (_oldConstraint != null) {
                _oldConstraint.enabled = false;
                GameObject.Destroy(_oldConstraint);
            }

            childToAdd.AddComponent<Child>().Initialize(TargetParent.transform);
            return;
        }
    
        public static void RemoveParentConstraint(this GameObject childAdded)
        {
            var _cons = childAdded.GetComponent<Child>();
            if (_cons != null)
            {
                GameObject.Destroy(_cons);
            }
        }


        public static void Obsolute_AddParentContraint(this GameObject childToAdd, GameObject TargetParent)
        {

            if (childToAdd == TargetParent) { return; }

            var c = childToAdd;

            ParentConstraint constraint = c.GetComponent<ParentConstraint>();
            if (constraint != null)
            {
                //Clear the old constraint
                GameObject.DestroyImmediate(constraint);
            }

            constraint = c.AddComponent<ParentConstraint>();


            constraint.AddSource(new ConstraintSource() { sourceTransform = TargetParent.transform, weight = 1 });
            //This does the equivalent of pressing the "Activate" button
            List<ConstraintSource> sources = new List<ConstraintSource>(constraint.sourceCount);
            constraint.GetSources(sources);

            for (int i = 0; i < sources.Count; i++)
            {
                Transform targetTransform = sources[i].sourceTransform;
                Debug.Log(targetTransform);
                Debug.Log(TargetParent.transform);


                //Vector3 positionOffset = sourceTransform.InverseTransformPoint(c.transform.position);
                Vector3 positionOffset = c.transform.position - targetTransform.position;

                Quaternion rotationOffset = Quaternion.Inverse(targetTransform.rotation * Quaternion.Inverse(c.transform.rotation));

                /*********/
                //var originParent = c.transform.parent;
                //c.transform.parent = TargetParent.transform;
                //Vector3 positionOffset = c.transform.localPosition;

                //Vector3 rotationOffset = c.transform.localEulerAngles;
                //c.transform.parent = originParent;
                /*********/

                constraint.SetTranslationOffset(i, positionOffset);
                constraint.SetRotationOffset(i, rotationOffset.eulerAngles);
                Debug.Log($"positionOffset: {positionOffset}");
                Debug.Log($"rotationOffset: {rotationOffset.eulerAngles}");
            }

            //And remember to turn it on!
            constraint.weight = 1f;//Master weight

            constraint.constraintActive = true;

        }
    }
}