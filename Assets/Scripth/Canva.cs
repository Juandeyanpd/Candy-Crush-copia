using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Canva : MonoBehaviour
{
    public int score = 0;
    public TMP_Text score_text;

    [SerializeField] int min, seg;
    [SerializeField] TMP_Text timer_text;

    public float restante;
    private bool enMarcha;

    private void Awake()
    {
        restante = (min * 60) + seg;
        enMarcha = true;
    }

    void Update()
    {
        if(enMarcha)
        {
            restante -= Time.deltaTime;
            if(restante < 1)
            {
                enMarcha = true;
                //Matar personaje
            }
            int tempMin = Mathf.FloorToInt(restante / 60);
            int tempSeg = Mathf.FloorToInt(restante % 60);
            timer_text.text = "Timer " + tempMin.ToString() + ":" + tempSeg.ToString();
        }
    }
    public void Start()
    {
        score_text.text = "Score: " + score.ToString();
    }

    public void Puntaje(int cantidad)
    {
        score += cantidad;
        score_text.text = "Score: " + score.ToString();
    }


}
