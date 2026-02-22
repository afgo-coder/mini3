using UnityEngine;

public class KiwiSpawnManager : MonoBehaviour
{
    public GameObject kiwiPrefab;

    [Header("Spawn Settings")]
    public LayerMask groundMask;
    public float spawnHeight = 4.5f;      // ✅ 최대 +4.5
    public float minX = -8f;
    public float maxX = 8f;
    public int spawnCount = 2;            // ✅ 2개 생성

    void Start()
    {
        SpawnKiwis();
    }

    void SpawnKiwis()
    {
        // Ground 레이어 콜라이더 하나 찾기
        Collider2D ground = FindGround();
        if (ground == null)
        {
            Debug.LogWarning("Ground not found!");
            return;
        }

        float groundY = ground.bounds.max.y; // 바닥의 상단 기준

        for (int i = 0; i < spawnCount; i++)
        {
            float randomX = Random.Range(minX, maxX);
            float randomY = groundY + Random.Range(0f, spawnHeight);

            Vector2 spawnPos = new Vector2(randomX, randomY);

            Instantiate(kiwiPrefab, spawnPos, Quaternion.identity);
        }
    }

    Collider2D FindGround()
    {
        Collider2D[] cols = Object.FindObjectsByType<Collider2D>(FindObjectsSortMode.None);

        foreach (var col in cols)
        {
            if (((1 << col.gameObject.layer) & groundMask) != 0)
                return col;
        }

        return null;
    }
}