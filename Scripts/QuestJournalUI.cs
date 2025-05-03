using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class QuestJournalUI : MonoBehaviour
{
    public static QuestJournalUI Instance { get; private set; }

    [SerializeField] private GameObject questJournalPanel; // Панель журнала квестов
    [SerializeField] private GameObject questEntryTemplate; // Шаблон записи квеста
    [SerializeField] private Transform questListContainer; // Контейнер для списка квестов
    [SerializeField] private Toggle showCompletedToggle; // Переключатель для завершённых квестов
    [SerializeField] private Button closeButton; // Кнопка закрытия
    private bool isJournalOpen = false;
    private List<string> completedQuests = new List<string>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Проверка назначенных компонентов
        if (questJournalPanel == null) Debug.LogError("QuestJournalUI: questJournalPanel не назначен!");
        if (questEntryTemplate == null) Debug.LogError("QuestJournalUI: questEntryTemplate не назначен!");
        if (questListContainer == null) Debug.LogError("QuestJournalUI: questListContainer не назначен!");
        if (showCompletedToggle == null) Debug.LogError("QuestJournalUI: showCompletedToggle не назначен!");
        if (closeButton == null) Debug.LogError("QuestJournalUI: closeButton не назначен!");

        // Инициализация UI
        questJournalPanel.SetActive(false);
        questEntryTemplate.SetActive(false);

        // Подписка на события
        showCompletedToggle.onValueChanged.AddListener(OnShowCompletedToggled);
        closeButton.onClick.AddListener(CloseJournal);
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestUpdated += OnQuestUpdated;
            QuestManager.Instance.OnQuestCompleted += OnQuestCompleted;
        }
        else
        {
            Debug.LogWarning("QuestJournalUI: QuestManager не найден!");
        }
    }

    void OnDestroy()
    {
        // Отписка от событий
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestUpdated -= OnQuestUpdated;
            QuestManager.Instance.OnQuestCompleted -= OnQuestCompleted;
        }
        showCompletedToggle.onValueChanged.RemoveListener(OnShowCompletedToggled);
        closeButton.onClick.RemoveListener(CloseJournal);
    }

    void Update()
    {
        // Открытие/закрытие журнала по клавише J
        if (Input.GetKeyDown(KeyCode.J))
        {
            ToggleJournal();
        }
    }

    public void ToggleJournal()
{
    isJournalOpen = !isJournalOpen;
    questJournalPanel.SetActive(isJournalOpen);

    // ✅ Повторная подписка на события
    if (QuestManager.Instance != null)
    {
        QuestManager.Instance.OnQuestUpdated -= OnQuestUpdated;
        QuestManager.Instance.OnQuestCompleted -= OnQuestCompleted;

        QuestManager.Instance.OnQuestUpdated += OnQuestUpdated;
        QuestManager.Instance.OnQuestCompleted += OnQuestCompleted;
    }

    if (isJournalOpen)
    {
        UpdateQuestList();
    }

    Debug.Log($"QuestJournalUI: Журнал квестов {(isJournalOpen ? "открыт" : "закрыт")}");
}


    private void CloseJournal()
    {
        isJournalOpen = false;
        questJournalPanel.SetActive(false);
        Debug.Log("QuestJournalUI: Журнал закрыт через кнопку");
    }

    private void OnShowCompletedToggled(bool showCompleted)
    {
        UpdateQuestList();
        Debug.Log($"QuestJournalUI: Показ завершённых квестов: {showCompleted}");
    }

    private void OnQuestUpdated(string questId)
    {
        if (isJournalOpen)
        {
            UpdateQuestList();
        }
        Debug.Log($"QuestJournalUI: Обновлён квест {questId}");
    }

    private void OnQuestCompleted(string questId)
    {
        completedQuests.Add(questId);
        if (isJournalOpen)
        {
            UpdateQuestList();
        }
        Debug.Log($"QuestJournalUI: Квест {questId} завершён");
    }

    private void UpdateQuestList()
    {
        // Очистка списка
        foreach (Transform child in questListContainer)
        {
            if (child.gameObject != questEntryTemplate)
            {
                Destroy(child.gameObject);
            }
        }

        var activeQuests = QuestManager.Instance.GetActiveQuests();
        bool showCompleted = showCompletedToggle.isOn;

        if (activeQuests.Count == 0 && (!showCompleted || completedQuests.Count == 0))
        {
            CreateEmptyEntry();
        }
        else
        {
            // Добавление активных квестов
            foreach (var questStatus in activeQuests)
            {
                CreateQuestEntry(questStatus.quest, false);
            }

            // Добавление завершённых квестов, если включён toggle
            if (showCompleted)
            {
                foreach (var questId in completedQuests)
                {
                    var quest = activeQuests.Find(q => q.quest.questId == questId)?.quest
                        ?? Resources.Load<Quest>($"Quests/{questId}");
                    if (quest != null)
                    {
                        CreateQuestEntry(quest, true);
                    }
                }
            }
        }
    }

    private void CreateEmptyEntry()
    {
        GameObject entry = Instantiate(questEntryTemplate, questListContainer);
        entry.SetActive(true);
        TMP_Text questNameText = entry.transform.Find("QuestName")?.GetComponent<TMP_Text>();
        if (questNameText != null) questNameText.text = "Нет активных квестов";
        Debug.Log("QuestJournalUI: Создано пустое сообщение: Нет активных квестов");
    }

    private void CreateQuestEntry(Quest quest, bool isCompleted)
    {
        GameObject entry = Instantiate(questEntryTemplate, questListContainer);
        entry.SetActive(true);

        TMP_Text questNameText = entry.transform.Find("QuestName")?.GetComponent<TMP_Text>();
        TMP_Text questDescriptionText = entry.transform.Find("QuestDescription")?.GetComponent<TMP_Text>();
        TMP_Text questStatusText = entry.transform.Find("QuestStatus")?.GetComponent<TMP_Text>();
        TMP_Text objectivesText = entry.transform.Find("QuestObjectives")?.GetComponent<TMP_Text>();

        string status = isCompleted ? "Completed" : "In Progress";
        string objectives = quest.objectives.Length > 0
            ? string.Join("\n", quest.objectives.Select(obj => $"{obj.description}: {obj.currentAmount}/{obj.requiredAmount}"))
            : "Нет целей";

        if (questNameText != null) questNameText.text = quest.title;
        else Debug.LogWarning($"QuestJournalUI: QuestName не найден для квеста {quest.questId}");
        if (questDescriptionText != null) questDescriptionText.text = quest.description;
        else Debug.LogWarning($"QuestJournalUI: QuestDescription не найден для квеста {quest.questId}");
        if (questStatusText != null) questStatusText.text = $"Status: {status}";
        else Debug.LogWarning($"QuestJournalUI: QuestStatus не найден для квеста {quest.questId}");
        if (objectivesText != null) objectivesText.text = objectives;
        else Debug.LogWarning($"QuestJournalUI: QuestObjectives не найден для квеста {quest.questId}");

        Debug.Log($"QuestJournalUI: Создана запись квеста: {quest.title}, Статус: {status}");
    }
}