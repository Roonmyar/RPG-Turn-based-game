using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Добавляем для работы с EventSystem

public class EnemyTrigger : MonoBehaviour
{
    public string battleSceneName; // Название боевой сцены (например, "Church_Battle")
    public GameObject battleButton; // UI-кнопка "В бой"
    private static EnemyTrigger currentlySelectedEnemy;

    void Start()
    {
        if (battleButton == null)
        {
            Debug.LogError($"{gameObject.name}: BattleButton не назначен в инспекторе!");
            return;
        }

        battleButton.SetActive(false);
        Button btn = battleButton.GetComponent<Button>();
        if (btn == null)
        {
            Debug.LogError($"{gameObject.name}: BattleButton не имеет компонента Button!");
            return;
        }

        btn.onClick.AddListener(() =>
        {
            Debug.Log($"Нажата кнопка 'В бой' на враге {gameObject.name}. Пытаюсь перейти в сцену: {battleSceneName}");
            if (string.IsNullOrEmpty(battleSceneName))
            {
                Debug.LogError($"battleSceneName пустое или не задано для врага {gameObject.name}!");
                return;
            }
            if (LocationManager.Instance == null)
            {
                Debug.LogError("LocationManager.Instance не инициализирован!");
                return;
            }
            Debug.Log("Вызываю LocationManager.Instance.GoToLocation...");
            LocationManager.Instance.GoToLocation(battleSceneName);
            Debug.Log("LocationManager.Instance.GoToLocation вызван успешно.");
            HideButton();
            currentlySelectedEnemy = null;
        });
    }

    void OnMouseDown()
    {
        if (gameObject.CompareTag("Enemy"))
        {
            Debug.Log($"{gameObject.name}: Клик по врагу обнаружен");
            if (currentlySelectedEnemy != null && currentlySelectedEnemy != this)
            {
                currentlySelectedEnemy.HideButton();
            }
            ShowButton();
            currentlySelectedEnemy = this;
        }
    }

    void Update()
    {
        if (currentlySelectedEnemy == this && Input.GetMouseButtonDown(0))
        {
            Debug.Log($"{gameObject.name}: Обнаружен клик мышью, проверяю цель клика");

            // Проверяем, кликнули ли по UI-элементу (например, по кнопке)
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log($"{gameObject.name}: Клик по UI-элементу (например, кнопке), кнопка остаётся видимой");
                return; // Не скрываем кнопку, если кликнули по UI
            }

            // Если клик не по UI, проверяем физический объект
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log($"{gameObject.name}: Raycast попал в объект: {hit.collider.name}");
                if (hit.collider.gameObject != gameObject && !hit.collider.CompareTag("Button"))
                {
                    HideButton();
                    currentlySelectedEnemy = null;
                    Debug.Log($"{gameObject.name}: Клик в другое место, кнопка скрыта");
                }
            }
            else
            {
                HideButton();
                currentlySelectedEnemy = null;
                Debug.Log($"{gameObject.name}: Клик вне объектов, кнопка скрыта");
            }
        }
    }

    private void ShowButton()
    {
        if (battleButton != null)
        {
            battleButton.SetActive(true);
            Debug.Log($"{gameObject.name}: Кнопка 'В бой' показана");
        }
    }

    private void HideButton()
    {
        if (battleButton != null)
        {
            battleButton.SetActive(false);
            Debug.Log($"{gameObject.name}: Кнопка 'В бой' скрыта");
        }
    }
}