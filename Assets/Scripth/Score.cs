using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Score : MonoBehaviour
{
    public int score = 0;
    public TMP_Text score_text;

    public int timer = 0;
    public TMP_Text timer_text;

    public void Start()
    {
        score_text.text ="Score: " + score.ToString();
    }

    public void Puntaje(int cantidad)
    {
        score += cantidad;
        score_text.text = "Score: " + score.ToString();
    }


}
