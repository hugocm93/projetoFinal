using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : ChessPiece
{
    //TODO: en passant

    public override bool[,] possibleMoves()
    {
        return _color == Color.White ? possibleMovesWhite() : possibleMovesBlack();
    }

    private bool[,] possibleMoves(Vector2Int[] diagonalPositions, Vector2Int[] middlePositions, int pawnLine, Color color)
    {
        var b = new bool[8, 8];

        //diagonals
        foreach(var pos in diagonalPositions)
        {
            if(!onBoard(pos))
                continue;

            var p = BoardManager._instance._chessPieces[pos.x, pos.y];
            if(p && (p._color != color))
                b[pos.x, pos.y] = true;
        }
            
        //middle - first move
        foreach(var pos in middlePositions)
        {
            if(!onBoard(pos))
                continue;
            
            var middlePos = BoardManager._instance._chessPieces[pos.x, pos.y];
            b[pos.x, pos.y] = (_position.y == pawnLine) && !middlePos;
        }

        //middle
        if(onBoard(middlePositions[0]))
        {
            var middlePiece = BoardManager._instance._chessPieces[middlePositions[0].x, middlePositions[0].y];
            if(!middlePiece)
                b[middlePositions[0].x, middlePositions[0].y] = true;
        }


        return b;
    }

    public bool[,] possibleMovesBlack()
    {
        return possibleMoves(new Vector2Int[]{_position + new Vector2Int(-1, 1), _position + new Vector2Int(1, 1)},
                             new Vector2Int[]
                             {
                                _position + new Vector2Int(0, 1), 
                                _position + new Vector2Int(0, 2)
                             },
                             1, Color.Black);
    }

    public bool[,] possibleMovesWhite()
    {
        return possibleMoves(new Vector2Int[]{_position + new Vector2Int(-1, -1), _position + new Vector2Int(1, -1)},
                            new Vector2Int[]
                            {
                                _position + new Vector2Int(0, -1), 
                                _position + new Vector2Int(0, -2)
                            },
                            6, Color.White);
    }
}