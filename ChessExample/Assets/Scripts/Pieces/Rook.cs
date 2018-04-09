using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rook : ChessPiece
{
    public override bool[,] possibleMoves()
    {
        var b = new bool[8, 8];
        bool[] directionDone = new bool[4];
        for(int i = 1; i < 8; i++)
        {
            for(int j = 0; j < 4; j++)
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

    private Vector2Int getDirection(int i, int j)
    {
        switch(j)
        {
            case 0: return new Vector2Int(i, 0);  //right
            case 1: return new Vector2Int(-i, 0); //left
            case 2: return new Vector2Int(0, i);  //up
            case 3: return new Vector2Int(0, -i);  //down
            default: return new Vector2Int();
        }
    }
}