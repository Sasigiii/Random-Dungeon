using System;
using UnityEngine;

// IComparable: 定义一个通用的比较方法，使对象可以排序。
public class Node : IComparable<Node>
{
    public Vector2Int GridPosition;
    public  int GCost = 0;
    public  int HCost = 0;
    public Node ParentNode;
    public int FCost => GCost + HCost;
    
    public Node(Vector2Int gridPosition)
    {
        this.GridPosition = gridPosition;
        ParentNode = null;
    }


    // 大于返回1，等于返回0，小于返回-1
    public int CompareTo(Node nodeToCompare)
    {
        int compare = FCost.CompareTo(nodeToCompare.FCost);
        
        if (compare == 0)
        {
            // 如果F值相等，比较H值
            compare = HCost.CompareTo(nodeToCompare.HCost);
        }
        
        return compare;
    }
}
