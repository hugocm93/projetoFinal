using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bishop : RangePiece
{
    public override bool[,] possibleMoves()
    {
        return base.possibleMoves(new bool[4]);
    }

    public override Vector2Int getDirection(int i, int j)
    {
        switch(j)
        {
            case 0: return new Vector2Int(i, i);  //rightup
            case 1: return new Vector2Int(-i, -i); //leftdown
            case 2: return new Vector2Int(-i, i);  //leftup
            case 3: return new Vector2Int(i, -i);  //rightdown
            default: return new Vector2Int();
        }
    }
}
