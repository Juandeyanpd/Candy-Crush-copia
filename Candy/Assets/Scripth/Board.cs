using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    Tile[,] board;
    public GameObject prefab;
    public int altura;
    public int ancho;
    public Camera cam;

    private void Start()
    {
        OrganizarCam();

        board = new Tile[altura, ancho];

        for(int i = 0; i < altura; i++)
        {
            for(int j = 0; j < ancho; j++)
            {
                GameObject go = Instantiate(prefab);
                go.transform.position = new Vector2(j, i);
                go.name = "Tile : ( "+j+" , "+i+" ) "; 
                go.transform.parent = transform;

                Tile tile = go.GetComponent<Tile>();
                board[i, j] = tile;
                tile.Intial( i, j);


            }
        }

    }

    void OrganizarCam()
    {
        cam.transform.position = new Vector3(((float)ancho / 2) - .5f, ((float)altura / 2) - .5f, -10);
    }
}
