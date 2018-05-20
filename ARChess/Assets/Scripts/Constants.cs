using System;
using UnityEngine;

namespace Util
{
    
public class Constants
{
    public static float _scale = 10.0f;
    public static Vector3 _origin = Vector3.zero; //new Vector3(0.6f, 0, 0.45f);
    public static float _tile_size = 1f;
    public static Vector2 _tile_offset = new Vector2(_tile_size / 2, _tile_size / 2);

    public Constants(){}

    public static Vector3 getTileCenter(Vector2Int position)
    {
        var positionFloat = new Vector2(position.x, position.y);
        var center2D = _tile_size * positionFloat + _tile_offset;
        return _scale * (_origin + new Vector3(center2D.x, 0, center2D.y));
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