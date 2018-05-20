using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;
using SharpChess.Model;


public class BoardManager : MonoBehaviour
{
    public static BoardManager _instance{set; get;}

	public List<GameObject> _chessPiecesPrefabs;
    public Material _selectedMat;
    public GameObject _cursorPrefab;

    private Moves _possibleMoves;
    private Square _squareFrom;
    private Square _squareTo;
    private Square _squareLastFrom;
    private Square _squareLastTo;
    private Piece _selectedPiece;

    private Dictionary<Piece, GameObject> _piecesGameObject;

    private System.Object _toBeDestroyedthreadLocker;
    private List<KeyValuePair<Piece, GameObject>> _toBeDestroyed;

    private System.Object _toBeRemovedthreadLocker;
    private List<Util.Pair<GameObject, Vector3>> _toBeMoved;

    private Material _previousMat;
    private Vector2Int _tileUnderCursor;
    private GameObject _cursor;
    private GameObject _cursorTarget;
    private readonly Vector2Int _none = new Vector2Int(-1, -1);

	void Start()
	{
        // Construtor
        _instance = this;
		_tileUnderCursor = _none;
        _piecesGameObject = new Dictionary<Piece, GameObject>();

        _toBeDestroyedthreadLocker = new System.Object();
        _toBeDestroyed = new List<KeyValuePair<Piece, GameObject>>();

        _toBeRemovedthreadLocker = new System.Object();
        _toBeMoved = new List<Util.Pair<GameObject, Vector3>>();

        // Inicializar highlights
        if(TileHighlight._instance)
            TileHighlight._instance.hideTileHighlights();

        // Desenhar cursor
        _cursor = Instantiate(_cursorPrefab, Util.Constants.getTileCenter(Vector2Int.zero), Quaternion.Euler(180, 0, 0)) as GameObject;
        _cursor.transform.SetParent(transform);
        _cursor.transform.position = new Vector3(0, 20, 0);
        _cursorTarget = GameObject.Find("CursorTarget");

        // Inicializar engine
        Game.BoardPositionChanged += BoardPositionChangedEvent;
        Game.GamePaused += dummy;
        Game.GameResumed += dummy;
        Game.GameSaved += dummy;
        Game.SettingsUpdated += dummy;
        Game.PlayerWhite.Brain.MoveConsideredEvent += dummy;
        Game.PlayerBlack.Brain.MoveConsideredEvent += dummy;
        Game.PlayerWhite.Brain.ThinkingBeginningEvent += dummy;
        Game.PlayerBlack.Brain.ThinkingBeginningEvent += dummy;
        Game.PlayerToPlay = Game.PlayerWhite;
        Game.New();
	}

    private void dummy()
    {
        Debug.Log("Dummy");
    }

    private void BoardPositionChangedEvent()
    {
        updatePieces();
        updateHighlights();
    }

    private void updatePieces()
    {
        foreach(var item in _piecesGameObject)
            if(!item.Key.IsInPlay)
            {
                lock(_toBeDestroyedthreadLocker)
                {
                    _toBeDestroyed.Add(item);
                }
            }

        Square square;
        for (int intOrdinal = 0; intOrdinal < Board.SquareCount; intOrdinal++)
        {
            square = Board.GetSquare(intOrdinal);
            if(square != null && square.Piece != null)
            {
                try
                {
                    var go = _piecesGameObject[square.Piece];
                    var positionTo = Util.Constants.getTileCenter(new Vector2Int(square.File, square.Rank));
                    lock(_toBeRemovedthreadLocker)
                    {
                        _toBeMoved.Add(new Util.Pair<GameObject, Vector3>(go, positionTo));
                    }
                }
                catch(KeyNotFoundException)
                {
                    int index = -1;
                    switch(square.Piece.Name)
                    {
                        case Piece.PieceNames.Pawn: index = 3; break;
                        case Piece.PieceNames.Rook: index = 5; break;
                        case Piece.PieceNames.Knight: index = 2; break;
                        case Piece.PieceNames.Bishop: index = 0; break;
                        case Piece.PieceNames.Queen: index = 4; break;
                        case Piece.PieceNames.King: index = 1; break;
                    }

                    var color = square.Piece.Player.Colour;
                    spawnChessPiece(index, new Vector2Int(square.File, square.Rank), color);
                }
            }
        }
    }

    private void updateHighlights()
    {
    }
	
	private void Update()
	{
        mouseLeftButtonClicked();
        updateCursor();
        movePieces();
        destroyOldGameObjects();

#if DEBUG
        drawChessboard();
#endif
	}

    private void movePieces()
    {
        lock(_toBeRemovedthreadLocker)
        {
            foreach(var item in _toBeMoved)
            {
                item.first.transform.position = item.second;
            }
            _toBeMoved.Clear();
        }
    }

    private void destroyOldGameObjects()
    {
        lock(_toBeDestroyedthreadLocker)
        {
            foreach(var item in _toBeDestroyed)
            {
                _piecesGameObject.Remove(item.Key);
                Destroy(item.Value);
            }
            _toBeDestroyed.Clear();
        }
    }

    public void mouseLeftButtonClicked()
    {
        if(!Input.GetMouseButtonDown(0))
            return;
            
        if(!Camera.main)
            return;

        RaycastHit hit;
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray, out hit, float.MaxValue, LayerMask.GetMask("ChessPlane")))
            _tileUnderCursor = new Vector2Int((int)(hit.point.x / Util.Constants._scale), (int)(hit.point.z / Util.Constants._scale));
        else
            _tileUnderCursor = _none;

        updateSelection();
    }

    private void updateCursor()
    {
        if(!_cursorTarget)
        {
            _cursor.transform.position = new Vector3(-1000, -1000, -1000);
            return;
        }
        
        var pos = _cursorTarget.transform.position;
        _cursor.transform.position = new Vector3(pos.x, _cursor.transform.position.y, pos.z);
    }

    private void selectPiece()
    {
        if(_tileUnderCursor == _none)
            return;
       
        var piece = Board.GetPiece(_tileUnderCursor.x, _tileUnderCursor.y);
        if(piece == null || piece.Player != Game.PlayerToPlay)
            return;

        _selectedPiece = piece;
        var go = _piecesGameObject[_selectedPiece];

        // Set selection outline
        _previousMat = go.GetComponentsInChildren<MeshRenderer>()[0].material;
        _selectedMat.mainTexture = _previousMat.mainTexture;
        changeMaterial(go, _selectedMat);

        bool[,] allowedMoves = new bool[8, 8];
        Moves legalMoves = new Moves();
        _selectedPiece.GenerateLegalMoves(legalMoves);
        foreach(Move move in legalMoves)
            allowedMoves[move.To.File, move.To.Rank] = true;
            
        TileHighlight._instance.highlightPossibleMoves(allowedMoves);
    }

    private void changeMaterial(GameObject piece, Material newMat)
    {
        MeshRenderer[] children;
        children = piece.GetComponentsInChildren<MeshRenderer>();
        foreach(var rend in children)
        {
            var mats = new Material[rend.materials.Length];
            for (var j = 0; j < rend.materials.Length; j++)
            {
                mats[j] = newMat;
            }
            rend.materials = mats;
        }
    }

    private void movePiece()
    {
        var squareTo = Board.GetSquare(_tileUnderCursor.x, _tileUnderCursor.y);
        Moves legalMoves = new Moves();
        _selectedPiece.GenerateLegalMoves(legalMoves);
        foreach(Move move in legalMoves)
        {
            if(move.To != squareTo)
                continue;

            //var pieceInDestination = Board.GetPiece(_tileUnderCursor.x, _tileUnderCursor.y);

            Game.MakeAMove(move.Name, move.Piece, move.To);
        }

        changeMaterial(_piecesGameObject[_selectedPiece], _previousMat);
        TileHighlight._instance.hideTileHighlights();
        _selectedPiece = null;
    }

	private void updateSelection()
    {
        if(_tileUnderCursor == _none)
            return;
        
        if(_selectedPiece != null)
        {
            movePiece();
            selectPiece();
        }
        else
        {
            selectPiece();
        }
	}
      
    public void selectButtonClicked()
    {
        _tileUnderCursor = new Vector2Int((int)(_cursor.transform.position.x / Util.Constants._scale), 
                                          (int)(_cursor.transform.position.z / Util.Constants._scale));
        if(!onBoard(_tileUnderCursor))
        {
            _tileUnderCursor = _none;
            return;
        }

        updateSelection();
    }

    private bool onBoard(Vector2Int v)
    {
        return (v.x >= 0 && v.x < 8) && (v.y >= 0 && v.y < 8);
    }

	private void drawChessboard()
	{
		var widthLine = Vector3.right * 8;
		var heightLine = Vector3.forward * 8; 

		for(int i = 0; i <= 8; i++)
		{
			var start = Vector3.forward * i;
			Debug.DrawLine(start, start + widthLine);

			start = Vector3.right * i;
			Debug.DrawLine(start, start + heightLine);
		}
	}

    private void spawnChessPiece(int index, Vector2Int position, Player.PlayerColourNames color)
    {
        var quaternion = Quaternion.identity;
        if(color == Player.PlayerColourNames.Black) // black pieces
            quaternion = Quaternion.Euler(0, 180, 0); //face pieces to the center of the board

        var go = Instantiate(_chessPiecesPrefabs[index], Util.Constants.getTileCenter(position), quaternion) as GameObject;
        go.transform.SetParent(transform);
        go.transform.localScale = go.transform.localScale * 5.0f;

        var piece = Board.GetPiece(position.x, position.y);
        _piecesGameObject[piece] = go;
    }
        
    private void endGame()
    {
        foreach(var item in _piecesGameObject)
            Destroy(item.Value);

        Start();
    }
}
