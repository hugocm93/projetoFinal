using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Queen : RangePiece
{
    public override bool[,] possibleMoves()
    {
        return base.possibleMoves(new bool[8]);
    }

    public override Vector2Int getDirection(int i, int j)
    {
        return new King().getDirection(i, j);
    }
}
