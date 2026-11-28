using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using ScrapNation.Presentation;

namespace ScrapNation.EditorTools
{
    public static class HumanoidAssemblyBaker
    {
        public const string Art = "Assets/_ScrapNation/Art/HumanoidAssemblyV01";
        public const string Folder = "Assets/_ScrapNation/Experiments/HumanoidAssemblyV01";
        public const string Prefab = Folder + "/SN_Humanoid_V01.prefab";
        public const string ScenePath = Folder + "/SN_Humanoid_Assembly_V01.unity";
        [Serializable] private class Bone { public string name, parent; public float x, y; }
        [Serializable] private class Part { public string name, joint; public float offsetX, offsetY; public int order; public bool enabled; }
        [Serializable] private class Layout { public float ppu; public Bone[] bones; public Part[] parts; }

        [MenuItem("Tools/Scrap Nation/Bake Humanoid Assembly V01")]
        public static void Bake()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Prefab))
                throw new InvalidOperationException("Generated prefab already exists. Run on a clean output folder.");
            var layout = JsonUtility.FromJson<Layout>(File.ReadAllText("Tools/HumanoidAssemblyV01/layout.json"));
            Directory.CreateDirectory(Folder);
            AssetDatabase.Refresh();
            // Author in an empty additive scene so the current scene stays intact.
            var previous = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            try
            {
                var actor = new GameObject("SN_Humanoid_V01", typeof(SortingGroup), typeof(Animator), typeof(HumanoidAssemblyPreview));
                var joints = new Dictionary<string, Transform>();
                var renderers = new Dictionary<string, SpriteRenderer>();
                foreach (var b in layout.bones)
                {
                    var joint = new GameObject(b.name).transform;
                    joint.position = new Vector3((b.x - 273) / layout.ppu, (703 - b.y) / layout.ppu);
                    joint.SetParent(b.parent.Length == 0 ? actor.transform : joints[b.parent], true);
                    joints.Add(b.name, joint);
                }
                foreach (var p in layout.parts)
                {
                    string path = Art + "/" + p.name + ".png";
                    var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    var settings = new TextureImporterSettings();
                    importer.ReadTextureSettings(settings);
                    settings.spriteAlignment = (int)SpriteAlignment.Center;
                    settings.spriteMeshType = SpriteMeshType.FullRect;
                    importer.SetTextureSettings(settings);
                    importer.spritePixelsPerUnit = layout.ppu;
                    importer.alphaIsTransparency = true;
                    importer.mipmapEnabled = false;
                    importer.filterMode = FilterMode.Bilinear;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.SaveAndReimport();
                    var view = new GameObject(p.name + "_View", typeof(SpriteRenderer));
                    view.transform.SetParent(joints[p.joint], false);
                    view.transform.localPosition = new Vector3(p.offsetX, p.offsetY) / layout.ppu;
                    var sr = view.GetComponent<SpriteRenderer>();
                    sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    if (!sr.sprite) throw new InvalidOperationException(path);
                    sr.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat");
                    sr.sortingOrder = p.order;
                    sr.enabled = p.enabled;
                    renderers.Add(p.name,sr);
                }
                var preview = actor.GetComponent<HumanoidAssemblyPreview>();
                preview.shirt = renderers["Torso"]; preview.jacket = renderers["Jacket"]; preview.scarf = renderers["Scarf"];
                preview.feet = new[]{renderers["Foot_Near"],renderers["Foot_Far"]};
                preview.boots = new[]{renderers["Boot_Near"],renderers["Boot_Far"]};
                preview.animator = actor.GetComponent<Animator>();
                preview.animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                var controller = AnimatorController.CreateAnimatorControllerAtPath(Folder + "/Assembly.controller");
                foreach (string motion in new[]{"Idle","Walk","Work"})
                {
                    var clip = MakeClip(motion, actor.transform, joints, layout.ppu);
                    AssetDatabase.CreateAsset(clip, Folder + "/" + motion + ".anim");
                    controller.AddMotion(clip);
                }
                preview.animator.runtimeAnimatorController = controller;
                PrefabUtility.SaveAsPrefabAssetAndConnect(actor, Prefab, InteractionMode.AutomatedAction);
                var camera = new GameObject("Assembly Camera", typeof(Camera)).GetComponent<Camera>();
                camera.tag = "MainCamera";
                camera.orthographic = true; camera.orthographicSize = 1.15f;
                camera.transform.position = new Vector3(0, .96f, -10);
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(.65f,.62f,.59f);
                if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new IOException(ScenePath);
                AssetDatabase.SaveAssets();
            }
            finally
            {
                SceneManager.SetActiveScene(previous);
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static AnimationClip MakeClip(string motion, Transform root, Dictionary<string,Transform> bones, float ppu)
        {
            const int samples = 48;
            float duration = motion == "Idle" ? 2 : motion == "Walk" ? 1 : 1.5f;
            var clip = new AnimationClip { name = motion, frameRate = 48 };
            var channels = new Dictionary<(string,string), AnimationCurve>();
            void Key(string joint, string property, float time, float value)
            {
                var id = (joint,property);
                if (!channels.TryGetValue(id,out var curve)) channels[id] = curve = new AnimationCurve();
                curve.AddKey(time,value);
            }
            for (int i=0;i<=samples;i++)
            {
                float phase = (float)i/samples, t=phase*duration;
                float sine=Mathf.Sin(2*Mathf.PI*phase);
                void Angle(string joint,float a) => Key(joint,"localEulerAnglesRaw.z",t,a);
                float drop = motion == "Walk" ? (-9+2*Mathf.Cos(4*Mathf.PI*phase))/ppu : 0;
                Key("Pelvis","m_LocalPosition.y",t,bones["Pelvis"].localPosition.y+drop);
                Angle("Spine",motion=="Work" ? -8+7*sine : 1.4f*sine);
                Angle("Neck",motion=="Work" ? 3-3*sine : -.7f*sine);
                foreach (var side in new[]{"Near","Far"})
                {
                    bool near=side=="Near";
                    float wave=near?sine:-sine;
                    float arm=motion=="Walk"?16*wave:motion=="Work"?18+13*wave:2*wave;
                    Angle("UpperArm_"+side,arm);
                    Angle("Forearm_"+side,motion=="Work"?34+19*wave:4+3*wave);
                    if (motion!="Walk") continue;
                    // Stance travels backwards at a constant belt speed. Solve the ankle in two links.
                    float f=Mathf.Repeat(phase+(near?0:.5f),1);
                    float x=f<=.5f ? 20-80*f : -20+80*(f-.5f);
                    float lift=f<=.5f ? 0 : 15*Mathf.Sin((f-.5f)*2*Mathf.PI);
                    var leg=bones["Shin_"+side]; var foot=bones["Foot_"+side];
                    Vector2 top=leg.localPosition, bottom=foot.localPosition;
                    Vector2 goal=top+bottom+new Vector2(x/ppu,lift/ppu-drop);
                    float a=top.magnitude,b=bottom.magnitude;
                    float bend=Mathf.Acos(Mathf.Clamp((goal.sqrMagnitude-a*a-b*b)/(2*a*b),-1,1));
                    // Forward-facing knee; distal link turns clockwise in this side view.
                    bend=-bend;
                    float thighAngle=Mathf.Atan2(goal.y,goal.x)-Mathf.Atan2(b*Mathf.Sin(bend),a+b*Mathf.Cos(bend));
                    float restTop=Mathf.Atan2(top.y,top.x), restBottom=Mathf.Atan2(bottom.y,bottom.x);
                    float upper=Mathf.DeltaAngle(0,(thighAngle-restTop)*Mathf.Rad2Deg);
                    float lower=Mathf.DeltaAngle(0,(bend-restBottom+restTop)*Mathf.Rad2Deg);
                    Angle("Thigh_"+side,upper); Angle("Shin_"+side,lower); Angle("Foot_"+side,-upper-lower);
                }
            }
            foreach(var item in channels)
            {
                var curve=item.Value;
                for(int k=0;k<curve.length;k++)
                {
                    AnimationUtility.SetKeyLeftTangentMode(curve,k,AnimationUtility.TangentMode.Linear);
                    AnimationUtility.SetKeyRightTangentMode(curve,k,AnimationUtility.TangentMode.Linear);
                }
                string path=AnimationUtility.CalculateTransformPath(bones[item.Key.Item1],root);
                AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve(path,typeof(Transform),item.Key.Item2),curve);
            }
            var settings=AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime=true;
            AnimationUtility.SetAnimationClipSettings(clip,settings);
            return clip;
        }
    }
}
