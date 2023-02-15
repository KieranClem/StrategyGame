using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AOEMovement : MonoBehaviour
{
    [HideInInspector] public State CurrentState = State.NotBeingControlled;
    private Rigidbody rigidbody;
    private PlayerInputActions playerInputActions;
    public float Speed = 10f;
    private Vector3 startpos;
    private ControllableUnit UnitAiming;
    private EnemyUnitAI enemyInControl;
    List<Unit> UnitsInRange = new List<Unit>();


    // Start is called before the first frame update
    void Start()
    {
        startpos = this.transform.position;
        playerInputActions = new PlayerInputActions();
        playerInputActions.AimControls.Enable();
        rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (CurrentState == State.BeingControlled)
        {
            float dist = Mathf.Floor(Vector3.Distance(startpos, transform.position));

            Vector2 inputVector = playerInputActions.AimControls.Movement.ReadValue<Vector2>();
            if (dist > UnitAiming.UnitClass.UnitRange)
            {
                rigidbody.AddForce(-rigidbody.velocity * (Speed * 10f));
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

            if (dist > UnitAiming.UnitClass.UnitRange)
            {
                rigidbody.velocity = Vector3.zero;
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
            UnitsInRange.Add(other.GetComponent<ControllableUnit>().UnitClass);
        }

        if(other.tag == "EnemyUnit")
        {
            UnitsInRange.Add(other.GetComponent<EnemyUnitAI>().EnemyClass);
        }
    }

    public void SwitchToAOE(ControllableUnit OriginalUnit)
    {
        CurrentState = State.BeingControlled;
        UnitAiming = OriginalUnit;
    }

    public void Fire(InputAction.CallbackContext context)
    {
        if (context.performed && CurrentState == State.BeingControlled)
        {
            if (UnitsInRange.Count > 0)
            {
                foreach (Unit unit in UnitsInRange)
                {
                    unit.TakeDamage(UnitAiming.UnitClass.UnitAttack);
                }

                StartCoroutine(ExitAOE(true));
            }
        }
        
    }

    public void BackToUnit(InputAction.CallbackContext context)
    {
        if (context.performed && CurrentState == State.BeingControlled)
        {
            StartCoroutine(ExitAOE(false));
        }
    }

    private IEnumerator ExitAOE(bool TurnEnded)
    {
        CurrentState = State.NotBeingControlled;
        playerInputActions.AimControls.Disable();
        StartCoroutine(UnitAiming.ReturnUnitControl(TurnEnded));
        yield return new WaitForSeconds(0.1f);
        Destroy(this.gameObject);
    }

    public void EnemyTurn(EnemyUnitAI enemy)
    {
        enemyInControl = enemy;
        CurrentState = State.Waiting;
    }

    public IEnumerator EnemyAttack()
    {
        if (UnitsInRange.Count > 0)
        {
            foreach (Unit unit in UnitsInRange)
            {
                unit.TakeDamage(enemyInControl.EnemyClass.UnitAttack);
            }

            enemyInControl.NotifOfAttackFinishing();
            yield return new WaitForSeconds(0.01f);
            Destroy(this.gameObject);

        }
    }
}
