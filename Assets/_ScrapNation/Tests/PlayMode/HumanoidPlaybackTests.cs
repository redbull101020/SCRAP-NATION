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
    public class HumanoidPlaybackTests
    {
        [UnityTest] public IEnumerator ScenePlaysAllLoopsAndSwapsInPlace()
        {
            const string path="Assets/_ScrapNation/Experiments/HumanoidAssemblyV01/SN_Humanoid_Assembly_V01.unity";
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(path,new LoadSceneParameters(LoadSceneMode.Single));
            var actors=Object.FindObjectsByType<HumanoidAssemblyPreview>(FindObjectsSortMode.None);
            Assert.That(actors.Length,Is.EqualTo(1));
            var actor=actors[0]; var id=actor.GetEntityId();
            var animator=actor.animator;
            Assert.That(Camera.main,Is.Not.Null);
            foreach(bool left in new[]{false,true})
            foreach(string motion in new[]{"Idle","Walk","Work"})
            {
                actor.FaceLeft(left);
                animator.Play(motion,0,0); animator.Update(0);
                float duration=animator.GetCurrentAnimatorStateInfo(0).length;
                for(int i=0;i<=48;i++)
                {
                    var before=animator.GetCurrentAnimatorStateInfo(0);
                    Assert.That(before.IsName(motion));
                    bool alt=(i/8)%2!=0;
                    actor.SetClothing(alt); actor.SetEquipment(alt); actor.SetBoots(!alt);
                    Assert.That(actor.jacket.enabled && !actor.shirt.enabled,Is.EqualTo(alt));
                    Assert.That(actor.scarf.enabled,Is.EqualTo(alt));
                    for(int f=0;f<2;f++)
                    {
                        Assert.That(actor.feet[f].enabled,Is.EqualTo(alt));
                        Assert.That(actor.boots[f].enabled,Is.EqualTo(!alt));
                    }
                    Assert.That(actor.GetEntityId(),Is.EqualTo(id));
                    Assert.That(animator.GetCurrentAnimatorStateInfo(0).normalizedTime,Is.EqualTo(before.normalizedTime));
                    Assert.That(actor.transform.localScale.x,Is.EqualTo(left?-1:1));
                    Assert.That(before.normalizedTime,Is.EqualTo(i/48f).Within(.002f));
                    animator.Update(duration/48);
                }
            }
            yield return null;
            LogAssert.NoUnexpectedReceived();
        }
    }
}
#endif
