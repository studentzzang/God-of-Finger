using UnityEngine;
using UnityEngine.UI;

public class NormalDialogueUI : DialogueUIBase
{
    [Header("Normal Dialogue")]
    [SerializeField] private Image npcPortraitImage;

    public override void ApplyVisual(DialogueVisual visual)
    {
        if (npcPortraitImage == null) return;

        var sprite = visual != null ? visual.portrait : null;

        if (sprite != null)
        {
            npcPortraitImage.sprite = sprite;
            npcPortraitImage.gameObject.SetActive(true);
        }
        else
        {
            npcPortraitImage.sprite = null;
            npcPortraitImage.gameObject.SetActive(false);
        }
    }

    protected override void BindToManager()
    {
        DialogueManager.Instance.BindNormalUI(this);
    }

    protected override void UnbindFromManager()
    {
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.BindNormalUI(null);
    }
}