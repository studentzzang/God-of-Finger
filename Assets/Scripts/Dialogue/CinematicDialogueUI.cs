using UnityEngine;
using UnityEngine.UI;

public class CinematicDialogueUI : DialogueUIBase
{
    [Header("Cinematic Slots")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image characterImage;

    public override void ApplyVisual(DialogueVisual visual)
    {
        // 배경
        if (backgroundImage != null)
        {
            var bg = visual != null ? visual.background : null;

            if (bg != null)
            {
                backgroundImage.sprite = bg;
                backgroundImage.gameObject.SetActive(true);
            }
            else
            {
                backgroundImage.sprite = null;
                //backgroundImage.gameObject.SetActive(false);
            }
        }

        // 캐릭터(초상화/스탠딩)
        if (characterImage != null)
        {
            var ch = visual != null ? visual.portrait : null;

            if (ch != null)
            {
                characterImage.sprite = ch;
                characterImage.gameObject.SetActive(true);
            }
            else
            {
                characterImage.sprite = null;
                characterImage.gameObject.SetActive(false);
            }
        }
    }
    
    protected override void BindToManager()
    {
        DialogueManager.Instance.BindCinematicUI(this);
    }

    protected override void UnbindFromManager()
    {
        // 이미 매니저가 없을 수도 있어서 체크
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.BindCinematicUI(null);
    }
}