using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePiece : MonoBehaviour
{
    public int cordenadaX;
    public int cordenadaY;

    public float tiempoMovimiento;
    bool seEjecuto = true;
    public AnimationCurve curve;

    public TipoInterpolacion tipoInterpolacion;

    void MoverPieza(Vector3 posicionFinal, float timeMovement)
    {
        if(seEjecuto == true)
        {
            StartCoroutine(Corrutine(posicionFinal, timeMovement));
        }
    }

    IEnumerator Corrutine(Vector3 posicionFinal, float timeMovement)
    {
        bool llegoAlPunto = false;
        seEjecuto = false;
        Vector3 posicionInicial = transform.position;
        float tiempoTranscurrido = 0;

        while(!llegoAlPunto)
        {
            if (Vector3.Distance(transform.position, posicionFinal) < 0.01f)
            {
                llegoAlPunto = true;
                seEjecuto = true;
                transform.position = new Vector3((int)posicionFinal.x, (int)posicionFinal.y);
            }

            float t = tiempoTranscurrido / tiempoMovimiento;

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

    private void Update()
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
    }

    public void Cordenada(int x, int y)
    {
        cordenadaX = x;
        cordenadaY = y;
    }
}
