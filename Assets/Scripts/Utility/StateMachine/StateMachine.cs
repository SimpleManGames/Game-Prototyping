using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// StateMachine
/// IState acts as slots for the StateMachine
/// </summary>
[Serializable]
public class StateMachine
{
    [Header("State Machine")]
    public bool debugStateChanges = false;

    protected IState _previousState;
    public IState PreviousState
    {
        get { return _previousState; }
    }

    protected IState _currentState;
    /// <summary>
    /// Set this in order to change the state
    /// The setter will handle exiting the previous state
    /// And will also state the new state
    /// </summary>
    public IState CurrentState
    {
        get { return _currentState; }
        set
        {
            // Avoid setting the to the same state
            if (_currentState == value)
                return;

            _previousState = _currentState;

            // Excute the Exit function of the state we are leaving
            _currentState?.Exit();

            if (debugStateChanges)
                Debug.Log(_currentState?.GetType().ToString() + " => " + value?.GetType().ToString());

            _currentState = value;

            // Excute the State function of the state we are entering
            _currentState.Start();
        }
    }

    public List<IState> states = new List<IState>();

    public StateMachine()
    {

    }
    public StateMachine(IState startState)
    {
        CurrentState = startState;
    }

    public void AddState(IState state)
    {
        states.Add(state);
    }
    public void Update()
    {
        foreach (IState state in states)
            if (state.StateConditional())
                CurrentState = state;
    }

    // Cleans up the CurrentState
    ~StateMachine()
    {
        CurrentState?.Exit();
    }
}