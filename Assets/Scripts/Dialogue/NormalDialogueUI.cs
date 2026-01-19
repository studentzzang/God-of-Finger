using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 필드(일반) 대화용 UI.
/// - DialogueUIBase를 상속
/// - NPC 초상화를 대화창 왼쪽 위에 표시
/// </summary>
public class NormalDialogueUI : DialogueUIBase
{
    [Header("Normal Dialogue")]
    [SerializeField] private Image npcPortraitImage;

    /// <summary>
    /// NPC 초상화를 세팅한다.
    /// sprite가 null이면 기존 이미지를 유지한다.
    /// </summary>
    public void SetNpcPortrait(Sprite sprite)
    {
        if (sprite == null || npcPortraitImage == null)
            return;

        npcPortraitImage.sprite = sprite;
        npcPortraitImage.gameObject.SetActive(true);
    }

    /// <summary>
    /// 초상화를 숨긴다 (필요할 때만 사용).
    /// </summary>
    public void HidePortrait()
    {
        if (npcPortraitImage)
            npcPortraitImage.gameObject.SetActive(false);
    }

    private void Awake()
    {
        // DialogueManager에 Normal UI로 등록
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.BindNormalUI(this);
        }
    }

    private void OnDestroy()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.BindNormalUI(null);
        }
    }
    

}