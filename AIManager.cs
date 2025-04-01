using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class AIManager : SerializedMonoBehaviour
{
    public AIState startState;
    public List<AIState> states = new List<AIState>();

    private List<AIState> stateBag = new List<AIState>();

    public int numberOfCopiesInBag = 2;

    public AIState GetNextState(EnemyAI ai, bool setFloats = true)
    {
        AIState state = DrawFromBag();
        if (setFloats) {
            ai.tempFloats["LASTSTATE_TIME"] = Time.time + state.stayOnStateTime;
        } 
        return state;
    }

    public AIState DrawFromBag()
    {
        if (stateBag.Count == 0)
        {
            for (int i = 0; i < numberOfCopiesInBag; i++)
            {
                stateBag.AddRange(states);
            }
        }

        AIState state = stateBag[Random.Range(0, stateBag.Count)];
        stateBag.Remove(state);
        return state;
    }

    public AIState GetFirstState(EnemyAI ai)
    {
        return startState;
    }

    public bool ShouldChangeState(EnemyAI ai)
    {
        if (!ai.currentState.ShouldLeaveState(ai))
            return false;

        return !ai.tempFloats.ContainsKey("LASTSTATE_TIME") || ai.tempFloats["LASTSTATE_TIME"] < Time.time;
    }



}
