using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class CombatManager : MonoBehaviour
{
    public enum BattleState { START, HERO_TURN, ENEMY_TURN, WON, LOST }


    [System.Serializable]
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

        [Header("UI Text Displays")]
        public TMPro.TextMeshProUGUI hpText;  
        public TMPro.TextMeshProUGUI mpText;  
    }

    [Header("Player Character")]
    [SerializeField] private CharacterStats hero;

    [System.Serializable]
    public class EnemyWaveConfig
    {
        public string waveName;
        public GameObject[] enemyGameObjects;
        public int[] enemyHealthPools;
    }

    [Header("Wave Tracking Layout")]
    [SerializeField] private List<EnemyWaveConfig> waves = new List<EnemyWaveConfig>();
    private int currentWaveIndex = 0;

    [Header("UI Floating Menu")]
    [SerializeField] private RectTransform overheadMenuCanvas;
    [SerializeField] private Vector3 menuOffset = new Vector3(0, 1.5f, 0);
    [SerializeField] private Button attackButton;
    [SerializeField] private Button magicButton;
    [SerializeField] private Button defendButton;

    [Header("Current State")]
    public BattleState state;
    [Header("Overworld Selector")]
    [SerializeField] private string overworldName;

    [Header("Slash Visual Effects")]
    [SerializeField] private GameObject slashPrefab;
    [SerializeField] private Vector3 slashOffset = new Vector3(0f, 0f, 0f);

    void Start()
    {
        state = BattleState.START;
        InitializeBars();
        UpdateStatusUI();
        SetHeroBarsVisible(true);

        for (int i = 0; i < waves.Count; i++)
        {
            SetWaveActiveState(i, i == 0);
        }

        StartCoroutine(SetupBattleRoutine());
    }

    IEnumerator SetupBattleRoutine()
    {
        overheadMenuCanvas.gameObject.SetActive(false);
        yield return new WaitForSeconds(1f);
        StartHeroTurn();
    }

    void InitializeBars()
    {
        if (GameManager.Instance != null)
        {
            int levelBonusHP = (GameManager.Instance.partyLevel - 1) * 15;
            hero.maxHP += levelBonusHP;

            if (GameManager.Instance.playerCurrentHP == -1)
            {
                GameManager.Instance.InitializeNewGameVitals(hero.maxHP, hero.maxMP);
            }

            hero.currentHP = GameManager.Instance.playerCurrentHP;
            hero.currentMP = GameManager.Instance.playerCurrentMP;
        }
        else
        {
            hero.currentHP = hero.maxHP;
            hero.currentMP = hero.maxMP;
        }

        UpdateCharacterBars();
    }

    public void UpdateStatusUI()
    {
        UpdateCharacterBars();
    }

    void UpdateCharacterBars()
    {
        if (hero.hpSlider != null)
        {
            hero.hpSlider.minValue = 0;
            hero.hpSlider.maxValue = hero.maxHP;
            hero.hpSlider.value = hero.currentHP;
        }

        if (hero.mpSlider != null)
        {
            hero.mpSlider.minValue = 0;
            hero.mpSlider.maxValue = hero.maxMP;
            hero.mpSlider.value = hero.currentMP;
        }

        if (hero.hpText != null)
        {
            hero.hpText.text = $"{hero.currentHP} / {hero.maxHP}";
        }

        if (hero.mpText != null)
        {
            hero.mpText.text = $"{hero.currentMP} / {hero.maxMP}";
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.playerCurrentHP = hero.currentHP;
            GameManager.Instance.playerCurrentMP = hero.currentMP;
        }
    }

    void SetHeroBarsVisible(bool visible)
    {
        if (hero.hpSlider != null) hero.hpSlider.gameObject.SetActive(visible);
        if (hero.mpSlider != null) hero.mpSlider.gameObject.SetActive(visible);
    }

    void PositionMenuAbove(Transform target)
    {
        overheadMenuCanvas.gameObject.SetActive(true);
    }

    void StartHeroTurn()
    {
        if (hero.currentHP <= 0) { EndBattle(); return; }

        state = BattleState.HERO_TURN;
        hero.isDefending = false;
        UpdateStatusUI();
        PositionMenuAbove(hero.worldObject);
        ConfigureMenuButtons();
    }

    void ConfigureMenuButtons()
    {
        magicButton.interactable = hero.currentMP >= 10;
        attackButton.interactable = true;
        defendButton.interactable = true;
    }

    public void OnAttackAction()
    {
        overheadMenuCanvas.gameObject.SetActive(false);

        int targetIndex = FindFirstLivingEnemyIndex();
        if (targetIndex != -1)
        {
            waves[currentWaveIndex].enemyHealthPools[targetIndex] -= 20;
            string enemyName = waves[currentWaveIndex].enemyGameObjects[targetIndex].name;
            Debug.Log($"[ACTION] {hero.name} Attacked {enemyName}! HP remaining: {waves[currentWaveIndex].enemyHealthPools[targetIndex]}");

            // SPAWN SLASH EFFECT HERE
            SpawnSlashEffect(waves[currentWaveIndex].enemyGameObjects[targetIndex].transform);
        }

        CheckEnemiesAlive();
    }

    public void OnMagicAction()
    {
        overheadMenuCanvas.gameObject.SetActive(false);

        hero.currentMP = Mathf.Max(0, hero.currentMP - 10);
        UpdateStatusUI();

        int targetIndex = FindFirstLivingEnemyIndex();
        if (targetIndex != -1)
        {
            waves[currentWaveIndex].enemyHealthPools[targetIndex] -= 35;
            string enemyName = waves[currentWaveIndex].enemyGameObjects[targetIndex].name;
            Debug.Log($"[MAGIC] {hero.name} Cast on {enemyName}! HP remaining: {waves[currentWaveIndex].enemyHealthPools[targetIndex]}");

            // SPAWN SLASH EFFECT HERE
            SpawnSlashEffect(waves[currentWaveIndex].enemyGameObjects[targetIndex].transform);
        }

        CheckEnemiesAlive();
    }

    public void OnDefendAction()
    {
        overheadMenuCanvas.gameObject.SetActive(false);
        hero.isDefending = true;
        StartEnemyTurn();
    }

    int FindFirstLivingEnemyIndex()
    {
        var currentWave = waves[currentWaveIndex];
        for (int i = 0; i < currentWave.enemyHealthPools.Length; i++)
        {
            if (currentWave.enemyHealthPools[i] > 0) return i;
        }
        return -1;
    }

    void CheckEnemiesAlive()
    {
        var currentWave = waves[currentWaveIndex];

        for (int i = 0; i < currentWave.enemyHealthPools.Length; i++)
        {
            if (currentWave.enemyHealthPools[i] <= 0 && currentWave.enemyGameObjects[i] != null)
            {
                Destroy(currentWave.enemyGameObjects[i]);
            }
        }

        if (FindFirstLivingEnemyIndex() == -1)
        {
            if (currentWaveIndex < waves.Count - 1)
            {
                StartCoroutine(TransitionToNextWave());
            }
            else
            {
                state = BattleState.WON;
                EndBattle();
            }
        }
        else
        {
            StartEnemyTurn();
        }
    }

    IEnumerator TransitionToNextWave()
    {
        state = BattleState.START;
        overheadMenuCanvas.gameObject.SetActive(false);

        Debug.Log($"Wave {currentWaveIndex + 1} Cleared! Preparing next encounter wave...");
        yield return new WaitForSeconds(1.5f);

        SetWaveActiveState(currentWaveIndex, false);
        currentWaveIndex++;
        SetWaveActiveState(currentWaveIndex, true);

        Debug.Log($"Entering: {waves[currentWaveIndex].waveName}!");
        yield return new WaitForSeconds(1.0f);

        StartHeroTurn();
    }

    void SetWaveActiveState(int index, bool isActive)
    {
        if (index >= 0 && index < waves.Count)
        {
            foreach (var enemy in waves[index].enemyGameObjects)
            {
                if (enemy != null) enemy.SetActive(isActive);
            }
        }
    }

    void StartEnemyTurn()
    {
        state = BattleState.ENEMY_TURN;
        StartCoroutine(EnemyTurnRoutine());
    }

    IEnumerator EnemyTurnRoutine()
    {
        yield return new WaitForSeconds(0.6f);
        var currentWave = waves[currentWaveIndex];

        for (int i = 0; i < currentWave.enemyHealthPools.Length; i++)
        {
            GameObject currentEnemy = currentWave.enemyGameObjects[i];

            if (currentWave.enemyHealthPools[i] > 0 && currentEnemy != null && state != BattleState.LOST)
            {
                EnemyBehaviour behavior = currentEnemy.GetComponent<EnemyBehaviour>();

                if (behavior != null)
                {
                    yield return StartCoroutine(behavior.ExecuteAttackSequence(this, hero));
                }
                else
                {
                    Debug.LogWarning($"[WARNING] {currentEnemy.name} is missing an EnemyBehaviour script component!");
                    hero.currentHP = Mathf.Max(0, hero.currentHP - 15);
                    UpdateStatusUI();
                    yield return new WaitForSeconds(1f);
                }
            }
        }

        if (hero.currentHP <= 0)
        {
            state = BattleState.LOST;
            EndBattle();
        }
        else if (state != BattleState.LOST && state == BattleState.ENEMY_TURN)
        {
            StartHeroTurn();
        }
    }

    [Header("Encounter Rewards")]
    [SerializeField] private int xpRewardForThisMatch = 50;

    void EndBattle()
    {
        if (state == BattleState.WON)
        {
            Debug.Log("Combat Stage Won!");

            if (GameManager.Instance == null)
            {
                GameManager.Instance = FindFirstObjectByType<GameManager>();
            }

            string targetOverworld = overworldName;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddXP(xpRewardForThisMatch);
                GameManager.Instance.ProgressToNextEncounter();
                GameManager.Instance.isSkeletonDefeated = true;
                GameManager.Instance.isReturningFromCombat = true;

                targetOverworld = GameManager.Instance.GetCurrentOverworldName();
            }
            else
            {
                Debug.LogError("CRITICAL: No GameManager object found in the scene! Defaulting destination.");
            }

            SceneManager.LoadScene(overworldName);
        }
    }
    void SpawnSlashEffect(Transform enemyTransform)
    {
        if (slashPrefab != null && enemyTransform != null)
        {
            // Spawns the SLASY animation precisely on top of the targeted enemy's position
            Vector3 spawnPosition = enemyTransform.position + slashOffset;
            Instantiate(slashPrefab, spawnPosition, Quaternion.identity);
        }
    }
}