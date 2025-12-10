using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    public GameObject gameOverPanel;
    private bool isGameOver = false;


    void Start()
    {
        if(gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (Player1.Instance != null)
        {
            Player1.Instance.OnDied += ShowGameOver;
        }
    }

    private void ShowGameOver()
    {
        if (isGameOver) return;
        {
            isGameOver = true;
            Time.timeScale = 0f;
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }


    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    private void OnDestroy()
    {
        if (Player1.Instance != null)
        {
            Player1.Instance.OnDied -= ShowGameOver;
        }
    }

    void Update()
    {
        
    }
}
