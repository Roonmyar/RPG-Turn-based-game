using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueController : MonoBehaviour
{
    public static DialogueController Instance { get; private set; }

    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public TMP_Text speakerText;
    public Button acceptButton;
    public Button declineButton;
    public Button nextButton;

    private Dialogue currentDialogue;
    private int currentLineIndex = 0;
    private bool isQuestDialogue = false;
    private bool hasReachedEnd = false;

    void Awake()
    {
        Debug.Log("DialogueController: Awake() - Начало инициализации.");
        if (Instance != null && Instance != this)
        {
            Debug.Log("DialogueController: Awake() - Дубликат DialogueController уничтожен.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("DialogueController: Awake() - DialogueController успешно инициализирован как синглтон.");
    }

    void Start()
    {
        Debug.Log("DialogueController: Start() - Инициализация контроллера диалогов.");
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
            Debug.Log("DialogueController: Start() - Панель диалога скрыта.");
        }
        else
        {
            Debug.LogError("DialogueController: Start() - dialoguePanel не назначен!");
        }

        if (acceptButton != null)
        {
            acceptButton.onClick.AddListener(OnAcceptButtonClicked);
            Debug.Log("DialogueController: Start() - acceptButton настроен.");
        }
        else
        {
            Debug.LogWarning("DialogueController: Start() - acceptButton не назначен.");
        }

        if (declineButton != null)
        {
            declineButton.onClick.AddListener(OnDeclineButtonClicked);
            Debug.Log("DialogueController: Start() - declineButton настроен.");
        }
        else
        {
            Debug.LogWarning("DialogueController: Start() - declineButton не назначен.");
        }

        if (nextButton != null)
        {
            nextButton.onClick.AddListener(OnNextButtonClicked);
            Debug.Log("DialogueController: Start() - nextButton настроен.");
        }
        else
        {
            Debug.LogWarning("DialogueController: Start() - nextButton не назначен.");
        }
    }

    public void StartDialogue(Dialogue dialogue, bool isQuest = false)
    {
        Debug.Log($"DialogueController: StartDialogue() - Начало диалога, строк: {dialogue.lines.Length}, isQuest: {isQuest}");
        currentDialogue = dialogue;
        currentLineIndex = 0;
        isQuestDialogue = isQuest;
        hasReachedEnd = false;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
            Debug.Log("DialogueController: StartDialogue() - Панель диалога активирована.");
        }
        else
        {
            Debug.LogError("DialogueController: StartDialogue() - dialoguePanel не назначен!");
            return;
        }

        if (acceptButton != null)
        {
            acceptButton.gameObject.SetActive(false);
            Debug.Log("DialogueController: StartDialogue() - acceptButton скрыт на старте.");
        }
        if (declineButton != null)
        {
            declineButton.gameObject.SetActive(false);
            Debug.Log("DialogueController: StartDialogue() - declineButton скрыт на старте.");
        }

        UpdateDialogue();
    }

    private void UpdateDialogue()
    {
        Debug.Log($"DialogueController: UpdateDialogue() - Текущий индекс: {currentLineIndex}, всего строк: {currentDialogue.lines.Length}");

        if (currentDialogue == null || dialogueText == null)
        {
            Debug.LogError("DialogueController: UpdateDialogue() - currentDialogue или dialogueText не заданы!");
            EndDialogue();
            return;
        }

        // Всегда обновляем текст для текущей строки
        dialogueText.text = currentDialogue.lines[currentLineIndex].text;
        Debug.Log($"DialogueController: UpdateDialogue() - Установлен текст диалога: {dialogueText.text}");

        if (speakerText != null)
        {
            speakerText.text = currentDialogue.lines[currentLineIndex].speaker;
            Debug.Log($"DialogueController: UpdateDialogue() - Установлен текст говорящего: {speakerText.text}");
        }
        else
        {
            Debug.LogWarning("DialogueController: UpdateDialogue() - speakerText не назначен.");
        }

        // Проверяем, достигли ли последней строки диалога
        bool isLastLine = currentLineIndex >= currentDialogue.lines.Length - 1;

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(!isLastLine);
            Debug.Log($"DialogueController: UpdateDialogue() - nextButton видимость: {!isLastLine}");
        }

        // Если это последняя строка и это квестовый диалог
        if (isLastLine && isQuestDialogue)
        {
            if (acceptButton != null)
            {
                acceptButton.gameObject.SetActive(true);
                Debug.Log("DialogueController: UpdateDialogue() - acceptButton показан на последней строке.");
            }
            if (declineButton != null)
            {
                declineButton.gameObject.SetActive(true);
                Debug.Log("DialogueController: UpdateDialogue() - declineButton показан на последней строке.");
            }
            hasReachedEnd = true;
        }
        else if (isLastLine && !isQuestDialogue)
        {
            EndDialogue();
        }
    }

    private void OnNextButtonClicked()
    {
        Debug.Log("DialogueController: OnNextButtonClicked() - Переход к следующей строке диалога.");

        if (currentLineIndex < currentDialogue.lines.Length - 1)
        {
            currentLineIndex++;
            UpdateDialogue();
        }
    }

    private void OnAcceptButtonClicked()
    {
        Debug.Log("DialogueController: OnAcceptButtonClicked() - Квест принят.");
        if (isQuestDialogue)
        {
            QuestManager.Instance.UpdateQuestObjective("IntroQuest", "TalkToTavernNPC", 1);
            Debug.Log("DialogueController: Диалог: Квест принят!");
        }
        EndDialogue();
    }

    private void OnDeclineButtonClicked()
    {
        Debug.Log("DialogueController: OnDeclineButtonClicked() - Квест отклонён.");
        Debug.Log("DialogueController: Диалог: Квест отклонён!");
        EndDialogue();
    }

    private void EndDialogue()
    {
        Debug.Log("DialogueController: EndDialogue() - Завершение диалога.");
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
            Debug.Log("DialogueController: EndDialogue() - Панель диалога скрыта.");
        }
        currentDialogue = null;
        currentLineIndex = 0;
        isQuestDialogue = false;
        hasReachedEnd = false;

        if (acceptButton != null) acceptButton.gameObject.SetActive(false);
        if (declineButton != null) declineButton.gameObject.SetActive(false);
        if (nextButton != null) nextButton.gameObject.SetActive(false);
    }
}

[System.Serializable]
public class Dialogue
{
    public DialogueLine[] lines;
}