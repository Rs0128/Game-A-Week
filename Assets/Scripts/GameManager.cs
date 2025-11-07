using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// ゲーム全体の進行を管理するクラス。
/// ・3秒のカウントダウン
/// ・30秒の制限時間
/// ・スコア(正解数)の記録
/// ・終了時にリザルトシーンへ遷移
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI countdownText; // 3秒カウントダウン用
    public TextMeshProUGUI timerText;     // 残り時間表示
    public TextMeshProUGUI countText;     // 成功回数表示

    private float timeLeft = 30f;         // 制限時間
    private int successCount = 0;         // 正解数
    private bool isPlaying = false;       // ゲーム中フラグ

    void Start()
    {
        StartCoroutine(StartCountdown());
    }

    /// <summary>
    /// 3秒間のカウントダウンを行い、その後ゲームを開始する。
    /// </summary>
    IEnumerator StartCountdown()
    {
        int count = 3;
        while (count > 0)
        {
            countdownText.text = count.ToString();
            yield return new WaitForSeconds(1f);
            count--;
        }
        countdownText.text = "START!";
        yield return new WaitForSeconds(0.5f);
        countdownText.text = "";

        isPlaying = true;
    }

    void Update()
    {
        if (!isPlaying) return;

        // 残り時間を減少
        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            EndGame();
        }

        // 表示更新
        timerText.text = $"Time: {timeLeft:F1}";
        countText.text = $"Score: {successCount}";
    }

    /// <summary>
    /// 成功／失敗を受け取り、スコアを加算する。
    /// </summary>
    public void RegisterAttempt(bool success)
    {
        if (success)
            successCount++;
    }

    /// <summary>
    /// ゲーム終了処理。スコアを保存し、リザルトシーンへ遷移。
    /// </summary>
    void EndGame()
    {
        isPlaying = false;

        // スコアを保存してResultSceneへ渡す
        PlayerPrefs.SetInt("FinalScore", successCount);
        PlayerPrefs.Save();

        SceneManager.LoadScene("Result");
    }
}
