using System.Collections.Generic;
using UnityEngine;

public static class AStar
{
    public static Stack<Vector3> BuildPath(Room room, Vector3Int startGridPos, Vector3Int endGridPos)
    {
        // 转换为局部坐标
        startGridPos -= (Vector3Int)room.TemplateLowerBounds;
        endGridPos -= (Vector3Int)room.TemplateLowerBounds;
        
        // openSet: 用于存储待检查的节点
        // closedSet: 用于存储已检查的节点
        List<Node> openNodeList = new List<Node>();
        HashSet<Node> closedNodeHashSet = new HashSet<Node>();
        
        // 获取房间的网格节点 计算宽和高
        GridNodes gridNodes = new GridNodes(room.TemplateUpperBounds.x - room.TemplateLowerBounds.x + 1, 
            room.TemplateUpperBounds.y - room.TemplateLowerBounds.y + 1);
        
        Node startNode = gridNodes.GetGridNode(startGridPos.x, startGridPos.y);
        Node targetNode = gridNodes.GetGridNode(endGridPos.x, endGridPos.y);
        
        Node endPathNode = FindShortestPath(startNode, targetNode, gridNodes, openNodeList, closedNodeHashSet, 
            room.InstantiatedRoom);

        if (endPathNode != null)
        {
            return CreatePathStack(endPathNode, room);
        }
        
        return null;
    }

    private static Stack<Vector3> CreatePathStack(Node targetNode, Room room)
    {
        Stack<Vector3> movementPath = new Stack<Vector3>();
        Node nextNode = targetNode;

        Vector3 cellMidPoint = room.InstantiatedRoom.grid.cellSize * 0.5f;
        cellMidPoint.z = 0f;
        while (nextNode != null)
        {
            // 将节点的位置转换为世界坐标
            Vector3 worldPos = room.InstantiatedRoom.grid.CellToWorld(new Vector3Int(nextNode.GridPosition.x 
                + room.TemplateLowerBounds.x, nextNode.GridPosition.y + room.TemplateLowerBounds.y, 0)) + cellMidPoint;
            movementPath.Push(worldPos);
            
            nextNode = nextNode.ParentNode;
        }
        
        return movementPath;
    }

    private static Node FindShortestPath(Node startNode, Node targetNode, GridNodes gridNodes, List<Node> openNodeList, 
        HashSet<Node> closedNodeHashSet, InstantiatedRoom roomInstantiatedRoom)
    {
        // 为open表添加开始节点
        openNodeList.Add(startNode);
        
        // 循环直到open表为空
        while (openNodeList.Count > 0)
        {
            // open表排序
            openNodeList.Sort();
            // 获取open表中F值最小的节点
            Node currentNode = openNodeList[0];
            // 将当前节点从open表中移除
            openNodeList.RemoveAt(0);
        
            if (currentNode == targetNode)
            {
                return currentNode;
            }
            
            // 将当前节点添加到closed表中
            closedNodeHashSet.Add(currentNode);
            
            // 获取当前节点周围的节点
            EvaluateCurrentNodeNeighbours(currentNode, targetNode, gridNodes, openNodeList, closedNodeHashSet, 
                roomInstantiatedRoom);
        }

        return null;
    }

    private static void EvaluateCurrentNodeNeighbours(Node currentNode, Node targetNode, GridNodes gridNodes,
        List<Node> openNodeList, HashSet<Node> closedNodeHashSet, InstantiatedRoom roomInstantiatedRoom)
    {
        Vector2Int currentNodeGridPos = currentNode.GridPosition;
        Node validNeighbourNode;

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                // 排除当前节点
                if (i == 0 && j == 0)
                {
                    continue;
                }
                
                // 获取当前节点的邻居节点
                validNeighbourNode = GetValidNodeNeighbour(currentNodeGridPos.x + i, currentNodeGridPos.y + j, 
                    gridNodes, closedNodeHashSet, roomInstantiatedRoom);

                if (validNeighbourNode != null)
                {
                    int movementPenaltyForGridSpace = roomInstantiatedRoom.AStarMovementPenalty[validNeighbourNode.GridPosition.x, 
                        validNeighbourNode.GridPosition.y];
                    // 计算当前节点到邻居节点的代价
                    int newCostToNeighbour = currentNode.GCost + GetDistance(currentNode, validNeighbourNode) + movementPenaltyForGridSpace;
                    // 是否在open表中
                    bool isValidNeighbourInOpenList = openNodeList.Contains(validNeighbourNode);
                    // 如果新的代价小于邻居节点的代价或者邻居节点不在open表中
                    if (newCostToNeighbour < validNeighbourNode.GCost || !isValidNeighbourInOpenList)
                    {
                        validNeighbourNode.GCost = newCostToNeighbour;
                        validNeighbourNode.HCost = GetDistance(validNeighbourNode, targetNode);
                        validNeighbourNode.ParentNode = currentNode;
                        
                        if (!isValidNeighbourInOpenList)
                        {
                            openNodeList.Add(validNeighbourNode);
                        }
                    }
                }
            }
        }
    }

    private static int GetDistance(Node currentNode, Node validNeighbourNode)
    {
        int distanceX = Mathf.Abs(currentNode.GridPosition.x - validNeighbourNode.GridPosition.x);
        int distanceY = Mathf.Abs(currentNode.GridPosition.y - validNeighbourNode.GridPosition.y);
        
        if(distanceX > distanceY)
        {
            // 14是斜向的代价，10是直线的代价
            return 14 * distanceY + 10 * (distanceX - distanceY);
        }
        else
        {
            return 14 * distanceX + 10 * (distanceY - distanceX);
        }
    }

    private static Node GetValidNodeNeighbour(int neighbourNodeXPos, int neighbourNodeYPos, GridNodes gridNodes, 
        HashSet<Node> closedNodeHashSet, InstantiatedRoom roomInstantiatedRoom)
    {
        // 判断邻居节点是否在房间的格子内
        if(neighbourNodeXPos >= roomInstantiatedRoom.Room.TemplateUpperBounds.x - roomInstantiatedRoom.Room.TemplateLowerBounds.x || 
           neighbourNodeXPos < 0 || neighbourNodeYPos >= roomInstantiatedRoom.Room.TemplateUpperBounds.y - roomInstantiatedRoom.Room.TemplateLowerBounds.y || 
           neighbourNodeYPos < 0)
        {
            return null;
        }
        
        // 获取邻居节点
        Node neighbourNode = gridNodes.GetGridNode(neighbourNodeXPos, neighbourNodeYPos);
        int movementPenaltyForGridSpace = roomInstantiatedRoom.AStarMovementPenalty[neighbourNodeXPos, neighbourNodeYPos];
        
        // 判断邻居节点是否已经评估过或者是障碍物
        if(movementPenaltyForGridSpace == 0 || closedNodeHashSet.Contains(neighbourNode))
        {
            return null;
        }
        else
        {
            return neighbourNode;
        }
    }
}
