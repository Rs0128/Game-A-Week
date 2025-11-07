using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 【LineDrawer】
/// プレイヤーがマウスクリックで円（Circle）を順番に選択して線でつないでいく処理を担当するクラス。
/// CircleManager が円を再生成するたびにリセットされ、最初の円からスタートする。
/// LineRenderer を用いて選択した順に線を描画する。
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class LineDrawer : MonoBehaviour
{
    [Header("参照設定")]
    public CircleManager circleManager;  // 円を生成・管理するクラス
    public TextMeshProUGUI messageText;  // ミス表示用のUIテキスト
    public GameManager gameManager;      // 成功・失敗を記録するゲームマネージャ

    private LineRenderer line;           // 実際に線を描画するLineRenderer
    private GameObject currentCircle;    // 現在選択中の円
    private List<GameObject> visited = new List<GameObject>(); // 訪問済みの円リスト

    private bool isAnimating = false;    // 線を描画中かどうか（連続クリック防止用）

    void OnEnable()
    {
        // CircleManager の「CirclesGenerated」イベントを監視して円生成完了を受け取る
        if (circleManager != null)
            circleManager.CirclesGenerated += OnCirclesGenerated;
    }

    void OnDisable()
    {
        // イベント登録解除
        if (circleManager != null)
            circleManager.CirclesGenerated -= OnCirclesGenerated;
    }

    void Start()
    {
        line = GetComponent<LineRenderer>();
        if (messageText != null) messageText.text = "";

        // CircleManager がすでに円を生成済みなら即初期化
        if (circleManager != null && circleManager.startCircle != null)
        {
            InitLine();
        }
        else
        {
            Debug.Log("LineDrawer: waiting for CircleManager to generate circles...");
        }
    }

    /// <summary>
    /// CircleManager が新しい円を生成したときに呼ばれるイベントハンドラ。
    /// 次のフレームで線の初期化を実行する。
    /// </summary>
    private void OnCirclesGenerated()
    {
        StartCoroutine(InitLineNextFrame());
    }

    /// <summary>
    /// 安全策として1フレーム遅らせて初期化を行うコルーチン。
    /// </summary>
    IEnumerator InitLineNextFrame()
    {
        yield return null;
        InitLine();
    }

    /// <summary>
    /// 線描画の初期設定。
    /// 開始円（startCircle）から線を描画する準備を行う。
    /// </summary>
    void InitLine()
    {
        if (line == null) line = GetComponent<LineRenderer>();

        // リセット処理
        visited.Clear();
        line.positionCount = 0;
        currentCircle = null;
        isAnimating = false;

        if (circleManager == null)
        {
            Debug.LogError("LineDrawer: circleManager is null in InitLine");
            return;
        }

        if (circleManager.startCircle == null)
        {
            Debug.LogError("LineDrawer: startCircle is null in InitLine");
            return;
        }

        // スタート位置設定
        currentCircle = circleManager.startCircle;

        // 線の見た目設定
        line.startWidth = 0.1f;
        line.endWidth = 0.1f;
        line.startColor = Color.green;
        line.endColor = Color.green;

        // 最初の円を訪問済みに登録
        visited.Add(currentCircle);
        line.positionCount = 1;
        line.SetPosition(0, currentCircle.transform.position);

        Debug.Log("LineDrawer: InitLine completed. start=" + currentCircle.name);
    }

    void Update()
    {
        // アニメーション中や開始円未設定なら入力を受け付けない
        if (isAnimating || currentCircle == null) return;

        // マウスクリック入力
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;

            // クリックしたオブジェクト（Circle）を取得
            GameObject clicked = GetHoveredCircle(mousePos);

            if (clicked != null && clicked != currentCircle)
            {
                // 「次に正しい円」かどうかを判定
                bool isCorrect = clicked == FindNearestUnvisitedCircle(currentCircle.transform.position);

                // 線アニメーションを開始
                StartCoroutine(AnimateLineToCircle(clicked, !isCorrect));
            }
        }
    }

    /// <summary>
    /// 線を次の円に向けてアニメーションしながら引くコルーチン。
    /// 正解でない場合はミス処理を行い、CircleManagerをリセットする。
    /// </summary>
    IEnumerator AnimateLineToCircle(GameObject target, bool isMiss)
    {
        if (target == null)
        {
            isAnimating = false;
            yield break;
        }

        isAnimating = true;

        // 保険: target がアクティブか確認
        if (!target.activeInHierarchy)
        {
            Debug.LogWarning("LineDrawer: clicked target not active");
        }

        // まれにpositionCountが0の場合（不正状態）
        if (line.positionCount == 0)
        {
            InitLine();
        }

        int insertIndex = visited.Count;
        if (line.positionCount < insertIndex + 1)
            line.positionCount = insertIndex + 1;

        Vector3 start = line.GetPosition(insertIndex - 1);
        Vector3 end = target.transform.position;

        // アニメーション（線を補間して引く）
        float t = 0f;
        float duration = 0.18f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            Vector3 pos = Vector3.Lerp(start, end, t);
            line.SetPosition(insertIndex, pos);
            yield return null;
        }

        line.SetPosition(insertIndex, end);

        // ミス処理
        if (isMiss)
        {
            ShowMiss(target);
            gameManager?.RegisterAttempt(false);

            // 全てリセットして次のセットへ
            circleManager.ResetCircles();
            yield return null;
            isAnimating = false;
            yield break;
        }

        // 正解の場合の処理
        if (target.TryGetComponent<SpriteRenderer>(out var sr))
            sr.color = Color.green;

        visited.Add(target);
        currentCircle = target;
        gameManager?.RegisterAttempt(true);

        isAnimating = false;

        // 全ての円を繋いだ場合
        if (visited.Count == circleManager.circles.Count)
        {
            Debug.Log("LineDrawer: All connected");
            yield return new WaitForSeconds(0.3f);
            circleManager.ResetCircles();
            yield return null;
        }
    }

    /// <summary>
    /// ミスした際のビジュアル・メッセージ処理。
    /// </summary>
    void ShowMiss(GameObject target)
    {
        if (target.TryGetComponent<SpriteRenderer>(out var sr))
            sr.color = Color.blue;

        if (messageText != null)
        {
            messageText.text = "MISS";
            messageText.color = Color.red;
            StartCoroutine(ClearMessageAfterDelay(0.8f));
        }
    }

    /// <summary>
    /// 一定時間後にメッセージをクリアする。
    /// </summary>
    IEnumerator ClearMessageAfterDelay(float sec)
    {
        yield return new WaitForSeconds(sec);
        if (messageText != null) messageText.text = "";
    }

    /// <summary>
    /// 現在位置から最も近い「未訪問」の円を返す。
    /// </summary>
    GameObject FindNearestUnvisitedCircle(Vector3 pos)
    {
        var candidates = circleManager.circles.Where(c => !visited.Contains(c)).ToList();
        if (candidates == null || candidates.Count == 0) return null;
        return candidates.OrderBy(c => Vector2.Distance(pos, c.transform.position)).FirstOrDefault();
    }

    /// <summary>
    /// マウス位置にあるCircle（Collider2D）を取得する。
    /// </summary>
    GameObject GetHoveredCircle(Vector3 mousePos)
    {
        Collider2D hit = Physics2D.OverlapPoint(mousePos);
        if (hit != null) return hit.gameObject;
        return null;
    }
}
