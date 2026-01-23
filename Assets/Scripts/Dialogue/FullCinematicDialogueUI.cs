using UnityEngine;
using UnityEngine.UI;

public class FullCinematicDialogueUI : DialogueUIBase
{
    [Header("Full Cinematic Dialogue (Background + Subtitle)")]
    [SerializeField] private Image backgroundImage;

    public override void ApplyVisual(DialogueVisual visual)
    {
        if (backgroundImage == null) return;

        var bg = visual != null ? visual.background : null;

        if (bg != null)
        {
            backgroundImage.sprite = bg;
            backgroundImage.gameObject.SetActive(true);
        }
        else
        {
            backgroundImage.sprite = null;
            backgroundImage.gameObject.SetActive(false);
        }
    }

    protected override void BindToManager()
    {
        DialogueManager.Instance.BindFullCinematicUI(this);
    }

    protected override void UnbindFromManager()
    {
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.BindFullCinematicUI(null);
    }
}