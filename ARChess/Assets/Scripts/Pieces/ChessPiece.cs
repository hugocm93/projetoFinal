using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ChessPiece : MonoBehaviour
{
    public enum Color{White, Black};

    public Vector2Int _position{set; get;}
    public Color _color;

    public abstract bool[,] possibleMoves();
        
    protected bool onBoard(Vector2Int v)
    {
        return (v.x >= 0 && v.x < 8) && (v.y >= 0 && v.y < 8);
    }
}
