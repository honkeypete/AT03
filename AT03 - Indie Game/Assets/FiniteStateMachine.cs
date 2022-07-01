using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FiniteStateMachine : MonoBehaviour
{
    protected IState entryState;

    public IState currentState { get; private set; }

    protected virtual void Awake()
    {

    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        if (entryState != null)
        {
            SetState(entryState);
        }
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (currentState != null)
        {
            currentState.OnStateUpdate();
        }
    }

    public void SetState(IState state)
    {
        if (currentState != null)
        {
            currentState.OnStateExit();
            //Debug.Log($"Leaving {currentState.GetType()}");
        }
        //Debug.Log($"Entering {state.GetType()}");
        currentState = state;
        currentState.OnStateEnter();
    }

    protected virtual void OnDrawGizmos()
    {
        if(currentState != null)
        {       
            currentState.DrawStateGizmos();
        }
    }
}

public interface IState
{
    public void OnStateEnter();
    public void OnStateUpdate();
    public void OnStateExit();
    public void DrawStateGizmos();
}
