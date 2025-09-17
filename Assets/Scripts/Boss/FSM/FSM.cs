using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSM<T>
{
    private State<T> _currentState; 
    Action<T, State<T>, State<T>> onTransition = delegate { }; 

    public FSM() { }

    public FSM(State<T> init)
    {
        SetInit(init); 
    }

    public void SetInit(State<T> init)
    {
        _currentState = init;
        _currentState.Awake(); 
    }

   
    public void Update()
    {
        _currentState.Execute();
    }

    public void FixedUpdate()
    {
        _currentState.FixedExecute();
    }


    public void Transition(T input)
    {
        State<T> newState = _currentState.GetTransition(input); 
        if (newState == null) return; 

        _currentState.Sleep(); 
        onTransition(input, _currentState, newState); 
        _currentState = newState; 
        _currentState.Awake(); 
    }

    
    public State<T> GetCurrentState()
    {
        return _currentState;
    }
}
