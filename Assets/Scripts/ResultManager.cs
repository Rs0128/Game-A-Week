using UnityEngine;
using TMPro;

/// <summary>
/// リザルト画面のスコア表示とUI制御を行うクラス。
/// Mainシーンで記録されたスコアをPlayerPrefs経由で受け取り、表示する。
/// </summary>
public class ResultManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI resultText; // 正解数表示用テキスト

    void Start()
    {
        // Mainシーンで保存したスコアを読み取る
        int finalScore = PlayerPrefs.GetInt("FinalScore", 0);

        // 表示更新
        if (resultText != null)
            resultText.text = $"Success: {finalScore}";
    }
}
