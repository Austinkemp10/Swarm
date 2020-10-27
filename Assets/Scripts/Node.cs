using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public int G { get; set; }
    public int H { get; set; }
    public int F { get; set; }
    public Node parent { get; set; }

    public bool visited { get; set; }

    public Vector3Int Position {get; set;}

    public Node(Vector3Int position)
    {
        this.Position = position;
    }
    public Node(Vector3Int position, Node newParent)
    {
        this.Position = position;
        this.parent = newParent;
    }
}