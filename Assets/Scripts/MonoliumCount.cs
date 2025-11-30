using UnityEngine;
using TMPro;

public class MonoliumCount : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI monoliumText;

    void Start()
    {
        if (monoliumText == null)
        {
            monoliumText = GetComponent<TextMeshProUGUI>();
        }

        Player1 player = Player1.Instance;

        if (player != null && monoliumText != null)
        {
            player.OnMonoliumCountChanged += UpdateMonoliumText;

            UpdateMonoliumText(player.MonoliumCountValue, player.MaxMonoliumValue);
        }
        else
        {
            Debug.LogError("Error: MonoliumCount no encontró el componente TextMeshPro");
        }
    }

    private void UpdateMonoliumText(int currentCount, int maxCount)
    {
        monoliumText.text = currentCount + " / " + maxCount;
    }

    void OnDestroy()
    {
        if (Player1.Instance != null)
        {
            Player1.Instance.OnMonoliumCountChanged -= UpdateMonoliumText;
        }
    }
}