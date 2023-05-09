using UnityEngine;

namespace Controller
{
    public class FadingSpriteSystem : MonoBehaviour
    {
        void Update()
        {
            foreach (var c in FadingObject.Instances)
            {
                if (c.gameObject.activeSelf)
                {
                    var alpha = Mathf.SmoothDamp(c.alpha, c.targetAlpha, ref c.velocity, 0.1f, 1f);
                    if (Mathf.Abs(c.alpha - alpha) < 0.001f) continue;
                    c.alpha = alpha;
                    var color = new Color(1, 1, 1, c.alpha);
                    foreach (var spriteRenderer in c.renderers)
                    {
                        spriteRenderer.color = color;
                    }
                }
            }
        }
    }
}