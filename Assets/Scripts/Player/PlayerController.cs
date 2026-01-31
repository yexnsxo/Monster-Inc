using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header("Health & UI")]
    public int maxHealth = 3;
    private int currentHealth;
    public TextMeshProUGUI hpText;

    [Header("Jump")]
    public float jumpForce = 12f;
    public int maxJumps = 2;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.3f;
    public LayerMask groundLayer;

    [Header("Slide Settings")]
    public float slideHeightRatio = 0.5f;
    // public float slideRotationAngle = -90f; // [삭제] 이제 애니메이션으로 하니까 필요 없음

    [Header("Fall (Game Over)")]
    public float fallY = -6f;

    [Header("Visual")]
    public Transform visual;          
    // private Quaternion visualOriginRot; // [삭제] 회전 원상복구도 필요 없음
    private SpriteRenderer sr;

    private Rigidbody2D rb;
    private CapsuleCollider2D col;
    private Animator anim;

    private int jumpCount = 0;
    private bool isGrounded = false;
    private bool isSliding = false;

    private Vector2 originalSize;
    private Vector2 originalOffset;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
        anim = GetComponent<Animator>(); 

        if (col != null)
        {
            originalSize = col.size;
            originalOffset = col.offset;
        }

        // 혹시 visual을 깜빡하고 안 넣었으면 자동으로 나 자신을 연결
        if (visual == null) visual = transform;

        if (visual != null)
        {
            // visualOriginRot = visual.localRotation; // [삭제]
            sr = visual.GetComponent<SpriteRenderer>();
            if(sr == null) sr = GetComponent<SpriteRenderer>(); 
        }

        currentHealth = maxHealth;
    }

    void Start()
    {
        UpdateHealthUI();
    }

    void Update()
    {
        // 1) 바닥 체크
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        if (isGrounded && !isSliding)
        {
            jumpCount = 0;
        }

        if (Keyboard.current == null) return;

        // 2) 점프 (Space)
        if (Keyboard.current.spaceKey.wasPressedThisFrame && !isSliding)
        {
            if (jumpCount < maxJumps)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                jumpCount++;
            }
        }

        // 3) 슬라이드 (Left Shift)
        if (Keyboard.current.leftShiftKey.isPressed)
        {
            if (isGrounded)
            {
                StartSlide();
            }
        }

        if (Keyboard.current.leftShiftKey.wasReleasedThisFrame)
        {
            EndSlide();
        }

        // 애니메이션 상태 업데이트
        if (anim != null)
        {
            // 점프 상태 업데이트
            anim.SetBool("isJumping", !isGrounded);
            
            // [중요] 슬라이드 상태도 계속 확실하게 동기화 (선택 사항이지만 안전함)
            anim.SetBool("isSliding", isSliding);
        }

        // 4) 낙사 게임오버
        if (transform.position.y < fallY)
            GameOver();
    }

    void StartSlide()
    {
        if (isSliding) return;
        isSliding = true;

        // ✅ [수정됨] 회전시키는 코드 삭제하고, 애니메이터 값을 켭니다.
        if (anim != null) anim.SetBool("isSliding", true);

        /* -- 삭제된 코드 --
        if (visual != null)
            visual.localRotation = Quaternion.Euler(0, 0, slideRotationAngle);
        */

        // 투명도 복구 (혹시 모를 안전장치)
        if (sr != null)
        {
            sr.enabled = true;
            var c = sr.color; c.a = 1f; sr.color = c;
        }

        // 콜라이더 크기 조절 (이건 유지해야 함! 물리적으로 피해야 하니까)
        if (col != null)
        {
            Vector2 newSize = originalSize;
            newSize.y = originalSize.y * slideHeightRatio;
            col.size = newSize;

            Vector2 newOffset = originalOffset;
            newOffset.y = originalOffset.y - (originalSize.y * (1 - slideHeightRatio) * 0.5f);
            col.offset = newOffset;
        }
    }

    void EndSlide()
    {
        if (!isSliding) return;
        isSliding = false;

        // ✅ [수정됨] 회전 복구 삭제하고, 애니메이터 값을 끕니다.
        if (anim != null) anim.SetBool("isSliding", false);

        /* -- 삭제된 코드 --
        if (visual != null)
            visual.localRotation = visualOriginRot;
        */

        // 콜라이더 복구
        if (col != null)
        {
            col.size = originalSize;
            col.offset = originalOffset;
        }
    }

    // ... (아래 충돌, 데미지, 게임오버 로직은 기존과 동일) ...
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            TakeDamage(1);
            Destroy(collision.gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Goal"))
        {
            GameClear();
        }
    }

    void TakeDamage(int damage)
    {
        currentHealth -= damage;
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            GameOver();
        }
    }

    void UpdateHealthUI()
    {
        if (hpText != null)
        {
            hpText.text = $"HP : {currentHealth} / {maxHealth}";
        }
    }

    void GameOver()
    {
        Debug.Log("GAME OVER");
        if (hpText != null) hpText.text = "GAME OVER";
        Time.timeScale = 0f;
    }

    void GameClear()
    {
        Debug.Log("Game Clear!");
        if (hpText != null)
        {
            hpText.color = Color.green;
            hpText.text = "MISSION COMPLETE!";
        }
        Time.timeScale = 0f;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}