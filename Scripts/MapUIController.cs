using UnityEngine;
using UnityEngine.UI;

public class MapUIController : MonoBehaviour
{
    public Button tavernButton; // Кнопка для перехода в таверну
    public Button caveButton;   // Кнопка для перехода в пещеру
    public Button churchButton; // Кнопка для перехода в церковь

    void Start()
    {
        // Проверяем, назначены ли кнопки
        if (tavernButton == null)
        {
            Debug.LogError("TavernButton не назначен в MapUIController!");
        }
        else
        {
            tavernButton.onClick.AddListener(() =>
            {
                if (LocationManager.Instance == null)
                {
                    Debug.LogError("LocationManager.Instance не инициализирован!");
                    return;
                }
                LocationManager.Instance.GoToLocation("Tavern");
            });
        }

        if (caveButton == null)
        {
            Debug.LogError("CaveButton не назначен в MapUIController!");
        }
        else
        {
            caveButton.onClick.AddListener(() =>
            {
                if (LocationManager.Instance == null)
                {
                    Debug.LogError("LocationManager.Instance не инициализирован!");
                    return;
                }
                LocationManager.Instance.GoToLocation("Cave");
            });
        }

        if (churchButton == null)
        {
            Debug.LogError("ChurchButton не назначен в MapUIController!");
        }
        else
        {
            churchButton.onClick.AddListener(() =>
            {
                if (LocationManager.Instance == null)
                {
                    Debug.LogError("LocationManager.Instance не инициализирован!");
                    return;
                }
                LocationManager.Instance.GoToLocation("Church");
            });
        }
    }
}