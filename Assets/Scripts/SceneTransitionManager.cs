using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// シーン遷移を一元管理するクラス。
/// 各シーン（タイトル／メイン／リザルト）間の移動を制御する。
/// ボタンにこのクラスのメソッドをアタッチして使用する。
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    /// <summary>
    /// タイトルシーンへ移動する。
    /// </summary>
    public void ToTitleScene()
    {
        SceneManager.LoadScene("Title");
    }

    /// <summary>
    /// メインシーン（ゲーム開始シーン）へ移動する。
    /// </summary>
    public void ToMainScene()
    {
        SceneManager.LoadScene("Main");
    }

    /// <summary>
    /// リザルトシーンへ移動する。
    /// </summary>
    public void ToResultScene()
    {
        SceneManager.LoadScene("Result");
    }

    /// <summary>
    /// アプリケーションを終了する。
    /// （エディタ上では再生を停止する）
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
