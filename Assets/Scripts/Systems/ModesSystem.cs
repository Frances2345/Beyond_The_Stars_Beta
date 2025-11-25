using UnityEngine;
using UnityEngine.SceneManagement;


public class ModesSystem : MonoBehaviour
{
    public void Historia()
    {
        SceneManager.LoadScene("PlayerSelector");
    }

    public void Rush()
    {
        Debug.Log("proximamente");
        SceneManager.LoadScene("Rush");
    }

    public void Titan()
    {
        Debug.Log("proximamente");
        SceneManager.LoadScene("Titan");
    }
    public void Tutorial()
    {
        Debug.Log("proximamente");
        SceneManager.LoadScene("Tutorial");
    }

    public void Anterior()
    {
        SceneManager.LoadScene("Menu");
    }
}
