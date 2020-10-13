using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualizeText : MonoBehaviour
{

    [SerializeField]
    private RectTransform arrow;

    public RectTransform myArrow { get => arrow; set => arrow = value; }
}
