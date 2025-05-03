using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class CharacterController : MonoBehaviour
{
    public enum CharacterRole { TankMage, RangedDD, Boss }
    public CharacterRole role = CharacterRole.TankMage;

    public float moveSpeed = 5f;
    public float baseMoveDistance = 5f;
    private float moveDistance;
    public float attackRange = 2f;
    public bool isEnemy = false;
    public bool isMainCharacter = false;
    public float strength = 10f;
    public float maxStrength = 10f;
    public float damage = 5f;
    public float maxMana = 10f;
    public float mana = 10f;
    public float armor = 5f;
    public float maxArmor = 5f;
    public float agility = 5f;
    private float remainingDistance;
    private bool _isMyTurn;
    public bool isMyTurn
    {
        get => _isMyTurn;
        set
        {
            if (_isMyTurn != value)
            {
                Debug.Log($"{gameObject.name}: Изменяю isMyTurn с {_isMyTurn} на {value}");
                _isMyTurn = value;
            }
        }
    }
    private bool turnJustStarted = false;
    private float turnStartDelay = 0.1f;
    private float turnStartTimer = 0f;
    private GameObject target;
    public bool isInCombat = false;
    private Animator animator;
    private AttackPanelController attackPanel;
    private SkillsPanelController skillsPanel;

    public RuntimeAnimatorController animatorController;

    private bool isHoldingTarget = false;
    public int level = 1;
    public float experience = 0f;
    public float expToNextLevel = 20f;
    private float baseExpRequirement = 20f;
    private const float expPerEnemyKill = 10f;

    public GameObject statsPrefab;
    public float statsHeight = 15f;
    private GameObject statsCanvasInstance;
    private TMP_Text strengthText;
    private TMP_Text armorText;
    private TMP_Text manaText;

    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;

    public bool isStunned = false;
    public bool isRooted = false;
    public float fortifyArmorBonus = 0f;
    private int fortifyTurnsLeft = 0;
    public float concentrationDamageMultiplier = 1f;
    private int concentrationTurnsLeft = 0;
    private float longShotMoveDebuff = 1f;
    private int longShotDebuffTurnsLeft = 0;

    public float telekinesisRange = 5f;
    public float telekinesisMoveDistance = 3f;
    public float tauntDuration = 1;
    public float shieldSlamDamage = 5f;
    public float fortifyArmorIncrease = 5f;
    public CharacterController protectedAlly;

    public float tripleShotBaseDamage = 5f;
    public float longShotDamageMultiplier = 2f;
    public GameObject astralPocketBombPrefab;

    public enum SkillType
    {
        None,
        Telekinesis, Taunt, ShieldSlam, Fortify, Bodyguard,
        TripleShot, Concentration, Freeze, LongShot, AstralPocket,
        Attack
    }
    private SkillType currentSkill = SkillType.None;
    private bool isSelectingTarget = false;
    private bool isAttacking = false;

    public Material redOutlineMaterial;
    public Material greenOutlineMaterial;
    private List<(GameObject obj, Material[] originalMaterials)> highlightedObjects = new List<(GameObject, Material[])>();

    private Dictionary<SkillType, string> skillDescriptions;
    private Dictionary<SkillType, float> skillManaCosts;
    private CharacterController lastAttacker;

    [SerializeField] private Vector3 initialPosition = new Vector3(0, 1, 0);

    void Start()
    {
        Debug.Log($"{gameObject.name}: Начало Start() в CharacterController");
        LoadFromGameManager();

        if (!isEnemy)
        {
            if (role == CharacterRole.TankMage)
            {
                if (strength == 10f) strength = 15f;
                if (maxStrength == 10f) maxStrength = 15f;
                if (mana == 10f) mana = 12f;
                if (maxMana == 10f) maxMana = 12f;
                if (armor == 5f) armor = 10f;
                if (maxArmor == 5f) maxArmor = 10f;
                attackRange = 4f;
                if (agility == 5f) agility = 4f;
                damage = strength * 0.8f;
            }
            else if (role == CharacterRole.RangedDD)
            {
                if (strength == 10f) strength = 8f;
                if (maxStrength == 10f) maxStrength = 8f;
                if (mana == 10f) mana = 10f;
                if (maxMana == 10f) maxMana = 10f;
                if (armor == 5f) armor = 3f;
                if (maxArmor == 5f) maxArmor = 3f;
                attackRange = 12f;
                if (agility == 5f) agility = 7f;
                damage = strength * 0.8f;
            }
        }
        else
        {
            if (role == CharacterRole.Boss)
            {
                if (strength == 10f) strength = 50f;
                if (maxStrength == 10f) maxStrength = 50f;
                if (mana == 10f) mana = 15f;
                if (maxMana == 10f) maxMana = 15f;
                if (armor == 5f) armor = 10f;
                if (maxArmor == 5f) maxArmor = 10f;
                attackRange = 4f;
                if (agility == 5f) agility = 3f;
                damage = 8f;
                transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            }
            else
            {
                if (strength == 10f) strength = 10f;
                if (maxStrength == 10f) maxStrength = 10f;
                if (mana == 10f) mana = 8f;
                if (maxMana == 10f) maxMana = 8f;
                if (armor == 5f) armor = 5f;
                if (maxArmor == 5f) maxArmor = 5f;
                attackRange = 2f;
                if (agility == 5f) agility = 5f;
                damage = strength * 0.8f;
            }
        }

        maxStrength = strength;
        maxMana = mana;
        maxArmor = armor;
        moveDistance = baseMoveDistance + (agility / 2f);
        remainingDistance = moveDistance;

        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning($"{gameObject.name}: Animator не найден на объекте, пытаюсь найти в дочерних объектах.");
            animator = GetComponentInChildren<Animator>();
        }

        if (animator == null)
        {
            Debug.LogError($"{gameObject.name}: Animator не найден ни на объекте, ни в дочерних объектах. Анимации не будут воспроизводиться!");
        }
        else
        {
            if (animatorController != null)
            {
                animator.runtimeAnimatorController = animatorController;
                Debug.Log($"{gameObject.name}: Назначен Animator Controller: {animatorController.name}");
            }
            else
            {
                Debug.LogWarning($"{gameObject.name}: Animator Controller не назначен в инспекторе. Использую текущий контроллер анимации.");
            }
        }

        attackPanel = FindObjectOfType<AttackPanelController>();
        if (attackPanel == null) Debug.LogWarning($"{gameObject.name}: AttackPanelController не найден в сцене!");
        skillsPanel = FindObjectOfType<SkillsPanelController>();
        if (skillsPanel == null) Debug.LogWarning($"{gameObject.name}: SkillsPanelController не найден в сцене!");

        if (statsPrefab != null)
        {
            SetupStatsUI();
        }
        else
        {
            Debug.LogError($"{gameObject.name}: StatsPrefab не назначен в инспекторе!");
        }

        if (TurnManager.Instance == null)
        {
            Debug.LogError($"{gameObject.name}: TurnManager.Instance равен null!");
        }
        else
        {
            TurnManager.Instance.RegisterCharacter(this);
            Debug.Log($"{gameObject.name}: Зарегистрирован в TurnManager. isEnemy = {isEnemy}, Role = {role}, Tag = {gameObject.tag}, Agility = {agility}, MoveDistance = {moveDistance}");
        }

        InitializeSkillDescriptions();
        InitializeSkillManaCosts();

        CheckCharacterActivation();
    }

    void Update()
    {
        if (turnJustStarted && turnStartTimer > 0)
        {
            turnStartTimer -= Time.deltaTime;
            if (turnStartTimer <= 0)
            {
                turnJustStarted = false;
                Debug.Log($"{gameObject.name}: Задержка начала хода завершена, можно обрабатывать пробел.");
            }
        }

        bool isBattleScene = SceneManager.GetActiveScene().name.EndsWith("_Battle");

        if (!isEnemy)
        {
            if (!isInCombat && !isBattleScene)
            {
                if (isMainCharacter)
                {
                    HandleFreeMovement();
                }
            }
            else if (isMyTurn)
            {
                HandlePlayerCombatTurn();
            }
        }
        else if (isMyTurn && isBattleScene)
        {
            HandleEnemyCombatTurn();
        }

        if (statsCanvasInstance != null)
        {
            UpdateStatsUI();
        }

        if (isMainCharacter && !isBattleScene)
        {
            GameManager.Instance.SaveCharacterPosition(gameObject.name, transform.position);
        }
    }

    void LoadFromGameManager()
    {
        bool isBattleScene = SceneManager.GetActiveScene().name.EndsWith("_Battle");

        if (GameManager.Instance != null)
        {
            // Используем текущую позицию объекта как начальную
            Vector3 defaultPosition = transform.position;
            Vector3 savedPosition = GameManager.Instance.GetCharacterPosition(gameObject.name, defaultPosition);
            transform.position = savedPosition;
            Debug.Log($"{gameObject.name}: Установлена позиция: {transform.position} (default: {defaultPosition})");

            GameManager.CharacterData stats = GameManager.Instance.GetCharacterStats(gameObject.name);
            if (stats != null)
            {
                strength = stats.strength;
                maxStrength = stats.maxStrength;
                damage = stats.strength * 0.8f;
                mana = stats.mana;
                maxMana = stats.maxMana;
                armor = stats.armor;
                maxArmor = stats.maxArmor;
                agility = stats.agility;
                level = stats.level;
                experience = stats.experience;
                expToNextLevel = stats.expToNextLevel;
            }
        }
        else
        {
            if (isMainCharacter && !isBattleScene)
            {
                transform.position = initialPosition;
                Debug.Log($"{gameObject.name}: GameManager не найден, установлена начальная позиция: {initialPosition}");
            }
        }
    }

    void CheckCharacterActivation()
    {
        bool isBattleScene = SceneManager.GetActiveScene().name.EndsWith("_Battle");

        if (!isBattleScene)
        {
            if (!isMainCharacter)
            {
                gameObject.SetActive(false);
                Debug.Log($"{gameObject.name} отключён, так как это не боевая сцена и он не главный герой.");
            }
            else
            {
                Debug.Log($"{gameObject.name} активен, так как это главный герой.");
            }
        }
        else
        {
            gameObject.SetActive(true);
            Debug.Log($"{gameObject.name} активен, так как это боевая сцена.");
        }
    }

    void HandleFreeMovement()
    {
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        if (input.magnitude > 0)
        {
            Vector3 moveDirection = Camera.main.transform.TransformDirection(input);
            moveDirection.y = 0;
            moveDirection = moveDirection.normalized;
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(moveDirection);
            if (animator != null) animator.SetFloat("Speed", input.magnitude);
        }
        else
        {
            if (animator != null) animator.SetFloat("Speed", 0f);
        }
    }

    void HandlePlayerCombatTurn()
    {
        if (isStunned)
        {
            Debug.Log($"{gameObject.name} оглушён и пропускает ход!");
            TurnManager.Instance.EndCurrentTurn();
            return;
        }

        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        Vector3 movement = Vector3.zero;

        if (input.magnitude > 0 && !isRooted)
        {
            Vector3 moveDirection = Camera.main.transform.TransformDirection(input);
            moveDirection.y = 0;
            moveDirection = moveDirection.normalized;
            movement = moveDirection * moveSpeed * Time.deltaTime;
            float distanceToMove = movement.magnitude * longShotMoveDebuff;

            if (remainingDistance >= distanceToMove)
            {
                transform.position += movement;
                remainingDistance -= distanceToMove;
                transform.rotation = Quaternion.LookRotation(moveDirection);
                if (animator != null) animator.SetFloat("Speed", input.magnitude);
            }
            else
            {
                if (animator != null) animator.SetFloat("Speed", 0f);
            }
        }
        else
        {
            if (animator != null) animator.SetFloat("Speed", 0f);
        }

        if (currentSkill == SkillType.None || !isSelectingTarget) 
        {
            if (!turnJustStarted && Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log($"{gameObject.name} завершил ход вручную (Space)");
                isSelectingTarget = false;
                isHoldingTarget = false;
                currentSkill = SkillType.None;
                ClearHighlights();
                if (skillsPanel != null && skillsPanel.skillPrompt != null)
                {
                    skillsPanel.skillPrompt.gameObject.SetActive(false);
                }
                TurnManager.Instance.EndCurrentTurn();
            }
            return;
        }

        if (currentSkill == SkillType.Attack)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.CompareTag("Enemy"))
                    {
                        target = hit.collider.gameObject;
                        float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
                        if (distanceToTarget <= attackRange)
                        {
                            PerformAttack();
                            isSelectingTarget = false;
                            currentSkill = SkillType.None;
                            ClearHighlights();
                            TurnManager.Instance.EndCurrentTurn();
                        }
                        else
                        {
                            Debug.Log($"{gameObject.name}: Цель слишком далеко для атаки (расстояние: {distanceToTarget}, максимум: {attackRange})");
                        }
                    }
                }
            }
        }
        else if (currentSkill == SkillType.Telekinesis)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.CompareTag("Movable"))
                    {
                        target = hit.collider.gameObject;
                        isHoldingTarget = true;
                        Debug.Log($"{gameObject.name}: Выбрана цель для телекинеза: {target.name}");
                    }
                }
            }
            else if (Input.GetMouseButton(0) && isHoldingTarget && target != null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
                    if (distanceToTarget <= telekinesisRange)
                    {
                        Vector3 hitPoint = hit.point;
                        Vector3 direction = (hitPoint - target.transform.position).normalized;
                        Vector3 newPosition = target.transform.position + direction * telekinesisMoveDistance;
                        newPosition.y = target.transform.position.y;
                        target.transform.position = newPosition;
                        Debug.Log($"Обновляю направление для телекинеза: hitPoint = {hitPoint}, newPosition = {newPosition}");
                    }
                }
            }
            else if (Input.GetMouseButtonUp(0) && isHoldingTarget)
            {
                Debug.Log($"{gameObject.name}: Завершаю телекинез");
                ConsumeMana(SkillType.Telekinesis);
                if (animator != null) animator.SetTrigger("MagicAttack");
                isSelectingTarget = false;
                isHoldingTarget = false;
                currentSkill = SkillType.None;
                ClearHighlights();
                if (skillsPanel != null && skillsPanel.skillPrompt != null)
                {
                    skillsPanel.skillPrompt.gameObject.SetActive(false);
                }
                TurnManager.Instance.EndCurrentTurn();
            }
        }
        else if (currentSkill == SkillType.ShieldSlam)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.CompareTag("Enemy"))
                    {
                        target = hit.collider.gameObject;
                        float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
                        if (distanceToTarget <= attackRange)
                        {
                            ShieldSlam();
                            isSelectingTarget = false;
                            currentSkill = SkillType.None;
                            ClearHighlights();
                            TurnManager.Instance.EndCurrentTurn();
                        }
                    }
                }
            }
        }
        else if (currentSkill == SkillType.TripleShot)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.CompareTag("Enemy"))
                    {
                        target = hit.collider.gameObject;
                        float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
                        if (distanceToTarget <= attackRange)
                        {
                            TripleShot();
                            isSelectingTarget = false;
                            currentSkill = SkillType.None;
                            ClearHighlights();
                            TurnManager.Instance.EndCurrentTurn();
                        }
                    }
                }
            }
        }
        else if (currentSkill == SkillType.Freeze)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.CompareTag("Enemy"))
                    {
                        target = hit.collider.gameObject;
                        float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
                        if (distanceToTarget <= attackRange)
                        {
                            Freeze();
                            isSelectingTarget = false;
                            currentSkill = SkillType.None;
                            ClearHighlights();
                            TurnManager.Instance.EndCurrentTurn();
                        }
                    }
                }
            }
        }
        else if (currentSkill == SkillType.LongShot)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.CompareTag("Enemy"))
                    {
                        target = hit.collider.gameObject;
                        float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
                        if (distanceToTarget <= attackRange)
                        {
                            LongShot();
                            isSelectingTarget = false;
                            currentSkill = SkillType.None;
                            ClearHighlights();
                            TurnManager.Instance.EndCurrentTurn();
                        }
                    }
                }
            }
        }
        else if (currentSkill == SkillType.Bodyguard)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.CompareTag("Player") && hit.collider.gameObject != gameObject)
                    {
                        target = hit.collider.gameObject;
                        Bodyguard();
                        isSelectingTarget = false;
                        currentSkill = SkillType.None;
                        ClearHighlights();
                        TurnManager.Instance.EndCurrentTurn();
                    }
                }
            }
        }
        else if (currentSkill == SkillType.AstralPocket)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    AstralPocket(hit.point);
                    isSelectingTarget = false;
                    currentSkill = SkillType.None;
                    ClearHighlights();
                    TurnManager.Instance.EndCurrentTurn();
                }
            }
        }

        if (isSelectingTarget && Input.GetKeyDown(KeyCode.Escape))
        {
            isSelectingTarget = false;
            isHoldingTarget = false;
            currentSkill = SkillType.None;
            ClearHighlights();
            Debug.Log($"{gameObject.name}: Выбор скилла отменён.");
            if (skillsPanel != null && skillsPanel.skillPrompt != null)
            {
                skillsPanel.skillPrompt.gameObject.SetActive(false);
            }
        }

        if (!turnJustStarted && Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log($"{gameObject.name} завершил ход вручную (Space)");
            isSelectingTarget = false;
            isHoldingTarget = false;
            currentSkill = SkillType.None;
            ClearHighlights();
            if (skillsPanel != null && skillsPanel.skillPrompt != null)
            {
                skillsPanel.skillPrompt.gameObject.SetActive(false);
            }
            TurnManager.Instance.EndCurrentTurn();
        }
    }

    void HandleEnemyCombatTurn()
    {
        if (isStunned)
        {
            Debug.Log($"{gameObject.name} оглушён и пропускает ход!");
            TurnManager.Instance.EndCurrentTurn();
            isStunned = false;
            Debug.Log($"{gameObject.name}: Оглушение закончилось.");
            return;
        }

        Debug.Log($"{gameObject.name}: Обрабатываю ход врага. isMyTurn = {isMyTurn}, isRooted = {isRooted}, remainingDistance = {remainingDistance}");

        CharacterController tauntTarget = FindObjectOfType<CharacterController>();
        if (tauntTarget != null && tauntTarget.role == CharacterRole.TankMage && tauntTarget.tauntDuration > 0)
        {
            target = tauntTarget.gameObject;
        }
        else
        {
            target = GameObject.FindGameObjectWithTag("Player");
        }

        if (target == null)
        {
            Debug.Log("Игрок не найден, враг завершает ход");
            TurnManager.Instance.EndCurrentTurn();
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
        if (distanceToTarget <= attackRange)
        {
            Debug.Log($"{gameObject.name} атакует {target.name}");
            Attack(target, true);
            if (animator != null) animator.SetTrigger("Attack");
            Debug.Log($"{gameObject.name} завершил атаку, завершаю ход");
            TurnManager.Instance.EndCurrentTurn();
        }
        else if (remainingDistance > 0 && !isRooted)
        {
            Vector3 direction = (target.transform.position - transform.position).normalized;
            Vector3 movement = direction * moveSpeed * Time.deltaTime;
            float distanceToMove = movement.magnitude;

            if (remainingDistance >= distanceToMove)
            {
                transform.position += movement;
                remainingDistance -= distanceToMove;
                transform.LookAt(target.transform);
                if (animator != null) animator.SetFloat("Speed", 1f);
            }

            if (remainingDistance <= 0.1f)
            {
                if (animator != null) animator.SetFloat("Speed", 0f);
                Debug.Log($"{gameObject.name} исчерпал дистанцию движения и завершает ход");
                TurnManager.Instance.EndCurrentTurn();
            }
        }
        else
        {
            if (animator != null) animator.SetFloat("Speed", 0f);
            Debug.Log($"{gameObject.name} не может двигаться (rooted или нет дистанции), завершает ход");
            TurnManager.Instance.EndCurrentTurn();
        }

        if (!isMyTurn)
        {
            Debug.Log($"{gameObject.name}: isMyTurn сброшен после завершения хода врага: {isMyTurn}");
        }

        if (isRooted)
        {
            isRooted = false;
            Debug.Log($"{gameObject.name}: Рут закончился.");
        }
    }

    public void StartTurn()
    {
        Debug.Log($"{gameObject.name}: Начало хода. isMyTurn = {isMyTurn}, isStunned = {isStunned}, isRooted = {isRooted}, remainingDistance = {moveDistance}");
        isMyTurn = true;
        remainingDistance = moveDistance;
        turnJustStarted = true;
        turnStartTimer = turnStartDelay;
        isAttacking = false;

        if (fortifyTurnsLeft > 0)
        {
            fortifyTurnsLeft--;
            if (fortifyTurnsLeft == 0)
            {
                armor -= fortifyArmorBonus;
                fortifyArmorBonus = 0;
                Debug.Log($"{gameObject.name}: Эффект Fortify закончился. Броня: {armor}");
            }
        }

        if (concentrationTurnsLeft > 0)
        {
            concentrationTurnsLeft--;
            if (concentrationTurnsLeft == 0)
            {
                concentrationDamageMultiplier = 1f;
                Debug.Log($"{gameObject.name}: Эффект Concentration закончился.");
            }
        }

        if (longShotDebuffTurnsLeft > 0)
        {
            longShotDebuffTurnsLeft--;
            if (longShotDebuffTurnsLeft == 0)
            {
                longShotMoveDebuff = 1f;
                Debug.Log("${gameObject.name}: Дебафф Long Shot закончился.");
            }
        }

        if (tauntDuration > 0)
        {
            tauntDuration--;
            if (tauntDuration == 0)
            {
                Debug.Log($"{gameObject.name}: Эффект Taunt закончился.");
            }
        }

        if (protectedAlly != null)
        {
            protectedAlly = null;
            Debug.Log($"{gameObject.name}: Эффект Bodyguard закончился.");
        }

        GameManager.Instance.SaveCharacterStats(this);
    }

    public void EndTurn()
    {
        Debug.Log($"{gameObject.name}: EndTurn вызван");
        isMyTurn = false;
        target = null;
        isSelectingTarget = false;
        isHoldingTarget = false;
        currentSkill = SkillType.None;
        isAttacking = false;
        ClearHighlights();
        if (animator != null) animator.SetFloat("Speed", 0f);
        Debug.Log($"{gameObject.name}: isMyTurn сброшен в {isMyTurn}");

        GameManager.Instance.SaveCharacterStats(this);
    }

    public void Attack(GameObject targetToAttack, bool attackStrength = true)
    {
        CharacterController targetController = targetToAttack.GetComponent<CharacterController>();
        MovableExplosiveObject explosive = targetToAttack.GetComponent<MovableExplosiveObject>();

        if (targetController != null)
        {
            CharacterController actualTarget = targetController;
            CharacterController[] allCharacters = FindObjectsOfType<CharacterController>();
            foreach (CharacterController character in allCharacters)
            {
                if (character.protectedAlly == targetController)
                {
                    actualTarget = character;
                    Debug.Log($"{gameObject.name}: Атака перенаправлена на {actualTarget.gameObject.name} из-за Bodyguard!");
                    break;
                }
            }

            lastAttacker = this;
            if (animator != null) animator.SetTrigger("Attack");
            int attackDamage = Mathf.FloorToInt(damage * concentrationDamageMultiplier);
            if (attackStrength)
            {
                actualTarget.TakeDamage(attackDamage);
            }
            else
            {
                actualTarget.ReduceArmor(attackDamage);
            }
            Debug.Log($"{gameObject.name} атакует {actualTarget.gameObject.name} на {attackDamage} урона!");
            GameManager.Instance.SaveCharacterStats(this);
            GameManager.Instance.SaveCharacterStats(actualTarget);
        }
        else if (explosive != null)
        {
            if (animator != null) animator.SetTrigger("Attack");
            int attackDamage = Mathf.FloorToInt(damage * concentrationDamageMultiplier);
            explosive.TakeDamage(attackDamage);
            Debug.Log($"{gameObject.name} атакует {targetToAttack.name} на {attackDamage} урона!");
        }
    }

    public void TakeDamage(int damage)
    {
        int remainingDamage = damage;

        if (armor > 0)
        {
            int armorReduction = Mathf.Min(Mathf.FloorToInt(armor), remainingDamage);
            armor -= armorReduction;
            remainingDamage -= armorReduction;
            Debug.Log($"{gameObject.name} блокировал {armorReduction} урона бронёй. Остаток брони: {armor}");
        }

        if (remainingDamage > 0)
        {
            strength -= remainingDamage;
            Debug.Log($"{gameObject.name} получил {remainingDamage} урона в здоровье. Здоровье: {strength}");
        }

        if (strength <= 0)
        {
            Die();
        }

        GameManager.Instance.SaveCharacterStats(this);
    }

    public void ReduceArmor(int amount)
    {
        armor = Mathf.Max(0, armor - amount);
        Debug.Log($"{gameObject.name} потерял {amount} брони. Остаток брони: {armor}");
        GameManager.Instance.SaveCharacterStats(this);
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} погиб!");
        if (lastAttacker != null && !lastAttacker.isEnemy && isEnemy)
        {
            lastAttacker.GainExperience(expPerEnemyKill);
            Debug.Log($"{lastAttacker.gameObject.name} получил {expPerEnemyKill} опыта за убийство {gameObject.name}");
            int crystalsDropped = role == CharacterRole.Boss ? 10 : 1;
            GameManager.Instance.AddCrystals(crystalsDropped);
            Debug.Log($"{lastAttacker.gameObject.name} получил {crystalsDropped} кристалл(ов) за убийство {gameObject.name}");
        }
        TurnManager.Instance.RemoveCharacter(this);
        Destroy(gameObject);

        if (SceneManager.GetActiveScene().name.EndsWith("_Battle"))
        {
            CharacterController[] enemies = FindObjectsOfType<CharacterController>();
            bool anyEnemiesLeft = false;
            foreach (var enemy in enemies)
            {
                if (enemy.isEnemy)
                {
                    anyEnemiesLeft = true;
                    break;
                }
            }

            if (!anyEnemiesLeft)
            {
                Debug.Log("Все враги побеждены! Отображаем UI победы и возвращаемся в основную локацию.");
                GameManager.Instance.ShowVictoryUI();
                string baseScene = SceneManager.GetActiveScene().name.Replace("_Battle", "");
                LocationManager.Instance.GoToLocation(baseScene);
            }
        }
    }

    public void GainExperience(float exp)
    {
        experience += exp;
        Debug.Log($"{gameObject.name} получил {exp} опыта. Текущий опыт: {experience}/{expToNextLevel}");

        while (experience >= expToNextLevel)
        {
            LevelUp();
        }

        GameManager.Instance.SaveCharacterStats(this);
    }

    private void LevelUp()
    {
        level++;
        experience -= expToNextLevel;
        expToNextLevel = baseExpRequirement * (1f + (level - 1) * 0.5f);
        Debug.Log($"{gameObject.name} достиг уровня {level}! Новый порог опыта: {expToNextLevel}");

        maxStrength += 2f;
        strength = maxStrength;
        damage = strength * 0.8f;
        maxMana += 1f;
        mana = maxMana;
        armor += 1f;
        maxArmor = armor;
        Debug.Log($"{gameObject.name}: Здоровье увеличено до {maxStrength}, мана до {maxMana}, броня до {maxArmor}, урон до {damage}");

        GameManager.Instance.SaveCharacterStats(this);
    }

    public void StartCombat()
    {
        isInCombat = true;
        if (animator != null)
        {
            animator.SetBool("IsInCombat", true);
            animator.SetFloat("Speed", 0f);
        }
        Debug.Log($"{gameObject.name}: Вступил в бой. isInCombat = {isInCombat}");
    }

    public void EndCombat()
    {
        isInCombat = false;
        isMyTurn = false;
        if (animator != null)
        {
            animator.SetBool("IsInCombat", false);
            animator.SetFloat("Speed", 0f);
        }
        Debug.Log($"{gameObject.name}: Вышел из боя. isInCombat = {isInCombat}, isMyTurn = {isMyTurn}");
    }

    private void SetupStatsUI()
    {
        if (statsPrefab == null)
        {
            Debug.LogError($"{gameObject.name}: StatsPrefab не назначен в инспекторе!");
            return;
        }

        statsCanvasInstance = Instantiate(statsPrefab, transform.position + Vector3.up * statsHeight, Quaternion.identity, transform);
        if (statsCanvasInstance == null)
        {
            Debug.LogError($"{gameObject.name}: Не удалось создать statsCanvasInstance!");
            return;
        }

        Transform strengthTransform = statsCanvasInstance.transform.Find("StrengthText");
        Transform armorTransform = statsCanvasInstance.transform.Find("ArmorText");
        Transform manaTransform = statsCanvasInstance.transform.Find("ManaText");

        if (strengthTransform != null)
        {
            strengthText = strengthTransform.GetComponent<TMP_Text>();
            Debug.Log($"{gameObject.name}: StrengthText найден");
        }
        else
        {
            Debug.LogError($"{gameObject.name}: StrengthText не найден в StatsPrefab!");
        }

        if (armorTransform != null)
        {
            armorText = armorTransform.GetComponent<TMP_Text>();
            Debug.Log($"{gameObject.name}: ArmorText найден");
        }
        else
        {
            Debug.LogError($"{gameObject.name}: ArmorText не найден в StatsPrefab!");
        }

        if (manaTransform != null)
        {
            manaText = manaTransform.GetComponent<TMP_Text>();
            Debug.Log($"{gameObject.name}: ManaText найден");
        }
        else
        {
            Debug.LogError($"{gameObject.name}: ManaText не найден в StatsPrefab!");
        }

        UpdateStatsUI();
    }

    private void UpdateStatsUI()
    {
        if (strengthText != null)
        {
            strengthText.text = Mathf.FloorToInt(strength).ToString();
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: strengthText равен null, не могу обновить здоровье!");
        }

        if (armorText != null)
        {
            armorText.text = Mathf.FloorToInt(armor).ToString();
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: armorText равен null, не могу обновить броню!");
        }

        if (manaText != null)
        {
            manaText.text = Mathf.FloorToInt(mana).ToString();
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: manaText равен null, не могу обновить ману!");
        }

        if (statsCanvasInstance != null)
        {
            statsCanvasInstance.transform.rotation = Camera.main.transform.rotation;
        }
    }

    public void ActivateSkill(SkillType skill)
    {
        if (!isMyTurn)
        {
            Debug.LogWarning($"{gameObject.name}: Не мой ход, невозможно активировать навык {skill}");
            return;
        }

        if (!HasEnoughMana(skill))
        {
            Debug.Log($"{gameObject.name}: Не хватает маны для использования {skill}! Требуется: {skillManaCosts[skill]}, осталось: {mana}");
            return;
        }

        Debug.Log($"{gameObject.name}: Активирую навык {skill}");
        currentSkill = skill;
        isSelectingTarget = true;

        if (skill == SkillType.Taunt || skill == SkillType.Fortify || skill == SkillType.Concentration)
        {
            ApplySelectedSkill(Vector3.zero);
            isSelectingTarget = false;
            currentSkill = SkillType.None;
            TurnManager.Instance.EndCurrentTurn();
        }
        else
        {
            HighlightTargets();
            Debug.Log($"Выберите цель для скилла: {skill}");
        }
    }

    private bool HasEnoughMana(SkillType skill)
    {
        if (skillManaCosts.ContainsKey(skill))
        {
            return mana >= skillManaCosts[skill];
        }
        return true;
    }

    private void ConsumeMana(SkillType skill)
    {
        if (skillManaCosts.ContainsKey(skill))
        {
            mana -= skillManaCosts[skill];
            Debug.Log($"{gameObject.name}: Потрачено {skillManaCosts[skill]} маны. Остаток: {mana}");
            GameManager.Instance.SaveCharacterStats(this);
        }
    }

    private void HighlightTargets()
    {
        ClearHighlights();

        Material outlineMaterial = GetOutlineMaterial(currentSkill);
        if (outlineMaterial == null)
        {
            Debug.LogWarning($"OutlineMaterial не найден для скилла {currentSkill}");
            return;
        }

        string targetTag = GetTargetTag(currentSkill);
        if (string.IsNullOrEmpty(targetTag))
        {
            Debug.LogWarning($"TargetTag не определён для скилла {currentSkill}");
            return;
        }

        float range = GetSkillRange(currentSkill);
        Debug.Log($"Ищу цели в радиусе {range} с тегом {targetTag}");
        Collider[] colliders = Physics.OverlapSphere(transform.position, range);
        Debug.Log($"Найдено объектов: {colliders.Length}");

        foreach (Collider col in colliders)
        {
            Debug.Log($"Проверяю объект: {col.gameObject.name}, Тег: {col.tag}");
            if (col.CompareTag(targetTag))
            {
                if (currentSkill == SkillType.Bodyguard && col.gameObject == gameObject)
                {
                    continue;
                }

                Renderer renderer = col.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material[] originalMaterials = renderer.materials;
                    Material[] newMaterials = new Material[originalMaterials.Length + 1];
                    for (int i = 0; i < originalMaterials.Length; i++)
                    {
                        newMaterials[i] = originalMaterials[i];
                    }
                    newMaterials[originalMaterials.Length] = outlineMaterial;
                    renderer.materials = newMaterials;
                    highlightedObjects.Add((col.gameObject, originalMaterials));
                    Debug.Log($"Выделен объект: {col.gameObject.name} с тегом {targetTag}");
                }
                else
                {
                    Debug.LogWarning($"Renderer не найден на объекте {col.gameObject.name}");
                }
            }
        }
    }

    private void ClearHighlights()
    {
        foreach (var (obj, originalMaterials) in highlightedObjects)
        {
            if (obj != null)
            {
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.materials = originalMaterials;
                }
            }
        }
        highlightedObjects.Clear();
    }

    private Material GetOutlineMaterial(SkillType skill)
    {
        switch (skill)
        {
            case SkillType.Attack:
            case SkillType.ShieldSlam:
            case SkillType.TripleShot:
            case SkillType.Freeze:
            case SkillType.LongShot:
                return redOutlineMaterial;
            case SkillType.Bodyguard:
            case SkillType.Telekinesis:
                return greenOutlineMaterial;
            default:
                return null;
        }
    }

    private string GetTargetTag(SkillType skill)
    {
        switch (skill)
        {
            case SkillType.ShieldSlam:
            case SkillType.TripleShot:
            case SkillType.Freeze:
            case SkillType.LongShot:
            case SkillType.Attack:
                return "Enemy";
            case SkillType.Telekinesis:
                return "Movable";
            case SkillType.Bodyguard:
                return "Player";
            default:
                return null;
        }
    }

    private float GetSkillRange(SkillType skill)
    {
        switch (skill)
        {
            case SkillType.Telekinesis:
                return telekinesisRange;
            default:
                return attackRange;
        }
    }

    private void ApplySelectedSkill(Vector3 hitPoint)
    {
        ConsumeMana(currentSkill);

        switch (currentSkill)
        {
            case SkillType.Taunt:
                Taunt();
                break;
            case SkillType.Fortify:
                Fortify();
                break;
            case SkillType.Concentration:
                Concentration();
                break;
        }

        if (skillsPanel != null && skillsPanel.skillPrompt != null)
        {
            skillsPanel.skillPrompt.gameObject.SetActive(false);
        }
    }

    private void PerformAttack()
    {
        if (target == null)
        {
            Debug.LogWarning($"{gameObject.name}: Не выбрана цель для атаки!");
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
        if (distanceToTarget <= attackRange)
        {
            Attack(target, true);
            Debug.Log($"{gameObject.name} атаковал {target.name}!");
            if (animator != null) animator.SetTrigger("Attack");
        }
        else
        {
            Debug.Log($"{gameObject.name}: Цель слишком далеко для атаки (расстояние: {distanceToTarget}, максимум: {attackRange})");
        }
    }

    private void Telekinesis(Vector3 hitPoint)
    {
        // Логика телекинеза уже перенесена в HandlePlayerCombatTurn
    }

    private void Taunt()
    {
        tauntDuration = 1;
        Debug.Log($"{gameObject.name} применил Taunt! Враги будут атаковать его в течение 1 хода.");
        if (animator != null) animator.SetTrigger("Taunt");
    }

    private void ShieldSlam()
    {
        if (target == null)
        {
            Debug.LogWarning($"{gameObject.name}: Не выбрана цель для ShieldSlam!");
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
        if (distanceToTarget <= attackRange)
        {
            CharacterController enemy = target.GetComponent<CharacterController>();
            if (enemy != null && enemy.isEnemy)
            {
                ConsumeMana(SkillType.ShieldSlam);
                enemy.TakeDamage(5);
                enemy.isStunned = true;
                Debug.Log($"{gameObject.name} применил Shield Slam на {target.name}, нанеся 5 урона и оглушив на 1 ход!");
                if (animator != null) animator.SetTrigger("Attack");
                GameManager.Instance.SaveCharacterStats(enemy);
            }
        }
        else
        {
            Debug.Log($"{gameObject.name}: Цель слишком далеко для Shield Slam (расстояние: {distanceToTarget}, максимум: {attackRange})");
        }
    }

    private void Fortify()
    {
        fortifyArmorBonus = fortifyArmorIncrease;
        armor += fortifyArmorBonus;
        fortifyTurnsLeft = 2;
        Debug.Log($"{gameObject.name} применил Fortify, увеличив броню на {fortifyArmorIncrease} на 2 хода. Новая броня: {armor}");
        if (animator != null) animator.SetTrigger("Support");
        GameManager.Instance.SaveCharacterStats(this);
    }

    private void Bodyguard()
    {
        if (target == null)
        {
            Debug.LogWarning($"{gameObject.name}: Не выбрана цель для Bodyguard!");
            return;
        }

        CharacterController ally = target.GetComponent<CharacterController>();
        if (ally != null && !ally.isEnemy && ally != this)
        {
            ConsumeMana(SkillType.Bodyguard);
            protectedAlly = ally;
            Debug.Log($"{gameObject.name} применил Bodyguard, защищая {ally.gameObject.name} до следующего хода!");
            if (animator != null) animator.SetTrigger("Support");
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: Неверная цель для Bodyguard!");
        }
    }

    private void TripleShot()
    {
        if (target == null)
        {
            Debug.LogWarning($"{gameObject.name}: Не выбрана цель для TripleShot!");
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
        if (distanceToTarget <= attackRange)
        {
            CharacterController enemy = target.GetComponent<CharacterController>();
            if (enemy != null && enemy.isEnemy)
            {
                ConsumeMana(SkillType.TripleShot);
                float shotDamage = tripleShotBaseDamage * concentrationDamageMultiplier;
                for (int i = 0; i < 3; i++)
                {
                    float damagePerShot = shotDamage * (1f - i * 0.2f);
                    enemy.TakeDamage(Mathf.FloorToInt(damagePerShot));
                    Debug.Log($"{gameObject.name} выстрелил в {target.name} (выстрел {i + 1}/3), нанеся {damagePerShot} урона!");
                }
                if (animator != null) animator.SetTrigger("Attack");
                GameManager.Instance.SaveCharacterStats(enemy);
            }
        }
        else
        {
            Debug.Log($"{gameObject.name}: Цель слишком далеко для Triple Shot (расстояние: {distanceToTarget}, максимум: {attackRange})");
        }
    }

    private void Concentration()
    {
        concentrationDamageMultiplier = 2f;
        concentrationTurnsLeft = 2;
        Debug.Log($"{gameObject.name} применил Concentration, увеличив урон в 2 раза на 2 хода!");
        if (animator != null) animator.SetTrigger("Support");
        GameManager.Instance.SaveCharacterStats(this);
    }

    private void Freeze()
    {
        if (target == null)
        {
            Debug.LogWarning($"{gameObject.name}: Не выбрана цель для Freeze!");
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
        if (distanceToTarget <= attackRange)
        {
            CharacterController enemy = target.GetComponent<CharacterController>();
            if (enemy != null && enemy.isEnemy)
            {
                ConsumeMana(SkillType.Freeze);
                enemy.TakeDamage(5);
                enemy.isStunned = true;
                Debug.Log($"{gameObject.name} заморозил {target.name}, нанеся 5 урона и оглушив на 1 ход!");
                if (animator != null) animator.SetTrigger("MagicAttack");
                GameManager.Instance.SaveCharacterStats(enemy);
            }
        }
        else
        {
            Debug.Log($"{gameObject.name}: Цель слишком далеко для Freeze (расстояние: {distanceToTarget}, максимум: {attackRange})");
        }
    }

    private void LongShot()
    {
        if (target == null)
        {
            Debug.LogWarning($"{gameObject.name}: Не выбрана цель для LongShot!");
            return;
        }

        CharacterController enemy = target.GetComponent<CharacterController>();
        if (enemy != null && enemy.isEnemy)
        {
            ConsumeMana(SkillType.LongShot);
            float longShotDamage = damage * longShotDamageMultiplier * concentrationDamageMultiplier;
            enemy.TakeDamage(Mathf.FloorToInt(longShotDamage));
            Debug.Log($"{gameObject.name} применил Long Shot на {target.name}, нанеся {longShotDamage} урона!");

            longShotMoveDebuff = 0.5f;
            longShotDebuffTurnsLeft = 1;
            Debug.Log($"{gameObject.name}: Дистанция хода уменьшена на 50% на 1 ход из-за Long Shot.");

            if (animator != null) animator.SetTrigger("Attack");
            GameManager.Instance.SaveCharacterStats(this);
            GameManager.Instance.SaveCharacterStats(enemy);
        }
    }

    private void AstralPocket(Vector3 targetPoint)
    {
        float distanceToTarget = Vector3.Distance(transform.position, targetPoint);
        if (distanceToTarget <= attackRange)
        {
            ConsumeMana(SkillType.AstralPocket);
            Instantiate(astralPocketBombPrefab, targetPoint, Quaternion.identity);
            Debug.Log($"{gameObject.name} установил взрывную бочку через Astral Pocket в {targetPoint}!");
            if (animator != null) animator.SetTrigger("MagicAttack");
        }
        else
        {
            Debug.Log($"{gameObject.name}: Точка слишком далеко для Astral Pocket (расстояние: {distanceToTarget}, максимум: {attackRange})");
        }
    }

    public void SelectAttack() { ActivateSkill(SkillType.Attack); }
    public void SelectTelekinesis() { ActivateSkill(SkillType.Telekinesis); }
    public void SelectTaunt() { ActivateSkill(SkillType.Taunt); }
    public void SelectShieldSlam() { ActivateSkill(SkillType.ShieldSlam); }
    public void SelectFortify() { ActivateSkill(SkillType.Fortify); }
    public void SelectBodyguard() { ActivateSkill(SkillType.Bodyguard); }
    public void SelectTripleShot() { ActivateSkill(SkillType.TripleShot); }
    public void SelectConcentration() { ActivateSkill(SkillType.Concentration); }
    public void SelectFreeze() { ActivateSkill(SkillType.Freeze); }
    public void SelectLongShot() { ActivateSkill(SkillType.LongShot); }
    public void SelectAstralPocket() { ActivateSkill(SkillType.AstralPocket); }

    private void InitializeSkillDescriptions()
    {
        skillDescriptions = new Dictionary<SkillType, string>
        {
            { SkillType.Attack, "Атакует врага, нанося урон в зависимости от силы атаки." },
            { SkillType.Telekinesis, $"Перемещает бочку на расстояние до {telekinesisMoveDistance} в радиусе {telekinesisRange}. (1 маны)" },
            { SkillType.Taunt, "Заставляет врагов атаковать вас в течение 1 хода. (2 маны)" },
            { SkillType.ShieldSlam, "Наносит 5 урона и оглушает врага на 1 ход. (1 маны)" },
            { SkillType.Fortify, $"Увеличивает броню на {fortifyArmorIncrease} на 2 хода. (2 маны)" },
            { SkillType.Bodyguard, "Перенаправляет атаки на вас вместо выбранного союзника до следующего хода. (2 маны)" },
            { SkillType.TripleShot, $"Выпускает 3 выстрела по врагу, каждый наносит {tripleShotBaseDamage} урона (уменьшается с каждым выстрелом). (1 маны)" },
            { SkillType.Concentration, "Удваивает урон от атак на 2 хода. (2 маны)" },
            { SkillType.Freeze, "Наносит 5 урона и оглушает врага на 1 ход. (1 маны)" },
            { SkillType.LongShot, $"Наносит удвоенный урон ({longShotDamageMultiplier}x) и снижает дистанцию движения цели на 50% на 1 ход. (1 маны)" },
            { SkillType.AstralPocket, "Устанавливает взрывную бочку в выбранной точке. (1 маны)" }
        };
    }

    private void InitializeSkillManaCosts()
    {
        skillManaCosts = new Dictionary<SkillType, float>
        {
            { SkillType.Telekinesis, 1f },
            { SkillType.Taunt, 2f },
            { SkillType.ShieldSlam, 1f },
            { SkillType.Fortify, 2f },
            { SkillType.Bodyguard, 2f },
            { SkillType.TripleShot, 1f },
            { SkillType.Concentration, 2f },
            { SkillType.Freeze, 1f },
            { SkillType.LongShot, 1f },
            { SkillType.AstralPocket, 1f }
        };
    }

    public string GetSkillDescription(SkillType skill)
    {
        if (skillDescriptions.ContainsKey(skill))
        {
            return skillDescriptions[skill];
        }
        return "Описание не найдено.";
    }

    public float GetAgility()
    {
        return agility;
    }

    public int GetLevel()
    {
        return level;
    }

    public float GetExperience()
    {
        return experience;
    }

    public float GetExpToNextLevel()
    {
        return expToNextLevel;
    }

    public float GetMoveDistance()
    {
        return moveDistance;
    }

    public void AddExperience(int amount)
    {
        GainExperience(amount);
    }
}