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
    List<GameObject> UnitsInRange = new List<GameObject>();
    private AudioSource audioSource;
    public GameObject explosion;
    public GameObject smoke;
    public GameObject DamageText;

    // Start is called before the first frame update
    void Start()
    {
        startpos = this.transform.position;
        playerInputActions = new PlayerInputActions();
        playerInputActions.AimControls.Enable();
        rigidbody = GetComponent<Rigidbody>();
        audioSource = this.GetComponent<AudioSource>();
    }

    private void FixedUpdate()
    {
        if (CurrentState == State.BeingControlled)
        {
            float dist = Mathf.Floor(Vector3.Distance(startpos, transform.position));

            Vector2 inputVector = playerInputActions.AimControls.Movement.ReadValue<Vector2>();
            Vector2 rotationVector = playerInputActions.AimControls.RotateAOE.ReadValue<Vector2>();

            //pushes aoe back if they leave range
            if (dist > UnitAiming.UnitClass.UnitRange)
            {
                rigidbody.AddForce(-rigidbody.velocity * (Speed * 10f));
                rigidbody.velocity = Vector3.zero;
            }
            else
            {
                rigidbody.AddForce(new Vector3(inputVector.x, 0, inputVector.y) * Speed, ForceMode.Force);
            }

            //rotate AOE
            if (rotationVector.x > 0 || rotationVector.x < 0)
            {
                transform.Rotate(Vector3.up * rotationVector.x * 100f * Time.deltaTime);
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
            UnitsInRange.Add(other.gameObject);
        }

        if(other.tag == "EnemyUnit")
        {
            UnitsInRange.Add(other.gameObject);
        }

        if(other.tag == "ControllableHealer")
        {
            UnitsInRange.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "ControllableUnit")
        {
            UnitsInRange.Remove(other.gameObject);
        }

        if (other.tag == "EnemyUnit")
        {
            UnitsInRange.Remove(other.gameObject);
        }

        if (other.tag == "ControllableHealer")
        {
            UnitsInRange.Remove(other.gameObject);
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
            if (!UnitAiming.gameObject.CompareTag("ControllableHealer"))
            {
                if (UnitsInRange.Count > 0)
                {
                    foreach (GameObject unit in UnitsInRange)
                    {
                        bool Hitself = false;
                        if (unit == UnitAiming.gameObject)
                            Hitself = true;

                        if (unit.CompareTag("ControllableUnit") || unit.CompareTag("ControllableHealer"))
                        {
                            unit.GetComponent<ControllableUnit>().TakeDamage(UnitAiming.UnitClass.UnitAttack, Hitself);
                        }
                        else if (unit.CompareTag("EnemyUnit"))
                        {
                            unit.GetComponent<EnemyUnitAI>().TakeDamage(UnitAiming.UnitClass.UnitAttack, Hitself);
                        }

                        Vector3 PopUpSpawnPoint = new Vector3(unit.transform.position.x, unit.transform.position.y + 2, unit.transform.position.z);
                        GameObject TextPop = Instantiate(DamageText, PopUpSpawnPoint, Quaternion.identity);
                        TextPop.GetComponent<DamagePopUp>().SetUp(UnitAiming.UnitClass.UnitAttack);
                    }

                    StartCoroutine(ExplodeToEndOfTurn());
                }
            }
            else
            {
                if (UnitsInRange.Count > 0)
                {
                    
                    
                    UnitHealers healer = UnitAiming.GetComponent<ControllableHealers>().heal;
                    
                    foreach (GameObject unit in UnitsInRange)
                    {
                        if (unit.CompareTag("ControllableUnit") || unit.CompareTag("ControllableHealer"))
                        {
                            unit.GetComponent<ControllableUnit>().HealDamage(healer.HealAmount);
                        }
                        else if (unit.CompareTag("EnemyUnit"))
                        {
                            unit.GetComponent<EnemyUnitAI>().HealDamage(healer.HealAmount);
                        }

                        Vector3 PopUpSpawnPoint = new Vector3(unit.transform.position.x, unit.transform.position.y + 2, unit.transform.position.z);
                        GameObject TextPop = Instantiate(DamageText, PopUpSpawnPoint, Quaternion.identity);
                        TextPop.GetComponent<DamagePopUp>().SetUp(healer.HealAmount);
                    }

                    StartCoroutine(ExplodeToEndOfTurn());
                } 
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

    private IEnumerator ExplodeToEndOfTurn()
    {
        GameObject temp = Instantiate(explosion, this.transform.position, Quaternion.identity);
        audioSource.Play();
        CurrentState = State.NotBeingControlled;
        playerInputActions.AimControls.Disable();
        yield return new WaitForSeconds(explosion.GetComponent<ParticleSystem>().main.duration / 2);
        GameObject tempSmoke = Instantiate(smoke, new Vector3(this.transform.position.x, this.transform.position.y + 1, this.transform.position.z), Quaternion.identity);
        yield return new WaitForSeconds(explosion.GetComponent<ParticleSystem>().main.duration / 2);
        Destroy(temp);
        tempSmoke.GetComponent<DestroySmoke>().StartDestruction(smoke.GetComponent<ParticleSystem>().main.duration);
        StartCoroutine(ExitAOE(true));
    }

    private IEnumerator ExitAOE(bool TurnEnded)
    {
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
            foreach (GameObject unit in UnitsInRange)
            {
                bool Hitself = false;
                if (unit == enemyInControl.gameObject)
                    Hitself = true;

                if (unit.CompareTag("ControllableUnit") || unit.CompareTag("ControllableHealer"))
                {
                    unit.GetComponent<ControllableUnit>().TakeDamage(enemyInControl.EnemyClass.UnitAttack, Hitself);
                }
                else if (unit.CompareTag("EnemyUnit"))
                {
                    unit.GetComponent<EnemyUnitAI>().TakeDamage(enemyInControl.EnemyClass.UnitAttack, Hitself);
                }

                Vector3 PopUpSpawnPoint = new Vector3(unit.transform.position.x, unit.transform.position.y + 2, unit.transform.position.z);
                GameObject TextPop = Instantiate(DamageText, PopUpSpawnPoint, Quaternion.identity);
                TextPop.GetComponent<DamagePopUp>().SetUp(enemyInControl.EnemyClass.UnitAttack);

            }

            Instantiate(explosion, this.transform.position, Quaternion.identity);
            audioSource.Play();
            yield return new WaitForSeconds(explosion.GetComponent<ParticleSystem>().main.duration);

            enemyInControl.NotifOfAttackFinishing();
            yield return new WaitForSeconds(0.01f);
            Destroy(this.gameObject);

        }
    }
}
