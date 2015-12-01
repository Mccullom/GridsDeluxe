using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GridsDeluxe;

namespace GridsDeluxe.HexGrid
{


  public class GridHighlighter<Coords>
    where Coords : Coords2DAbstract, new()
  {

    private int _nextHighlightId = 0;
    int NextHighlightId
    {
      get
      {
        return ++_nextHighlightId;
      }
    }

    GridBaseAbstract<Coords> mGrid;
    public GridBaseAbstract<Coords> Grid
    {
      get { return mGrid; }
    }

    Dictionary<int, GameObject> mHighlights;

    public GridHighlighter(GridBaseAbstract<Coords> grid)
    {
      mGrid = grid;
      mHighlights = new Dictionary<int, GameObject>();
    }

    public int Highlight(List<uint> nodes, GameObject prefab)
    {
      if (nodes.Count > 0)
      {
        int id = NextHighlightId;

        GameObject nodesRoot = new GameObject();
        mHighlights[id] = nodesRoot;
        float diameter = mGrid.CellRadius * 2.0f;
        Vector3 scale = new Vector3(diameter, diameter, diameter);

        foreach (uint index in nodes)
        {
          Coords coord = new Coords();
          coord.Set(index);
          if (coord.isWithinRadius(mGrid.GridRadius))
          {
            GameObject marker = GameObject.Instantiate(prefab);

            marker.transform.localScale = scale;
            marker.transform.SetParent(nodesRoot.transform);
            marker.transform.localPosition = Grid.GetCellPos(coord);
          }
        }
        return id;
      }
      return 0;
    }

    public void RemoveHighlight(int id)
    {
      if (mHighlights.ContainsKey(id))
      {
        GameObject nodesRoot = mHighlights[id];
        mHighlights.Remove(id);
        GameObject.Destroy(nodesRoot);
      }
    }

  }
}