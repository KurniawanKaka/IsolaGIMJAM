using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{

    GameBaseState currentState;
    public AnalisisState analisisstate = new AnalisisState();
    public CheckState checkstate = new CheckState();
    public GameOverState gameoverstate = new GameOverState();
    public SetUpState setupstate = new SetUpState();

    public EndingState endingstate = new EndingState();
    public WaitingState waitingstate = new WaitingState();
    // Start is called before the first frame update
    void Start()
    {
        currentState = waitingstate;

        currentState.EnterState(this);
    }

    // Update is called once per frame
    void Update()
    {
        currentState.UpdateState(this);
    }

    public void SwitchState(GameBaseState gamestate)
    {
        currentState = gamestate;
        gamestate.EnterState(this);
    }
}
