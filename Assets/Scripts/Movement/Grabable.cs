///<summary>
/// 잡을 수 있는 모든 물체에 부착
/// 잡히느 대상 오브젝트는 트리거 콜라이더 필수 부착
///</summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum GrabState
{
    Idle,               // 아무 것도 안 하는 기본 상태
    ContactCheck,       // 손가락 구부린 상태에서 손끝 판정 대기
    GrabWindow,         // 한쪽 손끝이 닿아 잡기 타이머(12f) 작동 중
    Grabbing,           // 잡기 성공, 물체/손 고정 상태
    FailCooldown        // 잡기 실패 후 쿨타임(30f), 재시도 불가
}
public class Grabable : MonoBehaviour
{
    public int _friction = 5; //마찰력, 기본값 5 객체마다 설정필요
    public Transform targetHand; //손 오브젝트
    protected HandControl handControl;
    protected FingerPower _fingerPowerScript;
    protected int _fingerPower;

    Vector3 offset;

    public float _holdTimerLimit = 0.4f;
    public float _holdTimer = 0;
    public float _easyBanTime = 0.05f; //이 시간 내에 동시에 잡으면 오히려 실패 (쉽게처리방지)

    public float _failTimer = 0;
    public float _failTimerLimit = 1f;

    public bool _catchState = true;


    //Finchable 상속용
    protected virtual void OnGrabbed() { }          // 잡기 성공 순간 1회
    protected virtual void OnGrabReleased() { }     // 잡기 해제 순간 1회
    protected virtual bool CanFollowWhileGrabbing() => true; // Grabbing 중 Grabbed 허용 여부

    // ====== enum ======
    public enum ContactState
    {
        None,   // 손끝 접촉 없음
        One,    // 한쪽 손끝만 접촉
        Both    // 양쪽 손끝 접촉
    }

    public enum GrabState
    {
        Idle,         // 기본
        GrabWindow,   // 한쪽이 먼저 닿은 순간부터 잡기 타이머 작동 구간
        Grabbing,     // 잡기 성공(고정/따라다님)
        FailCooldown  // 잡기 실패 후 쿨다운(재시도 불가)
    }

    [SerializeField] private ContactState _contact = ContactState.None;
    [SerializeField] private GrabState _state = GrabState.Idle;

    private int _contactCount = 0;

    private void Start()
    {
        InitialSetting();
    }
    void InitialSetting()
    {
            handControl = targetHand.GetComponent<HandControl>();
            _fingerPowerScript = targetHand.GetComponent<FingerPower>();
            
    
    }


    Vector2 GetOffset()
    {
        return (Vector2)transform.position - (Vector2)targetHand.position;
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Finger")) return;

        _contactCount++;
        UpdateContactEnum();

        // "양손 접촉 순간" offset은 최초 1회만 잡아두는게 안정적
        if (_contact == ContactState.Both)
            offset = GetOffset();
    }

    protected void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Finger")) return;

        _contactCount--;
        UpdateContactEnum();
    }

    private void UpdateContactEnum()
    {
        if (_contactCount <= 0) _contact = ContactState.None;
        else if (_contactCount == 1) _contact = ContactState.One;
        else _contact = ContactState.Both;
    }

    protected virtual void LateUpdate()
    {
        CheckCatch();
        
        //Debug.Log((_state, _contact, _holdTimer, _failTimer, _catchState));
    }

    void CheckCatch()
    {
        switch (_state)
        {
            //Idle: 아무것도 안함
            case GrabState.Idle:
                {
                    //TODO: 여기에 제자리로 돌아가는 코드 만들어야할듯
                    _catchState = true;
                    _holdTimer = 0;

                    if (_contact == ContactState.One)
                    {
                        _state = GrabState.GrabWindow;
                        _holdTimer = 0;

                    }
                    else if (_contact == ContactState.Both)
                    {
                        _state = GrabState.GrabWindow;
                        _holdTimer = 0;
                    }
                    break;
                }

            // 2) GrabWindow: 잡기 타이머

            case GrabState.GrabWindow:
                {
                    // 접촉이 끊기면 초기화하고 Idle로
                    if (_contact == ContactState.None)
                    {
                        _holdTimer = 0;
                        _state = GrabState.Idle;


                        break;
                    }

                    if (_contact == ContactState.One)
                    {
                        _holdTimer += Time.deltaTime;

                        if (_holdTimer >= _holdTimerLimit)
                        {
                            // PPT: "잡기 타이머 끝나면 실패 타이머(쿨다운) 시작 + 재시도 불가"
                            _state = GrabState.FailCooldown;
                            _failTimer = 0;

                        }
                    }
                    else if (_contact == ContactState.Both)
                    {
              
                        if (_holdTimer < _holdTimerLimit && _holdTimer >=_easyBanTime)
                        {
                            _catchState = false;
                            _holdTimer = 0;
                            _state = GrabState.Grabbing;

                            offset = GetOffset();
                            OnGrabbed();
                            Grabbed();
                        }
                        else
                        {
                            _state = GrabState.FailCooldown;
                            _failTimer = 0;


                        }
                    }

                    break;
                }

            // 3) Grabbing: 잡기 상태(계속 따라감)

            case GrabState.Grabbing:
                {
                    if (handControl == null || !handControl._inputV || !handControl._inputSpace) // 즉시 잡기해제
                    {
                        
                        _catchState = true;
                        _holdTimer = 0;
                        _state = GrabState.Idle;
                        OnGrabReleased();
                        break;


                    }
                    _fingerPower = _fingerPowerScript._power;
                    Grabbed();

                    break;
                }
            // 4) FailCooldown: 실패 타이머 (재시도 불가)
            case GrabState.FailCooldown:
                {
                  
                    _failTimer += Time.deltaTime;

                    if (_failTimer >= _failTimerLimit)
                    {
                        _failTimer = 0;
                        _catchState = true;

                        _state = GrabState.Idle;
                    }

                    break;
                }
        }
    }
    void Grabbed()
    {
        transform.position = targetHand.position + offset;
    }
}
