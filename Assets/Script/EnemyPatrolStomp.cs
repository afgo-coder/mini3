using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPatrolStomp : MonoBehaviour
{
    [Header("Move")]
    public float speed = 2f;

    [Header("Checks")]
    public Transform groundCheck;
    public Vector2 groundBoxSize = new(0.30f, 0.12f);
    public LayerMask groundMask;

    public Transform wallCheck;
    public Vector2 wallBoxSize = new(0.10f, 0.35f);
    public LayerMask wallMask;

    [Header("Check Point Follow Dir")]
    public bool autoMoveChecksOnDir = true; // ✅ 진행방향 따라 체크포인트 이동
    public float checkLocalX = 0.30f;       // ✅ 앞쪽으로 뺄 거리(로컬)
    float groundLocalY, wallLocalY;

    [Header("Flip")]
    public bool flipWhenMovingRight = true;
    public float flipCooldown = 0.15f;

    [Header("Debug")]
    public bool debugLog = true;
    public float debugInterval = 0.5f;

    [Header("Stomp")]
    public float hitStunTime = 0.35f;
    public int stompToDie = 2;

    [Header("Touch Damage Ignore (When Stomped)")]
    public float stompIgnoreTouchTime = 0.20f; // ✅ 밟힌 직후 접촉데미지 무시 시간(추천 0.15~0.25)

    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer sr;

    int dir = 1;
    int stompCount = 0;
    bool isDead = false;
    bool isStunned = false;

    float flipTimer = 0f;
    float debugTimer = 0f;

    float ignoreTouchDamageTimer = 0f; // ✅ 추가

    // ✅ Player 쪽에서 "지금 이 적이 접촉 데미지를 줄 수 있는가?" 확인용
    public bool CanDealTouchDamage => !isDead && ignoreTouchDamageTimer <= 0f;

    // ✅ 밟힐 때 호출해서 일정 시간 접촉 데미지 막기
    public void IgnoreTouchDamage(float t)
    {
        ignoreTouchDamageTimer = Mathf.Max(ignoreTouchDamageTimer, t);
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponentInChildren<SpriteRenderer>();
        rb.freezeRotation = true;

        // ✅ Y는 고정, X만 dir에 따라 바뀌게(시작 위치 저장)
        if (groundCheck) groundLocalY = groundCheck.localPosition.y;
        if (wallCheck) wallLocalY = wallCheck.localPosition.y;

        ApplyFlip();
        if (autoMoveChecksOnDir) ApplyCheckPositions(); // ✅ 시작부터 방향 맞춤
    }

    void FixedUpdate()
    {
        if (flipTimer > 0f) flipTimer -= Time.fixedDeltaTime;

        // ✅ 추가: 밟힌 직후 접촉데미지 무시 타이머 감소
        if (ignoreTouchDamageTimer > 0f)
            ignoreTouchDamageTimer -= Time.fixedDeltaTime;

        if (isDead || isStunned)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        rb.linearVelocity = new Vector2(dir * speed, rb.linearVelocity.y);

        bool hasGround = groundCheck != null &&
                         Physics2D.OverlapBox(groundCheck.position, groundBoxSize, 0f, groundMask);

        bool hitWall = wallCheck != null &&
                       Physics2D.OverlapBox(wallCheck.position, wallBoxSize, 0f, wallMask);

        if (debugLog)
        {
            debugTimer -= Time.fixedDeltaTime;
            if (debugTimer <= 0f)
            {
                debugTimer = debugInterval;
                Debug.Log($"[{name}] dir={dir} hasGround={hasGround} hitWall={hitWall} ignoreTouch={ignoreTouchDamageTimer:0.00}");
            }
        }

        if (flipTimer <= 0f && (hitWall || !hasGround))
        {
            Flip();
        }
    }

    void Flip()
    {
        dir *= -1;
        flipTimer = flipCooldown;

        ApplyFlip();
        if (autoMoveChecksOnDir) ApplyCheckPositions(); // ✅ 뒤집을 때 체크포인트도 앞쪽으로
    }

    void ApplyFlip()
    {
        if (!sr) return;
        sr.flipX = (dir > 0) ? flipWhenMovingRight : !flipWhenMovingRight;
    }

    void ApplyCheckPositions()
    {
        float x = Mathf.Abs(checkLocalX) * dir;

        if (groundCheck)
            groundCheck.localPosition = new Vector3(x, groundLocalY, groundCheck.localPosition.z);

        if (wallCheck)
            wallCheck.localPosition = new Vector3(x, wallLocalY, wallCheck.localPosition.z);
    }

    public void OnStomped()
    {
        if (isDead) return;

        // ✅ 밟힌 순간: 일정 시간 접촉 데미지 주지 않게
        IgnoreTouchDamage(stompIgnoreTouchTime);

        stompCount++;
        if (stompCount >= stompToDie) Die();
        else StartCoroutine(HitRoutine());
    }

    IEnumerator HitRoutine()
    {
        isStunned = true;
        if (anim) { anim.ResetTrigger("Hit"); anim.SetTrigger("Hit"); }
        yield return new WaitForSeconds(hitStunTime);
        isStunned = false;
    }

    void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;

        foreach (var c in GetComponentsInChildren<Collider2D>())
            c.enabled = false;

        if (anim) { anim.ResetTrigger("Die"); anim.SetTrigger("Die"); }
        Destroy(gameObject, 1.0f);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(groundCheck.position, groundBoxSize);
        }
        if (wallCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(wallCheck.position, wallBoxSize);
        }
    }
}

