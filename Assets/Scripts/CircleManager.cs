using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 丸（サークル）オブジェクトの生成・リセットを管理するクラス。
/// イベント CirclesGenerated を発行して生成完了を通知する。
/// </summary>
public class CircleManager : MonoBehaviour
{
    [Header("生成設定")]
    public GameObject circlePrefab;
    public int circleCount = 5;

    [Header("表示設定")]
    public Color startColor = Color.red;
    public Color normalColor = Color.white;

    [HideInInspector] public List<GameObject> circles = new List<GameObject>();
    [HideInInspector] public GameObject startCircle;

    // 生成完了を通知するイベント（LineDrawer などが購読）
    public event Action CirclesGenerated;

    void Start()
    {
        GenerateCircles();
    }

    public void GenerateCircles()
    {
        ClearCircles();

        for (int i = 0; i < circleCount; i++)
        {
            Vector3 pos = new Vector3(UnityEngine.Random.Range(-4f, 4f), UnityEngine.Random.Range(-3f, 3f), 0f);
            GameObject circle = Instantiate(circlePrefab, pos, Quaternion.identity, transform);

            // 安全に SpriteRenderer があれば色をセット
            var sr = circle.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = normalColor;

            // Collider2D が無ければ追加で警告（デバッグ支援）
            var col = circle.GetComponent<Collider2D>();
            if (col == null)
            {
                Debug.LogWarning($"Circle prefab missing Collider2D on instance {circle.name}");
            }

            circles.Add(circle);
        }

        // ランダムにスタートを選択して色を変える
        if (circles.Count > 0)
        {
            startCircle = circles[UnityEngine.Random.Range(0, circles.Count)];
            var sr = startCircle.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = startColor;
        }
        else
        {
            startCircle = null;
        }

        // 生成完了を通知（購読者がいれば呼ぶ）
        CirclesGenerated?.Invoke();
        Debug.Log("CircleManager: GenerateCircles completed, count=" + circles.Count);
    }

    public void ResetCircles()
    {
        GenerateCircles();
    }

    void ClearCircles()
    {
#if UNITY_EDITOR
        // Editor では即破棄して見やすくする (注意: editor専用)
        foreach (var c in circles)
            if (c != null) DestroyImmediate(c);
#else
        // Playモードでは Destroy（安全）
        foreach (var c in circles)
            if (c != null) Destroy(c);
#endif
        circles.Clear();
        startCircle = null;
    }
}
