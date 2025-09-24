using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 3f;
    private Animator animator;
    private Rigidbody2D rb;
    public InputSystem_Actions inputControl;
    public Vector2 inputDirection;
    public float jumpForce = 8f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        inputControl = new InputSystem_Actions();
        rb.gravityScale = 4f;
    }
    private void OnEnable()
    {
        inputControl.Enable();
    }
    private void OnDisable()
    {
        inputControl.Disable();
    }

    // Update is called once per frame
    // 2D Movement, move left and right in a 2D side-scroller

    void Update()
    {
        inputDirection = inputControl.Player.Move.ReadValue<Vector2>();
        if (animator != null)
        {
            animator.SetFloat("Horizontal", inputDirection.x);
        }
        if (inputControl.Player.Jump.triggered)
        {
            Jump();
        }
        rb.linearVelocity = new Vector2(inputDirection.x * speed, rb.linearVelocity.y);
    }

    private void Jump()
    {
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse); 
    }

}
