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
    private int WaitingPlayerUnits;
    private int WaitingEnemyUnits;
    public Text DisplayTurn;
    private CurrentTurn turn = CurrentTurn.PlayerTurn;
    
    // Start is called before the first frame update
    void Start()
    {
        foreach(GameObject PUnits in GameObject.FindGameObjectsWithTag("PlayerUnits"))
        {
            PlayerUnits.Add(PUnits);
        }

        foreach(GameObject EUnits in GameObject.FindGameObjectsWithTag("EnemyUnits"))
        {
            EnemyUnits.Add(EUnits);
        }

        DisplayTurn.text = "Player Turn";
        WaitingPlayerUnits = 0;
        WaitingEnemyUnits = 0;

        turn = CurrentTurn.PlayerTurn;
    }

    private void ChangeCurrentTurn()
    {
        if(turn == CurrentTurn.PlayerTurn)
        {
            turn = CurrentTurn.EnemyTurn;
            DisplayTurn.text = "Enemy Turn";
            WaitingEnemyUnits = 0;
        }
        else if(turn == CurrentTurn.EnemyTurn)
        {
            turn = CurrentTurn.PlayerTurn;
            DisplayTurn.text = "Player Turn";
            WaitingPlayerUnits = 0;
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
    }
}
