using System.Collections;
using UnityEngine;

public class ScreenFader : MonoBehaviour
{
    [SerializeField] private CanvasGroup cg;
    [SerializeField] private float duration = 0.25f;

    private void Awake()
    {
        if (!cg) cg = GetComponentInChildren<CanvasGroup>();
        cg.alpha = 0f;
        cg.blocksRaycasts = false;
    }

    public IEnumerator FadeOut()
    {
        cg.blocksRaycasts = true;
        yield return FadeTo(1f);
    }

    public IEnumerator FadeIn()
    {
        yield return FadeTo(0f);
        cg.blocksRaycasts = false;
    }

    private IEnumerator FadeTo(float target)
    {
        float start = cg.alpha;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }
        cg.alpha = target;
    }
}