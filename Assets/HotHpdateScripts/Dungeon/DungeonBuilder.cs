using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YooAsset;
using Object = UnityEngine.Object;


[DisallowMultipleComponent]
public class DungeonBuilder : MonoSingleton<DungeonBuilder>
{
    public Dictionary<string, Room> DungeonBuilderRoomDictionary = new Dictionary<string, Room>();
    public Dictionary<string, RoomTemplateSO> RoomTemplateDictionary = new Dictionary<string, RoomTemplateSO>();
    private List<RoomTemplateSO> _roomTemplateList = null;
    private RoomNodeTypeListSO _roomNodeTypeList;
    private bool _dungeonBuildSuccessful;
    private static readonly int AlphaSlider = Shader.PropertyToID("Alpha_Slider");


    private void Awake()
    {
        LoadRoomNodeTypeList();
    }

    private void OnEnable()
    {
        var dimmed = AssetMgr.LoadAssetSync<Material>("Assets/GameRes/Materials/Dungeon/DungeonLightShader_Dimmed.mat");
        dimmed.SetFloat(AlphaSlider, 0f);
        // GameResource.Instance.dimmedMaterial.SetFloat(AlphaSlider, 0f);        
    }
    
    private void OnDisable()
    {
        var dimmed = AssetMgr.LoadAssetSync<Material>("Assets/GameRes/Materials/Dungeon/DungeonLightShader_Dimmed.mat");
        dimmed.SetFloat(AlphaSlider, 0f);
        // GameResource.Instance.dimmedMaterial.SetFloat(AlphaSlider, 1f);
    }
    
    
    private void LoadRoomNodeTypeList()
    {
        _roomNodeTypeList = GameResource.Instance.roomNodeTypeList;
    }
    
    
    
    public bool GenerateDungeon(DungeonLevelSO currentDungeonLevel)
    {
        _roomTemplateList = currentDungeonLevel.roomTemplateList;
        
        // 加载SO room templates到字典中
        LoadRoomTemplatesIntoDictionary();
        
        _dungeonBuildSuccessful = false;
        int dungeonBuildAttempts = 0;

        while (!_dungeonBuildSuccessful && dungeonBuildAttempts < Settings.MaxDungeonBuildAttempts)
        {
            dungeonBuildAttempts++;
            // 从列表中随机选择一个room node Graph
            RoomNodeGraphSO roomNodeGraph = SelectRandomRoomNodeGraph(currentDungeonLevel.roomNodeGraphList);
            
            int dungeonRebuildAttemptsForNodeGraph = 0;
            _dungeonBuildSuccessful = false;

            while (!_dungeonBuildSuccessful && dungeonRebuildAttemptsForNodeGraph < Settings.MaxDungeonRebuildAttemptsForRoomGraph)
            {
                // 清理dungeon room gameobjects和 dungeon room dictionary 
                ClearDungeon();
                dungeonRebuildAttemptsForNodeGraph++;
                // 尝试构建随机的dungeon -- 从选择的room node graph
                _dungeonBuildSuccessful = AttemptToBuildRandomDungeon(roomNodeGraph);
            }
            
            if (_dungeonBuildSuccessful)
            {
                // 实例化 Room GameObjects
                InstantiatedRoomGameObjects();
            }
        }

        return _dungeonBuildSuccessful;
    }
    
    /// <summary>
    /// TODO:改成从YooAsset加载资源
    /// </summary>
    private void InstantiatedRoomGameObjects()
    {
        foreach (var keyValuePair in DungeonBuilderRoomDictionary)
        {
            Room room = keyValuePair.Value;

            Vector3 roomPosition = new Vector3(room.LowerBounds.x - room.TemplateLowerBounds.x,
                room.LowerBounds.y - room.TemplateLowerBounds.y, 0f);
            
            
            // // 用Addressables加载资源 方便打成AB包
            // var handle = Addressables.LoadAssetAsync<GameObject>(room.Prefab.name).Result;
            // GameObject roomGameObject = Instantiate(handle, roomPosition, Quaternion.identity, transform);
            // InstantiatedRoom instantiatedRoom = roomGameObject.GetComponentInChildren<InstantiatedRoom>();
            // instantiatedRoom.Room = room;
            // instantiatedRoom.Initialise(roomGameObject);
            // room.InstantiatedRoom = instantiatedRoom;
            
            
            // TODO:YooAsset加载资源
            // 已经成功, 记得别用异步就行，就算要异步也要等await handle之后再做实例化或者其他操作
            var roomGameObject = YooAssets.LoadAssetSync<GameObject>(room.Prefab.name)
                .InstantiateSync(roomPosition, Quaternion.identity, transform);
            
            InstantiatedRoom instantiatedRoom = roomGameObject.GetComponentInChildren<InstantiatedRoom>();
            instantiatedRoom.Room = room;
            // 实例化Instantiated Room
            instantiatedRoom.Initialise(roomGameObject);
            room.InstantiatedRoom = instantiatedRoom;

            // // 实例化
            // GameObject roomGameObject = Instantiate(room.Prefab, roomPosition, Quaternion.identity, transform);
            //
            // InstantiatedRoom instantiatedRoom = roomGameObject.GetComponentInChildren<InstantiatedRoom>();
            //
            // instantiatedRoom.Room = room;
            // // 实例化Instantiated Room
            // instantiatedRoom.Initialise(roomGameObject);
            //
            // room.InstantiatedRoom = instantiatedRoom;

            // if (!room.RoomNodeType.isBossRoom)
            // {
            //     room.IsClearedOfEnemies = true;
            // }
        }
    }
    
    /// <summary>
    /// 加载room templates到字典中
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    private void LoadRoomTemplatesIntoDictionary()
    {
        RoomTemplateDictionary.Clear();

        // 加载_roomTemplateList到字典中
        foreach (var roomTemplate in _roomTemplateList)
        {
            if (!RoomTemplateDictionary.ContainsKey(roomTemplate.guid))
            {
                RoomTemplateDictionary.Add(roomTemplate.guid, roomTemplate);
            }
            else
            {
                Debug.LogError("RoomTemplateDictionary already contains key: " + roomTemplate.guid);
            }
        }
    }
    
    private RoomNodeGraphSO SelectRandomRoomNodeGraph(List<RoomNodeGraphSO> roomNodeGraphList)
    {
        if (roomNodeGraphList.Count > 0)
        {
            // 注意Random.Range是左闭右开的
            return roomNodeGraphList[UnityEngine.Random.Range(0, roomNodeGraphList.Count)];
        }
        else
        {
            Debug.LogError("roomNodeGraphList is empty");
            return null;
        }
    }
    
    /// <summary>
    /// 销毁已经生成的dungeon room gameobjects和清空dungeon room dictionary
    /// </summary>
    private void ClearDungeon()
    {
        if (DungeonBuilderRoomDictionary.Count > 0)
        {
            foreach (KeyValuePair<string, Room> keyValuePair in DungeonBuilderRoomDictionary)
            {
                Room room = keyValuePair.Value;

                // 如果房间已经实例化了，销毁它
                if (room.InstantiatedRoom != null)
                {
                    Destroy(room.InstantiatedRoom.gameObject);
                }
            }
            DungeonBuilderRoomDictionary.Clear();
        }
    }
    
    // ReSharper disable Unity.PerformanceAnalysis
    private bool AttemptToBuildRandomDungeon(RoomNodeGraphSO roomNodeGraph)
    {
        Queue<RoomNodeSO> openRoomNodeQueue = new Queue<RoomNodeSO>();

        
        // TODO: Test 在HostPlay模式下entranceNode存在报空的情况
        
        // 不是_roomNodeTypeList.list报的空
        
        // if (_roomNodeTypeList.list != null)
        // {
        //     Debug.Log("roomNodeTypeList is not null" + _roomNodeTypeList.list.Count);
        // }
        // else
        // {
        //     Debug.Log("roomNodeTypeList is null");
        // }

        // 也不是roomNodeGraph报的空
        //Debug.Log(roomNodeGraph ? "roomNodeGraph is not null" : "roomNodeGraph is null");

        
        // 能正常找到入口节点的TypeSO
        // if(_roomNodeTypeList.list.Find(x=>x.isEntrance) == null)
        // {
        //     Debug.Log("在RoomNodeTypeList中没有入口节点");
        // }
        // else
        // {
        //     Debug.Log("在RoomNodeTypeList中有入口节点" + _roomNodeTypeList.list.Find(x=>x.isEntrance).name);
        // }

        // Debug.Log(roomNodeGraph.GetRoomNode(_roomNodeTypeList.list.Find(x => x.isEntrance)) == null
        //     ? "在RoomNodeGraph中没有入口节点"
        //     : "在RoomNodeGraph中有入口节点");


        // 从RoomNodeGraph中获取入口节点
        RoomNodeSO entranceNode = roomNodeGraph.GetRoomNode(_roomNodeTypeList.list.Find(x => x.isEntrance));
        
        if (entranceNode != null)
        {
            openRoomNodeQueue.Enqueue(entranceNode);
        }
        else
        {
            Debug.LogError("在RoomNodeGraph中没有入口节点");
            return false; // 没有入口节点，不能生成地牢
        }
        
        // 用于检查房间是否重叠
        bool noRoomOverlaps = true;
        
        // 处理房间队列
        noRoomOverlaps = ProcessRoomsInOpenRoomNodeQueue(roomNodeGraph, openRoomNodeQueue, noRoomOverlaps);
        
        // 如果已经处理了所有的房间并且没有房间重叠，则返回true
        if(openRoomNodeQueue.Count == 0 && noRoomOverlaps)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    
    private bool ProcessRoomsInOpenRoomNodeQueue(RoomNodeGraphSO roomNodeGraph, Queue<RoomNodeSO> openRoomNodeQueue, bool noRoomOverlaps)
    {
        while (openRoomNodeQueue.Count > 0 && noRoomOverlaps == true)
        {
            // 从队列中取出一个房间
            RoomNodeSO roomNode = openRoomNodeQueue.Dequeue();

            foreach (var childRoomNode in roomNodeGraph.GetChildRoomNodes(roomNode))
            {
                openRoomNodeQueue.Enqueue(childRoomNode);
            }
            
            if(roomNode.roomNodeType.isEntrance)
            {
                RoomTemplateSO roomTemplate = GetRandomRoomTemplate(roomNode.roomNodeType);
                Room room = CreateRoomFromRoomTemplate(roomTemplate, roomNode);
                room.IsPositioned = true;
                
                DungeonBuilderRoomDictionary.Add(room.ID, room);
            }
            else
            {
                // Else 从node获取 parent room
                Room parentRoom = DungeonBuilderRoomDictionary[roomNode.parentRoomNodeList[0]];
                
                // 判断是否可以被放置而没有重叠
                noRoomOverlaps = CanPlaceRoomWithoutOverlaps(roomNode, parentRoom);
            }
        }
        
        return noRoomOverlaps;
    }
    
    private bool CanPlaceRoomWithoutOverlaps(RoomNodeSO roomNode, Room parentRoom)
    {
        // 初始化并假设重叠，直到证明相反
        bool roomOverlaps = true;

        // 当房间重叠时，尝试重新放置在房间所有可用的doorWay 知道房间被成功放置没有重叠
        while (roomOverlaps)
        {
            // 为Parent Room随机获取未连接的可用的doorways
            List<Doorway> unconnectedAvailableParentsDoorways = GetUnconnectedAvailableDoorways(parentRoom.DoorWayList).ToList();

            if (unconnectedAvailableParentsDoorways.Count == 0)
            {
                // 如果没有可用的doorways，返回false
                return false;
            }
            
            Doorway doorwayParent = unconnectedAvailableParentsDoorways[UnityEngine.Random.Range(0, unconnectedAvailableParentsDoorways.Count)];
            
            // 为 room node 获取和 parent room node 方向一样的 room template
            RoomTemplateSO roomTemplate = GetRandomRoomTemplateForRoomConsistentWithParent(roomNode, doorwayParent);
            
            // 创造一个room
            Room room = CreateRoomFromRoomTemplate(roomTemplate, roomNode);
            
            // 尝试放置房间 - 返回true 就代表没有重叠
            if (PlaceTheRoom(parentRoom, doorwayParent, room))
            {
                roomOverlaps = false;
                
                room.IsPositioned = true;
                
                DungeonBuilderRoomDictionary.Add(room.ID, room);
            }
            else
            {
                roomOverlaps = true;
            }
        }

        return true; // 没有重叠
    }
    
    
    /// <summary>
    /// 放置房间 - 没有重叠返回true
    /// </summary>
    /// <param name="parentRoom"></param>
    /// <param name="doorwayParent"></param>
    /// <param name="room"></param>
    /// <returns></returns>
    private bool PlaceTheRoom(Room parentRoom, Doorway doorwayParent, Room room)
    {
        // 获取当前room doorway的position
        Doorway doorway = GetOppositeDoorway(doorwayParent, room.DoorWayList);

        if (doorway == null)
        {
            // doorwayParent标记为不可用
            doorwayParent.isUnavailable = true;

            return false;
        }
        
        // Calculate 'world' grid parent doorway position
        Vector2Int parentDoorwayPosition = parentRoom.LowerBounds + doorwayParent.position - parentRoom.TemplateLowerBounds;
        
        Vector2Int adjustment = Vector2Int.zero;

        switch (doorway.orientation)
        {
            case Orientation.North:
                adjustment = new Vector2Int(0,-1);
                break;
            case Orientation.East:
                adjustment = new Vector2Int(-1,0);
                break;
            case Orientation.South:
                adjustment = new Vector2Int(0,1);
                break;
            case Orientation.West:
                adjustment = new Vector2Int(1,0);
                break;
            case Orientation.None:
                break;
            default:
                break;
        }
        
        // Calculate room lower bounds and upper bounds based on positioning to align with parent doorway
        room.LowerBounds = parentDoorwayPosition + adjustment + room.TemplateLowerBounds - doorway.position;
        room.UpperBounds = room.LowerBounds + room.TemplateUpperBounds - room.TemplateLowerBounds;
        
        Room overlappingRoom = CheckForRoomOverlap(room);

        if (overlappingRoom == null)
        {
            doorwayParent.isConnected = true;
            doorwayParent.isUnavailable = true;
            
            doorway.isConnected = true;
            doorway.isUnavailable = true;

            return true;
        }
        else
        {
            doorwayParent.isUnavailable = true;
            return false;
        }
        
    }
    
    
    
    private Doorway GetOppositeDoorway(Doorway parentDoorway, List<Doorway> doorWayList)
    {
        foreach (var doorwayToCheck in doorWayList)
        {
            if (parentDoorway.orientation == Orientation.East && doorwayToCheck.orientation == Orientation.West)
            {
                return doorwayToCheck;
            }
            else if(parentDoorway.orientation == Orientation.West && doorwayToCheck.orientation == Orientation.East)
            {
                return doorwayToCheck;
            }
            else if(parentDoorway.orientation == Orientation.North && doorwayToCheck.orientation == Orientation.South)
            {
                return doorwayToCheck;
            }
            else if(parentDoorway.orientation == Orientation.South && doorwayToCheck.orientation == Orientation.North)
            {
                return doorwayToCheck;
            }
        }

        return null;        
    }
    
    private Room CheckForRoomOverlap(Room roomToTest)
    {
        foreach (var keyValuePair in DungeonBuilderRoomDictionary)
        {
            Room room = keyValuePair.Value;

            if (room.ID == roomToTest.ID || !room.IsPositioned)
            {
                continue;
            }

            // 如果房间重叠
            if (IsOverLappingRoom(roomToTest, room))
            {
                return room;
            }
        }

        return null;
    }
    
    
    
    /// <summary>
    /// 检查两个房间是否彼此重叠 - 如果重叠返回true
    /// </summary>
    /// <param name="roomToTest"></param>
    /// <param name="room"></param>
    /// <returns></returns>
    private bool IsOverLappingRoom(Room room1, Room room2)
    {
        bool isOverlappingX= IsOverLappingInterval(room1.LowerBounds.x, room1.UpperBounds.x, room2.LowerBounds.x, room2.UpperBounds.x);
        
        bool isOverlappingY = IsOverLappingInterval(room1.LowerBounds.y, room1.UpperBounds.y, room2.LowerBounds.y, room2.UpperBounds.y);
        
        if (isOverlappingX && isOverlappingY)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    
    
    
    private bool IsOverLappingInterval(int imin1, int imax1, int imin2, int imax2)
    {
        if(Mathf.Max(imin1, imin2) <= Mathf.Min(imax1, imax2))
        {
            return true;
        }
        else
        {
            return false;
        } 
    }
    
    private IEnumerable<Doorway> GetUnconnectedAvailableDoorways(List<Doorway> roomDoorWayList)
    {
        foreach (var doorway in roomDoorWayList)
        {
            if(!doorway.isConnected && !doorway.isUnavailable)
            {
                yield return doorway;
            }
        }
    }
    
    /// <summary>
    /// 在考虑父节点的方向的情况下，为房间节点获取随机的房间模板
    /// </summary>
    /// <param name="roomNode"></param>
    /// <param name="doorwayParent"></param>
    /// <returns></returns>
    private RoomTemplateSO GetRandomRoomTemplateForRoomConsistentWithParent(RoomNodeSO roomNode, Doorway doorwayParent)
    {
        RoomTemplateSO roomTemplate = null;
        
        // 如果room node是一个走廊
        if (roomNode.roomNodeType.isCorridor)
        {
            switch (doorwayParent.orientation)
            {
                case Orientation.North:
                case Orientation.South:
                    roomTemplate = GetRandomRoomTemplate(_roomNodeTypeList.list.Find(x => x.isCorridorNS));
                    break;
                case Orientation.East:
                case Orientation.West:
                    roomTemplate = GetRandomRoomTemplate(_roomNodeTypeList.list.Find(x => x.isCorridorEW));
                    break;
                case Orientation.None:
                    break;
                default:
                    break;
            }
        }
        // 随机选一个模板
        else
        {
            roomTemplate = GetRandomRoomTemplate(roomNode.roomNodeType);
        }

        return roomTemplate;
    }
    
    private RoomTemplateSO GetRandomRoomTemplate(RoomNodeTypeSO roomNodeType)
    {
        List<RoomTemplateSO> matchingRoomTemplateList = new List<RoomTemplateSO>();

        foreach (var roomTemplate in _roomTemplateList)
        {
            // 添加匹配的房间模板到列表中
            // if (roomTemplate.roomNodeType == roomNodeType)
            // {
            //     matchingRoomTemplateList.Add(roomTemplate);
            // }

            if (roomTemplate.roomNodeType.AreEqual(roomNodeType))
            {
                matchingRoomTemplateList.Add(roomTemplate);
            }
        }

        if (matchingRoomTemplateList.Count == 0)
        {
            return null;
        }
        
        // 从列表中随机选择一个房间模板
        return matchingRoomTemplateList[UnityEngine.Random.Range(0, matchingRoomTemplateList.Count)];
    }
    
    
    private Room CreateRoomFromRoomTemplate(RoomTemplateSO roomTemplate, RoomNodeSO roomNode)
    {
        Room room = new Room();
        room.TemplateID = roomTemplate.guid;
        room.ID = roomNode.id;
        room.Prefab = roomTemplate.prefab;
        room.RoomNodeType = roomNode.roomNodeType;
        room.LowerBounds = roomTemplate.lowerBounds;
        room.UpperBounds = roomTemplate.upperBounds;
        room.SpawnPositionArray = roomTemplate.spawnPositionArray;
        room.EnemiesByLevelList = roomTemplate.enemiesByLevelList;
        room.RoomLevelEnemySpawnParametersList = roomTemplate.roomEnemySpawnParametersList;
        room.TemplateLowerBounds = roomTemplate.lowerBounds;
        room.TemplateUpperBounds = roomTemplate.upperBounds;

        // 深拷贝 因为List<>是引用类型
        room.ChildRoomIDList = CopyStringList(roomNode.childRoomNodeList);
        room.DoorWayList = CopyDoorWayList(roomTemplate.doorwayList);

        //为room设置Parent ID
        if(roomNode.parentRoomNodeList.Count == 0)
        {
            // 如果没有父节点，那么这个房间是入口
            // 入口
            room.ParentRoomID = "";
            room.IsPreviouslyVisited = true;
            
            // 设置GameManager中的CurrentRoom为入口
            GameManager.Instance.SetCurrentRoom(room);
        }
        else
        {
            room.ParentRoomID = roomNode.parentRoomNodeList[0];
        }
        
        if (room.GetNumberOfEnemiesToSpawn(GameManager.Instance.GetCurrentDungeonLevel()) == 0)
        {
            room.IsClearedOfEnemies = true;
        }
        
        return room;
    }
    
    // 深拷贝一个List<string>
    private List<string> CopyStringList(List<string> oldStringList)
    {
        List<string> newStringList = new List<string>();
        
        foreach (var oldString in oldStringList)
        {
            // 这会创建一个新的string对象，而不是创建对旧的string的引用
            newStringList.Add(oldString);
        }

        return newStringList;
    }
    
    private List<Doorway> CopyDoorWayList(List<Doorway> oldDoorwayList)
    {
        List<Doorway> newDoorwayList = new List<Doorway>();

        foreach (var doorWay in oldDoorwayList)
        {
            Doorway newDoorway = new Doorway();
            newDoorway.position = doorWay.position;
            newDoorway.orientation = doorWay.orientation;
            newDoorway.doorPrefab = doorWay.doorPrefab;
            newDoorway.isConnected = doorWay.isConnected;
            newDoorway.isUnavailable = doorWay.isUnavailable;
            newDoorway.doorwayStartCopyPosition = doorWay.doorwayStartCopyPosition;
            newDoorway.doorwayCopyTileWidth = doorWay.doorwayCopyTileWidth;
            newDoorway.doorwayCopyTileHeight = doorWay.doorwayCopyTileHeight;
            
            newDoorwayList.Add(newDoorway);
        }

        return newDoorwayList;
    }
    
    public RoomTemplateSO GetRoomTemplate(string roomTemplateID)
    {
        return CollectionExtensions.GetValueOrDefault(RoomTemplateDictionary, roomTemplateID);
    }
}
