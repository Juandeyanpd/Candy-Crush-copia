using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int indiceX;
    public int indiceY;

    public Board m_Board;

    public void Intial(int x, int y, Board board)
    {
        indiceX = x;
        indiceY = y;
        m_Board = board;
    }

    private void OnMouseDown()
    {
        m_Board.ClickedTile(this);
    }

    private void OnMouseEnter()
    {
        m_Board.DragToTile(this);
    }

    private void OnMouseUp()
    {
        m_Board.ReleaseTile(this);
    }
}
