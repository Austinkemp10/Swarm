using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AStarDebugger : MonoBehaviour
{
    private static AStarDebugger instance;

    public static AStarDebugger myInstance
    {
        get
        {
            if(instance == null)
            {
                instance = FindObjectOfType<AStarDebugger>();
            }

            return instance;
        }
    }

    [SerializeField]
    private Grid grid;

    [SerializeField]
    private Tilemap tilemap;

    [SerializeField]
    private Tile tile;

    [SerializeField]
    private Color openColor, closedColor, pathColor, currentColor, startColor, goalColor;

    [SerializeField]
    private Canvas canvas;

    [SerializeField]
    private GameObject debugTextPrefab;


    private List<GameObject> debugObjects = new List<GameObject>();


    public void createTiles(HashSet<Node> openList, HashSet<Node> closedList, Dictionary<Vector3Int, Node> allNodes,  Vector3Int start, Vector3Int goal, Stack<Vector3Int> path = null)
    {
        
    }

    /* =================================================================================================
     * Function             :               GenerateDebugText
     * Purpose              :               Generate the arrows for the squares to point to the parents
     * ===============================================================================================*/
    private void GenerateDebugText(Node node, VisualizeText visText)
    {
        //Check x and y value in respect to parent value to turn arrow
        if (node.parent.Position.x < node.Position.x && node.parent.Position.y == node.Position.y)
        {
            visText.myArrow.localRotation = Quaternion.Euler(new Vector3(0, 0, 180));
        }
        else if (node.parent.Position.x < node.Position.x && node.parent.Position.y > node.Position.y)
        {
            visText.myArrow.localRotation = Quaternion.Euler(new Vector3(0, 0, 135));
        }
        else if (node.parent.Position.x < node.Position.x && node.parent.Position.y < node.Position.y)
        {
            visText.myArrow.localRotation = Quaternion.Euler(new Vector3(0, 0, 225));
        }
        else if (node.parent.Position.x > node.Position.x && node.parent.Position.y == node.Position.y)
        {
            visText.myArrow.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        }
        else if (node.parent.Position.x > node.Position.x && node.parent.Position.y > node.Position.y)
        {
            visText.myArrow.localRotation = Quaternion.Euler(new Vector3(0, 0, 45));
        }
        else if (node.parent.Position.x > node.Position.x && node.parent.Position.y < node.Position.y)
        {
            visText.myArrow.localRotation = Quaternion.Euler(new Vector3(0, 0, -45));
        }
        else if (node.parent.Position.x == node.Position.x && node.parent.Position.y > node.Position.y)
        {
            visText.myArrow.localRotation = Quaternion.Euler(new Vector3(0, 0, 90));
        }
        else if (node.parent.Position.x == node.Position.x && node.parent.Position.y < node.Position.y)
        {
            visText.myArrow.localRotation = Quaternion.Euler(new Vector3(0, 0, 270));
        }
    }



    /* =================================================================================================
     * Function             :               colorTile
     * Purpose              :               Set the color of a tile
     * ===============================================================================================*/
    public void colorTile(Vector3Int position, Color color)
    {
        tilemap.SetTile(position, tile);
        tilemap.SetTileFlags(position, TileFlags.None);
        tilemap.SetColor(position, color);
    }




    /* =================================================================================================
     * Function             :               colorNeighbors
     * Purpose              :               color the neighbors of a tile(which tiles are touched)
     * ===============================================================================================*/
    public IEnumerator colorNeighbors(HashSet<Node> openList, HashSet<Node> closedList, List<Node> neighbors, Vector3Int start, Vector3Int goal, float count, bool pathExists, Stack<Vector3Int> path = null)
    {
        yield return new WaitForSeconds(count + 0.1f);


        foreach (Node n in neighbors)
        {
            if (n.Position != start && n.Position != goal && openList.Contains(n))
            {
                colorTile(n.Position, openColor);
            }          
            else if (n.Position != start && n.Position != goal && closedList.Contains(n))
            {
                colorTile(n.Position, closedColor);
            }
        }

        if (path != null)
        {
            foreach (Vector3Int pos in path)
            {

                if (pos != start && pos != goal)
                {
                    tilemap.RefreshTile(pos);
                    Debug.Log("x: " + pos.x + "   y: " + pos.y);
                    colorTile(pos, pathColor);
                }
            }

        }
    }
}
