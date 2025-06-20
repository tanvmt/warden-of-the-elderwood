using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [Header("Base Stats")]
    public int maxHealth = 100;
    public int currentHealth;
    public int damage = 10;
    public int maxSap = 10;
    public int currentSap;

    [Header("States")]
    public bool isDefending = false;
    public bool isStunned = false;

    void Awake()
    {
        if(!gameObject.CompareTag("Player"))
        {
            currentHealth = maxHealth;
        }
    }

    public void InitializeFromManager()
    {
        if (gameObject.CompareTag("Player"))
        {
            maxHealth = GameManager.Instance.playerMaxHealth;
            currentHealth = GameManager.Instance.playerCurrentHealth;
        }
    }

    public bool TakeDamage(int damageAmount)
    {
        if (isDefending)
        {
            damageAmount = Mathf.RoundToInt(damageAmount / 2f);
        }

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
            return true;
        }
        else
        {
            return false;
        }
    }

    public void Heal(int healAmount)
    {
        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    public void ModifySap(int sapAmount)
    {
        currentSap += sapAmount;
        currentSap = Mathf.Clamp(currentSap, 0, maxSap);
    }

    public void Die()
    {
        Debug.Log($"{gameObject.name} has died.");
        gameObject.SetActive(false);
    }
}
