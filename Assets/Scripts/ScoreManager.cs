using UnityEngine;
using System;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public event Action<int> OnScoreChanged;
    public event Action<int> OnHighScoreChanged;

    private int currentScore = 0;
    private int highScore = 0;

    private const string HighScoreKey = "HighScore";


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // Carga el High Score guardado. Si no existe, inicia en 0.
        highScore = PlayerPrefs.GetInt(HighScoreKey, 0);

        OnHighScoreChanged?.Invoke(highScore);

        currentScore = 0;
        OnScoreChanged?.Invoke(currentScore);
    }

    public void AddScore(int amount)
    {
        if (amount <= 0) return;

        currentScore += amount;
        OnScoreChanged?.Invoke(currentScore);

        if (currentScore > highScore)
        {
            highScore = currentScore;

            // Guarda el nuevo High Score
            PlayerPrefs.SetInt(HighScoreKey, highScore);
            PlayerPrefs.Save();

            OnHighScoreChanged?.Invoke(highScore);
            Debug.Log("¡Nuevo High Score guardado!: " + highScore);
        }
    }

    public int GetHighScore()
    {
        return highScore;
    }

    public void ResetHighScore()
    {
        PlayerPrefs.DeleteKey(HighScoreKey);
        highScore = 0;
        currentScore = 0;
        OnHighScoreChanged?.Invoke(highScore);
        OnScoreChanged?.Invoke(currentScore);
        Debug.Log("High Score reseteado.");
    }
}