
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Board : MonoBehaviour
{
    public int width;
    public int heigth;

    public int borderSize;

    public GameObject tilePrefab;
    public GameObject[] gamePiecesPrefabs;

    [Range(0.1f, .5f)]
    public float swapTime = .3f;

    Tile[,] m_allTiles;
    GamePiece[,] m_allGamePieces;

    [SerializeField] Tile m_clickedTile;
    [SerializeField] Tile m_targetTile;

    bool m_playerInputEnabled = true;

    Transform tileParent;
    Transform gamePieceParent;

    public AudioClip clip;
    public Canva score;

    private void Start()
    {
        //Se llaman los métodos desde que comienza el play y inicializa

        SetParents();

        m_allTiles = new Tile[width, heigth];
        m_allGamePieces = new GamePiece[width, heigth];

        SetupTiles();
        SetupCamera();
        FillBoard(10, .5f);
    }

    private void SetParents()
    {
        if(tileParent == null)
        {
            tileParent = new GameObject().transform;
            tileParent.name = "Tiles";
            tileParent.parent = this.transform;
        }

        if(gamePieceParent == null)
        {
            gamePieceParent = new GameObject().transform;
            gamePieceParent.name = "GamePieces";
            gamePieceParent = this.transform;
        }
    }

    void SetupTiles()
    {
        //Se instancia el tablero de juego, o sea los "Tiles"

        for (int x = 0; x < width; x++) //i para x
        {
            for (int y = 0; y < heigth; y++) //j para y
            {
                GameObject go = Instantiate(tilePrefab);
                go.name = "Tile : ( " + x + " , " + y + " ) ";
                go.transform.position = new Vector2(x, y);
                go.transform.parent = transform;
                Tile tile = go.GetComponent<Tile>();
                tile.m_Board = this;
                m_allTiles[x, y] = tile;
                //board[ancho, altura]
                //board[x, y]
                tile.Intial(x, y, this);
            }
        }
    }

    void SetupCamera()
    {
        //Aquí organizamos la cámara para que quepa todo el tablero 
        cam.transform.position = new Vector3(((float)width / 2) - .5f, ((float)heigth / 2) - .5f, -10);

        float aspectRatio = (float)Screen.width / (float)Screen.height;

        float sizeY = cam.orthographicSize = ((float)heigth / 2) + borderSize;
        float sizeX = cam.orthographicSize = (((float)width / 2) + borderSize) / (aspectRatio);

        cam.orthographicSize = sizeY > sizeX ? sizeY : sizeX;
    }

    GameObject PiezaAleatoria()
    {
        //Aquí instanciamos una pieza de juego aleatoria
        int indexAleatorio = Random.Range(0, gamePiecesPrefabs.Length);
        GameObject go = Instantiate(gamePiecesPrefabs[indexAleatorio]);

        GamePiece gamePiece = go.GetComponent<GamePiece>();
        gamePiece.m_Board = this;

        return go;
    }

    public void PiezaPosicion(GamePiece gp, int x, int y)
    {
        gp.transform.position = new Vector3(x, y, 0f);
        gp.SetCoord(x, y);
        m_allGamePieces[x, y] = gp;
    }

    public void FillBoard(int falseOffSet = 0, float moveTime = .1f)
    {
        List<GamePiece> addedPieces = new List<GamePiece>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < heigth; y++)
            {
                if (m_allGamePieces[x,y] == null)
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
                Debug.LogWarning("Se alcanzo el número máximo de interraciones");
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
            ClearPieceAt(gamePieces.coordinateX, gamePieces.coordinateY);
            LlenarMatrizAleatoriaEn(gamePieces.coordinateX, gamePieces.coordinateY);
        }
    }


    public void ClickedTile(Tile ini)
    {

        if (m_clickedTile == null)
        {
            m_clickedTile = ini;
        }

    }
    public void DragToTile(Tile fin)
    {
        if (m_clickedTile != null && EsVecino(m_clickedTile, fin))
        {
            m_targetTile = fin;
        }

    }
    public void ReleaseTile(Tile release)
    {
        if (m_clickedTile != null && m_targetTile != null)
        {
            CambioDeFichas(m_clickedTile, m_targetTile);
        }
        m_clickedTile = null;
        m_targetTile = null;
    }


    public void CambioDeFichas(Tile inicioT, Tile finalT)
    {
        StartCoroutine(Cambio(inicioT, finalT));
    }

    IEnumerator Cambio(Tile inicioT, Tile finalT)
    {
        GamePiece gPin = m_allGamePieces[inicioT.indiceX, inicioT.indiceY];
        GamePiece gFin = m_allGamePieces[finalT.indiceX, finalT.indiceY];

        if (gFin != null && gFin != null)
        {
            gPin.Move(finalT.indiceX, finalT.indiceY, swapTime);
            gFin.Move(inicioT.indiceX, inicioT.indiceY, swapTime);

            yield return new WaitForSeconds(swapTime);

            List<GamePiece> listasCombinadasInicio = EncontrarCoincidenciasEn(inicioT.indiceX, inicioT.indiceY);
            List<GamePiece> listasCombinadasFinal = EncontrarCoincidenciasEn(finalT.indiceX, finalT.indiceY);

            if (listasCombinadasInicio.Count == 0 && listasCombinadasFinal.Count == 0)
            {
                gPin.Move(inicioT.indiceX, inicioT.indiceY, swapTime);
                gFin.Move(finalT.indiceX, finalT.indiceY, swapTime);
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
        return (_x < width && _x >= 0 && _y >= 0 && _y < heigth);
    }


    List<GamePiece> EncontrarCoincidencias(int startX, int startY, Vector2 direccionDeBusqueda, int cantidadMinima = 3)
    {
        //Crear una lista de coincidencias Encontradas
        List<GamePiece> coincidencias = new List<GamePiece>();

        //Crear una referencia al gamepiece inicial
        GamePiece piezaIncial = null;

        if (EstaEnRango(startX, startY))
        {
            piezaIncial = m_allGamePieces[startX, startY];
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

        int valorMaximo = width > heigth ? width : heigth;

        for (int i = 1; i < valorMaximo - 1; i++)
        {
            siguienteX = startX + (int)Mathf.Clamp(direccionDeBusqueda.x, -1, 1) * i;
            siguienteY = startY + (int)Mathf.Clamp(direccionDeBusqueda.y, -1, 1) * i;

            if (!EstaEnRango(siguienteX, siguienteY))
            {
                break;
            }

            GamePiece siguientePieza = m_allGamePieces[siguienteX, siguienteY];

            if (siguientePieza == null)
            {
                break;
            }
            else
            {
                //Comparar si las piezas inicial y final son del mismo tipo
                if (piezaIncial.matchValue == siguientePieza.matchValue && !coincidencias.Contains(siguientePieza))
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
            matches = matches.Union(EncontrarCoincidenciasEn(gamePiece.coordinateX, gamePiece.coordinateY)).ToList();
        }
        return matches;
    }

    private List<GamePiece> EncontrarTodasLasCoincidencias()
    {
        List<GamePiece> todasLasCoincidencias = new List<GamePiece>();

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < heigth; j++)
            {
                var coincidencias = EncontrarCoincidenciasEn(i, j);
                todasLasCoincidencias = todasLasCoincidencias.Union(coincidencias).ToList();
            }
        }

        return todasLasCoincidencias;
    }

    public void ResaltarCoincidencias()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < heigth; j++)
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
                ResaltarTile(p.coordinateX, p.coordinateY, p.GetComponent<SpriteRenderer>().color);
            }
        }
    }

    private void ResaltarTile(int _x, int _y, Color _col)
    {
        SpriteRenderer sr = m_allTiles[_x, _y].GetComponent<SpriteRenderer>();
        sr.color = _col;
    }
        


    private void ClearBoard()
    {
        for (int _x = 0; _x < width; _x++)
        {
            for (int _y = 0; _y < heigth; _y++)
            {
                ClearPieceAt(_y, _y);
            }
        }
    }

    private void ClearPieceAt(int x, int y)
    {
        GamePiece pieceToClear = m_allGamePieces[x, y];
        if(pieceToClear != null)
        {
            m_allGamePieces[x, y] = null;
            Destroy(pieceToClear.gameObject);
        }
    }
    private void ClearPieceAt(List<GamePiece> gamePieces)
    {
        foreach(GamePiece gamePiece in gamePieces)
        {
            if (gamePiece != null)
            {
                ClearPieceAt(gamePiece.coordinateX, gamePiece.coordinateY);
            }
        }
    }

    List<GamePiece> CollapseColumn(int column, float CollapseTime = 0.1f)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();

        for (int i = 0; i < heigth-1; i++)
        {
            if (m_allGamePieces[column, i] == null)
            {
                for (int j = i + 1; j < heigth; j++)
                {
                    if (m_allGamePieces[column, j] != null)
                    {
                        m_allGamePieces[column, j].Move(column, i, CollapseTime * (j-i));
                        m_allGamePieces[column, i] = m_allGamePieces[column, j];
                        m_allGamePieces[column, i].SetCoord(column, i);
                        if (!movingPieces.Contains(m_allGamePieces[column, i]))
                        {
                            movingPieces.Add(m_allGamePieces[column, i]);
                        }
                        m_allGamePieces[column, j] = null;
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
            if (!collumnsIndex.Contains(gamePiece.coordinateX))
            {
                collumnsIndex.Add(gamePiece.coordinateX);
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
        m_playerInputEnabled = true;
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
        FillBoard();
        yield return null;
    }

    bool isColpase(List<GamePiece> gamePieces)
    {
        foreach (GamePiece gamePiece in gamePieces)
        {
            if (gamePiece != null)
            {
                if(gamePiece.transform.position.y - (float)gamePiece.coordinateY > 0.001f)
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
