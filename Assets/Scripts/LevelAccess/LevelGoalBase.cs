using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class LevelGoalBase : MonoBehaviour
{
    protected abstract string NextSceneName { get; }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            LoadNextLevel();
            Destroy(gameObject);
        }
    }

    protected void LoadNextLevel()
    {
        Debug.Log("Objeto tocado. Cambiando de nivel a: " + NextSceneName);
        SceneManager.LoadScene(NextSceneName);
    }
}
