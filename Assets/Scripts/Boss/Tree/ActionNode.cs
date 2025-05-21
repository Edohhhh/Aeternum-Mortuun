using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Nodo que ejecuta una acción específica en el árbol de decisiones

public class ActionNode : IDesitionNode
{
    // Guarda la acción que se quiere ejecutar
    private Action action;

    // Constructor: recibe la acción que este nodo va a ejecutar
    public ActionNode(Action action)
    {
        this.action = action;
    }

    // Ejecuta la acción
    public void Execute()
    {
        action();
    }
}
