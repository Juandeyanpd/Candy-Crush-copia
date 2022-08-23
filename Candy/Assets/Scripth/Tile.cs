using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int indiceX;
    public int indiceY;

    public void Intial(int cambioX, int cambioY)
    {
        indiceX = cambioX;
        indiceY = cambioY;
    } 
}
