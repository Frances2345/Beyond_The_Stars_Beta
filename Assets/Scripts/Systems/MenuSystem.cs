using UnityEngine;
using UnityEngine.SceneManagement;


public class MenuSystem : MonoBehaviour
{
    public void Jugar()
    {
        SceneManager.LoadScene("ModeSelector");
    }

    public void Salir()
    {
        Debug.Log("Salir del juego");
        Application.Quit();
    }

    public void Creditos()
    {
        SceneManager.LoadScene("Credits");
    }
}
