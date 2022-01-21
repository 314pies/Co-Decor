using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Sirenix.OdinInspector;
using System;

namespace CoDeco
{
    [CreateAssetMenu(fileName = "FurnitureDB", menuName = "ScriptableObjects/AddFurnitureDB", order = 1)]
    public class FurnitureDB : ScriptableObject
    {

        [SerializeField]
        public GameObject[] FurnitureRawPrefabs;

        [System.Serializable]
        public class FurnitureResource
        {
            public GameObject Prefab;
            public Sprite Icon;
        }

        [SerializeField]
        public FurnitureResource[] FurnitureResources;
              
#if UNITY_EDITOR
        [Header("Editor Tools")]
        public string IconsPath;
        /// <summary>
        /// Work kind of buggy, the AssetPreview.GetAssetPreview() doesn't seems always work
        /// </summary>
        [Button]
        public void GenerateRunTimeResources()
        {
            var _resources = new List<FurnitureResource>();
            foreach (var f in FurnitureRawPrefabs)
            {
                try
                {
                    var thumbNaik = AssetPreview.GetAssetPreview(f);


                    byte[] bytes = thumbNaik.EncodeToPNG();
                    string cardPath = "Assets/" + IconsPath + "/" + f.name + "_icon" + ".png";

                    System.IO.File.WriteAllBytes(cardPath, bytes);
                    AssetDatabase.ImportAsset(cardPath);
                    TextureImporter ti = (TextureImporter)TextureImporter.GetAtPath(cardPath);
                    ti.textureType = TextureImporterType.Sprite;
                    ti.SaveAndReimport();
                    var newResource = new FurnitureResource();
                    newResource.Prefab = f;
                    newResource.Icon = (Sprite)AssetDatabase.LoadAssetAtPath(cardPath, typeof(Sprite));
                    _resources.Add(newResource);
                }
                catch (Exception exp)
                {
                    Debug.LogError(exp);
                    Debug.Log(f.name);
                }               
            }
            FurnitureResources = _resources.ToArray();
            EditorUtility.SetDirty(this);
        }
#endif
    }
}

