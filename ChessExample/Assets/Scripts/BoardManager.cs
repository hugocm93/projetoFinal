using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager _instance{set; get;}
    private readonly Vector2Int _none = new Vector2Int(-1, -1);

    private bool[,] _allowedMoves{ get; set;}

    private ChessPiece.Color _turn;
    public ChessPiece[,] _chessPieces{ set; get;}
	public List<GameObject> _chessPiecesPrefabs;
	private List<GameObject> _activeChessPieces;
    private Material _previousMat;
    public Material _selectedMat;
    private ChessPiece _selectedPiece;
    private Vector2Int _tileUnderCursor;

	void Start()
	{
        _instance = this;
        _turn = ChessPiece.Color.Black;
        _chessPieces = new ChessPiece[8, 8];
		_tileUnderCursor = _none;
		_activeChessPieces = new List<GameObject>();
        TileHighlight._instance.hideTileHighlights();
		spawnAllPieces();
	}
	
	private void Update()
	{
		updateSelection();
		drawChessboard();

        if(Input.GetMouseButtonDown(0)) // left button
        {
            if(_tileUnderCursor != _none)
            {
                if(_selectedPiece)
                {
                    movePiece();
                    selectPiece();
                }
                else
                    selectPiece();
            }
        }
	}

    private void selectPiece()
    {
        if(_tileUnderCursor == _none)
            return;
        
        var piece = _chessPieces[_tileUnderCursor.x, _tileUnderCursor.y];
        if(!piece || (piece._color != _turn))
            return;
       
        _selectedPiece = piece;

        // Set selection outline
        _previousMat = _selectedPiece.GetComponentsInChildren<MeshRenderer>()[0].material;
        _selectedMat.mainTexture = _previousMat.mainTexture;
        ChangeMaterial(_selectedPiece, _selectedMat);

        _allowedMoves = _selectedPiece.possibleMoves();
        TileHighlight._instance.highlightPossibleMoves(_allowedMoves);
    }

    void ChangeMaterial(ChessPiece piece, Material newMat)
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
        if(_allowedMoves[_tileUnderCursor.x, _tileUnderCursor.y])
        {
            var pieceInDestination = _chessPieces[_tileUnderCursor.x, _tileUnderCursor.y];
            if(pieceInDestination && pieceInDestination._color != _turn)
            {
                if(pieceInDestination.GetType() == typeof(King))
                {
                    endGame();
                    return;
                }

                _activeChessPieces.Remove(pieceInDestination.gameObject);
                Destroy(pieceInDestination.gameObject);
            }

            _chessPieces[_selectedPiece._position.x, _selectedPiece._position.y] = null;
            _selectedPiece.transform.position = Util.getTileCenter(_tileUnderCursor);
            _selectedPiece._position = _tileUnderCursor;
            _chessPieces[_tileUnderCursor.x, _tileUnderCursor.y] = _selectedPiece;

            _turn = _turn == ChessPiece.Color.Black ? ChessPiece.Color.White : ChessPiece.Color.Black;
        }

        ChangeMaterial(_selectedPiece, _previousMat);
        TileHighlight._instance.hideTileHighlights();
        _selectedPiece = null;
    }

	private void updateSelection()
	{
		if(!Camera.main)
			return;

		RaycastHit hit;
		if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("ChessPlane")))
			_tileUnderCursor = new Vector2Int((int)hit.point.x, (int)hit.point.z);
		else
            _tileUnderCursor = _none;
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

		if(_tileUnderCursor != new Vector2Int(-1, -1))
		{
			var start = Vector3.forward * _tileUnderCursor.y + Vector3.right * _tileUnderCursor.x;
			Debug.DrawLine(start, start + new Vector3(1, 0, 1));
			Debug.DrawLine(start + new Vector3(1, 0, 0), start + new Vector3(0, 0, 1));
		}
	}

    private void spawnChessPieces(int index, Vector2Int position, Quaternion quaternion)
    {
        var go = Instantiate(_chessPiecesPrefabs[index], Util.getTileCenter(position), quaternion) as GameObject;
        go.transform.SetParent(transform);
        go.transform.localScale = go.transform.localScale * 0.5f;
        _chessPieces[position.x, position.y] = go.GetComponent<ChessPiece>();
        _chessPieces[position.x, position.y]._position = position;

        _activeChessPieces.Add(go); 
    }

	private void spawnAllPieces()
	{
        for(int i = 0; i < 8; i++)
		{
            for(int j = 0; j < 8; j++)
			{
				int index = -1; 
                if(i > 1 && i < 6)
                    break;
                
                if(i == 1 || i == 6)
                    index = 3; // pawn
				else if(j == 0 || j == 7)
					index = 5; // rook
				else if(j == 1 || j == 6)
					index = 2; // knight
				else if(j == 2 || j == 5)
					index = 0; // bishop
				else if(j == 3)
                    index = 4; // queen
                else if (j == 4)
                    index = 1; // king
					

                var quaternion = Quaternion.identity;
                if(i >= 6) // white pieces
                {
                    index += 6; 
                    quaternion = Quaternion.Euler(0, 180, 0); //face pieces to the center of the board
                }
                    
                spawnChessPieces(index, new Vector2Int(j, i), quaternion);
			}
		}
	}

    private void endGame()
    {
        Debug.Log(_turn + " wins");

        foreach(var obj in _activeChessPieces)
            Destroy(obj);

        Start();
    }
}