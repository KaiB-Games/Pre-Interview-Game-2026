using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class CombatManager : MonoBehaviour
{
    //Variable Initialization
    public enum BattleState { START, HERO1_TURN, HERO2_TURN, ENEMY_TURN, WON, LOST }
    [Header("Current State")]
    public BattleState state;

    [System.Serializable]

    //Stats Initialization
    public class CharacterStats
    {
        public string name;
        public Transform worldObject;
        public int currentHP = 100;
        public int maxHP = 100;
        public int currentMP = 30;
        public int maxMP = 30;
        public bool isDefending = false;

        [Header("UI Bars")]
        public Slider hpSlider;
        public Slider mpSlider;
    }

    [Header("Party & Enemies")]
    [SerializeField] private CharacterStats hero1;
    [SerializeField] private CharacterStats hero2;
    [SerializeField] private GameObject[] enemies;
    private int skeleton1HP = 120;
    private int skeleton2HP = 120;

    [Header("UI Floating Menu")]
    [SerializeField] private RectTransform overheadMenuCanvas;
    [SerializeField] private Vector3 menuOffset = new Vector3(0, 1.5f, 0);
    [SerializeField] private Button attackButton;
    [SerializeField] private Button magicButton;
    [SerializeField] private Button defendButton;

    void Start()
    {
        state = BattleState.START;
        InitializeBars();
        UpdateStatusUI();
        StartCoroutine(SetupBattleRoutine());
    }

    //Battle Routine/Flow
    IEnumerator SetupBattleRoutine()
    {
        overheadMenuCanvas.gameObject.SetActive(false);
        yield return new WaitForSeconds(1f);
        StartHero1Turn();
    }

    void InitializeBars()
    {
        // Set up the slider min/max limits
        if (hero1.hpSlider != null) { hero1.hpSlider.maxValue = hero1.maxHP; hero1.hpSlider.value = hero1.currentHP; }
        if (hero1.mpSlider != null) { hero1.mpSlider.maxValue = hero1.maxMP; hero1.mpSlider.value = hero1.currentMP; }
        if (hero2.hpSlider != null) { hero2.hpSlider.maxValue = hero2.maxHP; hero2.hpSlider.value = hero2.currentHP; }
        if (hero2.mpSlider != null) { hero2.mpSlider.maxValue = hero2.maxMP; hero2.mpSlider.value = hero2.currentMP; }
    }

    void UpdateStatusUI()
    {
        // Smoothly adjust the fill values on the UI
        if (hero1.hpSlider != null) hero1.hpSlider.value = hero1.currentHP;
        if (hero1.mpSlider != null) hero1.mpSlider.value = hero1.currentMP;
        if (hero2.hpSlider != null) hero2.hpSlider.value = hero2.currentHP;
        if (hero2.mpSlider != null) hero2.mpSlider.value = hero2.currentMP;
    }

    void PositionMenuAbove(Transform target)
    {
        overheadMenuCanvas.gameObject.SetActive(true);
        overheadMenuCanvas.position = target.position + menuOffset;
    }

    void StartHero1Turn()
    {
        if (hero1.currentHP <= 0) { StartHero2Turn(); return; }
        state = BattleState.HERO1_TURN;
        hero1.isDefending = false;
        PositionMenuAbove(hero1.worldObject);
        ConfigureMenuButtons(hero1);
    }

    void StartHero2Turn()
    {
        if (hero2.currentHP <= 0) { StartEnemyTurn(); return; }
        state = BattleState.HERO2_TURN;
        hero2.isDefending = false;
        PositionMenuAbove(hero2.worldObject);
        ConfigureMenuButtons(hero2);
    }

    void ConfigureMenuButtons(CharacterStats currentHero)
    {
        magicButton.interactable = currentHero.currentMP >= 10;
        attackButton.interactable = true;
        defendButton.interactable = true;
    }

    public void OnAttackAction()
    {
        overheadMenuCanvas.gameObject.SetActive(false);

        // Track who is attacking
        string activeHeroName = (state == BattleState.HERO1_TURN) ? hero1.name : hero2.name;

        if (skeleton1HP > 0)
        {
            skeleton1HP -= 20;
            Debug.Log($"[ACTION] {activeHeroName} Attacked Skeleton 1! Skeleton 1 HP remaining: {skeleton1HP}");
        }
        else if (skeleton2HP > 0)
        {
            skeleton2HP -= 20;
            Debug.Log($"[ACTION] {activeHeroName} Attacked Skeleton 2! Skeleton 2 HP remaining: {skeleton2HP}");
        }

        CheckEnemiesAlive();
    }


    public void OnMagicAction()
    {
        overheadMenuCanvas.gameObject.SetActive(false);

        CharacterStats activeHero = null;
        if (state == BattleState.HERO1_TURN) activeHero = hero1;
        else if (state == BattleState.HERO2_TURN) activeHero = hero2;

        if (activeHero == null)
        {
            Debug.LogError("CombatManager: Clicked Magic but it's nobody's turn! Current state: " + state);
            return;
        }

        activeHero.currentMP = Mathf.Max(0, activeHero.currentMP - 10);
        Debug.Log($"[ACTION] {activeHero.name} casted Magic! Remaining MP: {activeHero.currentMP}");

        if (skeleton1HP > 0)
        {
            skeleton1HP -= 35;
            Debug.Log("Magic hit Skeleton 1! Remaining HP: " + skeleton1HP);
        }
        else if (skeleton2HP > 0)
        {
            skeleton2HP -= 35;
            Debug.Log("Magic hit Skeleton 2! Remaining HP: " + skeleton2HP);
        }

        UpdateStatusUI();

        CheckEnemiesAlive();
    }

    public void OnDefendAction()
    {
        overheadMenuCanvas.gameObject.SetActive(false);
        CharacterStats activeHero = (state == BattleState.HERO1_TURN) ? hero1 : hero2;
        activeHero.isDefending = true;

        Debug.Log($"[ACTION] {activeHero.name} is now Defending! Incoming damage will be cut in half.");

        AdvanceTurn();
    }

    void CheckEnemiesAlive()
    {
        if (skeleton1HP <= 0 && enemies[0] != null) Destroy(enemies[0]);
        if (skeleton2HP <= 0 && enemies[1] != null) Destroy(enemies[1]);

        if (skeleton1HP <= 0 && skeleton2HP <= 0)
        {
            state = BattleState.WON;
            EndBattle();
        }
        else
        {
            AdvanceTurn();
        }
    }

    void AdvanceTurn()
    {
        if (state == BattleState.HERO1_TURN) StartHero2Turn();
        else if (state == BattleState.HERO2_TURN) StartEnemyTurn();
    }

    void StartEnemyTurn()
    {
        state = BattleState.ENEMY_TURN;
        StartCoroutine(EnemyTurnRoutine());
    }

    IEnumerator EnemyTurnRoutine()
    {
        yield return new WaitForSeconds(1f);
        if (skeleton1HP > 0) ExecuteSingleEnemyAttack("Skeleton 1");

        yield return new WaitForSeconds(1f);
        if (skeleton2HP > 0 && state != BattleState.LOST) ExecuteSingleEnemyAttack("Skeleton 2");

        if (hero1.currentHP <= 0 && hero2.currentHP <= 0)
        {
            state = BattleState.LOST;
            EndBattle();
        }
        else if (state != BattleState.LOST)
        {
            StartHero1Turn();
        }
    }

    void ExecuteSingleEnemyAttack(string enemyName)
    {
        CharacterStats targetHero = (Random.value > 0.5f) ? hero1 : hero2;
        if (targetHero.currentHP <= 0) targetHero = (targetHero == hero1) ? hero2 : hero1;

        int incomingDamage = 15;
        if (targetHero.isDefending) incomingDamage /= 2;

        targetHero.currentHP = Mathf.Max(0, targetHero.currentHP - incomingDamage);
        UpdateStatusUI();
    }

    void EndBattle()
    {
        if (state == BattleState.WON)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.isSkeletonDefeated = true;
                GameManager.Instance.isReturningFromCombat = true;
            }
            SceneManager.LoadScene("Overworld");
        }
        else if (state == BattleState.LOST)
        {
            Debug.Log("Game Over.");
        }
    }
}