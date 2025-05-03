using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameUIController : MonoBehaviour
{
    public static GameUIController Instance { get; private set; }

    public GameObject attributesPanel;
    public GameObject backpackPanel;
    public GameObject inventoryPanel;
    public GameObject questJournalPanel;

    public Button attributesButton;
    public Button backpackButton;
    public Button inventoryButton;
    public Button questJournalButton;

    public TMP_Text characterNameText;
    public TMP_Text levelText;
    public TMP_Text expText;
    public TMP_Text strengthText;
    public TMP_Text manaText;
    public TMP_Text armorText;
    public TMP_Text agilityText;
    public TMP_Text moveDistanceText;
    public TMP_Text crystalsText;

    public GameObject backpackItemTemplate;
    public Transform backpackItemsGrid;

    public GameObject inventoryItemTemplate;
    public GameObject equipmentSlots;
    public GameObject armorSlot;
    public GameObject helmetSlot;
    public GameObject bootsSlot;
    public GameObject inventoryItemsList;

    [SerializeField] private Sprite healingPotionIcon;
    [SerializeField] private Sprite shieldPotionIcon;
    [SerializeField] private Sprite manaCrystalIcon;
    [SerializeField] private Sprite healthGemIcon;
    [SerializeField] private Sprite ironArmorIcon;
    [SerializeField] private Sprite steelHelmetIcon;
    [SerializeField] private Sprite leatherBootsIcon;

    private bool isAttributesOpen = false;
    private bool isBackpackOpen = false;
    private bool isInventoryOpen = false;
    private bool isQuestJournalOpen = false;
    private bool attributesNeedUpdate = false; // Флаг для обновления атрибутов

    private CharacterController currentCharacter;

    private Dictionary<Item.ItemType, EquipmentSlot> equipmentSlotsDict = new Dictionary<Item.ItemType, EquipmentSlot>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            Debug.Log("GameUIController: Awake() - Дубликат GameUIController уничтожен.");
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("GameUIController: Awake() - GameUIController создан и сохранён между сценами.");
    }

    void Start()
    {
        Debug.Log("GameUIController: Start() - Начало инициализации GameUIController.");

        ValidateReferences();

        attributesPanel.SetActive(false);
        backpackPanel.SetActive(false);
        inventoryPanel.SetActive(false);
        questJournalPanel.SetActive(false);
        backpackItemTemplate.SetActive(false);
        inventoryItemTemplate.SetActive(false);

        attributesButton.onClick.AddListener(ToggleAttributesPanel);
        backpackButton.onClick.AddListener(ToggleBackpackPanel);
        inventoryButton.onClick.AddListener(ToggleInventoryPanel);
        questJournalButton.onClick.AddListener(ToggleQuestJournalPanel);

        if (QuestJournalUI.Instance == null)
        {
            Debug.LogError("GameUIController: Start() - QuestJournalUI не найден в сцене!");
        }
        else
        {
            Debug.Log("GameUIController: Start() - QuestJournalUI найден.");
        }

        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnTurnChanged += UpdateCurrentCharacter;
            Debug.Log("GameUIController: Start() - Подписка на OnTurnChanged.");
        }

        SceneManager.sceneLoaded += OnSceneLoaded;

        SetupEquipmentSlots();

        InitializeItems();

        Item.InitializeIcons(
            healingPotionIcon,
            shieldPotionIcon,
            manaCrystalIcon,
            healthGemIcon,
            ironArmorIcon,
            steelHelmetIcon,
            leatherBootsIcon
        );

        UpdateCurrentCharacterOnStart();
    }

    void OnDestroy()
    {
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnTurnChanged -= UpdateCurrentCharacter;
        }
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Debug.Log("GameUIController: OnDestroy() - GameUIController уничтожен.");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"GameUIController: OnSceneLoaded() - Сцена загружена: {scene.name}. Обновляю UI и данные персонажа.");
        UpdateCurrentCharacterOnStart();

        if (isAttributesOpen)
        {
            attributesNeedUpdate = true;
            UpdateAttributesPanel();
        }
        if (isBackpackOpen)
        {
            PositionBackpackPanel();
            UpdateBackpackItems();
        }
        if (isInventoryOpen)
        {
            UpdateInventoryItems();
            UpdateEquipmentSlots();
        }
        if (isQuestJournalOpen)
        {
            QuestJournalUI.Instance?.ToggleJournal();
        }
    }

    private void UpdateCurrentCharacterOnStart()
    {
        CharacterController[] characters = FindObjectsOfType<CharacterController>();
        foreach (var character in characters)
        {
            if (character.isMainCharacter)
            {
                currentCharacter = character;
                Debug.Log($"GameUIController: UpdateCurrentCharacterOnStart() - Найден главный персонаж: {currentCharacter.gameObject.name}");
                break;
            }
        }

        if (currentCharacter == null)
        {
            foreach (var character in characters)
            {
                if (!character.isEnemy)
                {
                    currentCharacter = character;
                    Debug.Log($"GameUIController: UpdateCurrentCharacterOnStart() - Главный персонаж не найден, выбран первый игрок: {currentCharacter.gameObject.name}");
                    break;
                }
            }
        }

        if (currentCharacter == null)
        {
            Debug.LogWarning("GameUIController: UpdateCurrentCharacterOnStart() - Персонаж не найден в сцене!");
        }
        else if (isAttributesOpen)
        {
            attributesNeedUpdate = true;
            UpdateAttributesPanel();
        }
    }

    private void UpdateCurrentCharacter(CharacterController newCharacter)
    {
        if (newCharacter != null && newCharacter != currentCharacter)
        {
            currentCharacter = newCharacter;
            Debug.Log($"GameUIController: UpdateCurrentCharacter() - Текущий персонаж обновлён: {currentCharacter.gameObject.name}");
            if (isAttributesOpen)
            {
                attributesNeedUpdate = true;
                UpdateAttributesPanel();
            }
            if (isBackpackOpen)
            {
                UpdateBackpackItems();
            }
            if (isInventoryOpen)
            {
                UpdateInventoryItems();
                UpdateEquipmentSlots();
            }
            if (isQuestJournalOpen)
            {
                QuestJournalUI.Instance?.ToggleJournal();
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            ToggleAttributesPanel();
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleBackpackPanel();
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventoryPanel();
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            ToggleQuestJournalPanel();
        }
    }

    private void ValidateReferences()
    {
        if (attributesPanel == null) Debug.LogError("GameUIController: ValidateReferences() - AttributesPanel не назначен!");
        if (backpackPanel == null) Debug.LogError("GameUIController: ValidateReferences() - BackpackPanel не назначен!");
        if (inventoryPanel == null) Debug.LogError("GameUIController: ValidateReferences() - InventoryPanel не назначен!");
        if (questJournalPanel == null) Debug.LogError("GameUIController: ValidateReferences() - QuestJournalPanel не назначен!");
        if (attributesButton == null) Debug.LogError("GameUIController: ValidateReferences() - AttributesButton не назначен!");
        if (backpackButton == null) Debug.LogError("GameUIController: ValidateReferences() - BackpackButton не назначен!");
        if (inventoryButton == null) Debug.LogError("GameUIController: ValidateReferences() - InventoryButton не назначен!");
        if (questJournalButton == null) Debug.LogError("GameUIController: ValidateReferences() - QuestJournalButton не назначен!");
        if (characterNameText == null) Debug.LogError("GameUIController: ValidateReferences() - CharacterNameText не назначен!");
        if (levelText == null) Debug.LogError("GameUIController: ValidateReferences() - LevelText не назначен!");
        if (expText == null) Debug.LogError("GameUIController: ValidateReferences() - ExpText не назначен!");
        if (strengthText == null) Debug.LogError("GameUIController: ValidateReferences() - StrengthText не назначен!");
        if (manaText == null) Debug.LogError("GameUIController: ValidateReferences() - ManaText не назначен!");
        if (armorText == null) Debug.LogError("GameUIController: ValidateReferences() - ArmorText не назначен!");
        if (agilityText == null) Debug.LogError("GameUIController: ValidateReferences() - AgilityText не назначен!");
        if (moveDistanceText == null) Debug.LogError("GameUIController: ValidateReferences() - MoveDistanceText не назначен!");
        if (crystalsText == null) Debug.LogError("GameUIController: ValidateReferences() - CrystalsText не назначен!");
        if (backpackItemTemplate == null) Debug.LogError("GameUIController: ValidateReferences() - BackpackItemTemplate не назначен!");
        if (backpackItemsGrid == null) Debug.LogError("GameUIController: ValidateReferences() - BackpackItemsGrid не назначен!");
        if (inventoryItemTemplate == null) Debug.LogError("GameUIController: ValidateReferences() - InventoryItemTemplate не назначен!");
        if (equipmentSlots == null) Debug.LogError("GameUIController: ValidateReferences() - EquipmentSlots не назначен!");
        if (armorSlot == null) Debug.LogError("GameUIController: ValidateReferences() - ArmorSlot не назначен!");
        if (helmetSlot == null) Debug.LogError("GameUIController: ValidateReferences() - HelmetSlot не назначен!");
        if (bootsSlot == null) Debug.LogError("GameUIController: ValidateReferences() - BootsSlot не назначен!");
        if (inventoryItemsList == null) Debug.LogError("GameUIController: ValidateReferences() - InventoryItemsList не назначен!");
        if (healingPotionIcon == null) Debug.LogError("GameUIController: ValidateReferences() - HealingPotionIcon не назначен!");
        if (shieldPotionIcon == null) Debug.LogError("GameUIController: ValidateReferences() - ShieldPotionIcon не назначен!");
        if (manaCrystalIcon == null) Debug.LogError("GameUIController: ValidateReferences() - ManaCrystalIcon не назначен!");
        if (healthGemIcon == null) Debug.LogError("GameUIController: ValidateReferences() - HealthGemIcon не назначен!");
        if (ironArmorIcon == null) Debug.LogError("GameUIController: ValidateReferences() - IronArmorIcon не назначен!");
        if (steelHelmetIcon == null) Debug.LogError("GameUIController: ValidateReferences() - SteelHelmetIcon не назначен!");
        if (leatherBootsIcon == null) Debug.LogError("GameUIController: ValidateReferences() - LeatherBootsIcon не назначен!");
    }

    public void ToggleAttributesPanel()
    {
        isAttributesOpen = !isAttributesOpen;
        attributesPanel.SetActive(isAttributesOpen);
        Debug.Log($"GameUIController: ToggleAttributesPanel() - Панель атрибутов: {(isAttributesOpen ? "открыта" : "закрыта")}");
        if (isAttributesOpen)
        {
            UpdateCurrentCharacterOnStart();
            if (currentCharacter != null)
            {
                attributesNeedUpdate = true;
                UpdateAttributesPanel();
            }
            else
            {
                Debug.LogWarning("GameUIController: ToggleAttributesPanel() - Не удалось обновить Attributes Panel: персонаж не найден!");
            }
            isBackpackOpen = false;
            backpackPanel.SetActive(false);
            isInventoryOpen = false;
            inventoryPanel.SetActive(false);
            isQuestJournalOpen = false;
            questJournalPanel.SetActive(false);
        }
    }

    public void ToggleBackpackPanel()
    {
        isBackpackOpen = !isBackpackOpen;
        backpackPanel.SetActive(isBackpackOpen);
        Debug.Log($"GameUIController: ToggleBackpackPanel() - Панель рюкзака: {(isBackpackOpen ? "открыта" : "закрыта")}");
        if (isBackpackOpen)
        {
            PositionBackpackPanel();
            UpdateBackpackItems();
            isAttributesOpen = false;
            attributesPanel.SetActive(false);
            isInventoryOpen = false;
            inventoryPanel.SetActive(false);
            isQuestJournalOpen = false;
            questJournalPanel.SetActive(false);
        }
    }

    public void ToggleInventoryPanel()
    {
        isInventoryOpen = !isInventoryOpen;
        inventoryPanel.SetActive(isInventoryOpen);
        Debug.Log($"GameUIController: ToggleInventoryPanel() - Панель инвентаря: {(isInventoryOpen ? "открыта" : "закрыта")}");
        if (isInventoryOpen)
        {
            UpdateInventoryItems();
            UpdateEquipmentSlots();
            isAttributesOpen = false;
            attributesPanel.SetActive(false);
            isBackpackOpen = false;
            backpackPanel.SetActive(false);
            isQuestJournalOpen = false;
            questJournalPanel.SetActive(false);
        }
    }

    public void ToggleQuestJournalPanel()
    {
        isQuestJournalOpen = !isQuestJournalOpen;
        questJournalPanel.SetActive(isQuestJournalOpen);
        Debug.Log($"GameUIController: ToggleQuestJournalPanel() - Панель журнала квестов: {(isQuestJournalOpen ? "открыта" : "закрыта")}");
        if (isQuestJournalOpen)
        {
            if (QuestJournalUI.Instance != null)
            {
                QuestJournalUI.Instance.ToggleJournal();
            }
            else
            {
                Debug.LogWarning("GameUIController: ToggleQuestJournalPanel() - QuestJournalUI не найден!");
            }
            isAttributesOpen = false;
            attributesPanel.SetActive(false);
            isBackpackOpen = false;
            backpackPanel.SetActive(false);
            isInventoryOpen = false;
            inventoryPanel.SetActive(false);
        }
    }

    private void UpdateAttributesPanel()
    {
        if (!attributesNeedUpdate) return;
        attributesNeedUpdate = false;

        if (currentCharacter == null)
        {
            Debug.LogWarning("GameUIController: UpdateAttributesPanel() - Текущий персонаж не установлен! Пытаюсь найти персонажа...");
            UpdateCurrentCharacterOnStart();
            if (currentCharacter == null)
            {
                Debug.LogError("GameUIController: UpdateAttributesPanel() - Персонаж так и не найден! Атрибуты не могут быть обновлены.");
                return;
            }
        }

        GameManager.CharacterData stats = GameManager.Instance.GetCharacterStats(currentCharacter.gameObject.name);
        if (stats == null)
        {
            characterNameText.text = currentCharacter.gameObject.name;
            levelText.text = $"Level: {currentCharacter.GetLevel()}";
            expText.text = $"Exp: {Mathf.FloorToInt(currentCharacter.GetExperience())}/{Mathf.FloorToInt(currentCharacter.GetExpToNextLevel())}";
            strengthText.text = $"Strength: {Mathf.FloorToInt(currentCharacter.strength)}/{Mathf.FloorToInt(currentCharacter.maxStrength)}";
            manaText.text = $"Mana: {Mathf.FloorToInt(currentCharacter.mana)}/{Mathf.FloorToInt(currentCharacter.maxMana)}";
            armorText.text = $"Armor: {Mathf.FloorToInt(currentCharacter.armor)}/{Mathf.FloorToInt(currentCharacter.maxArmor)}";
            agilityText.text = $"Agility: {Mathf.FloorToInt(currentCharacter.GetAgility())}";
            moveDistanceText.text = $"Move Distance: {currentCharacter.GetMoveDistance()}";
        }
        else
        {
            characterNameText.text = currentCharacter.gameObject.name;
            levelText.text = $"Level: {stats.level}";
            expText.text = $"Exp: {Mathf.FloorToInt(stats.experience)}/{Mathf.FloorToInt(stats.expToNextLevel)}";
            strengthText.text = $"Strength: {Mathf.FloorToInt(stats.strength)}/{Mathf.FloorToInt(stats.maxStrength)}";
            manaText.text = $"Mana: {Mathf.FloorToInt(stats.mana)}/{Mathf.FloorToInt(stats.maxMana)}";
            armorText.text = $"Armor: {Mathf.FloorToInt(stats.armor)}/{Mathf.FloorToInt(stats.maxArmor)}";
            agilityText.text = $"Agility: {Mathf.FloorToInt(stats.agility)}";
            moveDistanceText.text = $"Move Distance: {currentCharacter.GetMoveDistance()}";
        }

        crystalsText.text = $"Crystals: {GameManager.Instance.GetPlayerCrystals()}";
    }

    private void PositionBackpackPanel()
    {
        RectTransform buttonRect = backpackButton.GetComponent<RectTransform>();
        RectTransform panelRect = backpackPanel.GetComponent<RectTransform>();
        RectTransform canvasRect = backpackPanel.GetComponentInParent<Canvas>().GetComponent<RectTransform>();

        Vector2 buttonPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            RectTransformUtility.WorldToScreenPoint(null, buttonRect.position),
            null,
            out buttonPosition
        );

        Vector2 offset = new Vector2(0, buttonRect.rect.height / 2 + panelRect.rect.height / 2 + 10);
        panelRect.localPosition = buttonPosition + offset;

        Vector2 panelSize = panelRect.rect.size;
        Vector2 canvasSize = canvasRect.rect.size;
        Vector2 clampedPosition = panelRect.localPosition;

        clampedPosition.x = Mathf.Clamp(clampedPosition.x, -canvasSize.x / 2 + panelSize.x / 2, canvasSize.x / 2 - panelSize.x / 2);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, -canvasSize.y / 2 + panelSize.y / 2, canvasSize.y / 2 - panelSize.y / 2);

        panelRect.localPosition = clampedPosition;
    }

    private void UpdateBackpackItems()
    {
        foreach (Transform child in backpackItemsGrid)
        {
            if (child.gameObject != backpackItemTemplate)
            {
                Destroy(child.gameObject);
            }
        }

        List<Item> backpackItems = GameUIManager.Instance.GetBackpack();
        foreach (var item in backpackItems)
        {
            CreateBackpackItem(item);
        }
    }

    private void CreateBackpackItem(Item item)
    {
        GameObject itemObj = Instantiate(backpackItemTemplate, backpackItemsGrid);
        itemObj.SetActive(true);

        Image iconImage = itemObj.transform.Find("ItemIcon")?.GetComponent<Image>();
        if (iconImage != null) iconImage.sprite = item.Icon;

        TMP_Text nameText = itemObj.transform.Find("ItemName")?.GetComponent<TMP_Text>();
        if (nameText != null) nameText.text = item.Name;

        Button itemButton = itemObj.GetComponent<Button>();
        itemButton.onClick.AddListener(() => OnBackpackItemClicked(item));
    }

    private void OnBackpackItemClicked(Item item)
    {
        CharacterController character = TurnManager.Instance?.GetCurrentCharacter();
        if (character == null)
        {
            Debug.LogWarning("GameUIController: OnBackpackItemClicked() - Текущий персонаж не найден! Пытаюсь найти персонажа...");
            UpdateCurrentCharacterOnStart();
            character = currentCharacter;
            if (character == null)
            {
                Debug.LogError("GameUIController: OnBackpackItemClicked() - Персонаж так и не найден! Нельзя использовать предмет.");
                return;
            }
        }

        if (item.Name == "Healing Potion")
        {
            character.strength = Mathf.Min(character.strength + 5f, character.maxStrength);
            GameUIManager.Instance.RemoveFromBackpack(item);
            GameManager.Instance.SaveCharacterStats(character);
        }
        else if (item.Name == "Shield Potion")
        {
            character.armor = Mathf.Min(character.armor + 3f, character.maxArmor);
            GameUIManager.Instance.RemoveFromBackpack(item);
            GameManager.Instance.SaveCharacterStats(character);
        }

        UpdateBackpackItems();
    }

    private void SetupEquipmentSlots()
    {
        EquipmentSlot armor = armorSlot.GetComponent<EquipmentSlot>() ?? armorSlot.AddComponent<EquipmentSlot>();
        EquipmentSlot helmet = helmetSlot.GetComponent<EquipmentSlot>() ?? helmetSlot.AddComponent<EquipmentSlot>();
        EquipmentSlot boots = bootsSlot.GetComponent<EquipmentSlot>() ?? bootsSlot.AddComponent<EquipmentSlot>();

        armor.Initialize(Item.ItemType.Armor, this, armorSlot.transform.Find("UnequipButton")?.GetComponent<Button>());
        helmet.Initialize(Item.ItemType.Helmet, this, helmetSlot.transform.Find("UnequipButton")?.GetComponent<Button>());
        boots.Initialize(Item.ItemType.Boots, this, bootsSlot.transform.Find("UnequipButton")?.GetComponent<Button>());

        equipmentSlotsDict[Item.ItemType.Armor] = armor;
        equipmentSlotsDict[Item.ItemType.Helmet] = helmet;
        equipmentSlotsDict[Item.ItemType.Boots] = boots;
    }

    private void InitializeItems()
    {
        if (GameUIManager.Instance.GetBackpack().Count == 0)
        {
            GameUIManager.Instance.AddToBackpack(new Item("Healing Potion", healingPotionIcon, Item.ItemType.Consumable));
            GameUIManager.Instance.AddToBackpack(new Item("Shield Potion", shieldPotionIcon, Item.ItemType.Consumable));
        }

        if (GameUIManager.Instance.GetInventory().Count == 0)
        {
            GameUIManager.Instance.AddToInventory(new Item("Mana Crystal", manaCrystalIcon, Item.ItemType.Material));
            GameUIManager.Instance.AddToInventory(new Item("Health Gem", healthGemIcon, Item.ItemType.Material));
            GameUIManager.Instance.AddToInventory(new Item("Iron Armor", ironArmorIcon, Item.ItemType.Armor));
            GameUIManager.Instance.AddToInventory(new Item("Steel Helmet", steelHelmetIcon, Item.ItemType.Helmet));
            GameUIManager.Instance.AddToInventory(new Item("Leather Boots", leatherBootsIcon, Item.ItemType.Boots));
        }
    }

    private void UpdateInventoryItems()
    {
        Transform content = inventoryItemsList.transform.Find("Viewport/Content");
        foreach (Transform child in content)
        {
            if (child.gameObject != inventoryItemTemplate)
            {
                Destroy(child.gameObject);
            }
        }

        List<Item> items = GameUIManager.Instance.GetInventory();
        foreach (var item in items)
        {
            GameObject itemObj = Instantiate(inventoryItemTemplate, content);
            itemObj.SetActive(true);

            Image iconImage = itemObj.transform.Find("ItemIcon")?.GetComponent<Image>();
            if (iconImage != null) iconImage.sprite = item.Icon;

            TMP_Text nameText = itemObj.transform.Find("ItemName")?.GetComponent<TMP_Text>();
            if (nameText != null) nameText.text = item.Name;

            Button equipButton = itemObj.transform.Find("EquipButton")?.GetComponent<Button>();
            if (equipButton != null)
            {
                bool canEquip = item.Type == Item.ItemType.Armor || item.Type == Item.ItemType.Helmet || item.Type == Item.ItemType.Boots;
                equipButton.gameObject.SetActive(canEquip);
                if (canEquip)
                {
                    equipButton.onClick.RemoveAllListeners();
                    equipButton.onClick.AddListener(() => EquipItem(item));
                }
            }
        }
    }

    private void UpdateEquipmentSlots()
    {
        foreach (var slot in equipmentSlotsDict)
        {
            Item equippedItem = GameUIManager.Instance.GetEquippedItem(slot.Key);
            slot.Value.SetItem(equippedItem);
        }
    }

    private void EquipItem(Item item)
    {
        GameUIManager.Instance.EquipItem(item);
        UpdateInventoryItems();
        UpdateEquipmentSlots();
    }

    public void UnequipItem(EquipmentSlot slot)
    {
        GameUIManager.Instance.UnequipItem(slot.SlotType);
        UpdateInventoryItems();
        UpdateEquipmentSlots();
    }
}