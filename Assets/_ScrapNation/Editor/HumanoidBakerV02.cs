using System;
using System.IO;
using System.Linq;
using ScrapNation.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ScrapNation.EditorTools
{
    public static class HumanoidBakerV02
    {
        public const string Folder = "Assets/_ScrapNation/Experiments/HumanoidAssemblyV02";
        public const string Prefab = Folder + "/SN_Humanoid_V02.prefab";
        public const string Scene = Folder + "/SN_Humanoid_Assembly_V02.unity";
        private const string Art = "Assets/_ScrapNation/Art/HumanoidAssemblyV02/";
        [Serializable] private class Part
        {
            public string name, joint;
            public float offsetX, offsetY;
            public int order;
            public bool enabled, shared;
        }
        [Serializable] private class Layout { public Part[] parts; }

        [MenuItem("Tools/Scrap Nation/Bake Humanoid Assembly V02")]
        public static void Bake()
        {
            foreach (var output in new[] { Prefab, Scene })
                if (File.Exists(output) || Directory.Exists(output) || File.Exists(output + ".meta"))
                    throw new IOException("Bake output already exists: " + output);

            var parts = JsonUtility.FromJson<Layout>(File.ReadAllText("Tools/HumanoidAssemblyV02/layout.json")).parts;
            foreach (var p in parts.Where(p => !p.shared))
                if (!(AssetImporter.GetAtPath(Art + p.name + ".png") is TextureImporter))
                    throw new FileNotFoundException(p.name);
            Directory.CreateDirectory(Folder);
            AssetDatabase.Refresh();
            var previous = SceneManager.GetActiveScene();
            // Batch mode starts in an untitled scene. Unity requires a saved scene for additive authoring.
            if (Application.isBatchMode && string.IsNullOrEmpty(previous.path))
                previous = EditorSceneManager.OpenScene(HumanoidAssemblyBaker.ScenePath, OpenSceneMode.Single);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            try
            {
                var original = AssetDatabase.LoadAssetAtPath<GameObject>(HumanoidAssemblyBaker.Prefab);
                var actor = Object.Instantiate(original);
                actor.name = "SN_Humanoid_V02";
                var previousPreview = actor.GetComponent<HumanoidAssemblyPreview>();
                var material = previousPreview.shirt.sharedMaterial;
                Object.DestroyImmediate(previousPreview);
                var transforms = actor.GetComponentsInChildren<Transform>().ToDictionary(t => t.name);
                var views = actor.GetComponentsInChildren<SpriteRenderer>().ToDictionary(v => v.name);
                var preview = actor.AddComponent<HumanoidPreviewV02>();
                foreach (var p in parts)
                {
                    var oldName = p.name == "Jacket_Torso" ? "Jacket" : p.name;
                    if (!views.TryGetValue(oldName + "_View", out var renderer))
                        renderer = new GameObject().AddComponent<SpriteRenderer>();
                    renderer.name = p.name + "_View";
                    renderer.transform.SetParent(transforms[p.joint], false);
                    renderer.transform.localPosition = new Vector3(p.offsetX, p.offsetY) / 360f;
                    if (!p.shared) renderer.sprite = Import(p.name);
                    renderer.sharedMaterial = material;
                    renderer.sortingOrder = p.order;
                    renderer.enabled = p.enabled;
                    views[renderer.name] = renderer;
                }
                preview.shirt = views["Torso_View"];
                preview.jacket = views["Jacket_Torso_View"];
                preview.nearSleeve = views["Jacket_Sleeve_Near_View"];
                preview.farSleeve = views["Jacket_Sleeve_Far_View"];
                preview.scarf = views["Scarf_View"];
                preview.feet = new[] { views["Foot_Near_View"], views["Foot_Far_View"] };
                preview.boots = new[] { views["Boot_Near_View"], views["Boot_Far_View"] };
                PrefabUtility.SaveAsPrefabAssetAndConnect(actor, Prefab, InteractionMode.AutomatedAction, out bool saved);
                if (!saved) throw new IOException(Prefab);
                var camera = new GameObject("Assembly Camera").AddComponent<Camera>();
                camera.tag = "MainCamera";
                camera.orthographic = true;
                camera.orthographicSize = 1.15f;
                camera.transform.position = new Vector3(0, .96f, -10);
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(.65f, .62f, .59f);
                if (!EditorSceneManager.SaveScene(scene, Scene)) throw new IOException(Scene);
            }
            finally
            {
                SceneManager.SetActiveScene(previous);
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        public static void BakeAndCapture()
        {
            Bake();
            HumanoidCaptureV02.Capture();
        }

        private static Sprite Import(string name)
        {
            string path = Art + name + ".png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Center;
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.spritePixelsPerUnit = 360;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path) ?? throw new IOException(path);
        }
    }
}
