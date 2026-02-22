using UnityEngine;

public class KiwiPickup : MonoBehaviour
{
    public int healAmount = 1;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var player = other.GetComponent<Player>();
        if (player == null) return;

        if (player.Heal(healAmount))
            Destroy(gameObject);
    }
}
