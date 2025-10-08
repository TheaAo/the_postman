using UnityEngine;
using UnityEngine.InputSystem;
using Game.Dialogue.Runtime; // 引入对话事件命名空间

public class PlayerMovement : MonoBehaviour {
    [Header("Movement Settings")]
    public float speed = 3f;
    public float jumpForce = 8f;

    private Animator animator;
    private Rigidbody2D rb;
    private InputSystem_Actions inputControl;
    private Vector2 inputDirection;

    // 记录动作状态
    private bool _moveWasEnabled;
    private bool _jumpWasEnabled;

    void Awake() {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        inputControl = new InputSystem_Actions();
        rb.gravityScale = 4f;
    }

    void OnEnable() {
        inputControl.Enable();

        // ✅ 监听对话事件
        DialogueEvents.OnStarted += HandleDialogueStarted;
        DialogueEvents.OnEnded += HandleDialogueEnded;
    }

    void OnDisable() {
        DialogueEvents.OnStarted -= HandleDialogueStarted;
        DialogueEvents.OnEnded -= HandleDialogueEnded;

        inputControl.Disable();
    }

    private void HandleDialogueStarted(string graphId) {
        // 记录当前状态
        _moveWasEnabled = inputControl.Player.Move.enabled;
        _jumpWasEnabled = inputControl.Player.Jump.enabled;

        // ❌ 禁用移动和跳跃
        if (_moveWasEnabled) inputControl.Player.Move.Disable();
        if (_jumpWasEnabled) inputControl.Player.Jump.Disable();

        // 如果希望角色站定，可以强制速度清零
        rb.linearVelocity = Vector2.zero;
    }

    private void HandleDialogueEnded(string graphId) {
        // ✅ 对话结束恢复动作
        if (_moveWasEnabled) inputControl.Player.Move.Enable();
        if (_jumpWasEnabled) inputControl.Player.Jump.Enable();
    }

    void Update() {
        // 读取移动输入
        inputDirection = inputControl.Player.Move.ReadValue<Vector2>();

        // 动画控制
        if (animator != null) {
            animator.SetFloat("Horizontal", inputDirection.x);
        }

        // 跳跃
        if (inputControl.Player.Jump.triggered) {
            Jump();
        }

        // 实际移动（对话期间 Move 被禁用 → 这里 inputDirection 会是 0）
        rb.linearVelocity = new Vector2(inputDirection.x * speed, rb.linearVelocity.y);
    }

    private void Jump() {
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }
}
