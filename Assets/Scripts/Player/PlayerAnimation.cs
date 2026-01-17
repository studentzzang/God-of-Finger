using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Vector2 lastDir = Vector2.down;

    private void Awake()
    {
        if (!spriteRenderer)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void UpdateAnim(Vector2 moveInput)
    {
        bool moving = moveInput.sqrMagnitude > 0.001f;

        if (moving)
        {
            Vector2 dir = Snap4Dir(moveInput);
            lastDir = dir;

            anim.SetFloat("MoveX", dir.x);
            anim.SetFloat("MoveY", dir.y);

            // 👉 좌우 flip 처리
            if (dir.x != 0)
                spriteRenderer.flipX = dir.x > 0;
        }

        anim.SetBool("IsMoving", moving);
        anim.SetFloat("LastX", lastDir.x);
        anim.SetFloat("LastY", lastDir.y);
    }

    private Vector2 Snap4Dir(Vector2 v)
    {
        if (Mathf.Abs(v.x) > Mathf.Abs(v.y))
            return new Vector2(Mathf.Sign(v.x), 0);
        return new Vector2(0, Mathf.Sign(v.y));
    }
}