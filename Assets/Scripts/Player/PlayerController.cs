using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Jump")]
    public float jumpForce = 12f;
    public int maxJumps = 2;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;

    [Header("Slide")]
    // New Input System에서는 KeyCode 대신 직접 키를 확인합니다.
    public float slideHeightMultiplier = 0.55f;   // 콜라이더 높이 줄이는 비율(0~1)
    public float slideOffsetDownMultiplier = 0.2f; // 콜라이더를 아래로 살짝 내림

    [Header("Fall (Game Over)")]
    public float fallY = -6f;

    private Rigidbody2D rb;
    private CapsuleCollider2D col;

    private int jumpCount = 0;
    private bool isGrounded = false;
    private bool isSliding = false;

    private Vector2 originalSize;
    private Vector2 originalOffset;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();

        originalSize = col.size;
        originalOffset = col.offset;
    }

    void Update()
    {
        // 1) 바닥 체크
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        if (isGrounded)
        {
            jumpCount = 0;
        }

        // 키보드 입력 감지
        if (Keyboard.current == null) return;

        // 2) 점프 (1단/2단) - Space 키
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (jumpCount < maxJumps)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f); // 점프 안정화
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                jumpCount++;
            }
        }

        // 3) 슬라이드 (Shift 누를 때 콜라이더 높이 줄이기) - Left Shift 키
        if (Keyboard.current.leftShiftKey.wasPressedThisFrame)
            StartSlide();

        if (Keyboard.current.leftShiftKey.wasReleasedThisFrame)
            EndSlide();

        // 4) 낙사 게임오버
        if (transform.position.y < fallY)
            GameOver();
    }

    void StartSlide()
    {
        if (isSliding) return;
        isSliding = true;

        col.size = new Vector2(originalSize.x, originalSize.y * slideHeightMultiplier);
        col.offset = new Vector2(
            originalOffset.x,
            originalOffset.y - (originalSize.y * slideOffsetDownMultiplier)
        );
    }

    void EndSlide()
    {
        if (!isSliding) return;
        isSliding = false;

        col.size = originalSize;
        col.offset = originalOffset;
    }

    void GameOver()
    {
        Debug.Log("GAME OVER: Fell / Caught");
        Time.timeScale = 0f; // 일단 멈추기
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}