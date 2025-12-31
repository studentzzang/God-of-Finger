using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAction : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Rigidbody2D rb;

    private Vector2 moveInput;
    private Vector2 dirVector;
    private GameObject scanObject;
    private void Awake()
    {
        if (!rb)
        {
            rb = GetComponent<Rigidbody2D>();
        }
    }

    private void Update()
    {
        if (!GameManager.Instance.IsAction)
        {
            
            moveInput.x = Input.GetAxisRaw("Horizontal");
            moveInput.y = Input.GetAxisRaw("Vertical");
            moveInput = moveInput.normalized;

            if (moveInput != Vector2.zero)
                dirVector = moveInput;
            
        }
        else moveInput = Vector2.zero;

        if (Input.GetKeyDown(KeyCode.Space) && scanObject != null)
        {
            GameManager.Instance.Action(scanObject);
        }
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);

        Debug.DrawRay(rb.position, dirVector * 1.5f, Color.red);

        RaycastHit2D hit = Physics2D.Raycast(
            rb.position,
            dirVector,
            1f,
            LayerMask.GetMask("Object")
        );

        scanObject = hit.collider ? hit.collider.gameObject : null;
    }

}
