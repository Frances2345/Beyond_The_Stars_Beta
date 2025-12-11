using UnityEngine;
using System.Collections;
using TMPro;

public class TextFader : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    public float destroyDelay = 7f;

    void Start()
    {
        if (textComponent == null)
        {
            textComponent = GetComponent<TextMeshProUGUI>();
            if (textComponent == null)
            {
                enabled = false;
                return;
            }
        }

        StartCoroutine(StartTimerAndDestroy());
    }

    public void SetText(string message)
    {
        if (textComponent != null)
        {
            textComponent.text = message;
        }
    }

    private IEnumerator StartTimerAndDestroy()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }
}
