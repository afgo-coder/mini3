using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public Transform spawnPointsRoot;
    public List<GameObject> enemyPrefabs = new();

    [Header("Burst Spawn")]
    public int burstCount = 3;          // ✅ 한 번에 몇 마리
    public float burstInterval = 0.5f;  // ✅ 몇 초 간격
    public bool useUniquePoints = false;

    List<Transform> points = new();

    void Awake()
    {
        CachePoints();
    }

    void Start()
    {
        StartCoroutine(SpawnBurst());
    }

    void CachePoints()
    {
        points.Clear();
        if (spawnPointsRoot == null) return;

        for (int i = 0; i < spawnPointsRoot.childCount; i++)
            points.Add(spawnPointsRoot.GetChild(i));
    }

    IEnumerator SpawnBurst()
    {
        if (enemyPrefabs.Count == 0 || points.Count == 0) yield break;

        // 유니크 포인트를 쓰고 싶으면 셔플해서 앞에서부터 사용
        var pool = new List<Transform>(points);
        Shuffle(pool);

        for (int i = 0; i < burstCount; i++)
        {
            Transform p = useUniquePoints
                ? pool[i % pool.Count]
                : points[Random.Range(0, points.Count)];

            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            Instantiate(prefab, p.position, Quaternion.identity);

            yield return new WaitForSeconds(burstInterval);
        }
    }

    static void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }
}
