using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knight : ChessPiece
{
    public override bool[,] possibleMoves()
    {
        var b = new bool[8, 8];
        var offsets = new Vector2Int[]
        {
            new Vector2Int(-1, 2),  //upLeft
            new Vector2Int(1, 2),   //upRight
            new Vector2Int(-1, -2), //downLeft
            new Vector2Int(1, -2),  //downRight
            new Vector2Int(2, 1),   //leftUp
            new Vector2Int(2, -1),  //leftDown
            new Vector2Int(-2, 1),  //rightUp
            new Vector2Int(-2, -1)  //rightDown
        };
                
        foreach(var offset in offsets)
            checkKnightMove(_position + offset, ref b);

        return b;
    }

    private void checkKnightMove(Vector2Int position, ref bool[,] b)
    {
        if(!onBoard(position))
            return;

        var piece = BoardManager._instance._chessPieces[position.x, position.y];
        if(piece && (piece._color == _color))
            return; 

        b[position.x, position.y] = true;
    }
}
