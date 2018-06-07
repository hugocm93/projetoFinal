using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileHighlight : MonoBehaviour
{
    public static TileHighlight _instance{set; get;}
    public GameObject _tileHighlightPrefab;
    public List<GameObject> _tileHighlights;

    private void Start()
    {
        _instance = this;
        _tileHighlights = new List<GameObject>();
    }

    private GameObject getHighlightObject()
    {
        var go = _tileHighlights.Find(g => !g.activeSelf);
        if(!go)
        {
            go = Instantiate(_tileHighlightPrefab);
            go.transform.SetParent(transform);
            _tileHighlights.Add(go);
        }

        return go;
    }

    public void highlightPossibleMoves(bool[,] moves)
    {
        if(ConfigModel._showLegalMoves == false)
            return; 

        for(int i = 0 ; i < 8 ; i++)
            for(int j = 0 ; j < 8 ; j++)
                if(moves[i, j])
                {
                    var go = getHighlightObject();
                    go.SetActive(true);
                    go.transform.position = Util.Constants.getTileCenter(new Vector2Int(i, j));
                }
    }

    public void hideTileHighlights()
    {
        foreach(GameObject go in _tileHighlights)
            go.SetActive(false);
    }
}
