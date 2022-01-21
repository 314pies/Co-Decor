//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEditor;
//public class IconCreationManager : MonoBehaviour
//{
//    public GameObject[] objects;
//    public string IconsPath;
//    private RenderTexture texture;
//    private Camera camera;
//    public Transform anchor;
//    public float forward = 3.0f, up = 2.0f;
//    public Vector2 size = new Vector2(512, 512);

//    public Sprite Spr;

//    public void OnEnable()
//    {
//        camera = Camera.main;
//        texture = new RenderTexture((int)size.x, (int)size.y, 24);
//        foreach (GameObject original in objects)
//        {
//            GameObject go = Instantiate(original, anchor.transform.position, Quaternion.identity);
//            camera.transform.position = go.transform.position + go.transform.forward * forward;
//            camera.transform.position += Vector3.up * up;
//            camera.transform.LookAt(go.transform);
//            camera.targetTexture = texture;
//            camera.Render();
//            Texture2D tex = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);
//            Rect rectReadPicture = new Rect(0, 0, texture.width, texture.height);
//            RenderTexture.active = texture;
//            tex.ReadPixels(rectReadPicture, 0, 0);
//            Color32[] colors = tex.GetPixels32();
//            int i = 0;
//            Color32 transparent = colors[i];
//            for (; i < colors.Length; i++)
//            {
//                if (colors[i].Equals(transparent))
//                {
//                    colors[i] = new Color32();
//                }
//            }
//            tex.SetPixels32(colors);
//            RenderTexture.active = null;
//            string cardPath = "Assets/" + IconsPath + go.name + "_icon" + ".png";
//            byte[] bytes = tex.EncodeToPNG();
//            System.IO.File.WriteAllBytes(cardPath, bytes);
//            AssetDatabase.ImportAsset(cardPath);
//            TextureImporter ti = (TextureImporter)TextureImporter.GetAtPath(cardPath);
//            ti.textureType = TextureImporterType.Sprite;
//            ti.SaveAndReimport();
//            //Debug.Log(AssetDatabase.LoadAssetAtPath(cardPath, typeof(Sprite)));

//            DestroyImmediate(go.gameObject);
//            Spr = (Sprite)AssetDatabase.LoadAssetAtPath(cardPath, typeof(Sprite));
//        }
//    }
//}