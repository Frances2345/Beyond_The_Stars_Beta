using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


public class passLevel : MonoBehaviour
{
    public GameObject STAR;
    public GameObject jugador; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("pasando nivel");
        if( collision.gameObject == STAR )
        {
            SceneManager.LoadScene("Level 2");
        }
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }
}
