using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GamePiece : MonoBehaviour
{
    //Cordenadas
    public int coordinateX;
    public int coordinateY;

    //Variable board
    public Board m_Board;

    //Una bandera para dar orden o no de mover
    bool m_isMoving = false;

    //Es una variable para un tipo de curva y mover diferente la ficha
    public AnimationCurve curve;

    //Métodos de las características
    public InterpType interpolation;
    public MatchValue matchValue;

    //Se le dan los valores de parámetros a las cordenadas 
    public void SetCoord(int x, int y)
    {
        coordinateX = x;
        coordinateY = y;
    }

    //Se le da el parámetro a la variable board
    public void Init(Board board)
    {
        m_Board = board;
    }

    //Ejecuta la rutina de mover 
    public void Move(int x, int y, float moveTime)
    {
        if(!m_isMoving)
        {
            StartCoroutine(MoveRoutine(x, y, moveTime));
        }
    }

    //Esta rutina, es para los diferentes tipos de movimiento
    IEnumerator MoveRoutine(int destX, int destY, float timeToMove)
    {
        Vector2 startPosition = transform.position;
        bool reacedDestination = false;
        float elapsedTime = 0f;
        m_isMoving = true;

        while(!reacedDestination)
        {
            if (Vector2.Distance(transform.position, new Vector2 (destX, destY)) < 0.01f)
            {
                reacedDestination = true;
                if(m_Board != null)
                {
                    m_Board.PlaceGamePiece(this, destX, destY);
                }
                break;
            }

            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp(elapsedTime / timeToMove, 0f, 1f);

            switch(interpolation)
            {
                case InterpType.Linear:

                break;
                case InterpType.EaseIn:
                    t = 1 - Mathf.Cos(t * Mathf.PI * .5f);
                    break;
                case InterpType.EaseOut:
                    t = Mathf.Sin(t * Mathf.PI * .5f);
                    break;
                case InterpType.SmoothSetp:
                    t = t * t * (3 - 2);
                    break;
                case InterpType.SmootherStep:
                    t = t * t *t * (t * (t * 6 - 15) + 10);
                break;
            }

            transform.position = Vector2.Lerp(startPosition, new Vector2 (destX,destY), t);
            yield return null;
        }
        m_isMoving = false;
    }

    //Son diferentes "tags o nombres para tipos de movimiento
    public enum InterpType
    {
        Linear,
        EaseIn, 
        EaseOut,
        SmoothSetp,
        SmootherStep
    }

    //Son tipos de "Tags" para las fichas y identificar cada una
    public enum MatchValue
    {
        galleta,
        chocolate,
        helado,
        algodondeazucar,
        dulce,
        ponque,
        candy,
        barradechocolate
    }
}
