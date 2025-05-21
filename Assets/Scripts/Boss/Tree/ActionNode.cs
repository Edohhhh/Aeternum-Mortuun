using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Nodo que ejecuta una acci�n espec�fica en el �rbol de decisiones

public class ActionNode : IDesitionNode
{
    // Guarda la acci�n que se quiere ejecutar
    private Action action;

    // Constructor: recibe la acci�n que este nodo va a ejecutar
    public ActionNode(Action action)
    {
        this.action = action;
    }

    // Ejecuta la acci�n
    public void Execute()
    {
        action();
    }
}
