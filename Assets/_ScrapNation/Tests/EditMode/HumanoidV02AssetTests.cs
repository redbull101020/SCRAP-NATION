using System.Collections;
using System.Linq;
using NUnit.Framework;
using ScrapNation.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;

namespace ScrapNation.Tests
{
    public class HumanoidV02AssetTests
    {
        private const string Folder = "Assets/_ScrapNation/Experiments/HumanoidAssembly";
        private static GameObject Prefab(string version) =>
            AssetDatabase.LoadAssetAtPath<GameObject>($"{Folder}{version}/SN_Humanoid_{version}.prefab");

        [Test]
        public void ReusesSkeletonClipsAndCombinedFace()
        {
            var original = Prefab("V01");
            var actor = Prefab("V02");
            Assert.That(actor, Is.Not.Null);
            Assert.That(actor.GetComponentsInChildren<SortingGroup>(), Has.Length.EqualTo(1));
            var animators = actor.GetComponentsInChildren<Animator>();
            Assert.That(animators, Has.Length.EqualTo(1));
            Assert.That(animators[0].runtimeAnimatorController, Is.SameAs(original.GetComponent<Animator>().runtimeAnimatorController));
            var bones = original.GetComponentsInChildren<Transform>().Where(t => t != original.transform && !t.GetComponent<SpriteRenderer>()).ToArray();
            Assert.That(bones, Has.Length.EqualTo(16));
            Assert.That(actor.GetComponentsInChildren<Transform>().Count(t => t != actor.transform && !t.GetComponent<SpriteRenderer>()), Is.EqualTo(16));
            foreach (var bone in bones)
            {
                var path = AnimationUtility.CalculateTransformPath(bone, original.transform);
                var other = actor.transform.Find(path);
                Assert.That(other, Is.Not.Null, path);
                Assert.That(other.localPosition, Is.EqualTo(bone.localPosition), path);
                Assert.That(other.localRotation, Is.EqualTo(bone.localRotation), path);
                Assert.That(other.localScale, Is.EqualTo(bone.localScale), path);
            }
            foreach (var clip in animators[0].runtimeAnimatorController.animationClips)
            foreach (var binding in AnimationUtility.GetCurveBindings(clip))
                Assert.That(actor.transform.Find(binding.path), Is.Not.Null, binding.path);
            var head = original.GetComponentsInChildren<SpriteRenderer>().Single(s => s.name == "Head_View");
            var views = actor.GetComponentsInChildren<SpriteRenderer>();
            Assert.That(views.Single(s => s.name == "Head_View").sprite, Is.SameAs(head.sprite));
            foreach (var view in views)
            {
                Assert.That(view.sprite, Is.Not.Null, view.name);
                Assert.That(view.sharedMaterial, Is.SameAs(head.sharedMaterial), view.name);
                Assert.That(view.sprite.pixelsPerUnit, Is.EqualTo(360), view.name);
                Assert.That(view.sprite.texture.filterMode, Is.EqualTo(FilterMode.Bilinear), view.name);
            }
            var preview = actor.GetComponent<HumanoidPreviewV02>();
            Assert.That(preview.nearSleeve.transform.parent.name, Is.EqualTo("UpperArm_Near"));
            Assert.That(preview.farSleeve.transform.parent.name, Is.EqualTo("UpperArm_Far"));
            Assert.That(preview.jacket.transform.parent.name, Is.EqualTo("Spine"));
        }

        [Test]
        public void BothBootSolesFollowSupportBelt()
        {
            var actor = Object.Instantiate(Prefab("V02"));
            try
            {
                var clip = actor.GetComponent<Animator>().runtimeAnimatorController.animationClips.Single(c => c.name == "Walk");
                foreach (var boot in actor.GetComponent<HumanoidPreviewV02>().boots)
                {
                    float start = boot.name.Contains("Near") ? 0 : .5f;
                    Vector3 sole = boot.sprite.bounds.min;
                    clip.SampleAnimation(actor, start * clip.length);
                    Vector3 first = boot.transform.TransformPoint(sole);
                    for (int i = 1; i <= 48; i++)
                    {
                        float phase = i / 96f;
                        clip.SampleAnimation(actor, (start + phase) * clip.length);
                        Vector3 travel = (boot.transform.TransformPoint(sole) - first) * 360;
                        Assert.That(travel.y, Is.EqualTo(0).Within(1f), boot.name);
                        Assert.That(travel.x, Is.EqualTo(-80 * phase).Within(1f), boot.name);
                        Assert.That(Mathf.Abs(Mathf.DeltaAngle(boot.transform.eulerAngles.z, 0)), Is.LessThan(.15f));
                    }
                }
            }
            finally { Object.DestroyImmediate(actor); }
        }

        [UnityTest]
        public IEnumerator TwoPlayModeEntriesStartWithDefaultEquipment()
        {
            for (int entry = 0; entry < 2; entry++)
            {
                yield return new EnterPlayMode();
                var actor = Object.Instantiate(Prefab("V02"));
                var preview = actor.GetComponent<HumanoidPreviewV02>();
                Assert.IsTrue(preview.shirt.enabled);
                Assert.IsFalse(preview.jacket.enabled);
                Assert.IsFalse(preview.scarf.enabled);
                preview.SetClothing(true);
                preview.SetScarf(true);
                yield return new ExitPlayMode();
            }
        }
    }
}
