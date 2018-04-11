using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class King : RangePiece
{
    public override bool[,] possibleMoves()
    {
        bool sweepRange = false;
        return base.possibleMoves(new bool[8], sweepRange);
    }

    public override Vector2Int getDirection(int i, int j)
    {
        if(j < 4)
            return new Rook().getDirection(i, j);
        else
            return new Bishop().getDirection(i, j % 4);
    }
}
