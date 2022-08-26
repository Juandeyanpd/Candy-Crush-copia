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
    public int borde;
    public GameObject[] prefab_Pieces;
    public GamePiece[,] posiciones;

    private void Start()
    {
        OrganizarCam();
        CrearBoard();
        LlenarMatriz();
    }

    void CrearBoard() 
    {
        board = new Tile[altura, ancho];

        for (int i = 0; i < altura; i++)
        {
            for (int j = 0; j < ancho; j++)
            {
                GameObject go = Instantiate(prefab);
                go.transform.position = new Vector2(j, i);
                go.name = "Tile : ( " + j + " , " + i + " ) ";
                go.transform.parent = transform;

                Tile tile = go.GetComponent<Tile>();
                board[i, j] = tile;
                tile.Intial(i, j);
            }
        }
    }


    void OrganizarCam()
    {
        cam.transform.position = new Vector3(((float)ancho / 2) - .5f, ((float)altura / 2) - .5f, -10);

        float aspectRatio = (float)Screen.width / (float)Screen.height;

        float sizeY = cam.orthographicSize = ((float)altura / 2) + borde;
        float sizeX = cam.orthographicSize = (((float)ancho / 2)+borde) / (aspectRatio);

        cam.orthographicSize = sizeY > sizeX ? sizeY : sizeX;
    }

    GameObject PiezaAleatoria()
    {
        int indexAleatorio = Random.Range(0, prefab_Pieces.Length);
        GameObject go = Instantiate(prefab_Pieces[indexAleatorio]);

        return go;
    }

    void PiezaPosicion(GamePiece gp, int x, int y)
    {
        gp.transform.position = new Vector3(x, y, 0f);
    }

    void LlenarMatriz()
    {
        //posiciones = new GamePiece[altura, ancho];

        for(int i = 0; i < altura; i++)
        {
            for (int j = 0; j < ancho; j++)
            {
                GameObject go = PiezaAleatoria();
                PiezaPosicion(go.GetComponent<GamePiece>(), j, i);

            }
        }
    }
}
