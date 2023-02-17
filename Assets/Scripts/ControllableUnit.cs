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
    private Rigidbody rigidbody;
    private PlayerInputActions playerInputActions;
    public float Speed = 10f;
    private Vector3 startpos;
    private GameObject Cursor;
    private TurnManager turnManager;
    private GameObject SpawnedRangeIndicator;
    private float UnitHealth;
  


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

    // Update is called once per frame
    void Update()
    {
        if (CurrentState == State.BeingControlled)
        {
            //Movement
        }
    }

    private void FixedUpdate()
    {
        if (CurrentState == State.BeingControlled)
        {
            float dist = Vector3.Distance(startpos, transform.position);

            Vector2 inputVector = playerInputActions.UnitControls.Movement.ReadValue<Vector2>();
            if (dist > UnitClass.UnitMovement)
            {
                rigidbody.AddForce(-rigidbody.velocity * Speed);
                rigidbody.velocity = Vector3.zero;
            }
            else
            {
                rigidbody.AddForce(new Vector3(inputVector.x, 0, inputVector.y) * Speed, ForceMode.Force);
            }


            if (rigidbody.velocity.magnitude > Speed)
            {
                rigidbody.velocity = rigidbody.velocity.normalized * Speed;
            }

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
        GameObject range = Instantiate(UnitClass.MovementCircle, this.transform.position, Quaternion.identity);
        SpawnedRangeIndicator = range;
        SpawnedRangeIndicator.transform.localScale = new Vector3(UnitClass.UnitMovement * 2, SpawnedRangeIndicator.transform.localScale.y, UnitClass.UnitMovement * 2);
    }

    public void BackToCursor(InputAction.CallbackContext context)
    {
        if(context.performed && CurrentState == State.BeingControlled)
        {
            ReturnCursorControls();
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
            aoe.GetComponent<AOEMovement>().SwitchToAOE(GetComponent<ControllableUnit>());
            
        }
    }

    public IEnumerator ReturnUnitControl(bool TurnEnded)
    {
        yield return new WaitForSeconds(0.1f);
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

    private void ReturnCursorControls()
    {
        if (CurrentState != State.Waiting)
        {
            CurrentState = State.NotBeingControlled;
        }
        Destroy(SpawnedRangeIndicator);
        playerInputActions.UnitControls.Disable();
        StartCoroutine(Cursor.GetComponent<CursorControls>().ReturnCursorControl());
    }

    public void StartOfNewTurn()
    {
        startpos = this.transform.position;
        CurrentState = State.NotBeingControlled;
    }

    public void TakeDamage(float DamageDelt, bool hitBySelf)
    {
        UnitHealth = UnitClass.TakeDamage(DamageDelt, UnitHealth);
        if(UnitHealth <= 0)
        {
            if(hitBySelf)
            {
                UnitHealth = 1;
            }
            else
            {
                //Death state here
                DestroySelf();
            }

        }
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
