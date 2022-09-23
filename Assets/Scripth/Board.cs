
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

    void SetupCamera()
    {
        //Aquí organizamos la cámara para que quepa todo el tablero 
        Camera.main.transform.position = new Vector3((float)(width - 1) / 2f, (float)(heigth - 1) / 2f, -10f);
        float aspectRatio = (float)Screen.width / (float)Screen.height;
        float verticalSize = (float)heigth / 2 + (float)borderSize;
        float horizontalSize = ((float)width / 2 + (float)borderSize) / aspectRatio;
        Camera.main.orthographicSize = verticalSize > horizontalSize ? verticalSize : horizontalSize;
    }

    void SetupTiles()
    {
        //Se instancia el tablero de juego, o sea los "Tiles"

        for (int i = 0; i < width; i++) //i para x
        {
            for (int j = 0; j < heigth; j++) //j para y
            {
                GameObject tile = Instantiate(tilePrefab, new Vector2( i, j), Quaternion.identity);
                tile.name = $"Tile({i},{j})";
                if(tileParent != null)
                {
                    tile.transform.parent = tileParent;
                }
                m_allTiles[i, j] = tile.GetComponent<Tile>();
                //board[ancho, altura]
                //board[x, y]
                m_allTiles[ i, j].Intial(i, j, this);
            }
        }
    }

    public void FillBoard(int falseOffSet = 0, float moveTime = .1f)
    {
        List<GamePiece> addedPieces = new List<GamePiece>();

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < heigth; j++)
            {
                if (m_allGamePieces[i,j] == null)
                {
                    if(falseOffSet == 0)
                    {
                       GamePiece gamePiece = FillRandomAt(i, j);
                       addedPieces.Add(gamePiece);
                    }
                    else
                    {
                        GamePiece piece = FillRandomAt( i, j, falseOffSet, moveTime);
                        addedPieces.Add(piece);
                    }
                }
            }
        }

        int maxIterations = 20;
        int Iterations = 0;

        bool isFilled = false;

        while(!isFilled)
        {
            List<GamePiece> matches = FindAllMatches();

            if(matches.Count == 0)
            {
                isFilled = true;
                break;
            }
            else
            {
                matches = matches.Intersect(addedPieces).ToList();
                if (falseOffSet == 0)
                {
                   ReplaceWithRandom(matches);
                }
                else
                {
                    ReplaceWithRandom(matches, falseOffSet, moveTime);
                }
            }

            if(Iterations > maxIterations)
            {
                isFilled = true;
                Debug.LogWarning($"Board.FillBoard alcanzó el número máximo de interraciones: {maxIterations}, abortar");
            }
            Iterations++;
        }
    }


    public void ClickedTile(Tile tile)
    {

        if (m_clickedTile == null)
        {
            m_clickedTile = tile;
        }

    }
    public void DragToTile(Tile tile)
    {
        if (m_clickedTile != null && IsNextTo(m_clickedTile, tile))
        {
            m_targetTile = tile;
        }

    }
    public void ReleaseTile()
    {
        if (m_clickedTile != null && m_targetTile != null)
        {
            SwitchTiles(m_clickedTile, m_targetTile);
        }
        m_clickedTile = null;
        m_targetTile = null;
    }


    public void SwitchTiles(Tile m_clickedTile, Tile m_targetTile)
    {
        StartCoroutine(SwitchTilesRoutine(m_clickedTile, m_targetTile));
    }

    IEnumerator SwitchTilesRoutine(Tile clickedTile, Tile targetTile)
    {
        if (m_playerInputEnabled)
        {
            GamePiece clickedPiece = m_allGamePieces[clickedTile.indiceX, clickedTile.indiceY];
            GamePiece targetPiece = m_allGamePieces[targetTile.indiceX, targetTile.indiceY];

            if (clickedPiece != null && targetPiece != null)
            {
                clickedPiece.Move(targetTile.indiceX, targetTile.indiceY, swapTime);
                targetPiece.Move(clickedTile.indiceX, clickedTile.indiceY, swapTime);

                yield return new WaitForSeconds(swapTime);

                List<GamePiece> clickedPieceMatches = FindMatchesAt(clickedTile.indiceX, clickedTile.indiceY);
                List<GamePiece> targetPieceMatches = FindMatchesAt(targetTile.indiceX, targetTile.indiceY);

                if (clickedPieceMatches.Count == 0 && targetPieceMatches.Count == 0)
                {
                    clickedPiece.Move(clickedTile.indiceX, clickedTile.indiceY, swapTime);
                    targetPiece.Move(targetTile.indiceX, targetTile.indiceY, swapTime);
                    yield return new WaitForSeconds(swapTime);
                }
                else
                {
                    yield return new WaitForSeconds(swapTime);

                    ClearAndRefillBoard(clickedPieceMatches.Union(targetPieceMatches).ToList());
                    Sonido();
                    score.Puntaje(100);
                }
            }
        }

    }


    private void ClearAndRefillBoard(List<GamePiece> gamePieces)
    {
        StartCoroutine(ClearAndRefillRoutine(gamePieces));
    }


    List<GamePiece> FindMatches(int startX, int startY, Vector2 searchDirection, int minLenght = 3)
    {
        //Crear una lista de coincidencias Encontradas
        List<GamePiece> matches = new List<GamePiece>();
        //Crear una referencia al gamepiece inicial
        GamePiece startPiece = null;

        if (IsWithBounds(startX, startY))
        {
            startPiece = m_allGamePieces[startX, startY];
        }

        if (startPiece != null)
        {
            matches.Add(startPiece);
        }
        else
        {
            return null;
        }

        int nextX;
        int nextY;

        int maxValue = width > heigth ? width : heigth;

        for (int i = 1; i < maxValue - 1; i++)
        {
            nextX = startX + (int)Mathf.Clamp(searchDirection.x, -1, 1) * i;
            nextY = startY + (int)Mathf.Clamp(searchDirection.y, -1, 1) * i;

            if (!IsWithBounds(nextX, nextY))
            {
                break;
            }

            GamePiece nextPiece = m_allGamePieces[nextX, nextY];

            if (nextPiece == null)
            {
                break;
            }
            else
            {
                //Comparar si las piezas inicial y final son del mismo tipo
                if (nextPiece.matchValue == startPiece.matchValue && !matches.Contains(nextPiece))
                {
                    matches.Add(nextPiece);
                }
                else
                {
                    break;
                }
            }

        }

        if (matches.Count >= minLenght)
        {
            return matches;
        }
        else
        {
            return null;
        }
    }
    List<GamePiece> FindVerticalMatches(int startX, int startY, int minLenght = 3)
    {
        List<GamePiece> upwardMatches = FindMatches(startX, startY, Vector2.up, 2);
        List<GamePiece> downwardMatches = FindMatches(startX, startY, Vector2.down, 2);

        if (upwardMatches == null)
        {
            upwardMatches = new List<GamePiece>();
        }

        if (downwardMatches == null)
        {
            downwardMatches = new List<GamePiece>();
        }

        var combinedMatches = upwardMatches.Union(downwardMatches).ToList();
        return combinedMatches.Count >= minLenght ? combinedMatches : null;
    }
    List<GamePiece> FindHorizontalMatches(int startX, int startY, int minLenght = 3)
    {
        List<GamePiece> rightMatches = FindMatches(startX, startY, Vector2.right, 2);
        List<GamePiece> leftMatches = FindMatches(startX, startY, Vector2.left, 2);

        if (rightMatches == null)
        {
            rightMatches = new List<GamePiece>();
        }

        if (leftMatches == null)
        {
            leftMatches = new List<GamePiece>();
        }

        var combinedMatches = rightMatches.Union(leftMatches).ToList();
        return combinedMatches.Count >= minLenght ? combinedMatches : null;
    }
    private List<GamePiece> FindMatchesAt(int _x, int _y, int minLenght = 3)
    {
        List<GamePiece> horizontalMatches = FindHorizontalMatches(_x, _y, minLenght);
        List<GamePiece> verticalMatches = FindVerticalMatches(_x, _y, minLenght);

        if (horizontalMatches == null)
        {
            horizontalMatches = new List<GamePiece>();
        }

        if (verticalMatches == null)
        {
            verticalMatches = new List<GamePiece>();
        }

        var combinedMatches = horizontalMatches.Union(verticalMatches).ToList();
        return combinedMatches;
    }
    private List<GamePiece> FindMatchesAt(List<GamePiece> gamePieces, int minLenght = 3)
    {
        List<GamePiece> matches = new List<GamePiece>();

        foreach (GamePiece piece in gamePieces)
        {
            matches = matches.Union(FindMatchesAt(piece.coordinateX, piece.coordinateY, minLenght)).ToList();
        }
        return matches;
    }


    bool IsNextTo(Tile start, Tile end)
    {

        if (Mathf.Abs(start.indiceX - end.indiceX) == 1 && start.indiceY == end.indiceY)
        {
            return true;
        }
        if (Mathf.Abs(start.indiceY - end.indiceY) == 1 && start.indiceX == end.indiceX)
        {
            return true;
        }
        return false;
    }

    private List<GamePiece> FindAllMatches()
    {
        List<GamePiece> combinedMatches = new List<GamePiece>();

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < heigth; j++)
            {
                var matches = FindMatchesAt(i, j);
                combinedMatches = combinedMatches.Union(matches).ToList();
            }
        }
        return combinedMatches;
    }


    void HighlightTileOff(int x, int y)
    {
        SpriteRenderer spriteRenderer = m_allTiles[x, y].GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
    }

    void HighlightTileOn(int x, int y, Color col)
    {
        SpriteRenderer spriteRenderer = m_allTiles[x, y].GetComponent<SpriteRenderer>();
        spriteRenderer.color = col;
    }

    void HighlightMatchesAt(int x, int y)
    {
        HighlightTileOff(x, y);
        var combinedMatches = FindMatchesAt( x, y);
        if (combinedMatches.Count > 0)
        {
            foreach (GamePiece piece in combinedMatches)
            {
                HighlightTileOn(piece.coordinateX, piece.coordinateY, piece.GetComponent<SpriteRenderer>().color);
            }
        }
    }

    void HighlightMatches()
    {

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


    GamePiece FillRandomAt(int x, int y)
    {
        GameObject go = PiezaAleatoria();
        PiezaPosicion(go.GetComponent<GamePiece>(), x, y);
        return go.GetComponent<GamePiece>();
    }

    private void ReplaceWithRandom(List<GamePiece> coincidencias)
    {
        foreach (GamePiece gamePieces in coincidencias)
        {
            ClearPieceAt(gamePieces.coordinateX, gamePieces.coordinateY);
            FillRandomAt(gamePieces.coordinateX, gamePieces.coordinateY);
        }
    }


    bool IsWithBounds(int _x, int _y)
    {
        return (_x < width && _x >= 0 && _y >= 0 && _y < heigth);
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
        var listasCombinadas = FindMatchesAt(_x, _y);

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


    IEnumerator ClearAndRefillRoutine(List<GamePiece> gamePieces)
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
            matches = FindMatchesAt(movingPieces);

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
