using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New Quest", menuName = "Quest")]
public class Quest : ScriptableObject
{
    public string questId; // Уникальный идентификатор квеста
    public string title; // Название квеста
    [TextArea] public string description; // Описание квеста
    public QuestObjective[] objectives; // Список целей квеста
    public string rewardDescription; // Описание награды
    public int rewardCrystals; // Награда в кристаллах
    public int rewardExperience; // Награда в опыте
    public Item[] rewardItems; // Наградные предметы
    public int requiredLevel; // Минимальный уровень для принятия
    public string prerequisiteQuestId; // ID предыдущего квеста (если есть)
    public string nextQuestId; // ID следующего квеста в цепочке (если есть)
    public bool isCompleted; // Статус завершения квеста

    public void Initialize(string id, string questTitle, string questDescription, QuestObjective[] questObjectives)
    {
        questId = id;
        title = questTitle;
        description = questDescription;
        objectives = questObjectives;
        isCompleted = false;
    }

    [System.Serializable]
    public class QuestObjective
    {
        public enum ObjectiveType { KillEnemy, CollectItem, TalkToNPC, CollectCrystals } // Типы целей
        public ObjectiveType type; // Тип цели
        public string objectiveId; // Уникальный ID цели
        public string targetId; // ID цели (например, тип врага или предмета)
        public int requiredAmount; // Необходимое количество
        public int currentAmount; // Текущий прогресс
        public string description; // Описание цели

        public QuestObjective(string id, string desc, int required, ObjectiveType objectiveType, string target = "")
        {
            objectiveId = id;
            description = desc;
            currentAmount = 0;
            requiredAmount = required;
            type = objectiveType;
            targetId = target;
        }

        public bool IsCompleted => currentAmount >= requiredAmount;
    }
}