using UnityEngine;
using System.Linq;

public class NPCController : MonoBehaviour
{
    [SerializeField] private string questId = "IntroQuest"; // Исправлено с "TavernQuest" на "IntroQuest"
    [SerializeField] private string defaultDialogueMessage = "Привет, путник! Поговори со мной.";
    [SerializeField] private NPCHintUI hintUI;
    private bool hasInteracted = false;
    private bool isPlayerInTrigger = false;

    private DialogueList dialogueList;

    private void Awake()
    {
        if (GetComponent<Collider>() == null)
        {
            Debug.LogError($"NPCController: У {gameObject.name} отсутствует Collider!");
        }
        else if (!GetComponent<Collider>().isTrigger)
        {
            Debug.LogError($"NPCController: Collider на {gameObject.name} не установлен как Is Trigger!");
        }
        if (hintUI == null)
        {
            Debug.LogError($"NPCController: Hint UI не назначен на {gameObject.name}!");
        }

        TextAsset dialogueJson = Resources.Load<TextAsset>("Dialogues/Dialogues");
        if (dialogueJson != null)
        {
            Debug.Log($"NPCController: Файл Dialogues.json успешно загружен, содержимое: {dialogueJson.text}");
            dialogueList = JsonUtility.FromJson<DialogueList>(dialogueJson.text);
            if (dialogueList == null || dialogueList.dialogues == null)
            {
                Debug.LogError("NPCController: Ошибка десериализации Dialogues.json, dialogueList или dialogues равны null!");
                return;
            }
            Debug.Log($"NPCController: Загружено диалогов: {dialogueList.dialogues.Length}");
            foreach (var dialogue in dialogueList.dialogues)
            {
                Debug.Log($"NPCController: Найден диалог с questId: {dialogue.questId}, строк: {dialogue.lines.Length}");
            }
        }
        else
        {
            Debug.LogError("NPCController: Не удалось загрузить Dialogues.json из Resources! Убедитесь, что файл находится в Assets/Resources/Dialogues/");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"NPCController: OnTriggerEnter вызван на {gameObject.name}. Объект: {other.gameObject.name}, Тег: {other.tag}");
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = true;
            if (hintUI != null)
            {
                hintUI.ShowHint("Нажмите E для разговора");
            }
            Debug.Log($"NPCController: Игрок вошёл в 3D-триггер NPC {gameObject.name}");
        }
        else
        {
            Debug.LogWarning($"NPCController: Объект {other.gameObject.name} вошёл в триггер, но это не Player (тег: {other.tag})");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"NPCController: OnTriggerExit вызван на {gameObject.name}. Объект: {other.gameObject.name}, Тег: {other.tag}");
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = false;
            if (hintUI != null)
            {
                hintUI.HideHint();
            }
            hasInteracted = false;
            Debug.Log($"NPCController: Игрок покинул 3D-триггер NPC {gameObject.name}");
        }
    }

    private void Update()
    {
        if (isPlayerInTrigger && !hasInteracted && Input.GetKeyDown(KeyCode.E))
        {
            hasInteracted = true;
            if (DialogueController.Instance != null)
            {
                Debug.Log($"NPCController: Попытка найти диалог для questId: {questId}");
                DialogueData dialogueData = dialogueList?.dialogues.FirstOrDefault(d => d.questId == questId);
                Dialogue dialogue;

                if (dialogueData != null)
                {
                    Debug.Log($"NPCController: Диалог для questId {questId} найден, строк: {dialogueData.lines.Length}");
                    dialogue = new Dialogue
                    {
                        lines = dialogueData.lines
                    };
                }
                else
                {
                    Debug.LogWarning($"NPCController: Диалог для questId {questId} не найден, используется defaultDialogueMessage.");
                    dialogue = new Dialogue
                    {
                        lines = new DialogueLine[] { new DialogueLine { speaker = gameObject.name, text = defaultDialogueMessage } }
                    };
                }

                bool isQuestDialogue = !string.IsNullOrEmpty(questId);
                DialogueController.Instance.StartDialogue(dialogue, isQuestDialogue); // Убрали параметр dialogueData?.lines
                Debug.Log($"NPCController: Игрок начал диалог с NPC {gameObject.name} для квеста {questId}");
            }
            else
            {
                Debug.LogError("NPCController: DialogueController не найден в сцене!");
            }
        }
    }

    public void CompleteTalkObjective()
    {
        QuestManager.Instance.UpdateQuestObjective(questId, "TalkToTavernNPC", 1);
    }
}