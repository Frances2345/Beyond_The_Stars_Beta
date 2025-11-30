using UnityEngine;
using TMPro;

public class HealthPackCount : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI HealthPack;

    void Start()
    {
        if (HealthPack == null)
        {
            HealthPack = GetComponent<TextMeshProUGUI>();
        }

        Player1 player = Player1.Instance;

        if (player != null && HealthPack != null)
        {
            player.OnHealthPackChanged += UpdateHealthPackText;

            UpdateHealthPackText(player.MaxHealthPacks, player.MaxHealthPacks);
        }
        else
        {
            Debug.LogError("Error: HealthPackCount no encontró el componente TextMeshPro o Player1.Instance.");
        }
    }

    private void UpdateHealthPackText(int currentStacks, int maxStacks)
    {
        HealthPack.text = currentStacks + " / " + maxStacks;
    }

    void OnDestroy()
    {
        if (Player1.Instance != null)
        {
            Player1.Instance.OnHealthPackChanged -= UpdateHealthPackText;
        }
    }
}