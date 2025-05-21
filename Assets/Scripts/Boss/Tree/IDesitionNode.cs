using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Interfaz base para los nodos del árbol de decisión (acciones o preguntas)
// Todos los nodos deben implementar el método Execute()

public interface IDesitionNode
{
    void Execute();
}
