using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GamePiece : MonoBehaviour
{
    public int coordinateX;
    public int coordinateY;

    public Board m_Board;

    bool m_isMoving = false;
    public AnimationCurve curve;

    public InterpType interpolation;
    public MatchValue matchValue;

    public void SetCoord(int x, int y)
    {
        coordinateX = x;
        coordinateY = y;
    }

    public void Init(Board board)
    {
        m_Board = board;
    }

    public void Move(int x, int y, float moveTime)
    {
        if(!m_isMoving)
        {
            StartCoroutine(MoveRoutine(x, y, moveTime));
        }
    }

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

    public enum InterpType
    {
        Linear,
        EaseIn, 
        EaseOut,
        SmoothSetp,
        SmootherStep
    }

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
