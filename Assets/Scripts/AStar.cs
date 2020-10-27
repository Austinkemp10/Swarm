using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public enum TileType {START, GOAL, WATER, GRASS, ASTAR, BFS, BI, RESET}
[Serializable]
public class AStar : MonoBehaviour
{



    /* =================================================================================================
     * Global Variables
     * ===============================================================================================*/
    private float delay = 0f;
    private bool pathExists = false;
    private bool startExists = false;
    private bool goalExists = false;

    private bool goalFound = false;
    private Node goalNode;

    private Node start, end;

    private TileType tileType;

    private List<Vector3Int> visited = new List<Vector3Int>();
    private int[,] board = new int[17, 10];

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


    private void Start()
    {
        
    }


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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            //Run algorithm for A* with visualization
            delay = 0.0f;
            current = null;
            path = null;
            AStarAlgorithm();
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            //Run algorithm for DFS with visualization
            delay = 0.0f;
            BiSearch(/*new Node(startPos), new Node(goalPos)*/);


        } else if(Input.GetKeyDown(KeyCode.DownArrow))
        {
            //Run algorithm for BFS with visualization
            delay = 0.0f;
            BFSearch(/*new Node(startPos), new Node(goalPos)*/);

        } else if(Input.GetKeyDown(KeyCode.C))
        {
            AStarDebugger.myInstance.clearBoard(openList, closedList, visited, path);
        }

    }




    /* =================================================================================================
     * Function             :               clear
     * Purpose              :               used to clear the board
     * ===============================================================================================*/
    public void clear()
    {
        delay = 0.0f;
        AStarDebugger.myInstance.clearBoard(openList, closedList, visited, path);
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
    public void AStarAlgorithm()
    {
        current = null;
        path = null;
        start = new Node(startPos);
        end = new Node(goalPos);
        delay = 0.0f;
        if (current == null)
        {
            Init();
        }

        int count = 0;

        while (openList.Count > 0 && path == null)
        {
            Debug.Log("In while loop AStar");
            colorDelay();
             
            count++;
        }
        
        StartCoroutine(AStarDebugger.myInstance.colorNeighbors(openList, closedList, neighbors, startPos, goalPos, delay, pathExists, path));
    }



    /* =================================================================================================
     * Function             :               bfs
     * Purpose              :               function that drives the breadth first search
     * ===============================================================================================*/
    public void BFSearch()
    {
        delay = 0.0f;
        start = new Node(startPos);
        end = new Node(goalPos);


        path = null;
        Queue<Node> q = new Queue<Node>();
        List<Node> unvisited = null;
        Node c = null;
        goalFound = false;
        q.Enqueue(start);
        visited.Add(new Vector3Int(start.Position.x, start.Position.y, start.Position.z));
        Debug.Log("Start BFS");

        while (q.Count > 0 && path == null && !goalFound)
        {
            Debug.Log("In while");
            c = q.Dequeue();

            if(c == null)
            {
                Debug.Log("Trouble~!");
            }
            unvisited = getAdjCells(c);
            for (int i = 0; i < unvisited.Count; i++)
            {
                Debug.Log("In for");
                if (unvisited.ElementAt(i).Position != goalPos && unvisited.ElementAt(i).Position != startPos)
                {
                    StartCoroutine(AStarDebugger.myInstance.bfsColor(unvisited.ElementAt(i).Position, Color.cyan, delay));
                    delay = delay + 0.02f;
                }
                
                if (unvisited.ElementAt(i).Position == goalPos)
                {
                    goalFound = true;
                    Debug.Log("Path found");
                    break;
                }
                if (unvisited != null)
                {

                    unvisited.ElementAt(i).parent = c;
                    if (c.Position != goalPos && c.Position != startPos)
                    {
                        StartCoroutine(AStarDebugger.myInstance.bfsColor(c.Position, Color.blue, delay));
                        delay = delay + 0.02f;
                    }
                    
                    q.Enqueue(unvisited.ElementAt(i));
                }
                else
                {
                    if (q.Count > 0)
                    {
                        q.Dequeue();
                    }

                }
            }
            
        }
        if(goalFound)
        {
            Debug.Log("Goal Found");
            while ((c != null) && (c != start))
            {
                if (c.Position != goalPos)
                {
                    StartCoroutine(AStarDebugger.myInstance.bfsColor(c.Position, Color.red, delay));
                    c = c.parent;
                }

            }
        }
        

        visited.Clear();
    }



    private List<Node> getAdjCells(Node node)
    {
        int[,] moves = { { -1, 0 }, { 0, -1 }, { 1, 0 }, { 0, 1 }, { -1, 1 }, { -1, -1 }, { 1, -1 }, { 1, 1 } };
        List<Node> neighbors = new List<Node>();

        for(int n = 0; n < 8; n++)
        {
            int xVal = node.Position.x + moves[n, 0];
            int yVal = node.Position.y + moves[n, 1];
            if(xVal + 8 >= 0 && xVal + 8 <= 17 && yVal + 5 >= 0 && yVal + 5 <= 9)
            {
                Vector3Int neighborPos = new Vector3Int(node.Position.x + moves[n, 0], node.Position.y + moves[n, 1], node.Position.z);

                if (neighborPos != startPos && tilemap.GetTile(neighborPos) && !waterTiles.Contains(neighborPos) && !visited.Contains(neighborPos) && connectedDiagonally(node, getNode(neighborPos)))
                {
                    visited.Add(neighborPos);
                    neighbors.Add(new Node(neighborPos));

                }
            }
            
        }

        return neighbors;
    }




    private List<Node> getBiAdjCells(Node node, HashSet<Vector3Int> biVisited)
    {
        int[,] moves = { { -1, 0 }, { 0, -1 }, { 1, 0 }, { 0, 1 }, { -1, 1 }, { -1, -1 }, { 1, -1 }, { 1, 1 } };
        List<Node> neighbors = new List<Node>();

        for (int n = 0; n < 8; n++)
        {
            int xVal = node.Position.x + moves[n, 0];
            int yVal = node.Position.y + moves[n, 1];
            if (xVal + 8 >= 0 && xVal + 8 <= 17 && yVal + 5 >= 0 && yVal + 5 <= 9)
            {
                Vector3Int neighborPos = new Vector3Int(node.Position.x + moves[n, 0], node.Position.y + moves[n, 1], node.Position.z);

                if (neighborPos != startPos && tilemap.GetTile(neighborPos) && !waterTiles.Contains(neighborPos) && !biVisited.Contains(neighborPos) && connectedDiagonally(node, getNode(neighborPos)))
                {
                    visited.Add(neighborPos);
                    neighbors.Add(new Node(neighborPos));

                }
            }

        }

        return neighbors;
    }


    /* =================================================================================================
     * Function             :               DFSearch
     * Purpose              :               function that drives the depth first search
     * ===============================================================================================*/
    public void BiSearch()
    {
        //Reset status
        delay = 0.0f;
        start = new Node(startPos);
        end = new Node(goalPos);

        Queue<Node> q1 = new Queue<Node>();
        Queue<Node> q2 = new Queue<Node>();

        HashSet<Node> startVisit = new HashSet<Node>();
        HashSet<Node> goalVisit = new HashSet<Node>();

        HashSet<Vector3Int> visited1 = new HashSet<Vector3Int>();
        HashSet<Vector3Int> visited2 = new HashSet<Vector3Int>();

        Node foundStart = null;
        Node foundGoal = null;
        
        goalFound = false;
        
        q1.Enqueue(start);
        q2.Enqueue(end);
        
        Debug.Log("Start Bi-Directional");

        visited1.Add(start.Position);
        visited2.Add(end.Position);

        //While we have something in the queue and we haven't found the goal
        while (q1.Count > 0 || q2.Count > 0 && !goalFound)
        {

            //For each of the neighbors
            if (q1.Count > 0 && foundStart == null)
            {
                Node next = q1.Dequeue();

                List<Node> unvisited1 = getBiAdjCells(next, visited1);

                foreach (Node adjacent in unvisited1)
                {
                    adjacent.parent = next;
                    startVisit.Add(adjacent);
                    if(adjacent.Position != end.Position)
                    {
                        StartCoroutine(AStarDebugger.myInstance.bfsColor(adjacent.Position, Color.blue, delay));
                        delay = delay + 0.05f;
                    }
                    
                    if (visited2.Contains(adjacent.Position))
                    {
                        goalFound = true;
                        foundStart = adjacent;
                        while (foundStart != start)
                        {
                            Debug.Log(foundStart.parent);
                            StartCoroutine(AStarDebugger.myInstance.bfsColor(foundStart.Position, Color.red, delay));
                            //delay = delay + 0.05f;
                            foundStart = foundStart.parent;
                        }

                        foreach (Node x in goalVisit)
                        {
                            if (x.Position.x == adjacent.Position.x && x.Position.y == adjacent.Position.y)
                            {
                                foundGoal = x;
                            }
                        }
                        if (foundGoal != null)
                        {
                            while (foundGoal != start && foundGoal != end)
                            {
                                StartCoroutine(AStarDebugger.myInstance.bfsColor(foundGoal.Position, Color.red, delay));
                                //delay = delay + 0.05f;
                                foundGoal = foundGoal.parent;
                            }
                        }
                        return;
                    }
                    else if (visited1.Add(adjacent.Position))
                    {
                        q1.Enqueue(adjacent);
                    }
                }
            }

            if (q2.Count > 0)
            {
                Node next = q2.Dequeue();

                List<Node> unvisited2 = getBiAdjCells(next, visited2);

                foreach (Node adjacent in unvisited2)
                {
                    adjacent.parent = next;
                    goalVisit.Add(adjacent);
                    if (adjacent.Position != end.Position)
                    {
                        StartCoroutine(AStarDebugger.myInstance.bfsColor(adjacent.Position, Color.blue, delay));
                        delay = delay + 0.05f;
                    }
                    if (visited1.Contains(adjacent.Position))
                    {
                        goalFound = true;
                        foundGoal = adjacent;
                        foreach(Node x in startVisit)
                        {
                            if(x.Position.x == adjacent.Position.x && x.Position.y == adjacent.Position.y)
                            {
                                foundStart = x;
                            }
                        }
                        if(foundStart != null)
                        {
                            while (foundStart != start)
                            {
                                StartCoroutine(AStarDebugger.myInstance.bfsColor(foundStart.Position, Color.red, delay));
                                //delay = delay + 0.05f;
                                foundStart = foundStart.parent;
                            }
                        }
                        

                        while (foundGoal != end)
                        {
                            Debug.Log(foundGoal.parent);
                            StartCoroutine(AStarDebugger.myInstance.bfsColor(foundGoal.Position, Color.red, delay));
                            //delay = delay + 0.05f;
                            foundGoal = foundGoal.parent;
                        }
                        return;
                    }
                    else if (visited2.Add(adjacent.Position))
                    {
                        q2.Enqueue(adjacent);
                    }
                }
            }

        }
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
                    if(neighborPos != startPos && tilemap.GetTile(neighborPos) && !waterTiles.Contains(neighborPos) && connectedDiagonally(current, getNode(neighborPos)))
                    {
                        //Cant be parent
                        Node neighbor = getNode(neighborPos);
                        neighbors.Add(neighbor);
                    }
                    
                }
            }
        }

        StartCoroutine(AStarDebugger.myInstance.colorNeighbors(openList, closedList, neighbors, startPos, goalPos, delay, pathExists, path));
        delay = delay + 0.1f;
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

        if (tileType == TileType.ASTAR)
        {
            delay = 0.0f;
            current = null;
            path = null;
            AStarAlgorithm();

        }

        if (tileType == TileType.BFS)
        {
            delay = 0.0f;
            BFSearch(/*new Node(startPos), new Node(goalPos)*/);
        }

        if (tileType == TileType.BI)
        {
            delay = 0.0f;
            BiSearch(/*new Node(startPos), new Node(goalPos)*/);
        }

        if (tileType == TileType.RESET)
        {
            AStarDebugger.myInstance.clearBoard(openList, closedList, visited, path);
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

            if (tileType == TileType.START)
            {
                //Check if start is on map anywhere
                if (!startExists)
                {
                    startPos = clickPos;
                    startExists = true;
                    start = new Node(startPos);
                }
                else
                {
                    tilemap.SetTile(startPos, tiles[3]);
                    startPos = clickPos;
                    startExists = true;
                }
            }
            else if (tileType == TileType.GOAL)
            {
                if (!goalExists)
                {
                    goalPos = clickPos;
                    goalExists = true;
                    end = new Node(goalPos);
                }
                else
                {
                    tilemap.SetTile(goalPos, tiles[3]);
                    goalPos = clickPos;
                    goalExists = true;
                }
            }
            tilemap.SetTile(clickPos, tiles[(int)tileType]);
        }

        if(tileType == TileType.ASTAR)
        {
            delay = 0.0f;
            current = null;
            path = null;
            AStarAlgorithm();

        }

        if(tileType == TileType.BFS)
        {
            delay = 0.0f;
            BFSearch(/*new Node(startPos), new Node(goalPos)*/);
        }

        if (tileType == TileType.BI)
        {
            delay = 0.0f;
            BiSearch(/*new Node(startPos), new Node(goalPos)*/);
        }

        if (tileType == TileType.RESET)
        {
            AStarDebugger.myInstance.clearBoard(openList, closedList, visited, path);
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

        Vector3Int first = new Vector3Int(currentNode.Position.x + (direct.x * -1), currentNode.Position.y, currentNode.Position.z);

        Vector3Int second = new Vector3Int(currentNode.Position.x, currentNode.Position.y + (direct.y * -1), currentNode.Position.z);

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




    /* =================================================================================================
     * Function             :               generateBiPath
     * Purpose              :               Returns the optimal pathh
     * ===============================================================================================*/
    private Stack<Vector3Int> generateBiPath(Node start, Node end)
    {
        if(start != null)
        {
            Stack<Vector3Int> finalPath = new Stack<Vector3Int>();

            while (start.Position != end.Position)
            {
                finalPath.Push(start.Position);

                start = start.parent;
            }
            return finalPath;
        }

        return null;

    }




}
