using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// С��ͼ
/// </summary>
public class Minimap : MonoBehaviour
{
    public Transform dot;
    public Transform arrow;
    public Direction directionAxisd;
    public bool showDirectionArrow;
    //ҡ���ƶ����뾶
    public float maxRadius = 38;

    public Action<Vector2> onPointerDown;
    public Action<Vector2> onPointerUp;
    public Action<Vector2> onPointerMove;
    void Start()
    {
    }
    void Update()
    {
    }
}