using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ChessPiece : MonoBehaviour
{
    public enum Color{White, Black};

    public Vector2Int _position{set; get;}
    public Color _color;

    public virtual bool[,] possibleMoves()
    {
        return new bool[8, 8];
    }
}
