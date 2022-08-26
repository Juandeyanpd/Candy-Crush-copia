using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePiece : MonoBehaviour
{
    public int cordenadaX;
    public int cordenadaY;

    public void Cordenada(int x, int y)
    {
        cordenadaX = x;
        cordenadaY = y;
    }
}
