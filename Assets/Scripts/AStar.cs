using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public enum TileType {START, GOAL, WATER, GRASS, PATH, STEP, COMPLETE, RESET, DEBUG}
public class AStar : MonoBehaviour
{



    /* =================================================================================================
     * Global Variables
     * ===============================================================================================*/
    private float delay = 0f;
    private bool pathExists = false;

    private TileType tileType;

    [SerializeField]
    private Tilemap tilemap;

    [SerializeField]
    private Tile[] tiles;

    [SerializeField]
    private RuleTile water;

    [SerializeField]
    private Camera camera;

    [SerializeField]
    private LayerMask layerMask;

    private Vector3Int startPos, goalPos;

    private Node current;
    
    private HashSet<Node> openList;

    private HashSet<Node> closedList;

    List<Node> neighbors;

    private List<Vector3Int> waterTiles = new List<Vector3Int>();

    private Stack<Vector3Int> path;

    private Dictionary<Vector3Int, Node> allNodes = new Dictionary<Vector3Int, Node>();


    /* =================================================================================================
     * End of global variables
     * ===============================================================================================*/





    /* =================================================================================================
     * Function             :               Update
     * Purpose              :               Run algorithm when spacebar is pressed and change the tile
     *                                      that is clicked. 
     * ===============================================================================================*/
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(camera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, Mathf.Infinity, layerMask);

            if (hit.collider != null)
            {
                //Hit something in world
                Vector3 mouseWorldPos = camera.ScreenToWorldPoint(Input.mousePosition);
                Vector3Int clickPos = tilemap.WorldToCell(mouseWorldPos);

                changeTile(clickPos);
            }
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            //Run algorithm for A* with visualization

            Algorithm();
        }
    }



    /* =================================================================================================
     * Function             :               Init
     * Purpose              :               Generate starting lists and nodes for the A* script
     * ===============================================================================================*/
    private void Init()
    {
        current = getNode(startPos);

        openList = new HashSet<Node>();

        closedList = new HashSet<Node>();

        //1 Add start node to list
        openList.Add(current);
    }





    /* =================================================================================================
     * Function             :               Algorithm
     * Purpose              :               While there are items in the openList run the algorithm
     * ===============================================================================================*/
    private void Algorithm()
    {
        
        if (current == null)
        {
            Init();
        }

        int count = 0;

        while(openList.Count > 0 && path == null)
        {
            colorDelay();
             
            count++;
        }
        StartCoroutine(AStarDebugger.myInstance.colorNeighbors(openList, closedList, neighbors, startPos, goalPos, delay, pathExists, path));
    }




    /* =================================================================================================
     * Function             :               colorDelay
     * Purpose              :               Nested function that can be called to compartmentalize the
     *                                      algorithm into steps.
     * ===============================================================================================*/
    private void colorDelay()
    {

        List<Node> next = findNeighbors(current.Position);

        examineNeighbors(next, current);


        updateCurrentTile(ref current);


        path = generatePath(current);


    }



    /* =================================================================================================
     * Function             :               findNeighbors
     * Purpose              :               Find all the neighbors of the current node. 
     *                                      Also calls the Coroutine on colorNeighbors to color as the
     *                                      function runs.
     * ===============================================================================================*/
    private List<Node> findNeighbors(Vector3Int parentPosition)
    {

        neighbors = new List<Node>();      

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3Int neighborPos = new Vector3Int(parentPosition.x - x, parentPosition.y - y, parentPosition.z);

                if(y != 0 || x != 0)
                {
                    if(neighborPos != startPos && tilemap.GetTile(neighborPos) && !waterTiles.Contains(neighborPos))
                    {
                        //Cant be parent
                        Node neighbor = getNode(neighborPos);
                        neighbors.Add(neighbor);
                    }
                    
                }
            }
        }

        StartCoroutine(AStarDebugger.myInstance.colorNeighbors(openList, closedList, neighbors, startPos, goalPos, delay, pathExists, path));
        delay++;
        return neighbors;

    }




    /* =================================================================================================
     * Function             :               examineNeighbors
     * Purpose              :               Checks the status of the neighbors of the current node and 
     *                                      decides what action to take on them. Also calls the calcValues
     *                                      function to calculate the optimal path. 
     * ===============================================================================================*/
    private void examineNeighbors(List<Node> neighbors, Node current)
    {
        for (int i = 0; i < neighbors.Count; i++)
        {
            Node neighbor = neighbors[i];

            if (!connectedDiagonally(current, neighbor))
            {
                continue;
            }

            int gScore = determineGScore(neighbors[i].Position, current.Position);

            if(openList.Contains(neighbor))
            {
                
                if(current.G + gScore < neighbor.G)
                {
                    calcValues(current, neighbor, gScore);
                }
            }
            else if (!closedList.Contains(neighbor))
            {
                calcValues(current, neighbor, gScore);

                openList.Add(neighbor);
            }
        }
    }


    /* =================================================================================================
     * Function             :               calcValues
     * Purpose              :               Calculates the values of the vectors to determine the
     *                                      optimal path. 
     * ===============================================================================================*/
    private void calcValues(Node parent, Node neighbor, int cost)
    {
        neighbor.parent = parent;

        neighbor.G = parent.G + cost; //Accumulate cost of the path as we go

        neighbor.H = ((System.Math.Abs((neighbor.Position.x - goalPos.x)) + System.Math.Abs((neighbor.Position.y - goalPos.y))) * 10);

        neighbor.F = neighbor.G + neighbor.H;
    }



    /* =================================================================================================
     * Function             :               determineGScore
     * Purpose              :               calculate the score between current and neighbor nodes
     * ===============================================================================================*/
    private int determineGScore(Vector3Int neighbor, Vector3Int current)
    {
        int gScore = 0;

        int x = current.x - neighbor.x;
        int y = current.y - neighbor.y;

        if(System.Math.Abs(x - y) % 2 == 1)
        {
            gScore = 10;
        } else
        {
            gScore = 14;
        }
        return gScore;
    }



    /* =================================================================================================
     * Function             :               updateCurrentTile
     * Purpose              :               update the open and closed lists with the current tile info
     * ===============================================================================================*/
    private void updateCurrentTile(ref Node current)
    {       
        openList.Remove(current);
        closedList.Add(current);

        if(openList.Count > 0)
        {
            //Get surrounding square with lowest f score
            current = openList.OrderBy(x => x.F).First();
        }
    }



    /* =================================================================================================
     * Function             :               getNode
     * Purpose              :               get the node at a position
     * ===============================================================================================*/
    private Node getNode(Vector3Int position)
    {
        if(allNodes.ContainsKey(position))
        {
            return allNodes[position];
        }
        else
        {
            //New map area (Create the node to be used)
            Node node = new Node(position);
            allNodes.Add(position, node);
            return node;

        }
    }




    /* =================================================================================================
     * Function             :               changeTileType
     * Purpose              :               Set the selected tile
     * ===============================================================================================*/
    public void changeTileType(TileButton button)
    {
        //Set tile to selected button
        tileType = button.myTileType;

        if(tileType == TileType.STEP)
        {
            //Nothing yet
        }
        else if(tileType == TileType.COMPLETE)
        {
            //Nothing yet
        }
        else if(tileType == TileType.RESET)
        {
            tilemap.ClearAllTiles();
        }
        else if(tileType == TileType.DEBUG)
        {
            Algorithm();
        }
    }



    /* =================================================================================================
     * Function             :               changeTile
     * Purpose              :               Set the option to the currently selected tile. 
     * ===============================================================================================*/
    private void changeTile(Vector3Int clickPos)
    {
        //Function to change chosen tile into what we have selected
        if (tileType == TileType.WATER)
        {
            tilemap.SetTile(clickPos, water);
            waterTiles.Add(clickPos);
        } else
        {

            if(tileType == TileType.START)
            {
                startPos = clickPos;
            } else if (tileType == TileType.GOAL)
            {
                goalPos = clickPos;
            }
            tilemap.SetTile(clickPos, tiles[(int)tileType]);
        }
        
        
    }



    /* =================================================================================================
     * Function             :               connectedDiagonally
     * Purpose              :               returns a boolean of whether or not two tiles should be
     *                                      considered connected(algorithm cant jump diagnols)
     * ===============================================================================================*/
    private bool connectedDiagonally(Node currentNode, Node neighbor)
    {
        Vector3Int direct = currentNode.Position - neighbor.Position;

        Vector3Int first = new Vector3Int(current.Position.x + (direct.x * -1), current.Position.y, current.Position.z);

        Vector3Int second = new Vector3Int(current.Position.x, current.Position.y + (direct.y * -1), current.Position.z);

        if(waterTiles.Contains(first) || waterTiles.Contains(second))
        {
            return false;
        }
        return true;
    }



    /* =================================================================================================
     * Function             :               generatePath
     * Purpose              :               Returns the optimal pathh
     * ===============================================================================================*/
    private Stack<Vector3Int> generatePath(Node current)
    {
        if(current.Position == goalPos)
        {
            Stack<Vector3Int> finalPath = new Stack<Vector3Int>();

            while(current.Position != startPos)
            {
                finalPath.Push(current.Position);

                current = current.parent;
            }
            return finalPath;
        }

        return null;
        
    }
}
