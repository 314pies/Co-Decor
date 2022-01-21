using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities
{
    public static void ChangeEveryLayers(this GameObject Obj, string layerName)
    {
        Obj.layer = LayerMask.NameToLayer(layerName);

        for (int i = 0; i < Obj.transform.childCount; i++)
        {
            GameObject Go = Obj.transform.GetChild(i).gameObject;
            Go.layer = LayerMask.NameToLayer("ObjPreview");
        }
    }
}
