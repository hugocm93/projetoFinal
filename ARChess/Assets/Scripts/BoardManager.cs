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
    public enum Color{White, Black};

    private Moves _possibleMoves;
    private Square _squareFrom;
    private Square _squareTo;
    private Square _squareLastFrom;
    private Square _squareLastTo;

    private Material _previousMat;
    private Vector2Int _tileUnderCursor;
    private GameObject _cursor;
    private GameObject _cursorTarget;
	private List<GameObject> _activeChessPieces;
    private readonly Vector2Int _none = new Vector2Int(-1, -1);
    private GameObject _selectedPiece;

    private bool[,] _allowedMoves{ get; set;}

    public Vector2Int _enPassant;

	void Start()
	{
        // Construtor
        _instance = this;
		_tileUnderCursor = _none;
		_activeChessPieces = new List<GameObject>();

        // Inicializar highlights
        if(TileHighlight._instance)
            TileHighlight._instance.hideTileHighlights();

        // Desenhar cursor
        _cursor = Instantiate(_cursorPrefab, Util.getTileCenter(Vector2Int.zero), Quaternion.Euler(180, 0, 0)) as GameObject;
        _cursor.transform.SetParent(transform);
        _cursor.transform.position = new Vector3(0, 20, 0);
        _cursorTarget = GameObject.Find("CursorTarget");

        // Inicializar engine
        Game.BoardPositionChanged += BoardPositionChangedEvent;
        Game.GamePaused += dummy;
        Game.GameResumed += dummy;
        Game.GameSaved += dummy;
        Game.SettingsUpdated += dummy;
        Game.PlayerToPlay = Game.PlayerBlack;
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
        Debug.Log("updatePieces");
        Square square;

        for (int intOrdinal = 0; intOrdinal < Board.SquareCount; intOrdinal++)
        {
            square = Board.GetSquare(intOrdinal);

            if(square != null && square.Piece != null)
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
                spawnChessPieces(index, new Vector2Int(square.File, square.Rank), color);
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
        updateSelection();

#if DEBUG
        drawChessboard();
#endif
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
            _tileUnderCursor = new Vector2Int((int)(hit.point.x / Util._scale), (int)(hit.point.z / Util._scale));
        else
            _tileUnderCursor = _none;
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
       
        // //TODO: Atualizar peca selecionada
        //_selectedPiece = piece;

        // Set selection outline
        _previousMat = _selectedPiece.GetComponentsInChildren<MeshRenderer>()[0].material;
        _selectedMat.mainTexture = _previousMat.mainTexture;
        changeMaterial(_selectedPiece, _selectedMat);

        //TODO: atualizar tiles de movimentos possiveis
        //_allowedMoves = _selectedPiece.possibleMoves();
        //TileHighlight._instance.highlightPossibleMoves(_allowedMoves);
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
        //changeMaterial(_selectedPiece, _previousMat);
        //TileHighlight._instance.hideTileHighlights();
        //_selectedPiece = null;
    }

	private void updateSelection()
    {
        if(_tileUnderCursor == _none)
            return;
        
        if(_selectedPiece)
        {
            movePiece();
            selectPiece();
        }
        else
            selectPiece();
	}
      
    public void selectButtonClicked()
    {
        _tileUnderCursor = new Vector2Int((int)(_cursor.transform.position.x / Util._scale), 
                                          (int)(_cursor.transform.position.z / Util._scale));
        if(!onBoard(_tileUnderCursor))
        {
            _tileUnderCursor = _none;
            return;
        }
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

    private void spawnChessPieces(int index, Vector2Int position, Player.PlayerColourNames color)
    {
        var quaternion = Quaternion.identity;
        if(color == Player.PlayerColourNames.Black) // black pieces
            quaternion = Quaternion.Euler(0, 180, 0); //face pieces to the center of the board

        var go = Instantiate(_chessPiecesPrefabs[index], Util.getTileCenter(position), quaternion) as GameObject;
        go.transform.SetParent(transform);
        go.transform.localScale = go.transform.localScale * 5.0f;

        _activeChessPieces.Add(go); 
    }
        
    private void endGame()
    {
        foreach(var obj in _activeChessPieces)
            Destroy(obj);

        Start();
    }
}
