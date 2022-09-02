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

    public Tile inicial;
    public Tile final;


    private void Start()
    {
        posiciones = new GamePiece[ancho, altura];
        OrganizarCam();
        CrearBoard();
        LlenarMatriz();
    }

    void CrearBoard() 
    {
        board = new Tile[ancho, altura];

        for (int x = 0; x < ancho; x++) //i para x
        {
            for (int y = 0; y < altura; y++) //j para y
            {
                GameObject go = Instantiate(prefab);
                go.name = "Tile : ( " + x + " , " + y + " ) ";
                go.transform.position = new Vector2(x, y);
                go.transform.parent = transform;
                Tile tile = go.GetComponent<Tile>();
                tile.Intial(x, y);
                board[x, y] = tile; 
                //board[ancho, altura]
                //board[x, y]
                tile.board = this;
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

    public void PiezaPosicion(GamePiece gp, int x, int y)
    {
        gp.transform.position = new Vector3(x, y, 0f);

        posiciones[x, y] = gp;
    }

    public void LlenarMatriz()
    {
        for(int x = 0; x < ancho; x++)
        {
            for (int y = 0; y < altura; y++)
            {
                GameObject go = PiezaAleatoria();
                PiezaPosicion(go.GetComponent<GamePiece>(), x, y);
            }
        }
    }


    public void InicioMouse(Tile ini)
    {

        if (inicial == null)
        {
            inicial = ini;
        }

    }
    public void EndMouse(Tile fin)
    {
        if(inicial != null)
        {
            final = fin;
        }

    }
    public void Realice()
    {
        if(inicial != null && final != null)
        {
            CambioDeFichas(inicial, final);
        }
    }

    public void CambioDeFichas(Tile inicioT, Tile finalT)
    {
        GamePiece gPin = posiciones[inicioT.indiceX, inicioT.indiceY];
        GamePiece gFin = posiciones[finalT.indiceX, finalT.indiceY];

        gPin.MoverPieza(finalT.indiceX, finalT.indiceY, gPin.tiempoMovimiento);
        gFin.MoverPieza(inicioT.indiceX, inicioT.indiceY, gFin.tiempoMovimiento);

        inicial = null;
        final = null;
    }

}
