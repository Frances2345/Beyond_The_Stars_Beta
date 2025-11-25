using UnityEngine;
using UnityEngine.SceneManagement;


public class PlayerSelector : MonoBehaviour
{
    public void Norteamerica()
    {
        SceneManager.LoadScene("Level 1");
    }

    public void Europa()
    {
        Debug.Log("proximamente");
        SceneManager.LoadScene("");
    }

    public void Asia()
    {
        Debug.Log("proximamente");
        SceneManager.LoadScene("");
    }
   
    public void Anterior()
    {
        SceneManager.LoadScene("ModeSelector");
    }
}
