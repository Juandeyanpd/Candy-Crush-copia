
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class Board : MonoBehaviour
{
    //Tamaño del Board
    public int width;
    public int heigth;

    //Aumentar más el Size de la cámara
    public int borderSize;

    //Prefabs de objetos en la escena
    public GameObject tilePrefab;
    public GameObject[] gamePiecesPrefabs;

    //Cambiar el tiempo de movimiento o lo que desees en dicho rango
    [Range(0.1f, .5f)]
    public float swapTime = .3f;

    //Conjunto de los arrays, para recorrer la Board
    Tile[,] m_allTiles;
    GamePiece[,] m_allGamePieces;

    //Necesario para mover las fichas
    [SerializeField] Tile m_clickedTile;
    [SerializeField] Tile m_targetTile;

    //Bandera para permitir o no  el cambio de fichas
    bool m_playerInputEnabled = true;

    //Para organizar los tiles y fichas
    Transform tileParent;
    Transform gamePieceParent;

    //Audio y música
    public AudioClip clip;
    public Canva score;

    //Perder
    public MainMenu menu;

    //Movimientos
    public int movimientos;
    public TMP_Text movimientos_text;

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

    private void Update()
    {
        Movimiento();
    }

    private void SetParents()
    {
        //Aquí "inicializamos los tranforms y lo volvemos hijos de el empty que tiene este scripth
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
        //Aquí llenamos el board con las fichas
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

        //??
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
        //Aquí verificamos que el tile sea nulo y le colocamos el tile parámetro
        if (m_clickedTile == null)
        {
            m_clickedTile = tile;
        }

    }
    public void DragToTile(Tile tile)
    {
        //Aquí observamos si el tile es diferente de nulo y sí son vecinos los tiles que le damos, para ya colocarle el tile del parámetro
        if (m_clickedTile != null && IsNextTo(m_clickedTile, tile))
        {
            m_targetTile = tile;
        }

    }
    public void ReleaseTile()
    {
        //Aquí observamos si los tiles son diferentes de cero, para que se ejecute el método de cambiar los tiles y después volverlos nulos
        if (m_clickedTile != null && m_targetTile != null)
        {
            SwitchTiles(m_clickedTile, m_targetTile);
        }
        m_clickedTile = null;
        m_targetTile = null;
    }


    public void SwitchTiles(Tile m_clickedTile, Tile m_targetTile)
    {
        //Aquí ejecutamos el método de cambio de tiles rutina 
        StartCoroutine(SwitchTilesRoutine(m_clickedTile, m_targetTile));
    }

    IEnumerator SwitchTilesRoutine(Tile clickedTile, Tile targetTile)
    {
        //Aquí hacemos el movimiento de las fichas o el cambio entre ellas
        if (m_playerInputEnabled)
        {
            GamePiece clickedPiece = m_allGamePieces[clickedTile.indiceX, clickedTile.indiceY];
            GamePiece targetPiece = m_allGamePieces[targetTile.indiceX, targetTile.indiceY];

            if (clickedPiece != null && targetPiece != null)
            {
                clickedPiece.Move(targetTile.indiceX, targetTile.indiceY, swapTime);
                targetPiece.Move(clickedTile.indiceX, clickedTile.indiceY, swapTime);

                yield return new WaitForSeconds(swapTime);

                //??
                List<GamePiece> clickedPieceMatches = FindMatchesAt(clickedTile.indiceX, clickedTile.indiceY);
                List<GamePiece> targetPieceMatches = FindMatchesAt(targetTile.indiceX, targetTile.indiceY);

                //Aquí verificamos si no hay matches, entonces las fichas no cambian, sino que vuelven a su lugar; ya en el else, cuando se hace el movimiento se limpia y rellena las fichas de dichos tiles, además del puntaje y sonido.
                if (clickedPieceMatches.Count == 0 && targetPieceMatches.Count == 0)
                {
                    clickedPiece.Move(clickedTile.indiceX, clickedTile.indiceY, swapTime);
                    targetPiece.Move(targetTile.indiceX, targetTile.indiceY, swapTime);
                    movimientos--;
                    yield return new WaitForSeconds(swapTime);
                }
                else
                {
                    yield return new WaitForSeconds(swapTime);
                    movimientos--;
                    ClearAndRefillBoard(clickedPieceMatches.Union(targetPieceMatches).ToList());
                    Sonido();
                    score.Puntaje(100);
                }
            }
        }

    }


    private void ClearAndRefillBoard(List<GamePiece> gamePieces)
    {
        //Aquí se ejecuta el método de rutina de limpiar y rellenar con ficha nueva
        StartCoroutine(ClearAndRefillRoutine(gamePieces));
    }


    List<GamePiece> FindMatches(int startX, int startY, Vector2 searchDirection, int minLenght = 3)
    {
        //Crear una lista de coincidencias Encontradas
        List<GamePiece> matches = new List<GamePiece>();
        //Crear una referencia al gamepiece inicial
        GamePiece startPiece = null;

        //Aquí escogemos una ficha (la cual es la primera seleccionada
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

        //Aquí es para recorrer en X y Y una culumna y fila, para saber si hay matchs????????????????????????
        for (int i = 1; i < maxValue; i++)
        {
            nextX = startX + (int)Mathf.Clamp(searchDirection.x, -1, 1) * i;
            nextY = startY + (int)Mathf.Clamp(searchDirection.y, -1, 1) * i;

            if (!IsWithBounds(nextX, nextY))
            {
                break;
            }

            //Aquí se coge la siguiente ficha para verificar si es igual a la del comienzo para saber si agregarla en la lista y tener un posible match
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

        //Aquí se dice que el mínimo para un match es minLenght(3)
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
        //Se utiliza el recorreer una columna y fila, para encontrar posibles matchs, esta es la de vertical, abajo y arriba
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
        //Se utiliza el recorreer una columna y fila, para encontrar posibles matchs, esta es la de horizontal, derecha y izquierda
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
    private List<GamePiece> FindMatchesAt(int x, int y, int minLenght = 3)
    {
        //Ya aquí es donde se una tanto buscar en vertical y horizontal, para buscar mathcs
        List<GamePiece> horizontalMatches = FindHorizontalMatches(x, y, minLenght);
        List<GamePiece> verticalMatches = FindVerticalMatches(x, y, minLenght);

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
        //Aquí se une la lista del parámetro con la de este método
        List<GamePiece> matches = new List<GamePiece>();

        foreach (GamePiece piece in gamePieces)
        {
            matches = matches.Union(FindMatchesAt(piece.coordinateX, piece.coordinateY, minLenght)).ToList();
        }
        return matches;
    }


    bool IsNextTo(Tile start, Tile end)
    {
        //Aquí se revisa si el tile que seleciono y el que selecciones, esté en el rango de un tile, además de que no cuenta el diagonal
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
        //Aquí se une los matches encontrados con la nueva lista 
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
        //Aquí se coge los spritesRenderer de los tiles que escojamos y apaga el rgb
        SpriteRenderer spriteRenderer = m_allTiles[x, y].GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
    }

    void HighlightTileOn(int x, int y, Color col)
    {
        //Aquí se enciende los colores de los tiles
        SpriteRenderer spriteRenderer = m_allTiles[x, y].GetComponent<SpriteRenderer>();
        spriteRenderer.color = col;
    }

    void HighlightMatchesAt(int x, int y)
    {
        //Aquí se escoge los tiles que se encenderan, o sea los de los matches 
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
        //Aquí se hayan las posiciones que se necesitan para buscar los matches y encenderlos
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < heigth; j++)
            {
                HighlightMatchesAt(i, j);
            }
        }
    }

    void HighlightPieces(List<GamePiece> gamePieces)
    {
        //Aquí se asegura de que las fichas sean diferente de nulas, para poder encender los tiles
        foreach (GamePiece piece in gamePieces)
        {
            if(piece != null)
            {
                HighlightTileOn(piece.coordinateX, piece.coordinateY, piece.GetComponent<SpriteRenderer>().color);
            }
        }
    }


    private void ClearPieceAt(int x, int y)
    {
        //Aquí escojemos las piezas de juego que le mandemos de el arrays de piezas, para volverlas nullas y destruirlas; además de apagar el Highlight
        GamePiece pieceToClear = m_allGamePieces[x, y];
        if(pieceToClear != null)
        {
            m_allGamePieces[x, y] = null;
            Destroy(pieceToClear.gameObject);
        }
        HighlightTileOff( x, y);
    }
    private void ClearPieceAt(List<GamePiece> gamePieces)
    {
        //Aquí cogemos de la lista de piezas las piezas, para mandarlas al otro método, solo si son diferentes de nullas.
        foreach(GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                ClearPieceAt(piece.coordinateX, piece.coordinateY);
            }
        }
    }
    private void ClearBoard()
    {
        //Aquí le enviamos todas las posiciones respecto a los enteros de ancho y alto, para enviarlas a otro método.
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < heigth; j++)
            {
                ClearPieceAt(i, j);
            }
        }
    }


    GameObject GetRandomPiece()
    {
        //Aquí instanciamos una pieza de juego aleatoria y la retornamos.
        int randomInx = Random.Range(0, gamePiecesPrefabs.Length);
        if (gamePiecesPrefabs[randomInx] == null)
        {
            Debug.LogWarning($"La clase Board en el array de prefabs en la posición {randomInx} no contiene una pieza válida");
        }
        return gamePiecesPrefabs[randomInx];
    }
    public void PlaceGamePiece(GamePiece gamePiece, int x, int y)
    {
        //Aquí verificamos de la pieza no sea nula, además de cojer la posición de la pieza en la escena y la rotación, para darle una posición a las piezas del arrays de piezas, se verifica antes de que si esten en el límite los enteros en el arrays y se agrega la posición, al final la cordenada de la misma.
        if(gamePiece == null)
        {
            Debug.LogWarning($"gamePiece inválida");
        }

        gamePiece.transform.position = new Vector3(x, y, 0f);
        gamePiece.transform.rotation = Quaternion.identity;

        if (IsWithBounds( x, y))
        {
           m_allGamePieces[x, y] = gamePiece;
        }
        gamePiece.SetCoord(x, y);
    }


    private bool IsWithBounds(int x, int y)
    {
        //Aquí se returna enteros que recorren el "arrays"(Pues en realidad, es respecto a otros enteros) para que no se pasen o desborde
        return (x < width && x >= 0 && y >= 0 && y < heigth);
    }


    GamePiece FillRandomAt(int x, int y, int falseOffset = 0, float moveTime = .1f)
    {
        //Aquí organizamos una pieza random en un lugar de pieza, solo si randomPiece es diferente de nulo, además al ser diferente de nulo permite el move de las piezas; cuando el falseOffset es diferente de 0, las fichas se suben unas unidades en Y; la ficha se organiza en el empty que funciona como padre y ya se retorna la ficha.
        GamePiece randomPiece = Instantiate(GetRandomPiece(), Vector2.zero, Quaternion.identity).GetComponent<GamePiece>();
        if (randomPiece != null)
        {
            randomPiece.Init(this);
            PlaceGamePiece(randomPiece, x, y);

            if (falseOffset != 0)
            {
                randomPiece.transform.position = new Vector2(x, y + falseOffset);
                randomPiece.Move( x, y, moveTime);
            }
        }
        randomPiece.transform.parent = gamePieceParent;

        return randomPiece;
    }


    private void ReplaceWithRandom(List<GamePiece> gamePieces, int falseOffset = 0, float moveTime = .1f)
    {
        //Aquí por cada pieza de la lista, la limpiamos o destruimos por completo, además si el falseOffset es 0 se instancian ahí mismo o sino, se le da el efecto de caer desde arriba.
        foreach (GamePiece piece in gamePieces)
        {
            ClearPieceAt(piece.coordinateX, piece.coordinateY);
            if (falseOffset == 0)
            {
               FillRandomAt(piece.coordinateX, piece.coordinateY);
            }
            else
            {
                FillRandomAt(piece.coordinateX, piece.coordinateY, falseOffset, moveTime);
            }
        }
    }


    List<GamePiece> CollapseColumn(int column, float CollapseTime = .1f)
    {
        //Aquí escogemos una collumna para recorrerla 
        List<GamePiece> movingPieces = new List<GamePiece>();

        //Se revisa si hay espacios vacios y si hay piezas para hacerlas caer y darle sus nuevas cordenadas
        for (int i = 0; i < heigth - 1; i++)
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
        //Aquí recivimos dos listas, una para mover las piezas y la otra que recive las colmunas que necesitan que se revisen y colapsen
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
        //Revisa el indice o cordenada en X de la lista mandada por parámetro
        List<int> columns = new List<int>();
        foreach (GamePiece piece in gamePieces)
        {
            if (!columns.Contains(piece.coordinateX))
            {
                columns.Add(piece.coordinateX);
            }
        }
        return columns;
    }


    IEnumerator ClearAndRefillRoutine(List<GamePiece> gamePieces)
    {
        //Aquí se reprime el movimiento que se puede hacer, hasta que se ejecuta lo que hay dentro del do y después se verifica que se cumpla el while, y si no se cumple entonces permite otra vez el movimiento
        m_playerInputEnabled = false;
        List<GamePiece> matches = gamePieces;

        //Aquí se verifica si ya no hay matches para poder otra vez permitir el movimiento del jugador
        do
        {
            yield return StartCoroutine(ClearAndCollapseRoutine(gamePieces));
            yield return null;
            yield return StartCoroutine(RefillRoutine());
            matches = FindAllMatches();
            yield return new WaitForSeconds(.5f);
            Debug.Log("Esá falso");
        }
        while (matches.Count != 0);
        Debug.Log("true");
        m_playerInputEnabled = true;
    }
    IEnumerator ClearAndCollapseRoutine(List<GamePiece> gamePieces)
    {
        //Aquí se limpian las fichas de la lista del parámetro, además de organizar la columna e intanciar otras fichas 
        List<GamePiece> movingPieces = new List<GamePiece>();
        List<GamePiece> matches = new List<GamePiece>();
        HighlightPieces(gamePieces);
        yield return new WaitForSeconds(.5f);
        bool isFinished = false;

        while(!isFinished)
        {
            ClearPieceAt(gamePieces);
            yield return new WaitForSeconds(.25f);

            movingPieces = CollapseColumn(gamePieces);
            while(!isCollpased(gamePieces))
            {
                yield return null;
            }
            yield return new WaitForSeconds(.5f);

            matches = FindMatchesAt(movingPieces);

            if (matches.Count == 0)
            {
                isFinished = true;
                break;
            }
            else
            {
                yield return StartCoroutine(ClearAndCollapseRoutine(matches));
            }
        }
        yield return null;
    }


    IEnumerator RefillRoutine()
    {
        //Esta llama el método de rellenar todo el tablero
        FillBoard(10, .5f);
        yield return null;
    }



    private bool isCollpased(List<GamePiece> gamePieces)
    {
        //Este se revisa si ya las fichas llegaron a su destino cuando se colapsa una columna
        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                if(piece.transform.position.y - (float)piece.coordinateY > 0.001f)
                {
                    return false;
                }
            }
        }
        return true;
    }

    void Sonido()
    {
        //Aquí ejecuto mi sonido (al momento de hacer un match)
        AudioSource.PlayClipAtPoint(clip, gameObject.transform.position);
    }

    void Movimiento()
    {
        movimientos_text.text = "Movement: " + movimientos.ToString();
        if(movimientos == 0)
        {
            menu.LoadScene(0);
        }
    }
}
