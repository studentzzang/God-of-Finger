using UnityEngine;

public class QuitMinigame : MonoBehaviour
{
    [Header("조건")]
    public float yRangePx = 30f;

    public float holdSeconds = 1.5f;


    private float _anchorY;
    private float _timer;
    private bool _triggered;

    void Start()
    {
        _anchorY = Input.mousePosition.y;
    }

    void Update()
    {
        float currentY = Input.mousePosition.y;

        // 최초 1회 발동 후 다시 발동시키고 싶으면 이 줄을 지우고 triggered 처리 바꾸면 됨
        if (_triggered) return;

        float dy = Mathf.Abs(currentY - _anchorY);

        if (dy <= yRangePx)
        {
            _timer += Time.deltaTime;

            if (_timer >= holdSeconds)
            {
                _triggered = true;
                OnMouseStayedInYBand();
            }
        }
        else
        {
            // 범위를 벗어나면 기준점을 현재 위치로 재설정하고 타이머 리셋
            _anchorY = currentY;
            _timer = 0f;
        }
    }

    private void OnMouseStayedInYBand()
    {
        //TODO
        MinigameFlow.Instance.Exit(false);
    }

    // (외부에서 다시 발동 가능하게 리셋
    public void ResetTrigger()
    {
        _triggered = false;
        _timer = 0f;
        _anchorY = Input.mousePosition.y;
    }
}