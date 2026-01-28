
using TMPro;
using UnityEngine;

public class GameOverState : GameBaseState
{
    public GameObject teskan;
    public override void EnterState(GameStateManager gamestate)
    {
        teskan.SetActive(true);
        Debug.Log("TAMAT");
    }

    public override void UpdateState(GameStateManager gamestate)
    {

    }

    public override void OnEnterState(GameStateManager gamestate)
    {

    }

    public override void ExitState(GameStateManager gamestate)
    {

    }



}
