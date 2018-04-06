using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    private const float _tile_size = 1.0f;
    private const float _tile_offset = 0.5f;
    private readonly Vector2Int _none = new Vector2Int(-1, -1);

    private ChessPiece.Color _turn;
    public ChessPiece[,] _chessPieces{ set; get;}
	public List<GameObject> _chessPiecesPrefabs;
	private List<GameObject> _activeChessPieces;
    private ChessPiece _selectedPiece;
    private Vector2Int _tileUnderCursor;

	void Start()
	{
        _turn = ChessPiece.Color.Black;
        _chessPieces = new ChessPiece[8, 8];
		_tileUnderCursor = _none;
		_activeChessPieces = new List<GameObject>();
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
                    movePiece();
                else
                    selectPiece();
            }
        }
	}

    private void selectPiece()
    {
        ChessPiece piece = _chessPieces[_tileUnderCursor.x, _tileUnderCursor.y];
        if(!piece || (piece._color != _turn))
            return;

        _selectedPiece = piece;
    }

    private void movePiece()
    {
        if(_selectedPiece.possibleMove(_tileUnderCursor))
        {
            _chessPieces[_selectedPiece._position.x, _selectedPiece._position.y] = null;
            _selectedPiece.transform.position = getTileCenter(_tileUnderCursor);
            _chessPieces[_tileUnderCursor.x, _tileUnderCursor.y] = _selectedPiece;

            _turn = _turn == ChessPiece.Color.Black ? ChessPiece.Color.White : ChessPiece.Color.Black;
        }

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
		Vector3 widthLine = Vector3.right * 8;
		Vector3 heightLine = Vector3.forward * 8; 

		for(int i = 0; i <= 8; i++)
		{
			Vector3 start = Vector3.forward * i;
			Debug.DrawLine(start, start + widthLine);

			start = Vector3.right * i;
			Debug.DrawLine(start, start + heightLine);
		}

		if(_tileUnderCursor != new Vector2Int(-1, -1))
		{
			Vector3 start = Vector3.forward * _tileUnderCursor.y + Vector3.right * _tileUnderCursor.x;
			Debug.DrawLine(start, start + new Vector3(1, 0, 1));
			Debug.DrawLine(start + new Vector3(1, 0, 0), start + new Vector3(0, 0, 1));
		}
	}

    private void spawnChessPieces(int index, Vector2Int position, Quaternion quaternion)
	{
        GameObject go = Instantiate(_chessPiecesPrefabs[index], getTileCenter(position), quaternion) as GameObject;
        go.transform.SetParent(transform);
		go.transform.localScale = go.transform.localScale * 0.5f;
        _chessPieces[position.x, position.y] = go.GetComponent<ChessPiece>();
        _chessPieces[position.x, position.y]._position = position;

		_activeChessPieces.Add(go); 
	}

    private Vector3 getTileCenter(Vector2Int position)
	{
		Vector3 origin = Vector3.zero;
        origin.x = _tile_size * position.x + _tile_offset;
        origin.z = _tile_size * position.y + _tile_offset; 

		return origin;
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
					index = 1; // king
                else if (j == 4)
					index = 4; // queen

                Quaternion quaternion = Quaternion.identity;
                if(i >= 6) // white pieces
                {
                    index += 6; 
                    quaternion = Quaternion.Euler(0, 180, 0); //face pieces to the center of the board
                }
                    
                spawnChessPieces(index, new Vector2Int(j, i), quaternion);
			}
		}
	}
}