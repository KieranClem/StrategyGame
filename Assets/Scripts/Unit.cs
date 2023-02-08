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
    public GameObject AOE;

    void TakeDamage(float DamageTaken)
    {
        UnitHealth -= DamageTaken;
        if(UnitHealth <= 0)
        {
            DestroySelf();
        }
    }

    void Heal(float HealAmount)
    {
        UnitHealth += HealAmount;
        if(UnitHealth > UnitMaxHealth)
        {
            UnitHealth = UnitMaxHealth;
        }
    }

    void DestroySelf()
    {

    }
}
