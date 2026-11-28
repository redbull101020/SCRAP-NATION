#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using ScrapNation.Presentation;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ScrapNation.Tests
{
    public class HumanoidV02PlaybackTests
    {
        [UnityTest]
        public IEnumerator EquipmentSwapsPreserveActorAndClipPhase()
        {
            const string scene = "Assets/_ScrapNation/Experiments/HumanoidAssemblyV02/SN_Humanoid_Assembly_V02.unity";
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(scene, new LoadSceneParameters(LoadSceneMode.Single));
            var actors = Object.FindObjectsByType<HumanoidPreviewV02>();
            Assert.That(actors, Has.Length.EqualTo(1));
            var actor = actors[0];
            var identity = actor.GetEntityId();
            var animator = actor.GetComponent<Animator>();
            foreach (bool left in new[] { false, true })
            foreach (string motion in new[] { "Idle", "Walk", "Work" })
            {
                actor.FaceLeft(left);
                animator.Play(motion, 0, 0);
                animator.Update(0);
                float duration = animator.GetCurrentAnimatorStateInfo(0).length;
                for (int phase = 0; phase < 48; phase++)
                {
                    float before = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                    Assert.That(before, Is.EqualTo(phase / 48f).Within(.002f));
                    foreach (bool jacket in new[] { false, true })
                    foreach (bool boots in new[] { false, true })
                    foreach (bool scarf in new[] { false, true })
                    {
                        actor.SetClothing(jacket);
                        actor.SetBoots(boots);
                        actor.SetScarf(scarf);
                        Assert.That(actor.shirt.enabled, Is.EqualTo(!jacket));
                        Assert.That(actor.jacket.enabled, Is.EqualTo(jacket));
                        Assert.That(actor.nearSleeve.enabled, Is.EqualTo(jacket));
                        Assert.That(actor.farSleeve.enabled, Is.EqualTo(jacket));
                        Assert.That(actor.scarf.enabled, Is.EqualTo(scarf));
                        foreach (var foot in actor.feet) Assert.That(foot.enabled, Is.EqualTo(!boots));
                        foreach (var boot in actor.boots) Assert.That(boot.enabled, Is.EqualTo(boots));
                        Assert.That(actor.transform.localScale.x, Is.EqualTo(left ? -1 : 1));
                        Assert.That(actor.GetEntityId(), Is.EqualTo(identity));
                        var now = animator.GetCurrentAnimatorStateInfo(0);
                        Assert.IsTrue(now.IsName(motion));
                        Assert.That(now.normalizedTime, Is.EqualTo(before));
                    }
                    if (!left && motion == "Work" && phase == 12 &&
                        System.Environment.GetEnvironmentVariable("SN_CAPTURE_V02") == "1")
                        SavePlayModeFrame(Camera.main);
                    animator.Update(duration / 48);
                }
            }
            LogAssert.NoUnexpectedReceived();
        }

        private static void SavePlayModeFrame(Camera camera)
        {
            var target = RenderTexture.GetTemporary(480, 480, 24);
            var previous = RenderTexture.active;
            var previousCameraTarget = camera.targetTexture;
            var pixels = new Texture2D(480, 480, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                pixels.ReadPixels(new Rect(0, 0, 480, 480), 0, 0);
                System.IO.File.WriteAllBytes("Library/HumanoidV02PlayMode.png", pixels.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = previousCameraTarget;
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(target);
                Object.Destroy(pixels);
            }
        }
    }
}
#endif
