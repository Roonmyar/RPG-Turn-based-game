using UnityEngine;
using UnityEngine.SceneManagement;

public class LocationManager : MonoBehaviour
{
    public static LocationManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("LocationManager создан и установлен как Singleton.");
        }
        else
        {
            Debug.LogWarning("Обнаружен дубликат LocationManager, уничтожаю новый экземпляр.");
            Destroy(gameObject);
        }
    }

    public void GoToLocation(string locationName)
    {
        Debug.Log($"LocationManager: Переход на сцену {locationName}");
        SceneManager.LoadScene(locationName);
        Debug.Log($"LocationManager: Сцена {locationName} загружена");
    }

    void OnDestroy()
    {
        Debug.Log("LocationManager уничтожен.");
    }
}