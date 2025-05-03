using UnityEngine;
using System.Collections.Generic;
using System;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }

    private Dictionary<string, int> attributes = new Dictionary<string, int>();
    private List<Item> inventory = new List<Item>();
    private List<Item> backpack = new List<Item>();
    private Dictionary<Item.ItemType, Item> equippedItems = new Dictionary<Item.ItemType, Item>();

    [Serializable]
    public class ItemData
    {
        public string name;
        public string type;

        public static ItemData FromItem(Item item)
        {
            return new ItemData { name = item.Name, type = item.Type.ToString() };
        }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameUIManager: Awake() - GameUIManager создан и сохранён между сценами.");
        }
        else
        {
            Debug.Log("GameUIManager: Awake() - Дубликат GameUIManager уничтожен.");
            Destroy(gameObject);
        }

        Debug.Log("GameUIManager: Awake() - Начинаю загрузку данных.");
        // Очистим сохранённые позиции при старте игры
        ClearSavedPositions();
        LoadGame();
    }

    void Start()
    {
        Debug.Log("GameUIManager: Start() - Начало инициализации GameUIManager.");
    }

    void OnApplicationQuit()
    {
        Debug.Log("GameUIManager: OnApplicationQuit() - Сохранение игры перед выходом.");
        SaveGameWithKeys();
    }

    private void ClearSavedPositions()
    {
        PlayerPrefs.SetString("Position_Keys", "");
        Debug.Log("GameUIManager: ClearSavedPositions() - Сохранённые позиции очищены.");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadAllCharacterPositions(new Dictionary<string, Dictionary<string, Vector3>>());
        }
    }

    public void SetAttribute(string key, int value)
    {
        attributes[key] = value;
        Debug.Log($"GameUIManager: SetAttribute() - Установлен атрибут {key}: {value}");
        SaveGameWithKeys();
    }

    public int GetAttribute(string key, int defaultValue = 0)
    {
        int value = attributes.ContainsKey(key) ? attributes[key] : defaultValue;
        Debug.Log($"GameUIManager: GetAttribute() - Получен атрибут {key}: {value}");
        return value;
    }

    public void AddToInventory(Item item)
    {
        inventory.Add(item);
        Debug.Log($"GameUIManager: AddToInventory() - Добавлен предмет в инвентарь: {item.Name}");
        SaveGameWithKeys();
    }

    public void RemoveFromInventory(Item item)
    {
        inventory.Remove(item);
        Debug.Log($"GameUIManager: RemoveFromInventory() - Удалён предмет из инвентаря: {item.Name}");
        SaveGameWithKeys();
    }

    public List<Item> GetInventory()
    {
        Debug.Log($"GameUIManager: GetInventory() - Возвращено предметов в инвентаре: {inventory.Count}");
        return new List<Item>(inventory);
    }

    public void AddToBackpack(Item item)
    {
        backpack.Add(item);
        Debug.Log($"GameUIManager: AddToBackpack() - Добавлен предмет в рюкзак: {item.Name}");
        SaveGameWithKeys();
    }

    public void RemoveFromBackpack(Item item)
    {
        backpack.Remove(item);
        Debug.Log($"GameUIManager: RemoveFromBackpack() - Удалён предмет из рюкзака: {item.Name}");
        SaveGameWithKeys();
    }

    public List<Item> GetBackpack()
    {
        Debug.Log($"GameUIManager: GetBackpack() - Возвращено предметов в рюкзаке: {backpack.Count}");
        return new List<Item>(backpack);
    }

    public void EquipItem(Item item)
    {
        if (equippedItems.ContainsKey(item.Type))
        {
            Item oldItem = equippedItems[item.Type];
            inventory.Add(oldItem);
        }
        equippedItems[item.Type] = item;
        inventory.Remove(item);
        SaveGameWithKeys();
    }

    public void UnequipItem(Item.ItemType type)
    {
        if (equippedItems.ContainsKey(type))
        {
            Item item = equippedItems[type];
            inventory.Add(item);
            equippedItems.Remove(type);
            SaveGameWithKeys();
        }
    }

    public Item GetEquippedItem(Item.ItemType type)
    {
        Item item = equippedItems.ContainsKey(type) ? equippedItems[type] : null;
        return item;
    }

    public void SaveGame()
    {
        foreach (var attr in attributes)
        {
            PlayerPrefs.SetInt("Attribute_" + attr.Key, attr.Value);
        }

        List<ItemData> inventoryData = inventory.ConvertAll(item => ItemData.FromItem(item));
        PlayerPrefs.SetString("Inventory", JsonUtility.ToJson(new { items = inventoryData }));

        List<ItemData> backpackData = backpack.ConvertAll(item => ItemData.FromItem(item));
        PlayerPrefs.SetString("Backpack", JsonUtility.ToJson(new { items = backpackData }));

        List<ItemData> equippedData = new List<ItemData>();
        foreach (var pair in equippedItems)
        {
            equippedData.Add(ItemData.FromItem(pair.Value));
        }
        PlayerPrefs.SetString("EquippedItems", JsonUtility.ToJson(new { items = equippedData }));

        var gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            var positionsByScene = gameManager.GetAllCharacterPositionsByScene();
            foreach (var sceneEntry in positionsByScene)
            {
                string sceneName = sceneEntry.Key;
                foreach (var posEntry in sceneEntry.Value)
                {
                    string key = $"Position_{sceneName}_{posEntry.Key}";
                    Vector3 pos = posEntry.Value;
                    PlayerPrefs.SetFloat($"{key}_X", pos.x);
                    PlayerPrefs.SetFloat($"{key}_Y", pos.y);
                    PlayerPrefs.SetFloat($"{key}_Z", pos.z);
                }
            }

            foreach (var stat in gameManager.GetCharacterStats())
            {
                string json = JsonUtility.ToJson(stat.Value);
                PlayerPrefs.SetString("Stats_" + stat.Key, json);
            }

            PlayerPrefs.SetInt("PlayerCrystals", gameManager.GetCrystals());

            // Сохранение прогресса квестов
            foreach (var questStatus in QuestManager.Instance.GetActiveQuests())
            {
                GameManager.Instance.SaveQuestProgress(questStatus.quest);
            }

            PlayerPrefs.SetInt("HasDungeonMap", gameManager.GetDungeonMap() ? 1 : 0);
            PlayerPrefs.SetInt("HasChurchKey", gameManager.GetChurchKey() ? 1 : 0);
        }
        else
        {
            Debug.LogWarning("GameUIManager: SaveGame() - GameManager не найден!");
        }

        PlayerPrefs.Save();
    }

    public void LoadGame()
    {
        attributes.Clear();
        foreach (var key in PlayerPrefs.GetString("Attribute_Keys", "").Split(','))
        {
            if (!string.IsNullOrEmpty(key))
            {
                attributes[key] = PlayerPrefs.GetInt("Attribute_" + key, 0);
            }
        }

        inventory.Clear();
        string inventoryJson = PlayerPrefs.GetString("Inventory", "");
        if (!string.IsNullOrEmpty(inventoryJson))
        {
            var inventoryWrapper = JsonUtility.FromJson<SerializableList<ItemData>>(inventoryJson);
            foreach (var itemData in inventoryWrapper.items)
            {
                inventory.Add(Item.FromSerialized(itemData.name, itemData.type));
            }
        }

        backpack.Clear();
        string backpackJson = PlayerPrefs.GetString("Backpack", "");
        if (!string.IsNullOrEmpty(backpackJson))
        {
            var backpackWrapper = JsonUtility.FromJson<SerializableList<ItemData>>(backpackJson);
            foreach (var itemData in backpackWrapper.items)
            {
                backpack.Add(Item.FromSerialized(itemData.name, itemData.type));
            }
        }

        equippedItems.Clear();
        string equippedJson = PlayerPrefs.GetString("EquippedItems", "");
        if (!string.IsNullOrEmpty(equippedJson))
        {
            var equippedWrapper = JsonUtility.FromJson<SerializableList<ItemData>>(equippedJson);
            foreach (var itemData in equippedWrapper.items)
            {
                Item item = Item.FromSerialized(itemData.name, itemData.type);
                equippedItems[item.Type] = item;
            }
        }

        var gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            Dictionary<string, Dictionary<string, Vector3>> positionsByScene = new Dictionary<string, Dictionary<string, Vector3>>();
            // Предположим, что у нас есть список сцен (можно настроить вручную)
            string[] scenes = new string[] { "MainScene", "Dungeon", "Church" }; // Укажите имена ваших сцен
            foreach (string scene in scenes)
            {
                positionsByScene[scene] = new Dictionary<string, Vector3>();
                string[] characterNames = new string[] { "Player", "TankMage", "RangedDD" }; // Укажите имена ваших персонажей
                foreach (string characterName in characterNames)
                {
                    string key = $"Position_{scene}_{characterName}";
                    if (PlayerPrefs.HasKey($"{key}_X"))
                    {
                        float x = PlayerPrefs.GetFloat($"{key}_X");
                        float y = PlayerPrefs.GetFloat($"{key}_Y");
                        float z = PlayerPrefs.GetFloat($"{key}_Z");
                        positionsByScene[scene][characterName] = new Vector3(x, y, z);
                    }
                }
            }
            gameManager.LoadAllCharacterPositions(positionsByScene);

            Dictionary<string, GameManager.CharacterData> characterStats = new Dictionary<string, GameManager.CharacterData>();
            foreach (var key in PlayerPrefs.GetString("Stats_Keys", "").Split(','))
            {
                if (!string.IsNullOrEmpty(key))
                {
                    string json = PlayerPrefs.GetString("Stats_" + key, "");
                    if (!string.IsNullOrEmpty(json))
                    {
                        characterStats[key] = JsonUtility.FromJson<GameManager.CharacterData>(json);
                    }
                }
            }
            gameManager.LoadCharacterStats(characterStats);

            int crystals = PlayerPrefs.GetInt("PlayerCrystals", 0);
            gameManager.LoadCrystals(crystals);

            bool hasDungeonMap = PlayerPrefs.GetInt("HasDungeonMap", 0) == 1;
            bool hasChurchKey = PlayerPrefs.GetInt("HasChurchKey", 0) == 1;
            gameManager.LoadDungeonMap(hasDungeonMap);
            gameManager.LoadChurchKey(hasChurchKey);
        }
        else
        {
            Debug.LogWarning("GameUIManager: LoadGame() - GameManager не найден!");
        }

        Debug.Log("GameUIManager: LoadGame() - Игра загружена.");
    }

    [Serializable]
    private class SerializableList<T>
    {
        public List<T> items;
    }

    private void SaveKeys()
    {
        PlayerPrefs.SetString("Attribute_Keys", string.Join(",", attributes.Keys));
        if (GameManager.Instance != null)
        {
            var positionsByScene = GameManager.Instance.GetAllCharacterPositionsByScene();
            List<string> positionKeys = new List<string>();
            foreach (var sceneEntry in positionsByScene)
            {
                foreach (var posEntry in sceneEntry.Value)
                {
                    positionKeys.Add($"{sceneEntry.Key}_{posEntry.Key}");
                }
            }
            PlayerPrefs.SetString("Position_Keys", string.Join(",", positionKeys));

            PlayerPrefs.SetString("Stats_Keys", string.Join(",", GameManager.Instance.GetCharacterStats().Keys));
        }
    }

    public void SaveGameWithKeys()
    {
        SaveKeys();
        SaveGame();
    }

    public void ShowQuestNotification(string message)
    {
        Debug.Log($"UI: {message}");
        // Здесь можно добавить отображение уведомления в UI
    }
}