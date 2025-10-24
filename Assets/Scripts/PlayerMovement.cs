using UnityEngine;
using UnityEngine.InputSystem;
using Game.Dialogue.Runtime; // Introducing the dialogue event namespace

public class PlayerMovement : MonoBehaviour {
    [Header("Movement Settings")]
    public float speed = 3f;
    public float jumpForce = 8f;

    private Animator animator;
    private Rigidbody2D rb;
    private InputSystem_Actions inputControl;
    private Vector2 inputDirection;

    // Recording of movement status
    private bool _moveWasEnabled;
    private bool _jumpWasEnabled;

    [Header("Jump Setting")]
    public int maxJumps = 1;  // Maximum 1 jump

    private int jumpCount = 0;

    void Awake() {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        inputControl = new InputSystem_Actions();
        rb.gravityScale = 4f;
    }

    void OnEnable() {
        inputControl.Enable();

        // Listening to dialogue events
        DialogueEvents.OnStarted += HandleDialogueStarted;
        DialogueEvents.OnEnded += HandleDialogueEnded;
    }

    void OnDisable() {
        DialogueEvents.OnStarted -= HandleDialogueStarted;
        DialogueEvents.OnEnded -= HandleDialogueEnded;

        inputControl.Disable();
    }

    private void HandleDialogueStarted(string graphId) {
        // Record the current state
        _moveWasEnabled = inputControl.Player.Move.enabled;
        _jumpWasEnabled = inputControl.Player.Jump.enabled;

        // Disable movement and jumping
        if (_moveWasEnabled) inputControl.Player.Move.Disable();
        if (_jumpWasEnabled) inputControl.Player.Jump.Disable();

        // force the speed to be zeroed out
        rb.linearVelocity = Vector2.zero;
    }

    private void HandleDialogueEnded(string graphId) {
        // Resume action at the end of the dialogue
        if (_moveWasEnabled) inputControl.Player.Move.Enable();
        if (_jumpWasEnabled) inputControl.Player.Jump.Enable();
    }

    void Update() {
        // Read mobile inputs
        inputDirection = inputControl.Player.Move.ReadValue<Vector2>();

        // Animation control
        if (animator != null) {
            animator.SetFloat("Horizontal", inputDirection.x);
        }

        // You can only jump when you're on the ground.
        if (inputControl.Player.Jump.triggered && jumpCount < maxJumps)
        {
            Jump();
            jumpCount++;
        }

        // Actual movement (Move is disabled during the dialogue → here inputDirection will be 0)
        rb.linearVelocity = new Vector2(inputDirection.x * speed, rb.linearVelocity.y);
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            jumpCount = 0;
        }
    }

    private void Jump() {
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }
}
