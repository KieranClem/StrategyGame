using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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

    //UI elements 
    private Text DisplayHP;
    private Text DisplayAttack;
    

    void Start()
    {
        state = State.BeingControlled;
        rigidbody = GetComponent<Rigidbody>();
        //playerInput = GetComponent<PlayerInput>();

        playerInputActions = new PlayerInputActions();
        playerInputActions.CursorControls.Enable();
        turnManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<TurnManager>();

        DisplayHP = GameObject.FindGameObjectWithTag("HPTextbox").GetComponent<Text>();
        DisplayAttack = GameObject.FindGameObjectWithTag("AttackTextBox").GetComponent<Text>();

        HideUI();
    }

    private void Update()
    {
        if (state == State.NotBeingControlled && CurrentlyControlledUnit != null)
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
        switch(other.tag)
        {
            case "ControllableUnit":
                controllableUnit = other.GetComponent<ControllableUnit>();
                controllableUnit.ShowMovementRange();
                ShowUI(other.gameObject);
                break;
            case "EnemyUnit":
                other.GetComponent<EnemyUnitAI>().ShowMovementRange();
                ShowUI(other.gameObject);
                break;
            case "InvisableWalls":
                rigidbody.AddForce(-rigidbody.velocity * (Speed * 2f));
                rigidbody.velocity = Vector3.zero;
                break;
            case "ControllableHealer":
                controllableUnit = other.GetComponent<ControllableHealers>();
                controllableUnit.ShowMovementRange();
                ShowUI(other.gameObject);
                break;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        switch (other.tag)
        {
            case "ControllableUnit":
                controllableUnit.StopShowingMovementRange();
                controllableUnit = null;
                HideUI();
                break;
            case "EnemyUnit":
                other.GetComponent<EnemyUnitAI>().StopShowingMovementRange();
                HideUI();
                break;
            case "ControllableHealer":
                controllableUnit.StopShowingMovementRange();
                controllableUnit = null;
                HideUI();
                break;
        }
    }

    private void ShowUI(GameObject UnitToShow)
    {
        DisplayAttack.gameObject.SetActive(true);
        DisplayHP.gameObject.SetActive(true);
        if (controllableUnit)
        {
            if (controllableUnit.tag == "ControllableUnit")
            {
                DisplayHP.text = "HP: " + controllableUnit.UnitHealth;
                DisplayAttack.text = "Attack: " + controllableUnit.UnitClass.UnitAttack;
            }
            else
            {
                DisplayHP.text = "HP: " + controllableUnit.UnitHealth;
                DisplayAttack.text = "Heal Amount: " + controllableUnit.GetComponent<ControllableHealers>().heal.HealAmount;
            }
        }
        else
        {
            EnemyUnitAI enemyUnit = UnitToShow.GetComponent<EnemyUnitAI>();
            DisplayHP.text = "HP: " + enemyUnit.EnemyHealth;
            DisplayAttack.text = "Attack: " + enemyUnit.EnemyClass.UnitAttack;
        }

    }

    private void HideUI()
    {
        DisplayAttack.gameObject.SetActive(false);
        DisplayHP.gameObject.SetActive(false);
    }

    public void TakeControlOfUnit(InputAction.CallbackContext context)
    {

        if(context.performed)
        {
            if(controllableUnit && (state == State.BeingControlled) && (controllableUnit.CurrentState != State.Waiting))
            {
                state = State.NotBeingControlled;
                //playerInputActions.CursorControls.Disable();
                CurrentlyControlledUnit = controllableUnit.gameObject;
                HideUI();
                StartCoroutine(controllableUnit.SwitchToCurrentUnit());
                GetComponent<AudioSource>().Play();
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
