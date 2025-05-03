using UnityEngine;
using System.Collections.Generic;

public class MolotovCocktail : MonoBehaviour
{
    public float radius = 3f;
    public int damagePerTurn = 10;
    public int turnsToLive = 2;
    private int turnsRemaining;
    private List<CharacterController> affectedCharacters = new List<CharacterController>();

    void Start()
    {
        turnsRemaining = turnsToLive;
        Debug.Log("Коктейль Молотова активирован на " + turnsToLive + " хода");
        TurnManager.Instance.RegisterMolotov(this);
    }

    public void OnTurnStart()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
        affectedCharacters.Clear();

        foreach (Collider col in colliders)
        {
            CharacterController character = col.GetComponent<CharacterController>();
            if (character != null && !affectedCharacters.Contains(character))
            {
                affectedCharacters.Add(character);
                character.TakeDamage(damagePerTurn); // Используем новую механику урона
                Debug.Log($"{character.gameObject.name} получил {damagePerTurn} урона от коктейля Молотова");
            }
        }

        turnsRemaining--;
        Debug.Log($"Коктейлю Молотова осталось {turnsRemaining} ходов");

        if (turnsRemaining <= 0)
        {
            DestroyMolotov();
        }
    }

    private void DestroyMolotov()
    {
        Debug.Log("Коктейль Молотова исчерпал время действия и уничтожен");
        TurnManager.Instance.UnregisterMolotov(this);
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}