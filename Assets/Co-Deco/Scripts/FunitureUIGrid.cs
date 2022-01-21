using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoDeco
{
    public class FunitureUIGrid : MonoBehaviour
    {
        public Image Icon;

        [ShowInInspector]
        int ID;
        [ShowInInspector]
        public string FunName;

        GameSessionManager furniturePlacer;

        public void Initialize(int id, Sprite img, string funitureName, GameSessionManager placer)
        {
            Icon.sprite = img;
            ID = id;
            furniturePlacer = placer;
            FunName = funitureName;
        }

        public void OnGridClicked()
        {
            furniturePlacer.OnFurnitureGridClicked(ID);
        }

    }
}
