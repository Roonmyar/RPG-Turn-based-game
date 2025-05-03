using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq; // Добавляем LINQ

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    [SerializeField] private List<CharacterController> characters = new List<CharacterController>();
    [SerializeField] private List<MolotovCocktail> molotovs = new List<MolotovCocktail>();
    private int currentTurnIndex = 0;
    private CameraFollow cameraFollow;
    private bool isCombatActive = false;
    private int roundNumber = 0;

    // UI элементы
    public TMP_Text roundText;
    public GameObject characterList;
    public GameObject characterNameTemplate;
    private List<GameObject> characterNameEntries = new List<GameObject>();
    public int maxTurnDisplayCount = 8;

    // Событие для смены хода
    public event Action<CharacterController> OnTurnChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            Debug.Log("TurnManager создан и установлен как Singleton.");
        }
        else
        {
            Debug.LogWarning("Обнаружен дубликат TurnManager, уничтожаю новый экземпляр.");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        cameraFollow = Camera.main.GetComponent<CameraFollow>();
        if (cameraFollow == null)
        {
            Debug.LogError("CameraFollow не найден!");
        }

        Debug.Log($"Инициализация TurnManager. Коктейлей Молотова: {molotovs.Count}");
        UpdateTurnOrderUI();
        Debug.Log($"Инициализация TurnManager. Зарегистрировано персонажей: {characters.Count}");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Сцена загружена: {scene.name}, Это боевая сцена? {scene.name.EndsWith("_Battle")}");
        Debug.Log($"Текущий список коктейлей Молотова: {molotovs.Count}");
        UpdateTurnOrderUI();

        if (scene.name.EndsWith("_Battle"))
        {
            Debug.Log("Планирую запуск боя после инициализации персонажей");
            StartCoroutine(StartCombatWithDelay());
        }
    }

    private IEnumerator StartCombatWithDelay()
    {
        yield return new WaitForEndOfFrame();
        Debug.Log($"Запускаю бой после задержки. Зарегистрировано персонажей: {characters.Count}");
        StartCombat();
    }

    void Update()
    {
        if (!isCombatActive && Input.GetKeyDown(KeyCode.B))
        {
            Debug.Log("Ручной запуск боя по нажатию клавиши B.");
            StartCombat();
        }
    }

    public void EndCurrentTurn()
    {
        if (!isCombatActive)
        {
            Debug.LogWarning("Бой не активен, невозможно завершить ход.");
            return;
        }

        Debug.Log($"Завершаю ход для {characters[currentTurnIndex].gameObject.name}");
        characters[currentTurnIndex].EndTurn();

        int nextIndex = currentTurnIndex;
        int attempts = 0;

        do
        {
            nextIndex = (nextIndex + 1) % characters.Count;
            attempts++;
            Debug.Log($"Проверяю персонажа {nextIndex}: {(characters[nextIndex] != null ? characters[nextIndex].gameObject.name : "null")}, Strength: {(characters[nextIndex] != null ? characters[nextIndex].strength : -1)}, isInCombat: {(characters[nextIndex] != null ? characters[nextIndex].isInCombat : false)}");

            if (attempts > characters.Count)
            {
                Debug.LogWarning("Нет подходящих персонажей для хода. Завершаем бой.");
                EndCombat();
                return;
            }
        } while (
            characters[nextIndex] == null ||
            characters[nextIndex].strength <= 0 ||
            !characters[nextIndex].isInCombat
        );

        currentTurnIndex = nextIndex;

        if (currentTurnIndex == 0)
        {
            roundNumber++;
            Debug.Log($"Новый раунд: {roundNumber}");
            UpdateRoundText();
        }

        Debug.Log($"Передача хода к: {characters[currentTurnIndex].gameObject.name}");
        StartNextTurn();
    }

    void StartNextTurn()
    {
        Debug.Log($"Начинаю ход для {characters[currentTurnIndex].gameObject.name}, Index: {currentTurnIndex}, Round: {roundNumber}");
        foreach (MolotovCocktail molotov in molotovs.ToArray())
        {
            molotov.OnTurnStart();
        }

        UpdateCameraTarget();
        UpdateTurnOrderUI();

        Debug.Log($"Вызываю StartTurn для {characters[currentTurnIndex].gameObject.name}");
        characters[currentTurnIndex].StartTurn();
        OnTurnChanged?.Invoke(characters[currentTurnIndex]);
    }

    void UpdateCameraTarget()
    {
        if (cameraFollow != null)
        {
            cameraFollow.target = characters[currentTurnIndex].transform;
            Debug.Log($"Камера нацелена на {characters[currentTurnIndex].gameObject.name}");
        }
    }

    public void RegisterCharacter(CharacterController character)
    {
        if (!characters.Contains(character))
        {
            characters.Add(character);
            Debug.Log($"{character.gameObject.name} зарегистрирован в TurnManager. Всего персонажей: {characters.Count}, Agility: {character.GetAgility()}, isEnemy: {character.isEnemy}");
            UpdateTurnOrderUI();
        }
        else
        {
            Debug.LogWarning($"{character.gameObject.name} уже зарегистрирован в TurnManager!");
        }
    }

    public void RemoveCharacter(CharacterController character)
    {
        if (characters.Contains(character))
        {
            characters.Remove(character);
            Debug.Log($"{character.gameObject.name} удалён из списка персонажей в TurnManager. Осталось персонажей: {characters.Count}");
            UpdateTurnOrderUI();

            if (SceneManager.GetActiveScene().name.EndsWith("_Battle"))
            {
                bool anyEnemiesLeft = false;
                foreach (var c in characters)
                {
                    if (c.isEnemy)
                    {
                        anyEnemiesLeft = true;
                        break;
                    }
                }

                if (!anyEnemiesLeft)
                {
                    Debug.Log("Все враги побеждены! Возвращаемся в основную локацию.");
                    EndCombat();
                    string baseScene = SceneManager.GetActiveScene().name.Replace("_Battle", "");
                    LocationManager.Instance.GoToLocation(baseScene);
                }
            }
        }
        else
        {
            Debug.LogWarning($"{character.gameObject.name} не найден в списке персонажей TurnManager!");
        }
    }

    public void RegisterMolotov(MolotovCocktail molotov)
    {
        if (!molotovs.Contains(molotov))
        {
            molotovs.Add(molotov);
            Debug.Log("Коктейль Молотова зарегистрирован в TurnManager");
        }
    }

    public void UnregisterMolotov(MolotovCocktail molotov)
    {
        if (molotovs.Contains(molotov))
        {
            molotovs.Remove(molotov);
            Debug.Log("Коктейль Молотова удалён из TurnManager");
        }
    }

    public void StartCombat()
    {
        Debug.Log($"Попытка начать бой. Количество персонажей: {characters.Count}, Сцена: {SceneManager.GetActiveScene().name}, isCombatActive: {isCombatActive}");
        Debug.Log($"Герои: {string.Join(", ", characters.Where(c => !c.isEnemy).Select(c => c.gameObject.name))}");
        Debug.Log($"Враги: {string.Join(", ", characters.Where(c => c.isEnemy).Select(c => c.gameObject.name))}");
        if (characters.Count == 0)
        {
            Debug.LogWarning("Список персонажей пуст! Нельзя начать бой.");
            return;
        }

        if (isCombatActive)
        {
            Debug.Log("Бой уже идёт!");
            return;
        }

        characters.Sort((a, b) => b.GetAgility().CompareTo(a.GetAgility()));
        Debug.Log($"Персонажи отсортированы по ловкости: {string.Join(", ", characters.ConvertAll(c => $"{c.gameObject.name} (Agility: {c.GetAgility()})"))}");

        isCombatActive = true;
        roundNumber = 1;
        currentTurnIndex = 0;
        UpdateRoundText();
        UpdateCameraTarget();
        foreach (var character in characters)
        {
            Debug.Log($"Вызываю StartCombat для {character.gameObject.name}");
            character.StartCombat();
        }
        Debug.Log($"Начинаю бой: первый ход для {characters[currentTurnIndex].gameObject.name}");
        characters[currentTurnIndex].StartTurn();
        OnTurnChanged?.Invoke(characters[currentTurnIndex]);
        Debug.Log("Бой начался");
    }

    public void EndCombat()
    {
        isCombatActive = false;
        foreach (var character in characters)
        {
            character.EndCombat();
        }
        UpdateCameraTarget();
        Debug.Log("Бой закончился");
    }

    private void UpdateRoundText()
    {
        if (roundText != null)
        {
            roundText.text = $"Round {roundNumber}";
        }
        else
        {
            Debug.LogError("RoundText не назначен в TurnManager!");
        }
    }

    private void UpdateTurnOrderUI()
    {
        if (characterList == null || characterNameTemplate == null)
        {
            Debug.LogError("CharacterList или CharacterNameTemplate не назначены в TurnManager!");
            return;
        }

        Debug.Log($"Обновляю UI порядка ходов. Персонажей: {characters.Count}");
        foreach (GameObject entry in characterNameEntries)
        {
            if (entry != null)
            {
                Destroy(entry);
            }
        }
        characterNameEntries.Clear();

        if (characters.Count == 0)
        {
            Debug.LogWarning("Список персонажей пуст, UI порядка ходов не обновляется.");
            return;
        }

        for (int i = 0; i < Mathf.Min(maxTurnDisplayCount, characters.Count); i++)
        {
            int index = (currentTurnIndex + i) % characters.Count;
            if (characters[index] == null)
            {
                Debug.LogWarning($"Персонаж на индексе {index} равен null, пропускаю");
                continue;
            }
            GameObject newEntry = Instantiate(characterNameTemplate, characterList.transform);
            newEntry.SetActive(true);
            TMP_Text textComponent = newEntry.GetComponent<TMP_Text>();
            if (textComponent != null)
            {
                textComponent.text = characters[index].gameObject.name + (i == 0 ? " (Now)" : "");
            }
            else
            {
                Debug.LogWarning("TMP_Text не найден на characterNameTemplate!");
            }
            characterNameEntries.Add(newEntry);
        }
        Debug.Log($"UI порядка ходов обновлён. Создано записей: {characterNameEntries.Count}");
    }

    public CharacterController GetCurrentCharacter()
    {
        if (characters.Count == 0 || currentTurnIndex < 0 || currentTurnIndex >= characters.Count)
        {
            Debug.LogWarning("Нет доступных персонажей для получения текущего хода!");
            return null;
        }

        return characters[currentTurnIndex];
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Debug.Log("TurnManager уничтожен.");
    }
}