using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

enum CurrentTurn
{
    PlayerTurn,
    EnemyTurn
}

public class TurnManager : MonoBehaviour
{
    public List<GameObject> PlayerUnits = new List<GameObject>();
    public List<GameObject> EnemyUnits = new List<GameObject>();
    private int WaitingPlayerUnits = 0;
    private int WaitingEnemyUnits = 0;
    public Text DisplayTurn;
    private CurrentTurn turn = CurrentTurn.PlayerTurn;
    private Transform cursorLocation;
    private Transform currentlyMovingEnemy;
    private LevelLoader levelLoader;

    

    // Start is called before the first frame update
    void Start()
    {
        foreach(GameObject PUnits in GameObject.FindGameObjectsWithTag("ControllableUnit"))
        {
            if(!PUnits.GetComponent<ControllableUnit>().DestroySelfAtStart)
            {
                PlayerUnits.Add(PUnits);
            }
        }

        foreach (GameObject PUnits in GameObject.FindGameObjectsWithTag("ControllableHealer"))
        {
            if (!PUnits.GetComponent<ControllableUnit>().DestroySelfAtStart)
            {
                PlayerUnits.Add(PUnits);
            }
        }

        foreach (GameObject EUnits in GameObject.FindGameObjectsWithTag("EnemyUnit"))
        {
            EnemyUnits.Add(EUnits);
        }

        DisplayTurn.text = "Player Turn";
        WaitingPlayerUnits = 0;
        WaitingEnemyUnits = 0;
        cursorLocation = GameObject.FindGameObjectWithTag("Cursor").transform;
        turn = CurrentTurn.PlayerTurn;
        levelLoader = GetComponent<LevelLoader>();
    }

    private void FixedUpdate()
    {
        if(turn == CurrentTurn.EnemyTurn)
        {
            cursorLocation.position = currentlyMovingEnemy.position;
        }
    }

    private void ChangeCurrentTurn()
    {
        if(turn == CurrentTurn.PlayerTurn)
        {
            DisplayTurn.text = "Enemy Turn";
            WaitingEnemyUnits = 0;
            foreach(GameObject enemy in EnemyUnits)
            {
                enemy.GetComponent<EnemyUnitAI>().currentState = State.NotBeingControlled;
            }
            StartCoroutine(EnemyUnits[0].GetComponent<EnemyUnitAI>().ActivateMovement());
            currentlyMovingEnemy = EnemyUnits[0].GetComponent<Transform>();
            turn = CurrentTurn.EnemyTurn;
            cursorLocation.GetComponent<CursorControls>().state = State.Waiting;
        }
        else if(turn == CurrentTurn.EnemyTurn)
        {
            turn = CurrentTurn.PlayerTurn;
            DisplayTurn.text = "Player Turn";
            WaitingPlayerUnits = 0;
            foreach(GameObject playerUnit in PlayerUnits)
            {
                playerUnit.GetComponent<ControllableUnit>().StartOfNewTurn();

            }
            cursorLocation.GetComponent<CursorControls>().state = State.BeingControlled;
        }
    }

    public void AddWaitingPlayerUnit()
    {
        WaitingPlayerUnits += 1;
        if(WaitingPlayerUnits >= PlayerUnits.Count)
        {
            ChangeCurrentTurn();
        }
    }

    public void AddWaitingEnemyUnit()
    {
        WaitingEnemyUnits += 1;
        if(WaitingEnemyUnits >= EnemyUnits.Count)
        {
            ChangeCurrentTurn();
        }
        else
        {
            StartCoroutine(EnemyUnits[WaitingEnemyUnits].GetComponent<EnemyUnitAI>().ActivateMovement());
            currentlyMovingEnemy = EnemyUnits[WaitingEnemyUnits].transform;
        }
    }

    public void RemoveControllableUnit(ControllableUnit controllableUnit)
    {
        PlayerUnits.Remove(controllableUnit.gameObject);
        if(PlayerUnits.Count <= 0)
        {
            //Game Over conditions go here
        }
    }

    public void RemoveEnemyUnit(EnemyUnitAI enemyUnitAI)
    {
        EnemyUnits.Remove(enemyUnitAI.gameObject);
        if(EnemyUnits.Count <= 0)
        {
            levelLoader.LoadEndScene();
        }
    }

    //Checks if a destoryed and waiting unit was a part of the current turn, if it is the waiting variable will lower as to prevent a unit from not being able to attack
    public void CheckIfDestoryedUnitIsPartOfTurn(GameObject DeathRowUnit)
    {
        if(DeathRowUnit.CompareTag("ControllableUnit") && (turn == CurrentTurn.PlayerTurn))
        {
            WaitingPlayerUnits -= 1;
            Debug.Log("-1");
        }
        else if(DeathRowUnit.CompareTag("EnemyUnit") && (turn == CurrentTurn.EnemyTurn))
        {
            WaitingEnemyUnits -= 1;
        }

        Debug.Log(WaitingPlayerUnits);
    }
}
