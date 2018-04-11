using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rook : RangePiece
{
    public override bool[,] possibleMoves()
    {
        return base.possibleMoves(new bool[4]);
    }

    public override Vector2Int getDirection(int i, int j)
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