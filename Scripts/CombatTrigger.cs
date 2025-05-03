using UnityEngine;

public class CombatTrigger : MonoBehaviour
{
    private TurnManager turnManager;

    void Start()
    {
        turnManager = FindObjectOfType<TurnManager>();
        if (turnManager == null)
        {
            Debug.LogError("TurnManager не найден!");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && turnManager != null)
        {
            turnManager.StartCombat();
            Debug.Log("Игрок вошел в зону врага, бой начинается!");
        }
    }
}