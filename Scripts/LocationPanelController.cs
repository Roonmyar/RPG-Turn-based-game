using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LocationPanelController : MonoBehaviour
{
    public static LocationPanelController Instance { get; private set; }

    public GameObject locationPanel; // Ссылка на панель с кнопками локаций
    public Button toggleButton; // Ссылка на кнопку открытия/закрытия панели
    private bool isPanelVisible = false; // Флаг видимости панели

    void Awake()
    {
        // Реализуем паттерн Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Делаем объект персистентным
        Debug.Log($"{gameObject.name}: LocationPanelController теперь персистентен через сцены.");
    }

    void Start()
    {
        // Проверяем, что ссылки назначены
        if (locationPanel == null)
        {
            Debug.LogError($"{gameObject.name}: LocationPanel не назначен в инспекторе!");
            return;
        }

        if (toggleButton == null)
        {
            Debug.LogError($"{gameObject.name}: ToggleButton не назначен в инспекторе!");
            return;
        }

        // Изначально панель скрыта
        locationPanel.SetActive(false);
        isPanelVisible = false;

        // Добавляем слушатель на кнопку
        toggleButton.onClick.RemoveAllListeners(); // Удаляем старые слушатели на всякий случай
        toggleButton.onClick.AddListener(TogglePanel);

        // Подписываемся на событие смены сцены для отладки
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        // Отписываемся от события при уничтожении
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"{gameObject.name}: Сцена загружена: {scene.name}. LocationPanelController всё ещё активен.");
        // Проверяем, что ссылки всё ещё валидны
        if (locationPanel == null)
        {
            Debug.LogError($"{gameObject.name}: LocationPanel потерян после смены сцены!");
        }
        if (toggleButton == null)
        {
            Debug.LogError($"{gameObject.name}: ToggleButton потерян после смены сцены!");
        }
        else
        {
            // Переподключаем слушатель на случай, если кнопка была пересоздана
            toggleButton.onClick.RemoveAllListeners();
            toggleButton.onClick.AddListener(TogglePanel);
        }
    }

    void Update()
    {
        // Переключение панели по клавише M
        if (Input.GetKeyDown(KeyCode.M))
        {
            TogglePanel();
        }
    }

    private void TogglePanel()
    {
        if (locationPanel == null)
        {
            Debug.LogError($"{gameObject.name}: LocationPanel отсутствует, нельзя переключить видимость!");
            return;
        }

        isPanelVisible = !isPanelVisible;
        locationPanel.SetActive(isPanelVisible);
        Debug.Log($"{gameObject.name}: Панель {(isPanelVisible ? "показана" : "скрыта")}");
    }
}