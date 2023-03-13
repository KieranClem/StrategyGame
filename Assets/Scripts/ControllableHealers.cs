using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControllableHealers : ControllableUnit
{
    // Start is called before the first frame update
    void Start()
    {
        startpos = this.transform.position;
        playerInputActions = new PlayerInputActions();
        rigidbody = GetComponent<Rigidbody>();
        playerInputActions.UnitControls.Disable();
        Cursor = GameObject.FindGameObjectWithTag("Cursor");
        UnitClass.UnitHealth = UnitClass.UnitMaxHealth;
        turnManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<TurnManager>();
        UnitHealth = UnitClass.UnitMaxHealth;
    }

    public new void BackToCursor(InputAction.CallbackContext context)
    {
        if (context.performed && CurrentState == State.BeingControlled)
        {
            ReturnCursorControls();
        }
    }

    public new void Wait(InputAction.CallbackContext context)
    {
        if (context.performed && CurrentState == State.BeingControlled)
        {

            StartCoroutine(ReturnUnitControl(true));
        }
    }

    public new void Aiming(InputAction.CallbackContext context)
    {
        if (context.performed && CurrentState == State.BeingControlled)
        {
            GameObject aoe = Instantiate(UnitClass.AOE, this.transform.position, Quaternion.identity);
            CurrentState = State.NotBeingControlled;
            rigidbody.velocity = Vector3.zero;
            playerInputActions.UnitControls.Disable();
            aoe.transform.localScale = new Vector3(UnitClass.AOESize * 2, aoe.transform.localScale.y, UnitClass.AOESize * 2);
            aoe.GetComponent<AOEMovement>().SwitchToAOE(GetComponent<ControllableUnit>());

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
