using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GamePiece : MonoBehaviour
{
    public int cordenadaX;
    public int cordenadaY;

    bool seEjecuto = true;
    public AnimationCurve curve;

    public TipoInterpolacion tipoInterpolacion;
    public TipoFicha tipoFicha;

    public Board board;

    public void MoverPieza(int x, int y, float timeMovement)
    {
        if(seEjecuto == true)
        {
            StartCoroutine(Corrutine(new Vector3(x,y,0), timeMovement));
        }
    }

    IEnumerator Corrutine(Vector3 posicionFinal, float timeMovement)
    {
        seEjecuto = false;

        bool llegoAlPunto = false;
        float tiempoTranscurrido = 0;

        Vector3 posicionInicial = transform.position;

        while(!llegoAlPunto)
        {
            if (Vector3.Distance(transform.position, posicionFinal) < 0.01f)
            {
                llegoAlPunto = true;
                seEjecuto = true;

                board.PiezaPosicion(this, (int)posicionFinal.x, (int)posicionFinal.y);

                transform.position = new Vector3((int)posicionFinal.x, (int)posicionFinal.y, 0);
                break;
            }

            float t = tiempoTranscurrido / timeMovement;

            switch(tipoInterpolacion)
            {
                case TipoInterpolacion.Lineal:
                    t = curve.Evaluate(t);
                break;

                case TipoInterpolacion.Entrada:
                    t = 1 - Mathf.Cos(t * Mathf.PI * .5f);
                break;

                case TipoInterpolacion.Salida:
                    t = Mathf.Sin(t * Mathf.PI * .5f);
                break;

                case TipoInterpolacion.Suavizado:
                    t = t * t * (3 - 2);
                break;

                case TipoInterpolacion.MasSuavizado:
                    t = t * t *t * (t * (t * 6 - 15) + 10);
                break;

            }

            transform.position = Vector3.Lerp(posicionInicial, posicionFinal, t);
            tiempoTranscurrido += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        
    }

    public enum TipoInterpolacion
    {
        Lineal,
        Entrada, 
        Salida,
        Suavizado,
        MasSuavizado
    }

    public enum TipoFicha
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

    /*private void Update()
    {

        if (Input.GetKey(KeyCode.UpArrow))
        {
            MoverPieza(new Vector3(transform.position.x, transform.position.y + 1, 0), tiempoMovimiento);
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            MoverPieza(new Vector3(transform.position.x, transform.position.y - 1, 0), tiempoMovimiento);
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            MoverPieza(new Vector3(transform.position.x - 1, transform.position.y, 0), tiempoMovimiento);
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            MoverPieza(new Vector3(transform.position.x + 1, transform.position.y, 0), tiempoMovimiento);
        }
    }*/

    public void Cordenada(int x, int y)
    {
        cordenadaX = x;
        cordenadaY = y;
    }
}