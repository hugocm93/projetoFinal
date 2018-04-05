using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
	private const float _tile_size = 1.0f;
	private const float _tile_offset = 0.5f;

	private Vector2Int _selection;
	public List<GameObject> _chessPiecesPrefabs;
	private List<GameObject> _activeChessPieces;

	// Use this for initialization
	void Start()
	{
		_selection = new Vector2Int(-1, -1);
		_activeChessPieces = new List<GameObject>();
		spawnAllPieces();
	}
	
	// Update is called once per frame
	private void Update()
	{
		updateSelection();
		drawChessboard();
	}

	private void updateSelection()
	{
		if(!Camera.main)
			return;

		RaycastHit hit;
		if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("ChessPlane")))
		{ 
			Debug.Log(hit.point);
			_selection = new Vector2Int((int)hit.point.x, (int)hit.point.z);
		}
		else
		{
			_selection = new Vector2Int(-1, -1);
		}
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

		if(_selection != new Vector2Int(-1, -1))
		{
			Vector3 start = Vector3.forward * _selection.y + Vector3.right * _selection.x;
			Debug.DrawLine(start, start + new Vector3(1, 0, 1));
			Debug.DrawLine(start + new Vector3(1, 0, 0), start + new Vector3(0, 0, 1));
		}
	}

	private void spawnChessPieces(int index, Vector3 position)
	{
		GameObject go = Instantiate(_chessPiecesPrefabs[index], position, Quaternion.identity) as GameObject;
		go.transform.Translate(new Vector3(0, 0.7f, 0));
		go.transform.Rotate(new Vector3(90, 0, 0));
		go.transform.localScale = go.transform.localScale * 0.35f;

		_activeChessPieces.Add(go); 
	}

	private Vector3 getTileCenter(int x, int y)
	{
		Vector3 origin = Vector3.zero;
		origin.x = _tile_size * x + _tile_offset;
		origin.z = _tile_size * y + _tile_offset; 

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

                if(i >= 6)
                    index += 6; // white pieces
				
				spawnChessPieces(index, getTileCenter(j, i));
			}
		}
	}
}