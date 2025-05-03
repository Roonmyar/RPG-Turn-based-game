using UnityEngine;
using TMPro;

public class NPCHintUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI hintText; // Текст подсказки
    [SerializeField] private GameObject hintPanel; // Панель для текста (фон)

    private void Awake()
    {
        // Проверка назначенных компонентов
        if (hintText == null)
        {
            Debug.LogError($"NPCHintUI: hintText не назначен на {gameObject.name}!");
        }
        if (hintPanel == null)
        {
            Debug.LogError($"NPCHintUI: hintPanel не назначен на {gameObject.name}!");
        }
    }

    private void Start()
    {
        // Скрыть подсказку при старте
        HideHint();
    }

    public void ShowHint(string message)
    {
        if (hintPanel != null && hintText != null)
        {
            hintPanel.SetActive(true);
            hintText.text = message;
            Debug.Log($"NPCHintUI: Показана подсказка на {gameObject.name}: {message}");
        }
    }

    public void HideHint()
    {
        if (hintPanel != null)
        {
            hintPanel.SetActive(false);
            Debug.Log($"NPCHintUI: Подсказка скрыта на {gameObject.name}");
        }
    }
}