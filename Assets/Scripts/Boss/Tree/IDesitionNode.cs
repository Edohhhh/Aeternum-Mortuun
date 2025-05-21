using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Interfaz base para los nodos del �rbol de decisi�n (acciones o preguntas)
// Todos los nodos deben implementar el m�todo Execute()

public interface IDesitionNode
{
    void Execute();
}
