using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;

public class Index
{

}
/// <summary>
/// ��ͼ����
/// </summary>
public class Grid
{
    public float f;
    public float g;
    public float h;

    public Vector2 pos;
    public Vector2 index;
}
/// <summary>
/// A��Ѱ·
/// </summary>
public class AStar
{
    public AStar()
    {

    }

    public void CreateMap()
    {

    }

    public void mathG()
    {

    }

    public Grid getMinF(List<Grid> openList)
    {
        var minGrid = openList[0];
        var minF = minGrid.f;
        for(int i = 0; i < openList.Count; i++)
        {
            var grid = openList[i];
            if(grid.f < minF)
            {
                minGrid = grid;
                minF = grid.f;
            }
        }
        return minGrid;
    }

    public List<Vector2> FindPath(Vector2 start, Vector2 end)
    {
        var pathList = new List<Vector2>();
        if(start == end)
        {
            return pathList;
        }
        var openList = new List<Vector2>();
        var closeList = new List<Vector2>();
        openList.Add(start);
        while(openList.Count > 0)
        {
            var f = getMinF(openList);
        }

        return pathList;
    }
}