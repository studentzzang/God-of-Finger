using UnityEngine;

/// <summary>
/// 플레이어 이동 및 상호작용(레이캐스트 스캔 + Space 입력)을 처리한다.
/// 대화가 열려있을 때는 이동/스캔을 멈추고 Next만 허용한다.
/// </summary>
public class PlayerAction : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private PlayerAnimation playerAnimation;

    [SerializeField] private QuestSO testQuest; // 테스트용 퀘스트 -->> 추후 삭제!!
    [SerializeField] private QuestSO testQuest2; // 테스트용 퀘스트 -->> 추후 삭제!!
    [SerializeField] private QuestSO testQuest3; // 테스트용 퀘스트 -->> 추후 삭제!!
    [SerializeField] private QuestSO testQuest4; // 테스트용 퀘스트 -->> 추후 삭제!!
    [SerializeField] private QuestSO testQuest5; // 테스트용 퀘스트 -->> 추후 삭제!!
    [SerializeField] private QuestSO testQuest6; // 테스트용 퀘스트 -->> 추후 삭제!!
    
    private Vector2 moveInput;
    private Vector2 dirVector = Vector2.down; 
    private GameObject scanObject;

    private void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!playerAnimation) playerAnimation = GetComponentInChildren<PlayerAnimation>();
    }

    /// <summary>
    /// 입력 처리(스페이스 상호작용/대화 진행, 이동 입력)를 담당한다.
    /// </summary>
    private void Update()
    {
        //전환 중 일때 입력 무시
        if (PlayerInputLock.IsLocked)
        {
            moveInput = Vector2.zero;
            if (playerAnimation) playerAnimation.UpdateAnim(Vector2.zero);
            return;
        }

        // 대화 처리 로직 대화 중이면 Next, 아니면 대화 시작
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (DialogueManager.Instance.IsOpen) //대화창 열려있으면 다음으로
            {
                
                if (DialogueManager.Instance.IsWaitingChoice) return; // 선택지에서는 스페이스로 넘기지 않음

                else
                {
                    DialogueManager.Instance.Next();
                    return;
                    
                }

            }
            

            if (scanObject != null) // 열려있지 않음 / 상호작용 가능 물체가 있으면
            {
                var door = scanObject.GetComponent<DoorToScene>();
                if (door != null)
                {
                    door.Interact();
                    Debug.Log("Door Interact");
                    return;
                }

                // 퀘스트 NPC 우선 (멀티 -> 단일 순)
                // 멀티 퀘스트 기버
                var giverMulti = scanObject.GetComponent<NPCQuestGiverMulti>();
                if (giverMulti != null)
                {
                    var dialogue = giverMulti.GetDialogue();
                    if (dialogue != null)
                    {
                        DialogueManager.Instance.StartDialogue(dialogue);
                        return;
                    }
                }

                // 단일 퀘스트  << 추후 삭제 예정
                
                var giver = scanObject.GetComponent<NPCQuestGiver>();
                if (giver != null)
                {
                    var dialogue = giver.GetDialogue();
                    if (dialogue != null)
                    {
                        DialogueManager.Instance.StartDialogue(dialogue);
                        return;
                    }
                }
                
                //여기까지 삭제

                // 2) 일반 NPC 대화
                var npc = scanObject.GetComponent<NPCDialogue>();
                if (npc != null && npc.dialogue != null)
                {
                    DialogueManager.Instance.StartDialogue(npc.dialogue);
                    return;
                }
                
            }
        }

        // 대화 중 이동 x
        if (DialogueManager.Instance.IsOpen)
        {   
            moveInput = Vector2.zero;
            if (playerAnimation) playerAnimation.UpdateAnim(Vector2.zero);
            return;
        }

        // 이동 입력(대화 중이 아닐 때만)
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput = moveInput.normalized;
        if (playerAnimation) playerAnimation.UpdateAnim(moveInput);

        // 마지막 이동 방향을 기억(정지 중에도 레이 방향 유지)
        if (moveInput != Vector2.zero)
            dirVector = moveInput;
        
        // 테스트용: C 키 누르면 퀘스트 완료 처리
        if (Input.GetKeyDown(KeyCode.C))
        {
            QuestSignals.Raise("Quest1Clear");
            // if (testQuest != null)
            // {
            //     QuestManager.Instance.Complete(testQuest);
            //     Debug.Log($"[TEST] Quest Completed: {testQuest.questId}");
            // }
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            if (testQuest2 != null)
            {
                QuestManager.Instance.Complete(testQuest2);
                Debug.Log($"[TEST] Quest Completed: {testQuest2.questId}");
            }
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (testQuest3 != null)
            {
                QuestManager.Instance.Complete(testQuest3);
                Debug.Log($"[TEST] Quest Completed: {testQuest3.questId}");
            }
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            if (testQuest4 != null)
            {
                QuestManager.Instance.Complete(testQuest4);
                Debug.Log($"[TEST] Quest Completed: {testQuest4.questId}");
            }
        }
        
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (testQuest5 != null)
            {
                QuestManager.Instance.Complete(testQuest5);
                Debug.Log($"[TEST] Quest Completed: {testQuest5.questId}");
            }
        }
        
        
        
        
        
    }

    /// <summary>
    /// 물리 처리(이동 적용, 레이캐스트 스캔)를 담당한다.
    /// </summary>
    private void FixedUpdate()
    {
        // 대화 중 이동 x
        if (PlayerInputLock.IsLocked || DialogueManager.Instance.IsOpen)
        {
            rb.velocity = Vector2.zero;
            scanObject = null;
            return;
        }

        // Rigidbody2D 이동(물리 프레임)
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);

        // 상호작용 대상 스캔(전방 레이캐스트)
        Debug.DrawRay(rb.position, dirVector * 1.5f, Color.red);

        RaycastHit2D hit = Physics2D.Raycast(
            rb.position,
            dirVector,
            1.5f,
            LayerMask.GetMask("Object")
        );

        scanObject = hit.collider ? hit.collider.gameObject : null;
    }
}
