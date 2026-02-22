using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 6f;
    public float accel = 40f;
    public float decel = 50f;

    [Header("Jump")]
    public float jumpForce = 12f;
    public float coyoteTime = 0.08f;      // 바닥 떠난 직후 점프 허용
    public float jumpBuffer = 0.1f;       // 점프 입력 버퍼
    public float fallMultiplier = 2.2f;   // 낙하감
    public float lowJumpMultiplier = 1.6f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.12f);
    public LayerMask groundMask;

    [Header("Life")]
    public int startLife = 5;
    public int maxLife = 7;
    public int contactDamage = 1;
    public float invincibleDuration = 1.0f;
    public string damageTag = "Enemy";
    public string hitTriggerName = "Hit";
    public KiwiLifeUI lifeUI;

    Animator anim;
    Rigidbody2D rb;

    float x;
    float coyoteTimer;
    float jumpBufferTimer;

    float invincibleTimer;
    int currentLife;

    void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        currentLife = Mathf.Clamp(startLife, 0, maxLife);
        if (lifeUI != null) lifeUI.Initialize(maxLife, currentLife);
    }

    void Update()
    {
        x = Input.GetAxisRaw("Horizontal");
        if (x != 0)
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.flipX = x < 0;
        }

        // 바닥 체크
        bool grounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundMask);
        if (anim != null)
        {
            anim.SetBool("Grounded", grounded);
            anim.SetFloat("Speed", Mathf.Abs(x));
        }
        coyoteTimer = grounded ? coyoteTime : coyoteTimer - Time.deltaTime;

        // 점프 입력 버퍼
        if (Input.GetButtonDown("Jump")) jumpBufferTimer = jumpBuffer;
        else jumpBufferTimer -= Time.deltaTime;

        // 점프 실행
        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
        }

        // 점프감(더 빨리 떨어지고, 짧게 누르면 낮게 점프)
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1f) * Time.deltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump"))
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1f) * Time.deltaTime;
        }

        // 무적 타이머 감소
        if (invincibleTimer > 0f) invincibleTimer -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        float targetSpeed = x * moveSpeed;
        float speedDiff = targetSpeed - rb.linearVelocity.x;

        float rate = Mathf.Abs(targetSpeed) > 0.01f ? accel : decel;
        float force = speedDiff * rate;

        rb.AddForce(Vector2.right * force);

        // 최대 속도 제한(가속 힘 방식이어서 안전)
        rb.linearVelocity = new Vector2(Mathf.Clamp(rb.linearVelocity.x, -moveSpeed, moveSpeed), rb.linearVelocity.y);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
    }

    // ======================
    // Damage / Life
    // ======================
    void OnCollisionEnter2D(Collision2D collision)
    {
        TryTakeDamageFrom(collision.collider);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        TryTakeDamageFrom(collision.collider);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        TryTakeDamageFrom(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        TryTakeDamageFrom(other);
    }

    void TryTakeDamageFrom(Collider2D other)
    {
        if (other == null) return;
        var stompEnemy = other.GetComponentInParent<EnemyPatrolStomp>();
        if (stompEnemy != null && !stompEnemy.CanDealTouchDamage) return;
        if (!other.CompareTag(damageTag)) return;
        if (invincibleTimer > 0f) return;

        TakeDamage(contactDamage);
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;
        if (currentLife <= 0) return;

        currentLife = Mathf.Max(0, currentLife - amount);
        invincibleTimer = invincibleDuration;

        if (anim != null && !string.IsNullOrEmpty(hitTriggerName))
            anim.SetTrigger(hitTriggerName);

        if (lifeUI != null) lifeUI.SetLife(currentLife);

        if (currentLife <= 0)
        {
            rb.linearVelocity = Vector2.zero;
            // 여기서 죽음 처리
        }
    }

    public bool Heal(int amount)
    {
        if (amount <= 0) return false;
        if (currentLife >= maxLife) return false;

        int before = currentLife;
        currentLife = Mathf.Min(maxLife, currentLife + amount);

        if (lifeUI != null) lifeUI.SetLife(currentLife);
        return currentLife > before;
    }
}