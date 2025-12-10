using UnityEngine;
using UnityEngine.SceneManagement;

public class PassLevel1 : MonoBehaviour
{
    private const string nextScene = "level 2";

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
        Debug.Log("Objeto tocado. Cambiando de nivel a: " + nextScene);
        SceneManager.LoadScene(nextScene);
    }

}
