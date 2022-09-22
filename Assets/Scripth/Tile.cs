using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int indiceX;
    public int indiceY;

    public Board board;
    public GamePiece gamePiece;

    public void Intial(int cambioX, int cambioY)
    {
        indiceX = cambioX;
        indiceY = cambioY;
    }

    private void OnMouseDown()
    {
        board.InicioMouse(this);
    }

    private void OnMouseEnter()
    {
        board.EndMouse(this);
    }

    private void OnMouseUp()
    {
        board.Realice(this);
    }
}
