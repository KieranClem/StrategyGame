using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum State
{
    BeingControlled,
    NotBeingControlled,
    Waiting
}

public class ControllableUnit : MonoBehaviour
{
    public Unit UnitClass;
    [HideInInspector] public State CurrentState = State.NotBeingControlled;
    [HideInInspector] public Rigidbody rigidbody;
    [HideInInspector] public PlayerInputActions playerInputActions;
    public float Speed = 10f;
    [HideInInspector] public Vector3 startpos;
    [HideInInspector] public GameObject Cursor;
    [HideInInspector] public TurnManager turnManager;
    [HideInInspector] public GameObject SpawnedRangeIndicator;
    [HideInInspector] public float UnitHealth;
  


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

    private void FixedUpdate()
    {
        if (CurrentState == State.BeingControlled)
        {
            float dist = Vector3.Distance(startpos, transform.position);
            Vector2 inputVector = playerInputActions.UnitControls.Movement.ReadValue<Vector2>();

            //pushes unit back if it's moved past it's movement range
            if (dist > UnitClass.UnitMovement)
            {
                rigidbody.AddForce(-rigidbody.velocity * Speed);
                rigidbody.velocity = Vector3.zero;
            }
            else
            {
                rigidbody.AddForce(new Vector3(inputVector.x, 0, inputVector.y) * Speed, ForceMode.Force);
            }

            //max speed for unit moving around
            if (rigidbody.velocity.magnitude > Speed)
            {
                rigidbody.velocity = rigidbody.velocity.normalized * Speed;
            }

            //stops unit from moving forward more when out of unit range
            if (dist > UnitClass.UnitMovement)
            {
                rigidbody.velocity = Vector3.zero;
            }

            if (inputVector.x == 0 && inputVector.y == 0)
            {
                rigidbody.velocity = Vector3.zero;
            }
        }
    }

    public IEnumerator SwitchToCurrentUnit()
    {
        yield return new WaitForSeconds(0.1f);
        CurrentState = State.BeingControlled;
        playerInputActions.UnitControls.Enable();

    }

    public void ShowMovementRange()
    {
        Vector3 RangeSpawnLocation = new Vector3(this.transform.position.x, this.transform.position.y - 0.5f, this.transform.position.z);
        GameObject range = Instantiate(UnitClass.MovementCircle, RangeSpawnLocation, Quaternion.identity);
        SpawnedRangeIndicator = range;
        SpawnedRangeIndicator.transform.localScale = new Vector3(UnitClass.UnitMovement * 2, SpawnedRangeIndicator.transform.localScale.y, UnitClass.UnitMovement * 2);
    }

    public void StopShowingMovementRange()
    {
        if (SpawnedRangeIndicator)
        {
            Destroy(SpawnedRangeIndicator);
        }
    }

    public void BackToCursor(InputAction.CallbackContext context)
    {
        if(context.performed && CurrentState == State.BeingControlled)
        {
            ReturnCursorControls();
        }    
    }

    public void Wait(InputAction.CallbackContext context)
    {
        if (context.performed && CurrentState == State.BeingControlled)
        {
            
            StartCoroutine(ReturnUnitControl(true));
        }
    }

    public void Aiming(InputAction.CallbackContext context)
    {
        if(context.performed && CurrentState == State.BeingControlled)
        {
            GameObject aoe = Instantiate(UnitClass.AOE, this.transform.position, Quaternion.identity);
            CurrentState = State.NotBeingControlled;
            rigidbody.velocity = Vector3.zero;
            playerInputActions.UnitControls.Disable();
            aoe.transform.localScale = new Vector3(UnitClass.AOESize * 2, aoe.transform.localScale.y, UnitClass.AOESize * 2);
            aoe.GetComponent<AOEMovement>().SwitchToAOE(GetComponent<ControllableUnit>());

        }
    }

    public IEnumerator ReturnUnitControl(bool TurnEnded)
    {
        yield return new WaitForSeconds(0.1f);
        //Checks if the unit has finished it's turn or not
        if(!TurnEnded)
        {
            CurrentState = State.BeingControlled;
            playerInputActions.UnitControls.Enable();
        }
        else
        {
            CurrentState = State.Waiting;
            playerInputActions.UnitControls.Disable();
            turnManager.AddWaitingPlayerUnit();
            ReturnCursorControls();
        }
              
    }

    public void ReturnCursorControls()
    {
        if (CurrentState != State.Waiting)
        {
            CurrentState = State.NotBeingControlled;
            //if the player doesn't wait the unit will return to start pos
            transform.position = startpos;
        }
        StopShowingMovementRange();
        playerInputActions.UnitControls.Disable();
        StartCoroutine(Cursor.GetComponent<CursorControls>().ReturnCursorControl());
    }

    public void StartOfNewTurn()
    {
        startpos = this.transform.position;
        CurrentState = State.NotBeingControlled;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "InvisableWalls")
        {
            rigidbody.AddForce(-rigidbody.velocity * (Speed * 2f));
            rigidbody.velocity = Vector3.zero;
        }
    }

    public void TakeDamage(float DamageDelt, bool hitBySelf)
    {
        UnitHealth = UnitClass.TakeDamage(DamageDelt, UnitHealth);
        if(UnitHealth <= 0)
        {
            if(hitBySelf)
            {
                //prevents a unit from killing themself
                UnitHealth = 1;
            }
            else
            {
                //Death state here
                DestroySelf();
            }

        }
    }

    public void HealDamage(float DamageHeal)
    {
        UnitHealth = UnitClass.Heal(DamageHeal, UnitHealth);
    }

    private void DestroySelf()
    {
        turnManager.RemoveControllableUnit(this);
        if(CurrentState == State.Waiting)
        {
            turnManager.CheckIfDestoryedUnitIsPartOfTurn(this.gameObject);
        }
        Destroy(gameObject);
    }
}
