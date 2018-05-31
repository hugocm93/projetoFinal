using System;
using UnityEngine;

namespace Util
{
    
public class Constants
{
    public static float _scale = 10.0f;
    public static float _tile_size = 1f;
    public static Vector2 _tile_offset = new Vector2(_tile_size / 2, _tile_size / 2);

    private static Vector3 _origin = GameObject.Find("ChessBoard").transform.position;
     
    public static Vector3 getTileCenter(Vector2Int position)
    {
        var positionFloat = new Vector2(position.x, position.y);
        var center2D = _tile_size * positionFloat + _tile_offset;
        return _origin + _scale * new Vector3(center2D.x, 0, center2D.y);
    }

    public static Vector2Int getTile(Vector3 position3d)
    {
        var positionInBoard = position3d - _origin;
        return new Vector2Int((int)(positionInBoard.x / Util.Constants._scale), 
                              (int)(positionInBoard.z / Util.Constants._scale));
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