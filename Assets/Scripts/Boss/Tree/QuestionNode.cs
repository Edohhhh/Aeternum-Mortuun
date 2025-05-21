using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Nodo de decisión que evalúa una condición y ejecuta un nodo basado en el resultado
public class QuestionNode : IDesitionNode
{
    private IDesitionNode trueNode;  // Nodo a ejecutar si la condición es verdadera
    private IDesitionNode falseNode; // Nodo a ejecutar si la condición es falsa
    private Func<bool> question;    // Función que representa la condición a evaluar

    // Constructor que recibe los nodos de decisión y la función de la condición
    public QuestionNode(IDesitionNode trueNode, IDesitionNode falseNode, Func<bool> question)
    {
        this.trueNode = trueNode;
        this.falseNode = falseNode;
        this.question = question;
    }

    // Evalúa la condición y ejecuta el nodo correspondiente basado en el resultado
    public void Execute()
    {
        if (question()) // Si la condición es verdadera
            trueNode.Execute();  // Ejecuta el nodo verdadero
        else  // Si la condición es falsa
            falseNode.Execute(); // Ejecuta el nodo falso
    }
}
