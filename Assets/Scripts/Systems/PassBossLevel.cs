using UnityEngine;
using UnityEngine.SceneManagement;

public class PassBossLevel : MonoBehaviour
{
    private const string nextScene = "Boss Level";
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            LoadNextLevel();
            Destroy(gameObject);
        }
    }

    private void LoadNextLevel()
    {
        Debug.Log("Objeto tocado. Cambiando a la escena: " + nextScene);
        SceneManager.LoadScene(nextScene);
    }
}
