using UnityEngine;

public class PlayerAction : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Rigidbody2D rb;

    private Vector2 moveInput;
    private Vector2 dirVector = Vector2.down; 
    private GameObject scanObject;

    private void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // 대화 처리 로직 대화 중이면 Next, 아니면 대화 시작
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (DialogueManager.Instance.IsOpen) //대화창 열려있으면 다음으로
            {
                DialogueManager.Instance.Next();
                return;
            }

            if (scanObject != null) //열려있지 않음 / 상호작용 가능 물체가 있으면
            {
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
            return;
        }

        // 이동
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput = moveInput.normalized;

        
        if (moveInput != Vector2.zero)
            dirVector = moveInput;
    }

    private void FixedUpdate()
    {
        // 대화 중 이동 x
        if (DialogueManager.Instance.IsOpen)
        {
            rb.velocity = Vector2.zero;
            scanObject = null;
            return;
        }

        // movement
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);

        // ray
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
