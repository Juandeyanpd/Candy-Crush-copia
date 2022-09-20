using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

    [Range(0.1f, .5f)]
    public float swapTime = .3f;

    public bool puedeMover = true;

    public AudioClip clip;

    public Score score;

    private void Start()
    {
        //Se llaman los m�todos desde que comienza el play y inicializa
        
        posiciones = new GamePiece[ancho, altura];
        OrganizarCam();
        CrearBoard();
        LlenarMatriz();
    }

    void CrearBoard()
    {
        //Se instancia el tablero de juego, o sea los "Tiles"
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
                tile.board = this;
                board[x, y] = tile;
                //board[ancho, altura]
                //board[x, y]
                tile.Intial(x, y);
            }
        }
    }

    void OrganizarCam()
    {
        //Aqu� organizamos la c�mara para que quepa todo el tablero 
        cam.transform.position = new Vector3(((float)ancho / 2) - .5f, ((float)altura / 2) - .5f, -10);

        float aspectRatio = (float)Screen.width / (float)Screen.height;

        float sizeY = cam.orthographicSize = ((float)altura / 2) + borde;
        float sizeX = cam.orthographicSize = (((float)ancho / 2) + borde) / (aspectRatio);

        cam.orthographicSize = sizeY > sizeX ? sizeY : sizeX;
    }

    GameObject PiezaAleatoria()
    {
        //Aqu� instanciamos una pieza de juego aleatoria
        int indexAleatorio = Random.Range(0, prefab_Pieces.Length);
        GameObject go = Instantiate(prefab_Pieces[indexAleatorio]);

        GamePiece gamePiece = go.GetComponent<GamePiece>();
        gamePiece.board = this;

        return go;
    }

    public void PiezaPosicion(GamePiece gp, int x, int y)
    {
        gp.transform.position = new Vector3(x, y, 0f);
        gp.Cordenada(x, y);
        posiciones[x, y] = gp;
    }

    public void LlenarMatriz()
    {
        List<GamePiece> addedPieces = new List<GamePiece>();

        for (int x = 0; x < ancho; x++)
        {
            for (int y = 0; y < altura; y++)
            {
                if (posiciones[x,y] == null)
                {
                    GamePiece gamePiece = LlenarMatrizAleatoriaEn(x, y);
                    addedPieces.Add(gamePiece);
                }
            }
        }

        bool estaLlena = false;
        int interracion = 0;
        int interracionMaximas = 100;

        while(!estaLlena)
        {
            List<GamePiece> coincidencias = EncontrarTodasLasCoincidencias();

            if(coincidencias.Count == 0)
            {
                estaLlena = true;
                break;
            }
            else
            {
                coincidencias = coincidencias.Intersect(addedPieces).ToList();
                ReemplazarConPiezaAleatoria(coincidencias);
            }

            if(interracion > interracionMaximas)
            {
                estaLlena = true;
                Debug.LogWarning("Se alcanzo el n�mero m�ximo de interraciones");
            }
            interracion++;
        }
    }

    GamePiece LlenarMatrizAleatoriaEn(int x, int y)
    {
        GameObject go = PiezaAleatoria();
        PiezaPosicion(go.GetComponent<GamePiece>(), x, y);
        return go.GetComponent<GamePiece>();
    }

    private void ReemplazarConPiezaAleatoria(List<GamePiece> coincidencias)
    {
        foreach (GamePiece gamePieces in coincidencias)
        {
            ClearPieceAt(gamePieces.cordenadaX, gamePieces.cordenadaY);
            LlenarMatrizAleatoriaEn(gamePieces.cordenadaX, gamePieces.cordenadaY);
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
        if (inicial != null && EsVecino(inicial, fin))
        {
            final = fin;
        }

    }
    public void Realice(Tile release)
    {
        if (inicial != null && final != null)
        {
            CambioDeFichas(inicial, final);
        }
        inicial = null;
        final = null;
    }


    public void CambioDeFichas(Tile inicioT, Tile finalT)
    {
        StartCoroutine(Cambio(inicioT, finalT));
    }

    IEnumerator Cambio(Tile inicioT, Tile finalT)
    {
        GamePiece gPin = posiciones[inicioT.indiceX, inicioT.indiceY];
        GamePiece gFin = posiciones[finalT.indiceX, finalT.indiceY];

        if (gFin != null && gFin != null)
        {
            gPin.MoverPieza(finalT.indiceX, finalT.indiceY, swapTime);
            gFin.MoverPieza(inicioT.indiceX, inicioT.indiceY, swapTime);

            yield return new WaitForSeconds(swapTime);

            List<GamePiece> listasCombinadasInicio = EncontrarCoincidenciasEn(inicioT.indiceX, inicioT.indiceY);
            List<GamePiece> listasCombinadasFinal = EncontrarCoincidenciasEn(finalT.indiceX, finalT.indiceY);

            if (listasCombinadasInicio.Count == 0 && listasCombinadasFinal.Count == 0)
            {
                gPin.MoverPieza(inicioT.indiceX, inicioT.indiceY, swapTime);
                gFin.MoverPieza(finalT.indiceX, finalT.indiceY, swapTime);
            }
            else
            {
                listasCombinadasInicio = listasCombinadasInicio.Union(listasCombinadasFinal).ToList();
                ClearAndRefillBoard(listasCombinadasInicio);
                Sonido();
                score.Puntaje(100);
            }
        }

    }

    bool EsVecino(Tile _inicial, Tile _final)
    {

        if (Mathf.Abs(_inicial.indiceX - _final.indiceX) == 1 && _inicial.indiceY == _final.indiceY)
        {
            return true;
        }
        if (Mathf.Abs(_inicial.indiceY - _final.indiceY) == 1 && _inicial.indiceX == _final.indiceX)
        {
            return true;
        }
        return false;
    }

    bool EstaEnRango(int _x, int _y)
    {
        return (_x < ancho && _x >= 0 && _y >= 0 && _y < altura);
    }


    List<GamePiece> EncontrarCoincidencias(int startX, int startY, Vector2 direccionDeBusqueda, int cantidadMinima = 3)
    {
        //Crear una lista de coincidencias Encontradas
        List<GamePiece> coincidencias = new List<GamePiece>();

        //Crear una referencia al gamepiece inicial
        GamePiece piezaIncial = null;

        if (EstaEnRango(startX, startY))
        {
            piezaIncial = posiciones[startX, startY];
        }

        if (piezaIncial != null)
        {
            coincidencias.Add(piezaIncial);
        }
        else
        {
            return null;
        }

        int siguienteX;
        int siguienteY;

        int valorMaximo = ancho > altura ? ancho : altura;

        for (int i = 1; i < valorMaximo - 1; i++)
        {
            siguienteX = startX + (int)Mathf.Clamp(direccionDeBusqueda.x, -1, 1) * i;
            siguienteY = startY + (int)Mathf.Clamp(direccionDeBusqueda.y, -1, 1) * i;

            if (!EstaEnRango(siguienteX, siguienteY))
            {
                break;
            }

            GamePiece siguientePieza = posiciones[siguienteX, siguienteY];

            if (siguientePieza == null)
            {
                break;
            }
            else
            {
                //Comparar si las piezas inicial y final son del mismo tipo
                if (piezaIncial.tipoFicha == siguientePieza.tipoFicha && !coincidencias.Contains(siguientePieza))
                {
                    coincidencias.Add(siguientePieza);
                }
                else
                {
                    break;
                }
            }

        }

        if (coincidencias.Count >= cantidadMinima)
        {
            return coincidencias;
        }

        return null;
    }

    List<GamePiece> BusquedaVertical(int startX, int startY, int CantidadMinima = 3)
    {
        List<GamePiece> arriba = EncontrarCoincidencias(startX, startY, Vector2.up, 2);
        List<GamePiece> abajo = EncontrarCoincidencias(startX, startY, Vector2.down, 2);

        if (arriba == null)
        {
            arriba = new List<GamePiece>();
        }

        if (abajo == null)
        {
            abajo = new List<GamePiece>();
        }

        var listasCombinadas = arriba.Union(abajo).ToList();

        return listasCombinadas.Count >= CantidadMinima ? listasCombinadas : null;
    }

    List<GamePiece> BusquedaHorizontal(int startX, int startY, int CantidadMinima = 3)
    {
        List<GamePiece> derecha = EncontrarCoincidencias(startX, startY, Vector2.right, 2);
        List<GamePiece> izquierda = EncontrarCoincidencias(startX, startY, Vector2.left, 2);

        if (derecha == null)
        {
            derecha = new List<GamePiece>();
        }

        if (izquierda == null)
        {
            izquierda = new List<GamePiece>();
        }

        var listasCombinadas = derecha.Union(izquierda).ToList();

        return listasCombinadas.Count >= CantidadMinima ? listasCombinadas : null;
    }


    private List<GamePiece> EncontrarCoincidenciasEn(int _x, int _y)
    {
        List<GamePiece> horizontal = BusquedaHorizontal(_x, _y, 3);
        List<GamePiece> vertical = BusquedaVertical(_x, _y, 3);

        if (horizontal == null)
        {
            horizontal = new List<GamePiece>();
        }

        if (vertical == null)
        {
            vertical = new List<GamePiece>();
        }

        var listasCombinadas = horizontal.Union(vertical).ToList();

        return listasCombinadas;
    }

    private List<GamePiece> EncontrarCoincidenciasEn(List<GamePiece> gamePieces, int cantidadMinima = 3)
    {
        List<GamePiece> matches = new List<GamePiece>();

        foreach (GamePiece gamePiece in gamePieces)
        {
            matches = matches.Union(EncontrarCoincidenciasEn(gamePiece.cordenadaX, gamePiece.cordenadaY)).ToList();
        }
        return matches;
    }

    private List<GamePiece> EncontrarTodasLasCoincidencias()
    {
        List<GamePiece> todasLasCoincidencias = new List<GamePiece>();

        for (int i = 0; i < ancho; i++)
        {
            for (int j = 0; j < altura; j++)
            {
                var coincidencias = EncontrarCoincidenciasEn(i, j);
                todasLasCoincidencias = todasLasCoincidencias.Union(coincidencias).ToList();
            }
        }

        return todasLasCoincidencias;
    }

    public void ResaltarCoincidencias()
    {
        for (int i = 0; i < ancho; i++)
        {
            for (int j = 0; j < altura; j++)
            {
                ResaltarCoincidenciasEn(i, j);
            }
        }
    }

    private void ResaltarCoincidenciasEn(int _x, int _y)
    {
        var listasCombinadas = EncontrarCoincidenciasEn(_x, _y);

        if(listasCombinadas.Count > 0)
        {
            foreach (GamePiece p in listasCombinadas)
            {
                ResaltarTile(p.cordenadaX, p.cordenadaY, p.GetComponent<SpriteRenderer>().color);
            }
        }
    }

    private void ResaltarTile(int _x, int _y, Color _col)
    {
        SpriteRenderer sr = board[_x, _y].GetComponent<SpriteRenderer>();
        sr.color = _col;
    }
        


    private void ClearBoard()
    {
        for (int _x = 0; _x < ancho; _x++)
        {
            for (int _y = 0; _y < altura; _y++)
            {
                ClearPieceAt(_y, _y);
            }
        }
    }

    private void ClearPieceAt(int x, int y)
    {
        GamePiece pieceToClear = posiciones[x, y];
        if(pieceToClear != null)
        {
            posiciones[x, y] = null;
            Destroy(pieceToClear.gameObject);
        }
    }
    private void ClearPieceAt(List<GamePiece> gamePieces)
    {
        foreach(GamePiece gamePiece in gamePieces)
        {
            if (gamePiece != null)
            {
                ClearPieceAt(gamePiece.cordenadaX, gamePiece.cordenadaY);
            }
        }
    }

    List<GamePiece> CollapseColumn(int column, float CollapseTime = 0.1f)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();

        for (int i = 0; i < altura-1; i++)
        {
            if (posiciones[column, i] == null)
            {
                for (int j = i + 1; j < altura; j++)
                {
                    if (posiciones[column, j] != null)
                    {
                        posiciones[column, j].MoverPieza(column, i, CollapseTime * (j-i));
                        posiciones[column, i] = posiciones[column, j];
                        posiciones[column, i].Cordenada(column, i);
                        if (!movingPieces.Contains(posiciones[column, i]))
                        {
                            movingPieces.Add(posiciones[column, i]);
                        }
                        posiciones[column, j] = null;
                        break;
                    }
                }
            }
        }
        return movingPieces;
    }

    List<GamePiece> CollapseColumn(List<GamePiece> gamePieces)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();
        List<int> collumnsToCollapse = GetColumns(gamePieces);
        foreach (int column in collumnsToCollapse)
        {
            movingPieces = movingPieces.Union(CollapseColumn(column)).ToList();
        }
        return movingPieces;
    }

    List<int> GetColumns(List<GamePiece> gamePieces)
    {
        List<int> collumnsIndex = new List<int>();
        foreach (GamePiece gamePiece in gamePieces)
        {
            if (!collumnsIndex.Contains(gamePiece.cordenadaX))
            {
                collumnsIndex.Add(gamePiece.cordenadaX);
            }
        }
        return collumnsIndex;
    }

    void ClearAndRefillBoard(List<GamePiece> gamePieces)
    {
        StartCoroutine(ClearAndRefillBoardRoutine(gamePieces));
    }

    IEnumerator ClearAndRefillBoardRoutine(List<GamePiece> gamePieces)
    {
        yield return StartCoroutine(ClearAndCollapseColumn(gamePieces)); 
        yield return null;
        yield return StartCoroutine(RefillRoutine());
        puedeMover = true;
    }

    IEnumerator ClearAndCollapseColumn(List<GamePiece> gamePieces)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();
        List<GamePiece> matches = new List<GamePiece>();

        bool isFinished = false;

        while(!isFinished)
        {
            ClearPieceAt(gamePieces);
            yield return new WaitForSeconds(.5f);
            movingPieces = CollapseColumn(gamePieces);
            while(!isColpase(gamePieces))
            {
                yield return new WaitForEndOfFrame();
            }
            matches = EncontrarCoincidenciasEn(movingPieces);

            if (matches.Count == 0)
            {
                isFinished = true;
                break;
            }
            else
            {
                yield return StartCoroutine(ClearAndCollapseColumn(matches));
            }
        }
    }

    IEnumerator RefillRoutine()
    {
        LlenarMatriz();
        yield return null;
    }

    bool isColpase(List<GamePiece> gamePieces)
    {
        foreach (GamePiece gamePiece in gamePieces)
        {
            if (gamePiece != null)
            {
                if(gamePiece.transform.position.y - (float)gamePiece.cordenadaY > 0.001f)
                {
                    return false;
                }
            }
        }
        return true;
    }

    void Sonido()
    {
        AudioSource.PlayClipAtPoint(clip, gameObject.transform.position);
    }
}
