using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// ��������
/// </summary>
public enum GridType
{
    normal,
    obstacle
}
/// <summary>
/// ��ͼ����
/// </summary>
public class Grid
{
    public GridType type;
    public int x;
    public int y;
    public float f;
    public float g;
    public float h;
    public Grid parent;
}
/// <summary>
/// A��Ѱ·
/// </summary>
public class AStar
{
    private Grid[,] _map;
    private int _mapWidth;
    private int _mapHeight;
    public AStar()
    {

    }

    public void CreateMap(Grid[,] map)
    {
        _map = map;
        _mapWidth = _map.GetLength(0);
        _mapHeight = _map.GetLength(1);
    }

    private Grid getGridMinF(List<Grid> openList)
    {
        var minGrid = openList[0];
        var minF = minGrid.f;
        for (int i = 0; i < openList.Count; i++)
        {
            var grid = openList[i];
            if (grid.f < minF)
            {
                minGrid = grid;
                minF = grid.f;
            }
        }
        return minGrid;
    }

    private int GetGridH(Grid grid, Grid end)
    {
        //�������㷨
        var disX = Math.Abs(grid.x - end.x);
        var disY = Math.Abs(grid.y - end.y);
        return disX + disY;
    }

    private float GetGridG(Grid grid, Grid parent)
    {
        if (parent == null)
            return 0;
        return (float)Math.Abs(Math.Sqrt(Math.Pow(grid.x - parent.x, 2) + Math.Pow(grid.y - parent.y, 2))) + parent.g;
    }

    private int[,] _rounds = new int[8, 2] { { 0, -1 }, { 1, -1 }, { 1, 0 }, { 1, 1 }, { 0, 1 }, { -1, -1 }, { -1, 0 }, { -1, 1 } };
    private List<Grid> GetRoundGrids(Grid grid, List<Grid> closeList)
    {
        var roundGrids = new List<Grid>();
        int row = _rounds.GetLength(0);
        int col = _rounds.GetLength(1);
        for (int i = 0; i < row; i++)
        {
            var offsetX = _rounds[i, 0];
            var offsetY = _rounds[i, 1];
            var x = grid.x + offsetX;
            var y = grid.y + offsetY;
            if (x >= 0 && x < _mapWidth && y >= 0 && y < _mapHeight)
            {
                var point = _map[x, y];
                if (point != null && point.type == GridType.normal && closeList.IndexOf(point) < 0)
                {
                    roundGrids.Add(point);
                }
            }
        }
        return roundGrids;
    }


    public List<Grid> FindPath(Grid start, Grid end)
    {
        var pathList = new List<Grid>();
        //��ʼ����Ŀ���һ��ֱ�ӷ���
        if (start == end)
        {
            return pathList;
        }
        var openList = new List<Grid>();
        var closeList = new List<Grid>();
        openList.Add(start);
        while (openList.Count > 0)
        {
            var minGrid = getGridMinF(openList);
            if (minGrid == end)
            {
                break;
            }

            //������ǰ������Χ�ڵ�,Ĭ��8����
            var roundGrids = GetRoundGrids(minGrid, closeList);
            for (int i = 0; i < roundGrids.Count; i++)
            {
                var grid = roundGrids[i];
                if (openList.IndexOf(grid) >= 0)
                {
                    var newG = GetGridG(grid, minGrid);
                    //Debug.Log("�����µ�g="+newG +",�ϵ�g="+grid.g+ ",����x=" + grid.x + ",y=" + grid.y + ",��ǰ����x=" + minGrid.x + ",y=" + minGrid.y);
                    if (newG < grid.g)
                    {
                        //Debug.Log("����Ϊ�µ�");
                        grid.parent = minGrid;
                        grid.g = newG;
                        grid.f = grid.g + grid.h;
                    }
                }
                else
                {
                    grid.parent = minGrid;
                    grid.g = GetGridG(grid, grid.parent);
                    grid.h = GetGridH(grid, end);
                    grid.f = grid.g + grid.h;
                    openList.Add(grid);
                }
            }

            closeList.Add(minGrid);
            openList.Remove(minGrid);
        }

        //����·��
        var g = end;
        while (g != start && g.parent != null)
        {
            pathList.Add(g);
            g = g.parent;
        }

        return pathList;
    }
}