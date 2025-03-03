using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Test : MonoBehaviour
{
    public InstantiatedRoom _instantiatedRoom;
    public Grid _grid;
    private Tilemap _frontTilemap;
    public Tilemap _pathTilemap;
    private Vector3Int _startGridPos;
    private Vector3Int _endGridPos;
    public TileBase _startPathTile;
    public TileBase _finishPathTile;

    private Vector3Int _noValue = new Vector3Int(9999, 9999, 9999);
    private Stack<Vector3> _pathStack;

    private void OnEnable()
    {
        EventMgr.RegisterEvent(EventName.RoomChanged, OnRoomChanged);
    }

    private void OnDisable()
    {
        EventMgr.UnRegisterEvent(EventName.RoomChanged, OnRoomChanged);
    }

    private void Start()
    {
        _startPathTile = GameResource.Instance.preferredEnemyPathTile;
        _finishPathTile = GameResource.Instance.enemyUnwalkableCollisionTilesArray[0];
    }

    private object OnRoomChanged(object[] arg)
    {
        Debug.Log("OnRoomChanged");
        _pathStack = null;
        _instantiatedRoom = ((Room)arg[0]).InstantiatedRoom;
        if (_instantiatedRoom != null)
        {
            _frontTilemap = _instantiatedRoom.transform.Find("Grid/Tilemap4_Front").GetComponent<Tilemap>();
            _grid = _instantiatedRoom.transform.GetComponentInChildren<Grid>();
            _startGridPos = _noValue;
            _endGridPos = _noValue;
            
            SetUpPathTilemap();
        }

        return null;
    }

    private void SetUpPathTilemap()
    {
        Transform tilemapCloneTrans = _instantiatedRoom.transform.Find("Grid/Tilemap4_Front(Clone)");
        
        if(tilemapCloneTrans != null)
        {
            Debug.Log("tilemapCloneTrans != null");
            _pathTilemap = Instantiate(_frontTilemap, _grid.transform);
            _pathTilemap.GetComponent<TilemapRenderer>().sortingOrder = 2;
            _pathTilemap.GetComponent<TilemapRenderer>().material = GameResource.Instance.litMaterial;
            _pathTilemap.gameObject.tag = "Untagged";
        }
        else
        {
            _pathTilemap = _instantiatedRoom.transform.Find("Grid/Tilemap4_Front").GetComponent<Tilemap>();
            _pathTilemap.ClearAllTiles();
        }
    }

    private void Update()
    {
        if(_instantiatedRoom == null || _startPathTile == null || _finishPathTile == null || _grid == null || _pathTilemap == null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            ClearPath();
            SetStartPosition();
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            ClearPath();
            SetEndPosition();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            DisplayPath();
        }
    }

    
    private void ClearPath()
    {
        if (_pathStack == null)
        {
            return;
        }

        foreach (Vector3 worldPos in _pathStack)
        {
            _pathTilemap.SetTile(_grid.WorldToCell(worldPos), null);
        }

        _pathStack = null;

        _startGridPos = _noValue;
        _endGridPos = _noValue;
    }

    private void SetStartPosition()
    {
        if (_startGridPos == _noValue)
        {
            _startGridPos = _grid.WorldToCell(HelperUtilities.GetMouseWorldPosition(Input.mousePosition));
            if(!IsPosWithinBounds(_startGridPos))
            {
                _startGridPos = _noValue;
                return;
            }
            
            _pathTilemap.SetTile(_startGridPos, _startPathTile);
        }
        else
        {
            _pathTilemap.SetTile(_startGridPos, null);
            _startGridPos = _noValue;
        }
    }
    
    private void SetEndPosition()
    {
        if (_endGridPos == _noValue)
        {
            _endGridPos = _grid.WorldToCell(HelperUtilities.GetMouseWorldPosition(Input.mousePosition));
            if(!IsPosWithinBounds(_endGridPos))
            {
                _endGridPos = _noValue;
                return;
            }
            
            _pathTilemap.SetTile(_endGridPos, _finishPathTile);
        }
        else
        {
            _pathTilemap.SetTile(_endGridPos, null);
            _endGridPos = _noValue;
        }
    }

    private bool IsPosWithinBounds(Vector3Int position)
    {
        if(position.x < _instantiatedRoom.Room.TemplateLowerBounds.x || position.x > _instantiatedRoom.Room.TemplateUpperBounds.x ||
           position.y < _instantiatedRoom.Room.TemplateLowerBounds.y || position.y > _instantiatedRoom.Room.TemplateUpperBounds.y)
        {
            return false;
        }

        return true;
    }

    private void DisplayPath()
    {
        if(_startGridPos == _noValue || _endGridPos == _noValue)
        {
            return;
        }
        
        _pathStack = AStar.BuildPath(_instantiatedRoom.Room, _startGridPos, _endGridPos);
        if(_pathStack == null) return;
        foreach (Vector3 worldPos in _pathStack)
        {
            _pathTilemap.SetTile(_grid.WorldToCell(worldPos), _startPathTile);
        }
    }
}
