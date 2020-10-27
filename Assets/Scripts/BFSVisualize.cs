using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BFSVisualize : MonoBehaviour
{
    private static BFSVisualize instance;

    public static BFSVisualize myInstance
    {
        get
        {
            if(instance == null)
            {
                instance = FindObjectOfType<BFSVisualize>();
            }
            return instance;
        }  
    }

    [SerializeField]
    private Grid grid;

    private Tile tile;


    [SerializeField]
    private Color openColor, closedColor, pathColor, currentColor, startColor, goalColor;

    [SerializeField]
    private Canvas canvas;

    private List<GameObject> debugObjects = new List<GameObject>(); 

    

}
