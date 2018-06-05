using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using UnityEngine;
using Vuforia;
using SharpChess.Model;

public class BoardManager : MonoBehaviour
{
    public static BoardManager _instance{set; get;}

	public List<GameObject> _chessPiecesPrefabs;
    public Material _selectedMat;
    public GameObject _cursorPrefab;

    private AudioSource _audioSourceDragging;
    private AudioSource _audioSourceClicking;
    private Moves _possibleMoves;
    private Square _squareFrom;
    private Square _squareTo;
    private Square _squareLastFrom;
    private Square _squareLastTo;
    private Piece _selectedPiece;

    private Dictionary<Piece, GameObject> _piecesGameObject;

    private System.Object _toBeDestroyedThreadLocker;
    private List<KeyValuePair<Piece, GameObject>> _toBeDestroyed;

    private System.Object _toBeMovedThreadLocker;
    private List<Util.Pair<Piece, Vector3>> _toBeMoved;

    private System.Object _toBeUpdatedThreadLocker;
    private string _toBeUpdated;

    private Material _previousMat;
    private Vector2Int _tileUnderCursor;
    private GameObject _cursor;
    private GameObject _cursorTarget;

    public GameObject _statusPrefab;
    private GameObject _status;

    private string _fileName;

	void Start()
	{
        // Construtor
        _instance = this;
		_tileUnderCursor = Util.Constants._none;
        _piecesGameObject = new Dictionary<Piece, GameObject>();

        _toBeDestroyedThreadLocker = new System.Object();
        _toBeDestroyed = new List<KeyValuePair<Piece, GameObject>>();

        _toBeMovedThreadLocker = new System.Object();
        _toBeMoved = new List<Util.Pair<Piece, Vector3>>();

        _toBeUpdatedThreadLocker = new System.Object();

        // Inicializar highlights
        if(TileHighlight._instance)
            TileHighlight._instance.hideTileHighlights();

        // Desenhar cursor
        _cursor = Instantiate(_cursorPrefab, Util.Constants.getTileCenter(Vector2Int.zero), Quaternion.Euler(180, 0, 0)) as GameObject;
        _cursor.transform.SetParent(transform);

        _cursor.transform.position = new Vector3(0, 20, 0);
        _cursorTarget = GameObject.Find("CursorTarget");
        _audioSourceDragging = GameObject.Find("AudioSource").GetComponent<AudioSource>();
        _audioSourceClicking = GameObject.Find("AudioSource2").GetComponent<AudioSource>();
        _audioSourceDragging.loop = true;

        // Inicializar engine
        Game.BoardPositionChanged += BoardPositionChangedEvent;
        Game.BoardPositionChanged += dummy;
        Game.GamePaused += dummy;
        Game.GameResumed += dummy;
        Game.GameSaved += dummy;
        Game.SettingsUpdated += dummy;
        Game.PlayerWhite.Brain.MoveConsideredEvent += dummy;
        Game.PlayerBlack.Brain.MoveConsideredEvent += dummy;
        Game.PlayerWhite.Brain.ThinkingBeginningEvent += dummy;
        Game.PlayerBlack.Brain.ThinkingBeginningEvent += dummy;
        Game.PlayerToPlay = Game.PlayerWhite;
        Game.ShowThinking = true;
        Game.UseRandomOpeningMoves = true;
        Game.BackupGamePath = getPath("saveBackup");
        interfaceButtonClicked(Util.ButtonEnum.File1);
	}

    private void showStatusMessage(bool show, string message = "")
    {
        if(!show && _status != null)
        {
            Destroy(_status);
            _status = null;
        }
        else if(show)
        {
            if(_status == null)
                _status = Instantiate(_statusPrefab, Util.Constants.getBoardCenter(), Quaternion.identity) as GameObject;
            _status.GetComponent<TextMesh>().text = message;
            _status.transform.GetChild(0).gameObject.GetComponent<TextMesh>().text = message;
        }
    }
        
    private string getPath(string fileName = null)
    {
        if(fileName == null)
            fileName = _fileName;
        
        #if UNITY_EDITOR
        return Application.dataPath +"/Resources/" + fileName + ".xml";
        #elif UNITY_ANDROID
        return Application.persistentDataPath + fileName + ".xml";
        #elif UNITY_IPHONE
        return Application.persistentDataPath+"/" + fileName + ".xml";
        #else
        return Application.dataPath +"/"+ fileName + ".xml";
        #endif
        }

    private void Update()
    {
        mouseLeftButtonClicked();
        updateCursor();
        movePieces();
        destroyOldGameObjects();
        updateStatus();
        billBoardEffect();

        #if DEBUG
        drawChessboard();
        #endif
    }

    private void updateStatus()
    {
        lock(_toBeUpdatedThreadLocker)
        {
            if(_toBeUpdated != null)
                showStatusMessage(true, _toBeUpdated);
            else
                showStatusMessage(false);
        }
    }

    private void verifyCheck()
    {
        lock(_toBeUpdatedThreadLocker)
        {
            _toBeUpdated = null;

            if(Game.PlayerBlack.IsInCheck || Game.PlayerWhite.IsInCheck)
                _toBeUpdated = "Check!";
            if(Game.PlayerBlack.IsInCheckMate || Game.PlayerWhite.IsInCheckMate)
                _toBeUpdated = "Check Mate!";
        }
    }

    private void billBoardEffect()
    {
        if(_status == null)
            return; 

        Camera camera;
        var ARcameraObj = GameObject.Find("ARCamera");
        if(ARcameraObj == null)
            camera = Camera.main;
        else
            camera = ARcameraObj.GetComponent<Camera>();

        //Look at
        var point = camera.transform.position;
        _status.transform.rotation = Quaternion.LookRotation(_status.transform.position - point);
    }

    private void dummy()
    {
    }

    private void BoardPositionChangedEvent()
    {
        updatePieces();
        verifyCheck();
    }

    private void updatePieces()
    {
        foreach(var item in _piecesGameObject)
            if(!item.Key.IsInPlay)
            {
                lock(_toBeDestroyedThreadLocker)
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
                    var positionTo = Util.Constants.getTileCenter(new Vector2Int(square.File, square.Rank));
                    var go = _piecesGameObject[square.Piece];
                    lock(_toBeMovedThreadLocker)
                    {
                        _toBeMoved.Add(new Util.Pair<Piece, Vector3>(square.Piece, positionTo));
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
                    if(color == Player.PlayerColourNames.White)
                        index += 6;
                    
                    spawnChessPiece(index, new Vector2Int(square.File, square.Rank), color);
                }
            }
        }
    }

    private void movePieces()
    {
        lock(_toBeMovedThreadLocker)
        {
            List<Util.Pair<Piece, Vector3>> done = new List<Util.Pair<Piece, Vector3>>();
            foreach(var item in _toBeMoved)
            {
                if(containsPlayerMove() && item.first.Player != Game.PlayerToPlay)
                    continue;

                var go = _piecesGameObject[item.first];
                var dist = 30 + Vector3.Distance(go.transform.position, item.second);
                go.transform.position = Vector3.MoveTowards(go.transform.position, item.second,  dist * Time.deltaTime);
                if(go.transform.position == item.second)
                    done.Add(item);
            }

            foreach(var item in done)
                _toBeMoved.Remove(item);

            if(_toBeMoved.Count == 0)
                _audioSourceDragging.Pause();
            else if(!_audioSourceDragging.isPlaying)
                _audioSourceDragging.UnPause();
        }
    }

    private bool containsPlayerMove()
    {
        lock(_toBeMovedThreadLocker)
        {
            foreach(var item in _toBeMoved)
                if(item.first.Player == Game.PlayerToPlay)
                    return true;
        }
        return false;
    }

    private void destroyOldGameObjects()
    {
        lock(_toBeMovedThreadLocker)
        {
            if(_toBeMoved.Count != 0)
                return;
        }

        lock(_toBeDestroyedThreadLocker)
        {
            foreach(var item in _toBeDestroyed)
            {
                if(!onBoard(Util.Constants.getTile(item.Value.transform.position)))
                    continue;

                var tileSize = Util.Constants._tile_size * Util.Constants._scale;
                var boardLength = 9 * tileSize;
                var blackArea = new Vector3(boardLength, 0, Util.Constants._origin.z + boardLength / 2);
                var whiteArea = new Vector3(boardLength, 0, Util.Constants._origin.z);
                var area = item.Key.Player.Colour == Player.PlayerColourNames.White ? whiteArea : blackArea;
                item.Value.transform.position = area + new Vector3(4 * Random.value * tileSize, 0, 4 * Random.value * tileSize);
            }
            _toBeDestroyed.Clear();
        }
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
        if(_tileUnderCursor == Util.Constants._none)
            return;
       
        var piece = Board.GetPiece(_tileUnderCursor.x, _tileUnderCursor.y);
        if(piece == null || piece.Player != Game.PlayerToPlay)
            return;

        _selectedPiece = piece;
        GameObject go;
        try
        {
            go = _piecesGameObject[_selectedPiece];
        }
        catch(System.Exception)
        {
            _selectedPiece = null;
            return;
        }

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
            
        unSelectPiece();
    }

    private void unSelectPiece()
    {
        if(_selectedPiece != null)
        {
            changeMaterial(_piecesGameObject[_selectedPiece], _previousMat);
            TileHighlight._instance.hideTileHighlights();
            _selectedPiece = null;
        }
    }

	private void updateSelection()
    {
        if(_tileUnderCursor == Util.Constants._none)
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
        lock(_toBeMovedThreadLocker)
        {
            if(_toBeMoved.Count != 0)
                return;
        }

        _audioSourceClicking.Play();

        _tileUnderCursor = Util.Constants.getTile(_cursor.transform.position);
        if(!onBoard(_tileUnderCursor))
        {
            _tileUnderCursor = Util.Constants._none;
        }
        else
        {
            updateSelection();
            return;
        }

        RaycastHit hit;
        var ray = new Ray(_cursor.transform.position, Vector3.down);
        foreach(Util.ButtonEnum button in Util.ButtonEnum.GetValues(typeof(Util.ButtonEnum)))
        {
            var layerName = Util.Constants.ButtonEnumToString(button);
            if(Physics.Raycast(ray, out hit, float.MaxValue, LayerMask.GetMask(layerName)))
            {
                interfaceButtonClicked(button);
                return;
            }
        }
    }

    public void mouseLeftButtonClicked()
    {
        lock(_toBeMovedThreadLocker)
        {
            if(_toBeMoved.Count != 0 || !Input.GetMouseButtonDown(0) || !Camera.main)
                return;
        }

        _audioSourceClicking.Play();

        RaycastHit hit;
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray, out hit, float.MaxValue, LayerMask.GetMask("ChessPlane")))
            _tileUnderCursor = Util.Constants.getTile(hit.point);
        else
            _tileUnderCursor = Util.Constants._none;
        updateSelection();

        foreach(Util.ButtonEnum button in Util.ButtonEnum.GetValues(typeof(Util.ButtonEnum)))
        {
            var layerName = Util.Constants.ButtonEnumToString(button);
            if(Physics.Raycast(ray, out hit, float.MaxValue, LayerMask.GetMask(layerName)))
            {
                interfaceButtonClicked(button);
                return;
            }
        }
    }

    void interfaceButtonClicked(Util.ButtonEnum buttonEnum)
    {
        unSelectPiece();

        switch(buttonEnum)
        {
            case Util.ButtonEnum.NewGame: 
                Game.New();
                break;

            case Util.ButtonEnum.Undo:
                Game.BoardPositionChanged -= BoardPositionChangedEvent;
                Game.UndoMove();
                Game.BoardPositionChanged += BoardPositionChangedEvent;
                Game.UndoMove();
                break;

            case Util.ButtonEnum.Redo:
                Game.BoardPositionChanged -= BoardPositionChangedEvent;
                Game.RedoMove();
                Game.BoardPositionChanged += BoardPositionChangedEvent;
                Game.RedoMove();
                break;

            case Util.ButtonEnum.SaveGame:
                Game.Save(getPath());
            break;

            case Util.ButtonEnum.LoadGame:
                Game.Load(getPath());
            break;

            case Util.ButtonEnum.File1:
            case Util.ButtonEnum.File2:
            case Util.ButtonEnum.File3:
                foreach(var file in new Util.ButtonEnum[]{Util.ButtonEnum.File1, Util.ButtonEnum.File2, Util.ButtonEnum.File3})
                {
                    var f = GameObject.Find(Util.Constants.ButtonEnumToString(file));
                    f.GetComponent<MeshRenderer>().material.color = Color.white;
                }
                _fileName = Util.Constants.ButtonEnumToString(buttonEnum);
                var go = GameObject.Find(_fileName);
                go.GetComponent<MeshRenderer>().material.color = Color.yellow;
            break;
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
