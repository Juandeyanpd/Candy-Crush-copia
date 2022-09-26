using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    //Las cordenadas de los tiles
    public int indiceX;
    public int indiceY;

    //Variable Board
    public Board m_Board;

    //Le damos valores de parámetros a la variables
    public void Intial(int x, int y, Board board)
    {
        indiceX = x;
        indiceY = y;
        m_Board = board;
    }

    //Al momento del clikc, se llama método del board
    private void OnMouseDown()
    {
        m_Board.ClickedTile(this);
    }

    //Mientras se sostenga el mouse se ejecuta el método (o mejor dicho cuando entra en el colosionador sin necesidad de soltar)
    private void OnMouseEnter()
    {
        m_Board.DragToTile(this);
    }

    //Ya cuando se suelta, se ejecuta el método
    private void OnMouseUp()
    {
        m_Board.ReleaseTile();
    }
}
