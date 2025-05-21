using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Nodo de decisi�n que eval�a una condici�n y ejecuta un nodo basado en el resultado
public class QuestionNode : IDesitionNode
{
    private IDesitionNode trueNode;  // Nodo a ejecutar si la condici�n es verdadera
    private IDesitionNode falseNode; // Nodo a ejecutar si la condici�n es falsa
    private Func<bool> question;    // Funci�n que representa la condici�n a evaluar

    // Constructor que recibe los nodos de decisi�n y la funci�n de la condici�n
    public QuestionNode(IDesitionNode trueNode, IDesitionNode falseNode, Func<bool> question)
    {
        this.trueNode = trueNode;
        this.falseNode = falseNode;
        this.question = question;
    }

    // Eval�a la condici�n y ejecuta el nodo correspondiente basado en el resultado
    public void Execute()
    {
        if (question()) // Si la condici�n es verdadera
            trueNode.Execute();  // Ejecuta el nodo verdadero
        else  // Si la condici�n es falsa
            falseNode.Execute(); // Ejecuta el nodo falso
    }
}
