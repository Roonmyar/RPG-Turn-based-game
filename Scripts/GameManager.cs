using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance => _instance;

    // Храним позиции персонажей для каждой сцены
    private Dictionary<string, Dictionary<string, Vector3>> characterPositionsByScene = new Dictionary<string, Dictionary<string, Vector3>>();

    [System.Serializable]
    public class CharacterData
    {
        public float strength;
        public float maxStrength;
        public float mana;
        public float maxMana;
        public float armor;
        public float maxArmor;
        public float agility;
        public int level;
        public float experience;
        public float expToNextLevel;

        public CharacterData() { }

        public CharacterData(float strength, float maxStrength, float mana, float maxMana, float armor, float maxArmor, float agility, int level, float experience, float expToNextLevel)
        {
            this.strength = strength;
            this.maxStrength = maxStrength;
            this.mana = mana;
            this.maxMana = maxMana;
            this.armor = armor;
            this.maxArmor = maxArmor;
            this.agility = agility;
            this.level = level;
            this.experience = experience;
            this.expToNextLevel = expToNextLevel;
        }
    }
    private Dictionary<string, CharacterData> characterStats = new Dictionary<string, CharacterData>();

    private int playerCrystals = 0;
    private bool hasDungeonMap = false;
    private bool hasChurchKey = false;

    [SerializeField] private GameObject victoryUIPrefab;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
    }

    public void SaveCharacterPosition(string characterName, Vector3 position)
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (!characterPositionsByScene.ContainsKey(currentScene))
        {
            characterPositionsByScene[currentScene] = new Dictionary<string, Vector3>();
        }
        characterPositionsByScene[currentScene][characterName] = position;
        GameUIManager.Instance.SaveGameWithKeys();
    }

    public Vector3 GetCharacterPosition(string characterName, Vector3 defaultPosition)
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (characterPositionsByScene.ContainsKey(currentScene) && characterPositionsByScene[currentScene].ContainsKey(characterName))
        {
            return characterPositionsByScene[currentScene][characterName];
        }
        return defaultPosition;
    }

    public void SaveCharacterStats(CharacterController character)
    {
        characterStats[character.gameObject.name] = new CharacterData(
            character.strength,
            character.maxStrength,
            character.mana,
            character.maxMana,
            character.armor,
            character.maxArmor,
            character.agility,
            character.GetLevel(),
            character.GetExperience(),
            character.GetExpToNextLevel()
        );
        GameUIManager.Instance.SaveGameWithKeys();
    }

    public CharacterData GetCharacterStats(string characterName)
    {
        if (characterStats.ContainsKey(characterName))
        {
            return characterStats[characterName];
        }
        return null;
    }

    public void AddCrystals(int amount)
    {
        playerCrystals += amount;
        foreach (var quest in QuestManager.Instance.GetActiveQuests())
        {
            foreach (var objective in quest.quest.objectives)
            {
                if (objective.type == Quest.QuestObjective.ObjectiveType.CollectCrystals)
                {
                    QuestManager.Instance.UpdateQuestObjective(quest.quest.questId, objective.targetId, amount);
                }
            }
        }
        GameUIManager.Instance.SaveGameWithKeys();
    }

    public void AddExperience(string characterName, int exp)
    {
        var stats = GetCharacterStats(characterName);
        if (stats == null)
        {
            return;
        }
        stats.experience += exp;
        while (stats.experience >= stats.expToNextLevel)
        {
            stats.level++;
            stats.experience -= stats.expToNextLevel;
            stats.expToNextLevel *= 1.2f;
            stats.maxStrength += 10;
            stats.maxMana += 5;
            stats.maxArmor += 3;
            Debug.Log($"{characterName} достиг уровня {stats.level}!");
        }
        GameUIManager.Instance.SaveGameWithKeys();
    }

    public int GetPlayerCrystals()
    {
        return playerCrystals;
    }

    public void SpendCrystals(int amount)
    {
        playerCrystals = Mathf.Max(0, playerCrystals - amount);
        GameUIManager.Instance.SaveGameWithKeys();
    }

    public void SaveQuestProgress(Quest quest)
    {
        foreach (Quest.QuestObjective objective in quest.objectives)
        {
            PlayerPrefs.SetInt($"Quest_{quest.questId}_Objective_{objective.objectiveId}_CurrentAmount", objective.currentAmount);
        }
        PlayerPrefs.SetInt($"Quest_{quest.questId}_IsCompleted", quest.isCompleted ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void LoadQuestProgress(Quest quest)
    {
        foreach (Quest.QuestObjective objective in quest.objectives)
        {
            objective.currentAmount = PlayerPrefs.GetInt($"Quest_{quest.questId}_Objective_{objective.objectiveId}_CurrentAmount", 0);
        }
        quest.isCompleted = PlayerPrefs.GetInt($"Quest_{quest.questId}_IsCompleted", 0) == 1;
    }

    public void ResetQuestProgress(Quest quest)
    {
        foreach (Quest.QuestObjective objective in quest.objectives)
        {
            objective.currentAmount = 0;
            PlayerPrefs.SetInt($"Quest_{quest.questId}_Objective_{objective.objectiveId}_CurrentAmount", 0);
        }
        quest.isCompleted = false;
        PlayerPrefs.SetInt($"Quest_{quest.questId}_IsCompleted", 0);
        PlayerPrefs.Save();
        Debug.Log($"GameManager: Сброшен прогресс квеста {quest.questId}");
    }

    public void SetDungeonMap(bool hasMap)
    {
        hasDungeonMap = hasMap;
        GameUIManager.Instance.SaveGameWithKeys();
    }

    public bool HasDungeonMap()
    {
        return hasDungeonMap;
    }

    public void SetChurchKey(bool hasKey)
    {
        hasChurchKey = hasKey;
        GameUIManager.Instance.SaveGameWithKeys();
    }

    public bool HasChurchKey()
    {
        return hasChurchKey;
    }

    public void LoadCharacterPositions(Dictionary<string, Vector3> positions)
    {
        // Преобразуем старый формат в новый
        string currentScene = SceneManager.GetActiveScene().name;
        if (!characterPositionsByScene.ContainsKey(currentScene))
        {
            characterPositionsByScene[currentScene] = new Dictionary<string, Vector3>();
        }
        foreach (var pos in positions)
        {
            characterPositionsByScene[currentScene][pos.Key] = pos.Value;
        }
    }

    public void LoadCharacterStats(Dictionary<string, CharacterData> stats)
    {
        characterStats = stats;
    }

    public void LoadCrystals(int crystals)
    {
        playerCrystals = crystals;
    }

    public void LoadDungeonMap(bool hasMap)
    {
        hasDungeonMap = hasMap;
    }

    public void LoadChurchKey(bool hasKey)
    {
        hasChurchKey = hasKey;
    }

    public Dictionary<string, Vector3> GetCharacterPositions()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (characterPositionsByScene.ContainsKey(currentScene))
        {
            return characterPositionsByScene[currentScene];
        }
        return new Dictionary<string, Vector3>();
    }

    public Dictionary<string, CharacterData> GetCharacterStats()
    {
        return characterStats;
    }

    public int GetCrystals()
    {
        return playerCrystals;
    }

    public bool GetDungeonMap()
    {
        return hasDungeonMap;
    }

    public bool GetChurchKey()
    {
        return hasChurchKey;
    }

    public void ShowVictoryUI()
    {
        if (victoryUIPrefab != null)
        {
            Instantiate(victoryUIPrefab, Vector3.zero, Quaternion.identity);
            Debug.Log("UI победы отображено!");
        }
        else
        {
            Debug.LogWarning("VictoryUIPrefab не назначен в GameManager!");
        }
    }

    // Метод для получения всех сохранённых позиций (для обратной совместимости или сохранения)
    public Dictionary<string, Dictionary<string, Vector3>> GetAllCharacterPositionsByScene()
    {
        return characterPositionsByScene;
    }

    // Метод для загрузки всех позиций (для обратной совместимости)
    public void LoadAllCharacterPositions(Dictionary<string, Dictionary<string, Vector3>> positionsByScene)
    {
        characterPositionsByScene = positionsByScene;
    }
}