using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 연출(시네마틱) 대화 UI.
/// - DialogueUIBase를 상속
/// - 배경/캐릭터(스탠딩) 이미지를 가진다.
/// - Sprite가 null이면 기존 상태를 유지한다.
/// </summary>
public class CinematicDialogueUI : DialogueUIBase
{
    [Header("Cinematic Slots")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image characterImage;

    /// <summary>
    /// 배경 이미지를 교체한다.
    /// sprite가 null이면 유지한다.
    /// </summary>
    public void SetBackground(Sprite sprite)
    {
        if (backgroundImage == null) return;
        if (sprite == null) return; // 없으면 유지

        backgroundImage.sprite = sprite;
        backgroundImage.gameObject.SetActive(true);
    }

    /// <summary>
    /// 캐릭터(스탠딩) 이미지를 교체한다.
    /// sprite가 null이면 유지한다.
    /// </summary>
    public void SetCharacter(Sprite sprite)
    {
        if (characterImage == null) return;
        if (sprite == null) return; // 없으면 유지

        characterImage.sprite = sprite;
        characterImage.gameObject.SetActive(true);
    }

    /// <summary>
    /// 배경/캐릭터를 숨긴다 (필요할 때만 사용).
    /// </summary>
    public void HideBackground()
    {
        if (backgroundImage) backgroundImage.gameObject.SetActive(false);
    }

    public void HideCharacter()
    {
        if (characterImage) characterImage.gameObject.SetActive(false);
    }

    private void Awake()
    {
        // DialogueManager에 Cinematic UI로 등록
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.BindCinematicUI(this);
        }
    }

    private void OnDestroy()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.BindCinematicUI(null);
        }
    }
}