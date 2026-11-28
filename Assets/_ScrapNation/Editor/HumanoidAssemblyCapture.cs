using System;
using System.IO;
using ScrapNation.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace ScrapNation.EditorTools
{
    public static class HumanoidAssemblyCapture
    {
        // Source frames for the review sheet and recording. Keep transient frames outside Assets.
        [MenuItem("Tools/Scrap Nation/Capture Humanoid Assembly V01")]
        public static void Capture()
        {
            const int size = 240;
            string dir = "Library/HumanoidAssemblyFrames";
            Directory.CreateDirectory(dir);
            var previous = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            var target = RenderTexture.GetTemporary(size,size,24,RenderTextureFormat.ARGB32);
            var pixels = new Texture2D(size,size,TextureFormat.RGB24,false);
            var oldTarget = RenderTexture.active;
            try
            {
                var actor = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(HumanoidAssemblyBaker.Prefab));
                foreach (var t in actor.GetComponentsInChildren<Transform>()) t.gameObject.layer=31;
                var preview=actor.GetComponent<HumanoidAssemblyPreview>();
                var camera=new GameObject("Evidence camera").AddComponent<Camera>();
                camera.enabled=false;
                camera.cullingMask=1<<31;
                camera.orthographic=true;
                camera.orthographicSize=1.12f;
                camera.transform.position=new Vector3(0,.96f,-10);
                camera.clearFlags=CameraClearFlags.SolidColor;
                camera.backgroundColor=new Color(.68f,.65f,.61f);
                camera.targetTexture=target;
                var rest = new System.Collections.Generic.Dictionary<Transform,(Vector3 position, Quaternion rotation)>();
                foreach(var t in actor.GetComponentsInChildren<Transform>()) rest[t]=(t.localPosition,t.localRotation);
                foreach(string motion in new[]{"Idle","Walk","Work"})
                {
                    foreach(var item in rest) item.Key.SetLocalPositionAndRotation(item.Value.position,item.Value.rotation);
                    var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(HumanoidAssemblyBaker.Folder+"/"+motion+".anim");
                    for(int i=0;i<48;i++)
                    {
                        clip.SampleAnimation(actor,clip.length*i/48);
                        for(int variant=0;variant<2;variant++)
                        {
                            preview.FaceLeft(variant==1);
                            preview.SetClothing(variant==1);
                            preview.SetEquipment(variant==1);
                            preview.SetBoots(variant==0);
                            camera.Render();
                            RenderTexture.active=target;
                            pixels.ReadPixels(new Rect(0,0,size,size),0,0);
                            pixels.Apply(false);
                            File.WriteAllBytes(Path.Combine(dir,$"{motion}-{variant}-{i:00}.png"),pixels.EncodeToPNG());
                        }
                    }
                }
            }
            finally
            {
                RenderTexture.active=oldTarget;
                RenderTexture.ReleaseTemporary(target);
                UnityEngine.Object.DestroyImmediate(pixels);
                SceneManager.SetActiveScene(previous);
                EditorSceneManager.CloseScene(scene,true);
            }
        }
    }
}
