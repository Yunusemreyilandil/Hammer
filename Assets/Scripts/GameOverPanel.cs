using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverPanel : MonoBehaviour
{
    [SerializeField] private Button quitButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private TextMeshProUGUI scoreText;

    private void Awake()
    {
        quitButton.onClick.AddListener(OnQuitButtonClicked);
        restartButton.onClick.AddListener(OnRestartButtonClicked);
        // scoreText.gameObject.SetActive(true);
        // scoreText.text = "Score: " + ScoreManager.Instance.score;
    }

    private void OnRestartButtonClicked()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnQuitButtonClicked()
    {
        Application.Quit();
    }
}
