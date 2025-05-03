using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

public class SkillsPanelController : MonoBehaviour
{
    public GameObject skillButtonTemplate;
    public TMP_Text skillPrompt;
    private CharacterController currentCharacter;
    private GameObject[] skillButtons;

    // Для tooltip
    public GameObject tooltipPanel;
    public TextMeshProUGUI tooltipText;
    public enum TooltipPosition { Right, Left, Top, Bottom }
    [SerializeField] private TooltipPosition tooltipPosition = TooltipPosition.Right;
    [SerializeField] private Vector2 tooltipOffset = new Vector2(150, -50);
    [SerializeField] private float animationDuration = 0.3f;
    private Vector2 startPosition;
    private Vector2 targetPosition;
    private float animationTimer;
    private bool isAnimating;

    // Иконки для скиллов
    [SerializeField] private Sprite attackIcon;
    [SerializeField] private Sprite telekinesisIcon;
    [SerializeField] private Sprite tauntIcon;
    [SerializeField] private Sprite shieldSlamIcon;
    [SerializeField] private Sprite fortifyIcon;
    [SerializeField] private Sprite bodyguardIcon;
    [SerializeField] private Sprite tripleShotIcon;
    [SerializeField] private Sprite concentrationIcon;
    [SerializeField] private Sprite freezeIcon;
    [SerializeField] private Sprite longShotIcon;
    [SerializeField] private Sprite astralPocketIcon;

    private readonly Dictionary<CharacterController.SkillType, Sprite> skillIcons = new Dictionary<CharacterController.SkillType, Sprite>();

    void Start()
    {
        if (skillButtonTemplate == null)
        {
            Debug.LogError("skillButtonTemplate не назначен в SkillsPanelController!");
        }
        else
        {
            skillButtonTemplate.SetActive(false);
        }

        if (skillPrompt != null)
        {
            skillPrompt.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("SkillPrompt не назначен в SkillsPanelController!");
        }

        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("TooltipPanel не назначен в SkillsPanelController!");
        }

        if (tooltipText == null)
        {
            Debug.LogWarning("TooltipText не назначен в SkillsPanelController!");
        }

        // Инициализация словаря иконок
        InitializeSkillIcons();

        TurnManager.Instance.OnTurnChanged += UpdateSkillsPanel;
    }

    void OnDestroy()
    {
        TurnManager.Instance.OnTurnChanged -= UpdateSkillsPanel;
    }

    void Update()
    {
        if (isAnimating)
        {
            animationTimer += Time.deltaTime;
            float t = Mathf.Clamp01(animationTimer / animationDuration);
            tooltipPanel.GetComponent<RectTransform>().localPosition = Vector2.Lerp(startPosition, targetPosition, t);

            if (t >= 1f)
            {
                isAnimating = false;
            }
        }
    }

    private void InitializeSkillIcons()
    {
        skillIcons[CharacterController.SkillType.Attack] = attackIcon;
        skillIcons[CharacterController.SkillType.Telekinesis] = telekinesisIcon;
        skillIcons[CharacterController.SkillType.Taunt] = tauntIcon;
        skillIcons[CharacterController.SkillType.ShieldSlam] = shieldSlamIcon;
        skillIcons[CharacterController.SkillType.Fortify] = fortifyIcon;
        skillIcons[CharacterController.SkillType.Bodyguard] = bodyguardIcon;
        skillIcons[CharacterController.SkillType.TripleShot] = tripleShotIcon;
        skillIcons[CharacterController.SkillType.Concentration] = concentrationIcon;
        skillIcons[CharacterController.SkillType.Freeze] = freezeIcon;
        skillIcons[CharacterController.SkillType.LongShot] = longShotIcon;
        skillIcons[CharacterController.SkillType.AstralPocket] = astralPocketIcon;
    }

    public void UpdateSkillsPanel(CharacterController character)
    {
        currentCharacter = character;
        if (skillPrompt != null)
        {
            skillPrompt.gameObject.SetActive(false);
        }

        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
            isAnimating = false;
        }

        if (skillButtons != null)
        {
            foreach (GameObject button in skillButtons)
            {
                if (button != null)
                {
                    Destroy(button);
                }
            }
        }

        if (character.role == CharacterController.CharacterRole.TankMage)
        {
            skillButtons = new GameObject[6];
            skillButtons[0] = CreateSkillButton("Attack", () => ShowPrompt(character.SelectAttack, "Select an enemy to attack..."), CharacterController.SkillType.Attack);
            skillButtons[1] = CreateSkillButton("Telekinesis", () => ShowPrompt(character.SelectTelekinesis, "Select a movable object..."), CharacterController.SkillType.Telekinesis);
            skillButtons[2] = CreateSkillButton("Taunt", () => ShowPrompt(character.SelectTaunt, "Taunt applied!"), CharacterController.SkillType.Taunt);
            skillButtons[3] = CreateSkillButton("Shield Slam", () => ShowPrompt(character.SelectShieldSlam, "Select an enemy to attack..."), CharacterController.SkillType.ShieldSlam);
            skillButtons[4] = CreateSkillButton("Fortify", () => ShowPrompt(character.SelectFortify, "Fortify applied!"), CharacterController.SkillType.Fortify);
            skillButtons[5] = CreateSkillButton("Bodyguard", () => ShowPrompt(character.SelectBodyguard, "Select an ally to protect..."), CharacterController.SkillType.Bodyguard);
        }
        else if (character.role == CharacterController.CharacterRole.RangedDD)
        {
            skillButtons = new GameObject[6];
            skillButtons[0] = CreateSkillButton("Attack", () => ShowPrompt(character.SelectAttack, "Select an enemy to attack..."), CharacterController.SkillType.Attack);
            skillButtons[1] = CreateSkillButton("Triple Shot", () => ShowPrompt(character.SelectTripleShot, "Select an enemy to attack..."), CharacterController.SkillType.TripleShot);
            skillButtons[2] = CreateSkillButton("Concentration", () => ShowPrompt(character.SelectConcentration, "Concentration applied!"), CharacterController.SkillType.Concentration);
            skillButtons[3] = CreateSkillButton("Freeze", () => ShowPrompt(character.SelectFreeze, "Select an enemy to freeze..."), CharacterController.SkillType.Freeze);
            skillButtons[4] = CreateSkillButton("Long Shot", () => ShowPrompt(character.SelectLongShot, "Select an enemy to attack..."), CharacterController.SkillType.LongShot);
            skillButtons[5] = CreateSkillButton("Astral Pocket", () => ShowPrompt(character.SelectAstralPocket, "Select a point to place the bomb..."), CharacterController.SkillType.AstralPocket);
        }

        gameObject.SetActive(true);
    }

    private void ShowPrompt(UnityEngine.Events.UnityAction action, string promptText)
    {
        if (skillPrompt != null)
        {
            skillPrompt.text = promptText;
            skillPrompt.gameObject.SetActive(true);
            Debug.Log($"ShowPrompt: Показываю подсказку: {promptText}");
        }
        else
        {
            Debug.LogWarning("ShowPrompt: skillPrompt не назначен!");
        }

        if (action != null)
        {
            Debug.Log("ShowPrompt: Вызываю действие...");
            action.Invoke();
        }
        else
        {
            Debug.LogWarning("ShowPrompt: Действие (action) равно null!");
        }
    }

    private GameObject CreateSkillButton(string skillName, UnityEngine.Events.UnityAction action, CharacterController.SkillType skillType)
    {
        GameObject button = Instantiate(skillButtonTemplate, transform);
        button.SetActive(true);

        TMP_Text textComponent = button.GetComponentInChildren<TMP_Text>();
        if (textComponent != null)
        {
            textComponent.text = skillName;
        }

        Image iconImage = button.transform.Find("SkillIcon")?.GetComponent<Image>();
        if (iconImage != null && skillIcons.ContainsKey(skillType))
        {
            iconImage.sprite = skillIcons[skillType];
            iconImage.gameObject.SetActive(true);
        }
        else if (iconImage == null)
        {
            Debug.LogWarning($"Не удалось найти дочерний объект 'SkillIcon' в кнопке для скилла {skillName}. Убедитесь, что он добавлен в skillButtonTemplate.");
        }
        else if (!skillIcons.ContainsKey(skillType))
        {
            Debug.LogWarning($"Иконка для скилла {skillType} не назначена в SkillsPanelController.");
        }

        button.GetComponent<Button>().onClick.AddListener(action);
        AddTooltipEvents(button, skillType);

        return button;
    }

    private void AddTooltipEvents(GameObject buttonObj, CharacterController.SkillType skill)
    {
        if (buttonObj == null) return;

        EventTrigger trigger = buttonObj.GetComponent<EventTrigger>() ?? buttonObj.AddComponent<EventTrigger>();

        EventTrigger.Entry pointerEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        pointerEnter.callback.AddListener((data) => { ShowTooltip(skill); });
        trigger.triggers.Add(pointerEnter);

        EventTrigger.Entry pointerExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        pointerExit.callback.AddListener((data) => { HideTooltip(); });
        trigger.triggers.Add(pointerExit);
    }

    private void ShowTooltip(CharacterController.SkillType skill)
    {
        if (currentCharacter == null || tooltipPanel == null || tooltipText == null)
        {
            Debug.LogWarning("ShowTooltip: Один из объектов (currentCharacter, tooltipPanel, tooltipText) равен null!");
            return;
        }

        string description = currentCharacter.GetSkillDescription(skill);
        Debug.Log($"ShowTooltip: Показываю тултип для скилла {skill}. Описание: {description}");
        tooltipText.text = description;
        tooltipPanel.SetActive(true);

        Vector2 mousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            tooltipPanel.GetComponent<RectTransform>().parent as RectTransform,
            Input.mousePosition,
            null,
            out mousePosition
        );

        Vector2 finalOffset = tooltipOffset;
        switch (tooltipPosition)
        {
            case TooltipPosition.Right:
                finalOffset = new Vector2(Mathf.Abs(tooltipOffset.x), tooltipOffset.y);
                break;
            case TooltipPosition.Left:
                finalOffset = new Vector2(-Mathf.Abs(tooltipOffset.x), tooltipOffset.y);
                break;
            case TooltipPosition.Top:
                finalOffset = new Vector2(0, Mathf.Abs(tooltipOffset.y));
                break;
            case TooltipPosition.Bottom:
                finalOffset = new Vector2(0, -Mathf.Abs(tooltipOffset.y));
                break;
        }
        targetPosition = mousePosition + finalOffset;

        switch (tooltipPosition)
        {
            case TooltipPosition.Right:
                startPosition = mousePosition + new Vector2(finalOffset.x - 50, finalOffset.y);
                break;
            case TooltipPosition.Left:
                startPosition = mousePosition + new Vector2(finalOffset.x + 50, finalOffset.y);
                break;
            case TooltipPosition.Top:
                startPosition = mousePosition + new Vector2(finalOffset.x, finalOffset.y - 50);
                break;
            case TooltipPosition.Bottom:
                startPosition = mousePosition + new Vector2(finalOffset.x, finalOffset.y + 50);
                break;
        }

        tooltipPanel.GetComponent<RectTransform>().localPosition = startPosition;
        animationTimer = 0f;
        isAnimating = true;
    }

    private void HideTooltip()
    {
        if (tooltipPanel != null)
        {
            Debug.Log("HideTooltip: Скрываю тултип");
            tooltipPanel.SetActive(false);
            isAnimating = false;
        }
    }
}