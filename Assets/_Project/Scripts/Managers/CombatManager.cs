using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum CombatState
{
    START,
    PLAYERTURN,
    ENEMYTURN,
    WIN,
    LOSE
}
public class CombatManager : MonoBehaviour
{
    [Header("Prefabs & Spawn Points")]
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public Transform playerSpawnPoint;
    public Transform enemySpawnPoint;

    [Header("UI Elements")]
    public TextMeshProUGUI turnInfoText;
    public TextMeshProUGUI playerHealthText;
    public TextMeshProUGUI enemyHealthText;
    public TextMeshProUGUI playerSapText;

    [Header("Character Stats")]
    public CharacterStats playerStats;
    public CharacterStats enemyStats;

    [Header("Combat State")]
    public CombatState state;

    void Start()
    {
        state = CombatState.START;
        StartCoroutine(SetupBattle());
    }

    IEnumerator SetupBattle()
    {
        GameObject playerGO = Instantiate(playerPrefab, playerSpawnPoint);
        playerStats = playerGO.GetComponent<CharacterStats>();
        playerStats.InitializeFromManager();
        playerHealthText.text = $"Health: {playerStats.currentHealth}/{playerStats.maxHealth}";

        GameObject enemyGO = Instantiate(enemyPrefab, enemySpawnPoint);
        enemyStats = enemyGO.GetComponent<CharacterStats>();
        enemyHealthText.text = $"Health: {enemyStats.currentHealth}/{enemyStats.maxHealth}";

        turnInfoText.text = "Battle Start!";
        yield return new WaitForSeconds(2f);

        state = CombatState.PLAYERTURN;
        PlayerTurn();
    }

    void PlayerTurn()
    {
        turnInfoText.text = "Your turn!";
        playerStats.isDefending = false;
    }

    IEnumerator EnemyTurn()
    {
        turnInfoText.text = "Enemy's turn";
        yield return new WaitForSeconds(1f);

        if (enemyStats.isStunned)
        {
            turnInfoText.text = "Enemy is stunned!";
            enemyStats.isStunned = false;
            yield return new WaitForSeconds(1f);
        }
        else
        {
            turnInfoText.text = "Enemy attacks!";
            bool isPlayerDead = playerStats.TakeDamage(enemyStats.damage);
            playerHealthText.text = $"Health: {playerStats.currentHealth}/{playerStats.maxHealth}";
            yield return new WaitForSeconds(1f);
            if (isPlayerDead)
            {
                state = CombatState.LOSE;
                EndBattle();
            }
        }

        if (state != CombatState.LOSE)
        {
            state = CombatState.PLAYERTURN;
            PlayerTurn();
        }
    }

    void EndBattle()
    {
        if (state == CombatState.WIN)
        {
            turnInfoText.text = "You win!";
        }
        else if (state == CombatState.LOSE)
        {
            turnInfoText.text = "You lose!";
        }

        GameManager.Instance.playerCurrentHealth = playerStats.currentHealth;
    }

    public void OnAttackButton()
    {
        if (state != CombatState.PLAYERTURN)
        {
            return;
        }

        StartCoroutine(PlayerAttack());
    }

    IEnumerator PlayerAttack()
    {
        turnInfoText.text = "You attack!";
        bool isEnemyDead = enemyStats.TakeDamage(playerStats.damage);
        enemyHealthText.text = $"Health: {enemyStats.currentHealth}/{enemyStats.maxHealth}";
        playerStats.ModifySap(1);
        playerSapText.text = $"Sap: {playerStats.currentSap}/{playerStats.maxSap}";

        yield return new WaitForSeconds(1f);
        if (isEnemyDead)
        {
            state = CombatState.WIN;
            EndBattle();
        }
        else
        {
            state = CombatState.ENEMYTURN;
            StartCoroutine(EnemyTurn());
        }
    }

    public void OnDefendButton()
    {
        if (state != CombatState.PLAYERTURN)
        {
            return;
        }

        playerStats.isDefending = true;
        turnInfoText.text = "You are defending!";

        state = CombatState.ENEMYTURN;
        StartCoroutine(EnemyTurn());
    }

    public void OnSkillRootBindButton()
    {
        if (state != CombatState.PLAYERTURN)
        {
            return;
        }

        int skillCost = 2;
        if (playerStats.currentSap < skillCost)
        {
            turnInfoText.text = "Not enough sap!";
            return;
        }

        playerStats.ModifySap(-skillCost);
        playerSapText.text = $"Sap: {playerStats.currentSap}/{playerStats.maxSap}";
        turnInfoText.text = "You use Root Bind!";
        bool isEnemyDead = enemyStats.TakeDamage(Mathf.RoundToInt(playerStats.damage * 1.5f));
        enemyHealthText.text = $"Health: {enemyStats.currentHealth}/{enemyStats.maxHealth}";
        enemyStats.isStunned = true;

        if (isEnemyDead)
        {
            state = CombatState.WIN;
            EndBattle();
        }
        else
        {
            state = CombatState.ENEMYTURN;
            StartCoroutine(EnemyTurn());
        }
    }
}
