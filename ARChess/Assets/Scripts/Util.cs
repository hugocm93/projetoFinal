using System;
using UnityEngine;

public class Util
{
    public static float _scale = 10.0f;
    public static Vector3 _origin = Vector3.zero; //new Vector3(0.6f, 0, 0.45f);
    public static float _tile_size = 1f;
    public static Vector2 _tile_offset = new Vector2(_tile_size / 2, _tile_size / 2);

    public Util(){}

    public static Vector3 getTileCenter(Vector2Int position)
    {
        var positionFloat = new Vector2(position.x, position.y);
        var center2D = _tile_size * positionFloat + _tile_offset;
        return _scale * (_origin + new Vector3(center2D.x, 0, center2D.y));
    }
}