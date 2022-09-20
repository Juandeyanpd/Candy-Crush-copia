using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    public int score = 0;
    public Text scorre;

    public void Scorre()
    {
        scorre.text = "Score: " + score;
    }

    public void Puntaje(int cantidad)
    {
        score += cantidad;
    }
}
