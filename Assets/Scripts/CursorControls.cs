using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CursorControls : MonoBehaviour
{
    [HideInInspector]public State state = State.BeingControlled;
    private GameObject CurrentlyControlledUnit;
    private Rigidbody rigidbody;
    //private PlayerInput playerInput;
    private PlayerInputActions playerInputActions;
    public float Speed = 10f;

    private ControllableUnit controllableUnit;
    private TurnManager turnManager;

    void Start()
    {
        state = State.BeingControlled;
        rigidbody = GetComponent<Rigidbody>();
        //playerInput = GetComponent<PlayerInput>();

        playerInputActions = new PlayerInputActions();
        playerInputActions.CursorControls.Enable();
        turnManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<TurnManager>();
    }

    private void Update()
    {
        if (state == State.NotBeingControlled)
        {
            this.transform.position = CurrentlyControlledUnit.transform.position;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (state == State.BeingControlled)
        {

            Vector2 inputVector = playerInputActions.CursorControls.Movement.ReadValue<Vector2>();

            rigidbody.AddForce(new Vector3(inputVector.x, 0, inputVector.y) * Speed, ForceMode.Force);

            if (rigidbody.velocity.magnitude > Speed)
            {
                rigidbody.velocity = rigidbody.velocity.normalized * Speed;
            }

            if (inputVector.x == 0 && inputVector.y == 0)
            {
                rigidbody.velocity = Vector3.zero;
            }

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "ControllableUnit")
        {
            controllableUnit = other.GetComponent<ControllableUnit>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "ControllableUnit")
        {
            controllableUnit = null;
        }
    }

    public void TakeControlOfUnit(InputAction.CallbackContext context)
    {

        if(context.performed)
        {
            if(controllableUnit && (state == State.BeingControlled) && (controllableUnit.CurrentState != State.Waiting))
            {
                state = State.NotBeingControlled;
                playerInputActions.CursorControls.Disable();
                CurrentlyControlledUnit = controllableUnit.gameObject;
                StartCoroutine(controllableUnit.SwitchToCurrentUnit());
            }
        }
        
    }

    public IEnumerator ReturnCursorControl()
    {
        yield return new WaitForSeconds(0.1f);
        playerInputActions.CursorControls.Enable();
        state = State.BeingControlled;
        CurrentlyControlledUnit = null;
    }
}
