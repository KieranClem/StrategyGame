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


    // Start is called before the first frame update
    void Start()
    {
        startpos = this.transform.position;
        playerInputActions = new PlayerInputActions();
        rigidbody = GetComponent<Rigidbody>();
        playerInputActions.UnitControls.Disable();
        Cursor = GameObject.FindGameObjectWithTag("Cursor");
        UnitClass.UnitHealth = UnitClass.UnitMaxHealth;
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
            ReturnCursorControls();
        }
              
    }

    private void ReturnCursorControls()
    {
        CurrentState = State.NotBeingControlled;
        playerInputActions.UnitControls.Disable();
        StartCoroutine(Cursor.GetComponent<CursorControls>().ReturnCursorControl());
    }
}
