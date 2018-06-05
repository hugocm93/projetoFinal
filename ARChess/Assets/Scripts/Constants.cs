using System;
using UnityEngine;

namespace Util
{
    
public enum ButtonEnum
{
    NewGame, LoadGame, SaveGame, Undo, Redo, File1, File2, File3
};

public class Constants
{
    public static readonly float _scale = 10.0f;
    public static readonly float _tile_size = 1f;
    public static readonly Vector2 _tile_offset = new Vector2(_tile_size / 2, _tile_size / 2);
    public static readonly Vector3 _origin = GameObject.Find("ChessBoard").transform.position;
    public static readonly Vector2Int _none = new Vector2Int(-1, -1);
     
    public static Vector3 getTileCenter(Vector2Int position)
    {
        var positionFloat = new Vector2(position.x, position.y);
        var center2D = _tile_size * positionFloat + _tile_offset;
        return _origin + _scale * new Vector3(center2D.x, 0, center2D.y);
    }

    public static Vector3 getBoardCenter()
    {
            var offset = _tile_size * _scale * 4;
            return new Vector3(_origin.x + offset, _origin.y, _origin.z + offset);
    }

    public static Vector2Int getTile(Vector3 position3d)
    {
        var positionInBoard = position3d - _origin;
        if(positionInBoard.x < 0 || positionInBoard.z < 0)
            return _none;
            
        return new Vector2Int((int)(positionInBoard.x / Util.Constants._scale), 
                              (int)(positionInBoard.z / Util.Constants._scale));
    }

    public static string ButtonEnumToString(ButtonEnum e)
    {
        switch(e)
        {
            case ButtonEnum.NewGame: 
                return "NewGame";

            case ButtonEnum.LoadGame:
                return "LoadGame";

            case ButtonEnum.SaveGame:
                return "SaveGame";

            case ButtonEnum.Undo:
                return "Undo";

            case ButtonEnum.Redo:
                return "Redo";

            case ButtonEnum.File1:
                return "File1";

            case ButtonEnum.File2:
                return "File2";

            case ButtonEnum.File3:
                return "File3";

            default:
                return "";
        }
    }
};
        
public class Pair<T, U>
{
    public Pair(){}

    public Pair(T first, U second)
    {
        this.first = first;
        this.second = second;
    }

    public T first { get; set; }
    public U second { get; set; }
};
}