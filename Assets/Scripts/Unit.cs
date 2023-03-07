using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  enum UnitType
{
    PlayerUnit,
    EnemyUnit,
    NeutralUnit
}

[CreateAssetMenu(fileName = "New Unit", menuName = "Asset/Unit")]

public class Unit : ScriptableObject
{
    public string UnitName;
    public float UnitMaxHealth;
    public float UnitHealth;
    public float UnitMovement;
    public float UnitAttack;
    public float UnitDefense;
    public float UnitEvasiveness;
    public float UnitRange;
    public float AOESize;
    public GameObject AOE;
    public GameObject MovementCircle;

    private void Awake()
    {
        UnitHealth = UnitMaxHealth;
    }

    public float TakeDamage(float DamageTaken, float UnitCurrentHealth)
    {
        UnitCurrentHealth -= DamageTaken;
        return UnitCurrentHealth;
    }

    public void Heal(float HealAmount)
    {
        UnitHealth += HealAmount;
        if(UnitHealth > UnitMaxHealth)
        {
            UnitHealth = UnitMaxHealth;
        }
    }
}
