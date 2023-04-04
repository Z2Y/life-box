using UnityEngine;
using Utils;

namespace Controller
{
    [PrefabResource("Prefabs/vfx/Trail/CharacterTrail")]
    public class CharacterTrialRenderer : MonoBehaviour
    {
        private SpriteRenderer spRenderer;
        public float Alpha => spRenderer.color.a;

        public float defaultAlpha = 0.6f;

        private Vector3 originalScale;

        private Transform parent;

        private void Awake()
        {
            spRenderer = GetComponent<SpriteRenderer>();
            originalScale = transform.localScale;
        }

        public void Init(Transform parent)
        {
            var trailColor = spRenderer.color;
            trailColor.a = defaultAlpha;
            spRenderer.color = trailColor;
            transform.position = parent.position;
            transform.localScale = parent.localScale * (originalScale.x * defaultAlpha);
            transform.rotation = parent.rotation;
            this.parent = parent;
        }

        public void updateAlpha(float alpha)
        {
            var trailColor = spRenderer.color;
            trailColor.a -= alpha;
            spRenderer.color = trailColor;
            transform.localScale = parent.localScale * (originalScale.x * trailColor.a);
        }

        public void updateSortingOrder(int sortingOrder)
        {
            spRenderer.sortingOrder = sortingOrder;
        }
    }
}