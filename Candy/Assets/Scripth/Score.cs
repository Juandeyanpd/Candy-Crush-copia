using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Score : MonoBehaviour
{
    public int score = 0;
    public TMP_Text scorre;

    public void Scorre()
    {
        scorre.text = score.ToString("Score: ") + score;
        
    }

    public void Puntaje(int cantidad)
    {
        score += cantidad;
    }
}
