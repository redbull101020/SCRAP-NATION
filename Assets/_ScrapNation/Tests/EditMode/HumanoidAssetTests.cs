using System.Linq;
using NUnit.Framework;
using ScrapNation.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ScrapNation.Tests
{
    public class HumanoidAssetTests
    {
        const string Folder="Assets/_ScrapNation/Experiments/HumanoidAssemblyV01/";
        [Test] public void PrefabHasCompletePresentationBindings()
        {
            var actor=AssetDatabase.LoadAssetAtPath<GameObject>(Folder+"SN_Humanoid_V01.prefab");
            Assert.That(actor,Is.Not.Null);
            Assert.That(actor.GetComponent<SortingGroup>(),Is.Not.Null);
            var preview=actor.GetComponent<HumanoidAssemblyPreview>();
            Assert.That(preview.animator.runtimeAnimatorController.animationClips.Select(c=>c.name),
                        Is.EquivalentTo(new[]{"Idle","Walk","Work"}));
            var views=actor.GetComponentsInChildren<SpriteRenderer>(true);
            Assert.That(views.Length,Is.EqualTo(20));
            foreach(var view in views)
            {
                Assert.That(view.sprite,Is.Not.Null,view.name);
                Assert.That(view.sharedMaterial,Is.Not.Null,view.name);
                Assert.That(AssetDatabase.GetAssetPath(view.sharedMaterial),Is.EqualTo("Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat"),view.name);
                Assert.That(view.sprite.pixelsPerUnit,Is.EqualTo(360));
            }
            foreach(var t in actor.GetComponentsInChildren<Transform>())
                Assert.Zero(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject),t.name);
            Assert.That(AssetDatabase.LoadAssetAtPath<SceneAsset>(Folder+"SN_Humanoid_Assembly_V01.unity"),Is.Not.Null);
        }

        [Test] public void StanceAnkleTracksConstantBeltSpeedWithinOneSourcePixel()
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Folder+"SN_Humanoid_V01.prefab");
            var actor=Object.Instantiate(prefab);
            try
            {
                var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(Folder+"Walk.anim");
                foreach(var side in new[]{"Near","Far"})
                {
                    var ankle=actor.GetComponentsInChildren<Transform>().Single(t=>t.name=="Foot_"+side);
                    float start=side=="Near"?0f:.5f;
                    clip.SampleAnimation(actor,start*clip.length);
                    var first=ankle.position;
                    for(int i=1;i<=48;i++)
                    {
                        float phase=i/96f;
                        clip.SampleAnimation(actor,(start+phase)*clip.length);
                        Vector2 travel=(ankle.position-first)*360;
                        Assert.That(Mathf.Abs(travel.y),Is.LessThan(1f),side+" stance height");
                        Assert.That(Mathf.Abs(travel.x+80*phase),Is.LessThan(1f),side+" belt travel");
                        Assert.That(Mathf.Abs(Mathf.DeltaAngle(ankle.eulerAngles.z,0)),Is.LessThan(.15f),side+" sole angle");
                    }
                }
            }
            finally { Object.DestroyImmediate(actor); }
        }

        [Test] public void AllClipBindingsResolveToBonesAndLoop()
        {
            var actor=AssetDatabase.LoadAssetAtPath<GameObject>(Folder+"SN_Humanoid_V01.prefab");
            foreach(var clip in actor.GetComponent<Animator>().runtimeAnimatorController.animationClips)
            {
                Assert.That(AnimationUtility.GetAnimationClipSettings(clip).loopTime);
                var bindings=AnimationUtility.GetCurveBindings(clip);
                Assert.That(bindings,Is.Not.Empty);
                foreach(var b in bindings)
                {
                    Assert.That(actor.transform.Find(b.path),Is.Not.Null,b.path);
                    var keys=AnimationUtility.GetEditorCurve(clip,b).keys;
                    Assert.That(keys[keys.Length-1].value,Is.EqualTo(keys[0].value).Within(.001f));
                }
            }
        }
    }
}
