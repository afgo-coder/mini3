using UnityEngine;

public class HeadStomp : MonoBehaviour
{
    public EnemyPatrolStomp enemy;   

    public float bounceUp = 10f;     

    void Awake()
    {
        if (enemy == null) enemy = GetComponentInParent<EnemyPatrolStomp>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        
        var rb = other.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, bounceUp);

        enemy.OnStomped();
    }
}

