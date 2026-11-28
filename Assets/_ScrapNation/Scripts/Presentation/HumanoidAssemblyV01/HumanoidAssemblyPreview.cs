using UnityEngine;
using UnityEngine.InputSystem;

namespace ScrapNation.Presentation
{
    public sealed class HumanoidAssemblyPreview : MonoBehaviour
    {
        public SpriteRenderer shirt, jacket, scarf;
        public SpriteRenderer[] feet, boots;
        public Animator animator;

        public void SetClothing(bool alternate)
        {
            shirt.enabled = !alternate;
            jacket.enabled = alternate;
        }

        public void SetEquipment(bool equipped) => scarf.enabled = equipped;

        public void SetBoots(bool equipped)
        {
            foreach (var foot in feet) foot.enabled = !equipped;
            foreach (var boot in boots) boot.enabled = equipped;
        }

        public void FaceLeft(bool left) => transform.localScale = new Vector3(left ? -1 : 1, 1, 1);

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (keyboard.digit1Key.wasPressedThisFrame) animator.Play("Idle", 0, 0);
            if (keyboard.digit2Key.wasPressedThisFrame) animator.Play("Walk", 0, 0);
            if (keyboard.digit3Key.wasPressedThisFrame) animator.Play("Work", 0, 0);
            if (keyboard.cKey.wasPressedThisFrame) SetClothing(!jacket.enabled);
            if (keyboard.eKey.wasPressedThisFrame) SetEquipment(!scarf.enabled);
            if (keyboard.bKey.wasPressedThisFrame) SetBoots(!boots[0].enabled);
            if (keyboard.fKey.wasPressedThisFrame) FaceLeft(transform.localScale.x > 0);
        }
    }
}
