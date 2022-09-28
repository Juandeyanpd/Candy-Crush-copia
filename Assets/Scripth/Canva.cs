using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Canva : MonoBehaviour
{
    //Texto y entero de score
    private int score = 0;
    public int winScore;
    public TMP_Text score_text;

    //Entero y texto del tiempo
    [SerializeField] int min, seg;
    [SerializeField] TMP_Text timer_text;

    //Son variables del tiempo
    private float restante;
    private bool enMarcha;

    //Variable para utilizar método de Scena
    public MainMenu menu;

    //Aquí se le da valor a el entero y el bool
    private void Awake()
    {
        restante = (min * 60) + seg;
        enMarcha = true;
        //menu = FindObjectOfType<MainMenu>();
    }

    //Aquí está el código para hacer el temporizador
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

            if(tempSeg == 0 && tempMin == 0)
            {
                menu.LoadScene(4);
            }
        }

    }

    //Se hace el texto del score
    public void Start()
    {
        score_text.text = "Score: " + score.ToString();
    }

    //Ya este es para que al momento de enviarle los puntos, se actualicen en el score
    public void Puntaje(int cantidad)
    {
        score += cantidad;
        score_text.text = "Score: " + score.ToString();
        if(score >= winScore)
        {
            menu.LoadScene(5);
        }
    }
}
