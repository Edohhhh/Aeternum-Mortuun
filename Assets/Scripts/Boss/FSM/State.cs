using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State<T>
{
    protected FSM<T> _fsm; 
    protected Dictionary<T, State<T>> transitions = new Dictionary<T, State<T>>(); 

    
    public virtual void Awake()
    {
        Debug.Log("Awake state: " + this.GetType().Name);
    }

    public virtual void Execute() { }

    public virtual void FixedExecute() { }

    public virtual void Sleep() { }

    
    public void AddTransition(T input, State<T> state)
    {
        transitions[input] = state;
    }

    
    public void RemoveTransition(T input)
    {
        if (transitions.ContainsKey(input))
            transitions.Remove(input);
    }

   
    public void RemoveTransition(State<T> state)
    {
        foreach (var pair in transitions)
        {
            if (pair.Value == state)
            {
                transitions.Remove(pair.Key);
                break;
            }
        }
    }

    
    public State<T> GetTransition(T input)
    {
        if (transitions.TryGetValue(input, out State<T> nextState))
            return nextState;
        return null; 
    }
}
