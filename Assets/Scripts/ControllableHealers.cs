using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllableHealers : ControllableUnit
{
    public UnitHealers heal;

    // Start is called before the first frame update
    void Start()
    {
        UnitClass = heal;
        startpos = this.transform.position;
        playerInputActions = new PlayerInputActions();
        rigidbody = GetComponent<Rigidbody>();
        playerInputActions.UnitControls.Disable();
        Cursor = GameObject.FindGameObjectWithTag("Cursor");
        UnitClass.UnitHealth = UnitClass.UnitMaxHealth;
        turnManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<TurnManager>();
        UnitHealth = UnitClass.UnitMaxHealth;

    }
}
