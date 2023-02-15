using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyUnitAI : MonoBehaviour
{
    public State currentState = State.Waiting;
    public bool hasMoved = false;
    public Unit EnemyClass;
    //The speed in which the unit moves around the arena, not it's anything considering in regards to combat of how far the enemy can move
    public float UnitSpeed = 5f;
    public float AOESpeed = 5f;
    private Transform Target = null;
    private NavMeshAgent nav;
    private Vector3 startpos;
    private TurnManager turnManager;
    private bool Aiming = false;
    private GameObject AOE;

    // Start is called before the first frame update
    void Start()
    {
        nav = GetComponent<NavMeshAgent>();
        nav.isStopped = true;
        turnManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<TurnManager>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(currentState == State.BeingControlled && !Aiming)
        {
            float dist = Mathf.Floor(Vector3.Distance(startpos, transform.position));
            if(dist >= EnemyClass.UnitMovement)
            {
                EndTurn();
            }
            else if(Vector3.Distance(Target.position, this.transform.position) < EnemyClass.UnitRange)
            {
                AimAttack();
                nav.isStopped = true;
                Debug.Log("here");
            }
        }
        else if(Aiming)
        {
            float step = AOESpeed * Time.deltaTime;
            AOE.transform.position = Vector3.MoveTowards(AOE.transform.position, Target.position, step);
            if(Vector3.Distance(AOE.transform.position, Target.position) < 0.01f)
            {
                StartCoroutine(AOE.GetComponent<AOEMovement>().EnemyAttack());
                currentState = State.NotBeingControlled;
                Aiming = false;
            }
        }
    }

    public void NotifOfAttackFinishing()
    {
        EndTurn();
    }

    void AimAttack()
    {
        AOE = Instantiate(EnemyClass.AOE, this.transform.position, Quaternion.identity);
        AOE.GetComponent<AOEMovement>().EnemyTurn(this);
        Aiming = true;
    }

    public void ActivateMovement()
    {
        Collider[] colliders = Physics.OverlapSphere(this.transform.position, EnemyClass.UnitMovement + EnemyClass.UnitRange);
        Transform clostestPlayerUnit = null;
        float Distance = 0f;
        for(int i = 0; i < colliders.Length; i++)
        {
            if(colliders[i].tag == "ControllableUnit")
            {
                float tempDist = Vector3.Distance(transform.position, colliders[i].transform.position);
                if(clostestPlayerUnit == null || tempDist < Distance)
                {
                    Distance = tempDist;
                    clostestPlayerUnit = colliders[i].GetComponent<Transform>();
                }
            }
        }

        startpos = this.transform.position;
        Target = clostestPlayerUnit;
        nav.SetDestination(Target.position);
        nav.isStopped = false;
        currentState = State.BeingControlled;
    }

    private void EndTurn()
    {
        currentState = State.Waiting;
        nav.isStopped = true;
        turnManager.AddWaitingEnemyUnit();
    }
}
