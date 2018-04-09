using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class King : RangePiece
{
    public override bool[,] possibleMoves()
    {
        return base.possibleMoves(new bool[8], false);
    }

    public override Vector2Int getDirection(int i, int j)
    {
        switch(j)
        {
            case 0: return new Vector2Int(i, 0);  //right
            case 1: return new Vector2Int(-i, 0); //left
            case 2: return new Vector2Int(0, i);  //up
            case 3: return new Vector2Int(0, -i);  //down
            case 4: return new Vector2Int(i, i);  //rightup
            case 5: return new Vector2Int(-i, -i); //leftdown
            case 6: return new Vector2Int(-i, i);  //leftup
            case 7: return new Vector2Int(i, -i);  //rightdown
            default: return new Vector2Int();
        }
    }
}
