using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameBaseState : MonoBehaviour
{
    public abstract void EnterState(GameStateManager gamestate);

    public abstract void UpdateState(GameStateManager gamestate);

    public abstract void OnEnterState(GameStateManager gamestate);

    public abstract void ExitState(GameStateManager gamestate);
}
