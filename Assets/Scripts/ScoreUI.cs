using UnityEngine;
using TMPro;

public class ScoreUI : MonoBehaviour
{
    public TextMeshProUGUI scoreText;

    void Start()
    {
        if(ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged += UpdateScoreText;
            UpdateScoreText(0);
        }
    }

    private void UpdateScoreText(int newScore)
    {
        scoreText.text = " " + newScore.ToString();
    }

    public void OnDestroy()
    {
        if(ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= UpdateScoreText;
        }

    }
}
