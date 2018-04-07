using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : ChessPiece
{
    public override bool[,] possibleMoves()
    {
        bool[,] possibleMoves = new bool[8, 8];

        possibleMoves[4, 4] = true;

        return possibleMoves;
    }
}
