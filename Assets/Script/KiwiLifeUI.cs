using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KiwiLifeUI : MonoBehaviour
{
    [Header("UI")]
    public Transform lifeRoot;
    public Image kiwiPrefab;

    readonly List<Image> lifeIcons = new();
    int maxLife;

    public void Initialize(int max, int current)
    {
        maxLife = Mathf.Max(0, max);
        BuildIcons();
        SetLife(current);
    }

    public void SetLife(int current)
    {
        int clamped = Mathf.Clamp(current, 0, maxLife);
        for (int i = 0; i < lifeIcons.Count; i++)
            lifeIcons[i].enabled = i < clamped;
    }

    void BuildIcons()
    {
        if (lifeRoot == null || kiwiPrefab == null) return;

        for (int i = lifeRoot.childCount - 1; i >= 0; i--)
            Destroy(lifeRoot.GetChild(i).gameObject);

        lifeIcons.Clear();
        for (int i = 0; i < maxLife; i++)
        {
            Image icon = Instantiate(kiwiPrefab, lifeRoot);
            icon.gameObject.SetActive(true);
            lifeIcons.Add(icon);
        }
    }
}
