using UnityEngine;
using System.Collections;

namespace bullet.fx.pack
{
    public class TrailAutoDestruct : MonoBehaviour
    {
        public IEnumerator FadeOutTrail(LineRenderer line, float fadeTime)
        {
            float elapsed = 0f;
            // Store the initial properties as the original script does
            float startWidth = 0.05f;
            float endWidth = 0.01f;
            Color startColor = line.material.color;

            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
                line.startWidth = Mathf.Lerp(startWidth, 0f, elapsed / fadeTime);
                line.endWidth = Mathf.Lerp(endWidth, 0f, elapsed / fadeTime);
                line.material.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                yield return null;
            }
            // Destroy the game object this script is attached to (the trail)
            Destroy(gameObject);
            yield break;
        }
    }
}
