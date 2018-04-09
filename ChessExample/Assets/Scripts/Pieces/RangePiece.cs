using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RangePiece : ChessPiece
{
    protected bool[,] possibleMoves(bool[] directionDone, bool sweepBoard = true)
    {
        var b = new bool[8, 8];
        var range = sweepBoard ? 8 : 2;
        for(int i = 1; i < range; i++)
        {
            for(int j = 0; j < directionDone.Length ; j++)
            {
                if(directionDone[j])
                    continue;

                var piecePos = _position + getDirection(i, j);
                if(!onBoard(piecePos))
                {
                    directionDone[j] = true;
                    continue;
                }

                var piece = BoardManager._instance._chessPieces[piecePos.x, piecePos.y];
                if(piece && (piece._color == _color))
                {
                    directionDone[j] = true;
                    continue;
                }
                else if(piece && (piece._color != _color))
                    directionDone[j] = true;

                b[piecePos.x, piecePos.y] = true;
            }
        }

        return b;
    }

    public abstract Vector2Int getDirection(int i, int j);
}
