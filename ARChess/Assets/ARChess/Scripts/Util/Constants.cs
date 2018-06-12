//Autor: Hugo C. Machado

using UnityEngine;

namespace Util
{
    public class Constants
    {
        public static Constants _instance{set; get;}

        public readonly float _scale = 10.0f;
        public readonly float _tile_size = 1f;
        public readonly float _boardLength = 8 * 1f * 10.0f;
        public readonly Vector2 _tile_offset = new Vector2(1f / 2, 1f / 2);
        public readonly Vector2Int _none = new Vector2Int(-1, -1);
        public readonly Vector3 _origin = GameObject.Find("ChessBoard").transform.position;

        public static Constants getInstance()
        {
            if(_instance == null)
                _instance = new Constants();
            return _instance;
        }

        public static void deleteInstance()
        {
            _instance = null;
        }

        public Vector3 getOrigin()
        {
            return _origin;
        }
    
        public Vector3 getTileCenter(Vector2Int position)
        {
            var positionFloat = new Vector2(position.x, position.y);
            var center2D = _tile_size * positionFloat + _tile_offset;
            return getOrigin() + _scale * new Vector3(center2D.x, 0, center2D.y);
        }
    
        public Vector3 getBoardCenter()
        {
            var offset = _tile_size * _scale * 4;
            return new Vector3(getOrigin().x + offset, getOrigin().y, getOrigin().z + offset);
        }
    
        public Vector2Int getTile(Vector3 position3d)
        {
            var positionInBoard = position3d - getOrigin();
            if(positionInBoard.x < 0 || positionInBoard.z < 0)
                return _none;
                
            return new Vector2Int((int)(positionInBoard.x / getInstance()._scale), 
                                  (int)(positionInBoard.z / getInstance()._scale));
        }
    };
}
