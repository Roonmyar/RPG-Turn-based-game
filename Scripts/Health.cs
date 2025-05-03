using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHealth = 100;
    private int _currentHealth;

    void Start()
    {
        _currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;
        Debug.Log(gameObject.name + " получил " + damage + " урона. Осталось: " + _currentHealth);

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " уничтожен!");
        Destroy(gameObject); // Или анимация смерти
    }
}