using UnityEngine;
using UnityEngine.InputSystem;

namespace ScrapNation.Presentation
{
    public sealed class HumanoidPreviewV02 : MonoBehaviour
    {
        public SpriteRenderer shirt, jacket, nearSleeve, farSleeve, scarf;
        public SpriteRenderer[] feet, boots;

        public void SetClothing(bool alternate)
        {
            shirt.enabled = !alternate;
            jacket.enabled = alternate;
            nearSleeve.enabled = alternate;
            farSleeve.enabled = alternate;
        }

        public void SetBoots(bool equipped)
        {
            foreach (var foot in feet) foot.enabled = !equipped;
            foreach (var boot in boots) boot.enabled = equipped;
        }

        public void SetScarf(bool equipped) => scarf.enabled = equipped;
        public void FaceLeft(bool left) => transform.localScale = new Vector3(left ? -1 : 1, 1, 1);

        private void Update()
        {
            var keys = Keyboard.current;
            if (keys == null) return;
            if (keys.digit1Key.wasPressedThisFrame) GetComponent<Animator>().Play("Idle", 0, 0);
            if (keys.digit2Key.wasPressedThisFrame) GetComponent<Animator>().Play("Walk", 0, 0);
            if (keys.digit3Key.wasPressedThisFrame) GetComponent<Animator>().Play("Work", 0, 0);
            if (keys.cKey.wasPressedThisFrame) SetClothing(!jacket.enabled);
            if (keys.bKey.wasPressedThisFrame) SetBoots(!boots[0].enabled);
            if (keys.eKey.wasPressedThisFrame) SetScarf(!scarf.enabled);
            if (keys.fKey.wasPressedThisFrame) FaceLeft(transform.localScale.x > 0);
        }
    }
}
