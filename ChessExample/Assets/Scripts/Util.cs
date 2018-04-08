using System;
using UnityEngine;

public class Util
{
    public Util()
    {
    }

    public static Vector3 getTileCenter(Vector2Int position)
    {
        var _tile_size = 1.0f;
        var _tile_offset = 0.5f;

        var origin = Vector3.zero;
        origin.x = _tile_size * position.x + _tile_offset;
        origin.z = _tile_size * position.y + _tile_offset; 

        return origin;
    }
}