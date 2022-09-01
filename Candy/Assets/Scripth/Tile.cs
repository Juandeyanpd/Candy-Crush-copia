using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int indiceX;
    public int indiceY;

    public Board board;

    public void Intial(int cambioX, int cambioY)
    {
        indiceX = cambioX;
        indiceY = cambioY;
    }

    public void OnMouseDown(Board board)
    {
        board.inicial = this;
        Debug.Log("Tocó");
        //board.ActualTile(this);
    }

    public void OnMouseEnter(Board board)
    {
        board.final = this;
        Debug.Log("Movio");
        //board.ActualTile(this);
    }

    public void OnMouseUp(Board board)
    {
        Debug.Log("Soltó");
    }
}
