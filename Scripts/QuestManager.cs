using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    public delegate void QuestUpdatedHandler(string questId);
    public delegate void QuestCompletedHandler(string questId);
    public event QuestUpdatedHandler OnQuestUpdated;
    public event QuestCompletedHandler OnQuestCompleted;

    private List<QuestStatus> activeQuests = new List<QuestStatus>();
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
        LoadInitialQuests();
    }

    private void LoadInitialQuests()
{
    Quest introQuest = Resources.Load<Quest>("Quests/IntroQuest");
    if (introQuest != null)
    {
        GameManager.Instance.LoadQuestProgress(introQuest);
        AcceptQuest(introQuest);
    }
    else
    {
        Debug.LogError("QuestManager: Не удалось загрузить IntroQuest!");
    }
}

    public bool CanAcceptQuest(Quest quest)
{
    if (activeQuests.Any(q => q.quest.questId == quest.questId) || completedQuests.Contains(quest.questId))
    {
        Debug.Log($"QuestManager: Квест {quest.title} уже принят или завершён.");
        return false;
    }

    CharacterController player = FindObjectOfType<CharacterController>();
    if (player != null && player.GetLevel() < quest.requiredLevel)
    {
        Debug.Log($"QuestManager: Уровень игрока ({player.GetLevel()}) ниже требуемого ({quest.requiredLevel}) для квеста {quest.title}.");
        return false;
    }

    if (!string.IsNullOrEmpty(quest.prerequisiteQuestId) && !completedQuests.Contains(quest.prerequisiteQuestId))
    {
        Debug.Log($"QuestManager: Требуемый квест {quest.prerequisiteQuestId} для {quest.title} не завершён.");
        return false;
    }

    return true;
}

    public void AcceptQuest(Quest quest)
    {
        if (CanAcceptQuest(quest))
        {
            activeQuests.Add(new QuestStatus(quest));
            OnQuestUpdated?.Invoke(quest.questId);
            Debug.Log($"QuestManager: Квест принят: {quest.title}");

            // Если есть следующий квест, попытаемся его загрузить (но не принимать сразу)
            if (!string.IsNullOrEmpty(quest.nextQuestId))
            {
                Quest nextQuest = Resources.Load<Quest>($"Quests/{quest.nextQuestId}");
                if (nextQuest != null)
                {
                    Debug.Log($"QuestManager: Следующий квест {nextQuest.title} готов к принятию после завершения {quest.title}.");
                }
            }
        }
    }

    public void UpdateQuestObjective(string questId, string objectiveId, int amount)
    {
        QuestStatus questStatus = activeQuests.Find(q => q.quest.questId == questId);
        if (questStatus != null)
        {
            Quest.QuestObjective objective = questStatus.quest.objectives.FirstOrDefault(o => o.objectiveId == objectiveId);
            if (objective != null)
            {
                objective.currentAmount = Mathf.Min(objective.currentAmount + amount, objective.requiredAmount);
                OnQuestUpdated?.Invoke(questId);
                Debug.Log($"QuestManager: Обновлена цель квеста {questId}, цель {objectiveId}: {objective.currentAmount}/{objective.requiredAmount}");

                if (questStatus.quest.objectives.All(o => o.IsCompleted) && !questStatus.quest.isCompleted)
                {
                    CompleteQuest(questId);
                }
            }
        }
    }

    private void CompleteQuest(string questId)
    {
        QuestStatus questStatus = activeQuests.Find(q => q.quest.questId == questId);
        if (questStatus != null)
        {
            questStatus.quest.isCompleted = true;
            completedQuests.Add(questId);

            // Выдача наград
            GameManager.Instance.AddCrystals(questStatus.quest.rewardCrystals);
            CharacterController player = FindObjectOfType<CharacterController>();
            if (player != null)
            {
                player.AddExperience(questStatus.quest.rewardExperience);
                GameManager.Instance.SaveCharacterStats(player);
            }
            foreach (Item item in questStatus.quest.rewardItems)
            {
                if (item != null)
                {
                    GameUIManager.Instance.AddToInventory(item);
                }
            }

            activeQuests.Remove(questStatus);
            OnQuestCompleted?.Invoke(questId);
            Debug.Log($"QuestManager: Квест завершён: {questId}. Награда: {questStatus.quest.rewardCrystals} кристаллов, {questStatus.quest.rewardExperience} опыта.");

            // Попытка принять следующий квест
            if (!string.IsNullOrEmpty(questStatus.quest.nextQuestId))
            {
                Quest nextQuest = Resources.Load<Quest>($"Quests/{questStatus.quest.nextQuestId}");
                if (nextQuest != null)
                {
                    AcceptQuest(nextQuest);
                }
            }
        }
    }

    public List<QuestStatus> GetActiveQuests()
    {
        return activeQuests;
    }
}

public class QuestStatus
{
    public Quest quest;
    public QuestStatus(Quest q) => quest = q;
}