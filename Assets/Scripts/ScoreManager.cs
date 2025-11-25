using UnityEngine;
using System;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    public event Action<int> OnScoreChanged;
    private int currentScore = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        Debug.Log("Puntaje actual: " + currentScore);

        OnScoreChanged?.Invoke(currentScore);
    }

    public int GetScore()
    {
        return currentScore;
    }


}
