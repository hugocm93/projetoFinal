//Autor: Hugo C. Machado

using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using UnityEngine;
using Vuforia;
using SharpChess.Model;
using UnityEngine.SceneManagement;

public class BoardManager : MonoBehaviour
{
    public static BoardManager _instance{set; get;}

    //Prefabs
	public List<GameObject> _chessPiecesPrefabs;
    public GameObject _statusPrefab;

    //Elementos de cena - instanciados no unity
    public AudioSource _audioSourceClicking;
    public AudioSource _audioSourceDragging;
    public GameObject _cursorTarget;
    public GameObject _cursor;
    public GameObject _fileSelection;
    public GameObject _buttonSelection;
    public GameObject _virtualButton;
    public Material _pieceSelectedMaterial;
    public Camera _camera;

    //Elementos de cena - instanciados no script
    private GameObject _whiteTimer;
    private GameObject _blackTimer;
    private GameObject _checkLabel;
    
    //Controle de captura de pecas
    private Dictionary<Piece, GameObject> _piecesGameObject;
    private System.Object _pieceToBeCapturedThreadLocker;
    private List<KeyValuePair<Piece, GameObject>> _pieceToBeCaptured;

    //Controle de movimentacao de pecas
    private System.Object _pieceToBeMovedThreadLocker;
    private List<Util.Pair<Piece, Vector3>> _pieceToBeMoved;
    
    // Controle de label 3d de check e checkmate
    private System.Object _checkLabelThreadLock;
    private string _checkText;

    //Controle de efeito de billboard para labels 3d
    private List<GameObject> _3dLabels;

    //Controle selecao de pecas
    private Piece _selectedPiece;
    private Material _previousPieceMaterial;
    private Vector3 _lastCursorPosition;
    private Vector2Int _selectedSquare;
    private Util.Scheduler _clickScheduler;

    //Controle de tempo geral
    private Util.Scheduler _generalScheduler;

    //Controle de arquivo de save
    private string _fileName;

    void Awake()
    {
        _instance = this;
    }

	void Start()
	{
        //Inicializar campos
        _selectedSquare = Util.Constants.getInstance()._none;
        _piecesGameObject = new Dictionary<Piece, GameObject>();
        _pieceToBeCapturedThreadLocker = new System.Object();
        _pieceToBeCaptured = new List<KeyValuePair<Piece, GameObject>>();
        _pieceToBeMovedThreadLocker = new System.Object();
        _pieceToBeMoved = new List<Util.Pair<Piece, Vector3>>();
        _checkLabelThreadLock = new System.Object();
        _generalScheduler = new Util.Scheduler();
        _clickScheduler = new Util.Scheduler();
        _3dLabels = new List<GameObject>();

        // Inicializar highlights
        if(TileHighlight._instance)
            TileHighlight._instance.hideTileHighlights();

        // Inicializar cursor
        if(_cursor != null)
        {
            _cursor.transform.SetParent(transform);
            _lastCursorPosition = _cursor.transform.position;
        }

        // 3d labels de timer
        var x = (Util.Constants.getInstance()._boardLength + Util.Constants.getInstance().getOrigin().x) / -2;
        var blackPoint = new Vector3(x, 0, Util.Constants.getInstance().getOrigin().z + Util.Constants.getInstance()._boardLength);
        var whitePoint = new Vector3(x, 0, Util.Constants.getInstance().getOrigin().z);
        initializeTimerLabel(out _whiteTimer, whitePoint);
        initializeTimerLabel(out _blackTimer, blackPoint);
        _3dLabels.Add(_whiteTimer);
        _3dLabels.Add(_blackTimer);

        //Inicializar audio
        _audioSourceDragging.loop = true;
        _audioSourceDragging.volume = ConfigModel._sound == true ? 1 : 0;
        _audioSourceClicking.volume = ConfigModel._sound == true ? 1 : 0;

        //Inicializar modo de selecao
        if(ConfigModel._selection == Selection.Time && _virtualButton != null)
            _virtualButton.SetActive(false);

        // Inicializar engine
        addGameCallbacks();

        bool isPlayerWhite = ConfigModel._player == Player.PlayerColourNames.White;
        Game.PlayerWhite.Intellegence =  isPlayerWhite ? 
                                        Player.PlayerIntellegenceNames.Human : Player.PlayerIntellegenceNames.Computer;
        Game.PlayerBlack.Intellegence = isPlayerWhite ? 
                                        Player.PlayerIntellegenceNames.Computer : Player.PlayerIntellegenceNames.Human;
        Game.DifficultyLevel = ConfigModel._difficulty;
        Game.MaximumSearchDepth = ConfigModel._difficulty;
        Game.UseRandomOpeningMoves = true;
        Game.BackupGamePath = getPath("saveBackup");
        Game.ShowThinking = true;

        //Inicializar botao de arquivo
        setSelectedFile(Util.ButtonEnum.File1);

        //Tipo do viewer
        if(!ConfigModel._glassesOn)
            DigitalEyewearARController.Instance.SetEyewearType(DigitalEyewearARController.EyewearType.None);
	}

    private void addGameCallbacks()
    {
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
    }

    private void Update()
    {
        //Checa inputs
        updateCursor();
        checkIfMouseLeftButtonWasClicked();

        //Atualiza interface
        movePiecesToBeMoved();
        capturePiecesToBeCaptured();
        updateTimers();
        updateCheckLabel();
        billboardEffect();
        #if DEBUG
        drawChessboard();
        #endif

        // Executa schedulers
        _generalScheduler.ExecuteSchedule();
        _clickScheduler.ExecuteSchedule();
    }

    private void initializeTimerLabel(out GameObject _timerLabel, Vector3 point)
    {
        _timerLabel = Instantiate(_statusPrefab, Util.Constants.getInstance().getBoardCenter(), Quaternion.identity) as GameObject;
        _timerLabel.transform.SetParent(transform);
        _timerLabel.transform.position = point;
        _timerLabel.GetComponent<TextMesh>().color = Color.black;
        _timerLabel.transform.GetChild(0).gameObject.GetComponent<TextMesh>().color = Color.white;
    } 

    private void showCheckMessage(bool show)
    {
        if(!show && _checkLabel != null)
        {
            _3dLabels.Remove(_checkLabel);
            Destroy(_checkLabel);
            _checkLabel = null;
        }
        else if(show)
        {
            if(_checkLabel == null)
            {
                _checkLabel = Instantiate(_statusPrefab, Util.Constants.getInstance().getBoardCenter(), Quaternion.identity) as GameObject;
                _checkLabel.transform.SetParent(transform);
            }
            if(!_3dLabels.Contains(_checkLabel))
                _3dLabels.Add(_checkLabel);

            _checkLabel.GetComponent<TextMesh>().text = _checkText;
            _checkLabel.transform.GetChild(0).gameObject.GetComponent<TextMesh>().text = _checkText;
        }
    }

    private void updateTimers()
    {
        updateTimer(_whiteTimer, Game.PlayerWhite);
        updateTimer(_blackTimer, Game.PlayerBlack);
    }

    private void updateTimer(GameObject timer, Player player)
    {
        var clock = player.Clock;
        var time = clock.TimeElapsedDisplay.Hours + "h:" + clock.TimeElapsedDisplay.Minutes + "m:" + clock.TimeElapsedDisplay.Seconds + "s";
        timer.GetComponent<TextMesh>().text = time;
        timer.transform.GetChild(0).gameObject.GetComponent<TextMesh>().text = time;
    }
        
    private string getPath(string fileName = null)
    {
        if(fileName == null)
            fileName = _fileName;
        
        #if UNITY_EDITOR
        return Application.dataPath + "/Resources/" + fileName + ".xml";
        #elif UNITY_ANDROID
        return Application.persistentDataPath + fileName + ".xml";
        #elif UNITY_IPHONE
        return Application.persistentDataPath+"/" + fileName + ".xml";
        #else
        return Application.dataPath +"/"+ fileName + ".xml";
        #endif
    }

    private void updateCheckLabel()
    {
        lock(_checkLabelThreadLock)
        {
            showCheckMessage(_checkText != null);
        }
    }

    private void verifyCheck()
    {
        lock(_checkLabelThreadLock)
        {
            _checkText = null;

            if(Game.PlayerBlack.IsInCheck || Game.PlayerWhite.IsInCheck)
                _checkText = "Check!";
            if(Game.PlayerBlack.IsInCheckMate || Game.PlayerWhite.IsInCheckMate)
                _checkText = "Check Mate!";
        }
    }

    private void billboardEffect()
    {
        var targetPoint = _camera.transform.position;
        targetPoint *= 10;

        foreach(var label in _3dLabels)
            label.transform.rotation = Quaternion.LookRotation(label.transform.position - targetPoint);

        var goList = new List<GameObject>();
        foreach(Util.ButtonEnum button in Util.ButtonEnum.GetValues(typeof(Util.ButtonEnum)))
        {
            var go = GameObject.Find(Util.HelperButtonEnum.ButtonEnumToString(button));
            goList.Add(go);
        }  

        goList.Add(_fileSelection);

        foreach(var go in goList)
        {
            var goPos = go.transform.position;
            if(Mathf.Approximately(goPos.z, targetPoint.z))
                return;

            var tetha = Mathf.Rad2Deg * Mathf.Atan((goPos.x - targetPoint.x)/(goPos.z - targetPoint.z));

            if(goPos.z < targetPoint.z)
                tetha += 180;

            go.transform.rotation = Quaternion.Euler(0, tetha, 0);
        }
    }

    private void dummy()
    {
        //Necessaria pois a game engine inevitávelmente tenta chamar 
        //alguns eventos não interessantes, porém devem existir as callbacks
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
                lock(_pieceToBeCapturedThreadLocker)
                {
                    _pieceToBeCaptured.Add(item);
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
                    var positionTo = Util.Constants.getInstance().getTileCenter(new Vector2Int(square.File, square.Rank));
                    var go = _piecesGameObject[square.Piece];
                    lock(_pieceToBeMovedThreadLocker)
                    {
                        _pieceToBeMoved.Add(new Util.Pair<Piece, Vector3>(square.Piece, positionTo));
                    }
                }
                catch(KeyNotFoundException)
                {
                    var color = square.Piece.Player.Colour;
                    int index = getIndex(square.Piece.Name, color);
                    spawnChessPiece(index, new Vector2Int(square.File, square.Rank), color);
                }
            }
        }
    }

    private int getIndex(Piece.PieceNames name, Player.PlayerColourNames color)
    {
        int index = -1;
        switch(name)
        {
            case Piece.PieceNames.Pawn: index = 3; break;
            case Piece.PieceNames.Rook: index = 5; break;
            case Piece.PieceNames.Knight: index = 2; break;
            case Piece.PieceNames.Bishop: index = 0; break;
            case Piece.PieceNames.Queen: index = 4; break;
            case Piece.PieceNames.King: index = 1; break;
        }

        if(color == Player.PlayerColourNames.White)
            index += 6;

        return index;
    }

    private void movePiecesToBeMoved()
    {
        lock(_pieceToBeMovedThreadLocker)
        {
            List<Util.Pair<Piece, Vector3>> done = new List<Util.Pair<Piece, Vector3>>();
            foreach(var item in _pieceToBeMoved)
            {
                if(containsPlayerMove() && item.first.Player != Game.PlayerToPlay)
                    continue;

                var go = _piecesGameObject[item.first];
                var dist = 30 + Vector3.Distance(go.transform.position, item.second);
                go.transform.position = Vector3.MoveTowards(go.transform.position, item.second,  dist * Time.deltaTime);
                if(Vector3.Distance(go.transform.position, item.second) <= 0.01)
                    done.Add(item);
            }

            foreach(var item in done)
            {
                var go = _piecesGameObject[item.first];
                var checkConsistencyList = new List<Util.Pair<Piece.PieceNames, string>>();
                checkConsistencyList.Add(new Util.Pair<Piece.PieceNames, string>(Piece.PieceNames.Pawn, "Pawn"));
                checkConsistencyList.Add(new Util.Pair<Piece.PieceNames, string>(Piece.PieceNames.Queen, "Queen"));
                foreach(var pair in checkConsistencyList)
                    if(item.first.Name == pair.first && !go.name.Contains(pair.second))
                    {
                        var index = getIndex(pair.first, item.first.Player.Colour);
                        _piecesGameObject[item.first] = instantiatePiece(index, go.transform.position, item.first.Player.Colour);
                        Destroy(go);
                    }

                _pieceToBeMoved.Remove(item);
            }

            if(_pieceToBeMoved.Count == 0)
                _audioSourceDragging.Pause();
            else if(!_audioSourceDragging.isPlaying)
                _audioSourceDragging.UnPause();
        }
    }

    private bool containsPlayerMove()
    {
        lock(_pieceToBeMovedThreadLocker)
        {
            foreach(var item in _pieceToBeMoved)
                if(item.first.Player == Game.PlayerToPlay)
                    return true;
        }
        return false;
    }

    private void capturePiecesToBeCaptured()
    {
        lock(_pieceToBeMovedThreadLocker)
        {
            if(_pieceToBeMoved.Count != 0)
                return;
        }

        lock(_pieceToBeCapturedThreadLocker)
        {
            foreach(var item in _pieceToBeCaptured)
            {
                if(!onBoard(Util.Constants.getInstance().getTile(item.Value.transform.position)))
                    continue;

                var tileSize = Util.Constants.getInstance()._tile_size * Util.Constants.getInstance()._scale;
                var boardLength = 8 * tileSize;
                var blackArea = new Vector3(boardLength, 0, Util.Constants.getInstance().getOrigin().z + boardLength / 2);
                var whiteArea = new Vector3(boardLength, 0, Util.Constants.getInstance().getOrigin().z);
                var area = item.Key.Player.Colour == Player.PlayerColourNames.White ? whiteArea : blackArea;
                item.Value.transform.position = area + new Vector3(4 * Random.value * tileSize, 0, 4 * Random.value * tileSize);
            }
            _pieceToBeCaptured.Clear();
        }
    }

    private void updateCursor()
    {
        if(!_cursorTarget)
            return;

        //Atualiza posicao do cursor de acordo com o marcador AR
        var pos = _cursorTarget.transform.position;
        _cursor.transform.position = new Vector3(pos.x, _cursor.transform.position.y, pos.z);

        //Cadastra evento de clique cado o modo de selecao seja timer e o cursor encontra-se parado em um lugar
        var newCursorPos = _cursor.transform.position;
        if(Vector3.Distance(newCursorPos, _lastCursorPosition) > 5 && ConfigModel._selection == Selection.Time)
        {
            _clickScheduler.Clear();
            _lastCursorPosition = newCursorPos;
            _clickScheduler.RegisterEvent(500, new Util.FunctionPointer(selectVirtualButtonClicked));
        }
    }

    private void selectPiece()
    {
        if(_selectedSquare == Util.Constants.getInstance()._none)
            return;
       
        var piece = Board.GetPiece(_selectedSquare.x, _selectedSquare.y);
        if(piece == null || piece.Player != Game.PlayerToPlay || piece.Player.Colour != ConfigModel._player)
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

        //Usa o material que indica selecao na peca
        _previousPieceMaterial = go.GetComponentsInChildren<MeshRenderer>()[0].material;
        _pieceSelectedMaterial.mainTexture = _previousPieceMaterial.mainTexture;
        changeMaterial(go, _pieceSelectedMaterial);

        //Mostra os movimentos legais da peca
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
            for(var j = 0; j < rend.materials.Length; j++)
                mats[j] = newMat;

            rend.materials = mats;
        }
    }

    private void movePiece()
    {
        var squareTo = Board.GetSquare(_selectedSquare.x, _selectedSquare.y);
        Moves legalMoves = new Moves();
        _selectedPiece.GenerateLegalMoves(legalMoves);
        foreach(Move move in legalMoves)
        {
            if(move.To != squareTo)
                continue;

            if(move.IsPromotion() && move.Name != Move.MoveNames.PawnPromotionQueen)
                continue;

            Game.MakeAMove(move.Name, move.Piece, move.To);
        }
            
        unSelectPiece();
    }

    private void unSelectPiece()
    {
        if(_selectedPiece == null)
            return;

        changeMaterial(_piecesGameObject[_selectedPiece], _previousPieceMaterial);
        TileHighlight._instance.hideTileHighlights();
        _selectedPiece = null;
    }

	private void updateSelection()
    {
        if(_selectedSquare == Util.Constants.getInstance()._none)
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
      
    public void selectVirtualButtonClicked()
    {
        lock(_pieceToBeMovedThreadLocker)
        {
            if(_pieceToBeMoved.Count != 0)
                return;
        }

        _audioSourceClicking.Play();
        selectButton(_virtualButton);
        _generalScheduler.RegisterEvent(300, new Util.FunctionPointer(unselectButton));

        _selectedSquare = Util.Constants.getInstance().getTile(_cursor.transform.position);
        if(onBoard(_selectedSquare))
        {
            updateSelection();
        }
        else
        {
            RaycastHit hit;
            var ray = new Ray(_cursor.transform.position, Vector3.down);
            foreach(Util.ButtonEnum button in Util.ButtonEnum.GetValues(typeof(Util.ButtonEnum)))
            {
                var layerName = Util.HelperButtonEnum.ButtonEnumToString(button);
                if(Physics.Raycast(ray, out hit, float.MaxValue, LayerMask.GetMask(layerName)))
                {
                    interfaceButtonClicked(button);
                    return;
                }
            }
        }
    }

    public void checkIfMouseLeftButtonWasClicked()
    {
        lock(_pieceToBeMovedThreadLocker)
        {
            if(_pieceToBeMoved.Count != 0 || !Input.GetMouseButtonDown(0))
                return;
        }

        _audioSourceClicking.Play();

        RaycastHit hit;
        var ray = _camera.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray, out hit, float.MaxValue, LayerMask.GetMask("ChessPlane")))
            _selectedSquare = Util.Constants.getInstance().getTile(hit.point);
        else
            _selectedSquare = Util.Constants.getInstance()._none;
        updateSelection();

        foreach(Util.ButtonEnum button in Util.ButtonEnum.GetValues(typeof(Util.ButtonEnum)))
        {
            var layerName = Util.HelperButtonEnum.ButtonEnumToString(button);
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
                if(Game.PlayerWhite.Intellegence == Player.PlayerIntellegenceNames.Human)
                    ConfigModel._player = Player.PlayerColourNames.White; 
                else
                    ConfigModel._player = Player.PlayerColourNames.Black; 
            break;

            case Util.ButtonEnum.File1:
            case Util.ButtonEnum.File2:
            case Util.ButtonEnum.File3:
                setSelectedFile(buttonEnum);
            break;

            case Util.ButtonEnum.Menu:
                SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().name);
                pressedEffect(Util.ButtonEnum.Menu);
                Util.Constants.deleteInstance();
                _instance = null;
                Game.BoardPositionChanged -= BoardPositionChangedEvent;
                Game.New();
                Game.PausePlay();

                SceneManager.LoadScene("Menu");
                return;
        }

        switch(buttonEnum)
        {
            case Util.ButtonEnum.NewGame: 
            case Util.ButtonEnum.Undo:
            case Util.ButtonEnum.Redo:
            case Util.ButtonEnum.SaveGame:
            case Util.ButtonEnum.LoadGame:
                pressedEffect(buttonEnum);
                _generalScheduler.RegisterEvent(2000, new Util.FunctionPointer(Game.ResumePlay));
                break;

            default:
                break;
        }
    }

    private void pressedEffect(Util.ButtonEnum buttonEnum)
    {
        var name = Util.HelperButtonEnum.ButtonEnumToString(buttonEnum);
        selectButton(name);
        _generalScheduler.RegisterEvent(300, new Util.FunctionPointer(unselectButton));
    }

    public void selectButton(string name)
    {
        var go = GameObject.Find(name);
        if(go == null || !go.activeSelf)
            return;
        
        selectButton(go);
    }

    public void selectButton(GameObject go)
    {
        var pos = go.transform.position;
        pos.y = 0;
        _buttonSelection.transform.position = pos;
        _buttonSelection.transform.rotation = go.transform.rotation;
        _buttonSelection.SetActive(true);
    }

    public void unselectButton()
    {
        _buttonSelection.SetActive(false);
    }

    public void setSelectedFile(Util.ButtonEnum buttonEnum)
    {
        _fileName = Util.HelperButtonEnum.ButtonEnumToString(buttonEnum);
        var go = GameObject.Find(_fileName);
        _fileSelection.transform.position = go.transform.position;
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
        var piece = Board.GetPiece(position.x, position.y);
        _piecesGameObject[piece] = instantiatePiece(index, Util.Constants.getInstance().getTileCenter(position), color);
    }

    private GameObject instantiatePiece(int index, Vector3 tilePosition, Player.PlayerColourNames color)
    {
        var quaternion = Quaternion.identity;
        if(color == Player.PlayerColourNames.Black) // black pieces
            quaternion = Quaternion.Euler(0, 180, 0); //face pieces to the center of the board

        var go = Instantiate(_chessPiecesPrefabs[index], tilePosition, quaternion) as GameObject;
        go.transform.SetParent(transform);
        go.transform.localScale = go.transform.localScale * 5.0f;

        return go;
    }
}
