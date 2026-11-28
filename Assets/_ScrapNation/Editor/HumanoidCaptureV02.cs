using System.IO;
using System.Linq;
using ScrapNation.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ScrapNation.EditorTools
{
    public static class HumanoidCaptureV02
    {
        [MenuItem("Tools/Scrap Nation/Capture Humanoid Assembly V02")]
        public static void Capture()
        {
            const int size = 280;
            const string dir = "Library/HumanoidFramesV02";
            Directory.CreateDirectory(dir);
            var previous = SceneManager.GetActiveScene();
            // Batch mode starts in an untitled scene. Unity requires a saved scene for additive authoring.
            if (Application.isBatchMode && string.IsNullOrEmpty(previous.path))
                previous = EditorSceneManager.OpenScene(HumanoidAssemblyBaker.ScenePath, OpenSceneMode.Single);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            var target = RenderTexture.GetTemporary(size, size, 24, RenderTextureFormat.ARGB32);
            var pixels = new Texture2D(size, size, TextureFormat.RGB24, false);
            var oldTarget = RenderTexture.active;
            try
            {
                var actor = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(HumanoidBakerV02.Prefab));
                var preview = actor.GetComponent<HumanoidPreviewV02>();
                var rest = actor.GetComponentsInChildren<Transform>().ToDictionary(t => t, t => (t.localPosition, t.localRotation));
                foreach (var t in rest.Keys) t.gameObject.layer = 31;
                var camera = new GameObject("Assembly capture").AddComponent<Camera>();
                camera.enabled = false;
                camera.cullingMask = 1 << 31;
                camera.orthographic = true;
                camera.orthographicSize = 1.12f;
                camera.transform.position = new Vector3(0, .96f, -10);
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.green;
                camera.targetTexture = target;
                bool firstRender = true;
                foreach (var clip in actor.GetComponent<Animator>().runtimeAnimatorController.animationClips)
                {
                    foreach (var item in rest)
                        item.Key.SetLocalPositionAndRotation(item.Value.localPosition, item.Value.localRotation);
                    for (int phase = 0; phase < 48; phase++)
                    {
                        clip.SampleAnimation(actor, clip.length * phase / 48);
                        for (int facing = 0; facing < 2; facing++)
                        for (int coat = 0; coat < 2; coat++)
                        for (int boots = 0; boots < 2; boots++)
                        for (int scarf = 0; scarf < 2; scarf++)
                        {
                            preview.FaceLeft(facing == 1);
                            preview.SetClothing(coat == 1);
                            preview.SetBoots(boots == 1);
                            preview.SetScarf(scarf == 1);
                            // The first offscreen draw can use stale sprite transforms after import.
                            if (firstRender)
                            {
                                camera.Render();
                                firstRender = false;
                            }
                            camera.Render();
                            RenderTexture.active = target;
                            pixels.ReadPixels(new Rect(0, 0, size, size), 0, 0);
                            string file = $"{clip.name}-{facing}{coat}{boots}{scarf}-{phase:00}.png";
                            File.WriteAllBytes(Path.Combine(dir, file), pixels.EncodeToPNG());
                        }
                    }
                }
            }
            finally
            {
                RenderTexture.active = oldTarget;
                RenderTexture.ReleaseTemporary(target);
                Object.DestroyImmediate(pixels);
                SceneManager.SetActiveScene(previous);
                EditorSceneManager.CloseScene(scene, true);
            }
        }
    }
}
