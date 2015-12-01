using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GridsDeluxe
{

  public abstract class GridBaseAbstract<Coords> : MonoBehaviour
    where Coords : Coords2DAbstract, new()
  {
    #region constants

    public const int sMaxGridRadius = short.MaxValue;

    #endregion

    #region Editor Vars

    public float _initialCellRadius;
    public int _initialGridRadius;

    public MeshCollider _groundPlane;

    #endregion

    #region Grid Control Interfaces

    GridNodeFactory mNodeFactory;
    public GridNodeFactory NodeFactory
    {
      get { return mNodeFactory; }
    }
    GridSizeProvider mSizeProvider;
    public GridSizeProvider SizeProvider
    {
      get { return mSizeProvider; }
    }

    public abstract float CellRadius
    {
      get;
    }
    public abstract int GridRadius
    {
      get;
    }

    public abstract int SetGridRadius(int rad);

    #endregion

    #region Cell Helper Funcs

    public abstract Vector3 GetOffset(int dir);

    public abstract bool CellIsInsideGrid(Coords cell);

    public abstract Vector3 GetCellPos(Coords cell);

    public abstract void CreateNodeAt(Coords coords);

    #endregion

    #region Search funcs

    public HashSet<uint> GetNodesInAbsoluteRadius(Coords start, int radius)
    {
      HashSet<uint> nodes = new HashSet<uint>();



      return nodes;
    }

    public Dictionary<uint, int> GetNodesInReachableRange(Coords start, GridAgentAbstract<Coords> avatar, int radius)
    {
      Dictionary<uint, int> nodes = new Dictionary<uint, int>();

      DepthSearch<Coords> search = new DepthSearch<Coords>(this, start, radius);
      search.RunToEnd();
      List<uint> results;
      search.GetResults(out results);

      return nodes;
    }

    // Finds a path, but ignores blocked cells
    public List<uint> FindPathAbsolute(Coords start, Coords end)
    {
      List<uint> path = new List<uint>();

      GetLineTo<Coords> search = new GetLineTo<Coords>(this, start, end);
      search.RunToEnd();
      search.GetResults(out path);

      return path;
    }

    // finds a path that is traversible by the given avatar
    public List<uint> FindPathTo(Coords start, Coords end, GridAgentAbstract<Coords> avatar)
    {
      List<uint> path = new List<uint>();

      FindPath<Coords> search = new FindPath<Coords>(this, start, end, avatar);
      search.RunToEnd();
      search.GetResults(out path);

      return path;
    }

    #endregion

    #region Grid Sampling Funcs

    public abstract Coords GetCell(Vector3 pos);

    public bool GetRayIntersectIndex(Ray ray, out uint index)
    {
      RaycastHit hit;
      if (_groundPlane.Raycast(ray, out hit, 10000.0f))
      {
        Vector3 point = ray.GetPoint(hit.distance);
        Coords cell = GetCell(point / CellRadius);
        index = cell.index;
        return true;
      }
      index = 0;
      return false;
    }
    
    #endregion

    #region Grid Resize

    public abstract Coords StepToInnerRing(Coords pos);
    public abstract Coords StepToOuterRing(Coords pos);

    void GrowGrid(int delta)
    {
      if (delta + GridRadius > short.MaxValue)
      {
        delta = sMaxGridRadius - GridRadius;
      }

      // find current wl corner
      Coords pos = new Coords();
      pos.Set((short)(-GridRadius), 0);

      float radius = CellRadius; // this triggers calculating other values
      Vector3 scale = 2.0f * new Vector3(CellRadius, CellRadius, CellRadius);

      for (int k = 0; k < delta; ++k)
      {
        for (int i = 0; i < 6; i++)
        {
          for (int c = 0; c < GridRadius; c++)
          {
            CreateNodeAt(pos);
            pos = pos.GetNeighbor(i) as Coords;
          }
        }
        SetGridRadius(GridRadius + 1);
        pos = StepToOuterRing(pos);
      }
    }

    void ShrinkGrid(int delta)
    {
      if (GridRadius - delta < 1)
      {
        delta = sMaxGridRadius - GridRadius;
      }

      // find current wl corner
      Coords pos = new Coords();
      pos.Set((short)(1 - GridRadius), 0);

      float radius = CellRadius; // this triggers calculating other values
      Vector3 scale = 2.0f * new Vector3(CellRadius, CellRadius, CellRadius);

      // for each ring to remove
      for (int k = 0; k < delta; ++k)
      {
        // for each hex direction
        for (int i = 0; i < 6; i++)
        {

          for (int c = 0; c < GridRadius; c++)
          {
            if (mNodes.ContainsKey(pos.index))
            {
              GridNode node = GetNode(pos.index) as GridNode;
              node.Destroy();
              mNodes.Remove(pos.index);
            }
            pos = pos.GetNeighbor(i) as Coords;
          }
        }
        SetGridRadius(GridRadius - 1);
        pos = StepToInnerRing(pos); 
      }
    }

    void BuildGridRadius(int radius)
    {
      if (radius < 1)
      {
        radius = 1;
      }
      else if (radius > sMaxGridRadius)
      {
        radius = sMaxGridRadius;
      }

      if (radius > GridRadius)
      {
        GrowGrid(radius - GridRadius);
      }
      else if (radius < GridRadius)
      {
        ShrinkGrid(GridRadius - radius);
      }
      _initialGridRadius = GridRadius;

      if (_groundPlane)
      {
        float scale = 2.0f * CellRadius * (2 * GridRadius - 1);
        _groundPlane.transform.localScale = new Vector3(scale, scale, 1.0f);
      }
    }

    protected void FixupGridRadius()
    {
      if (GridRadius != _initialGridRadius)
      {
        BuildGridRadius(_initialGridRadius);
        _initialGridRadius = GridRadius;
      }
    }

    #endregion

    #region Node Helper Funcs

    protected Dictionary<uint, GridNode> mNodes;

    public GridNode GetNode(uint index)
    {
      if (mNodes.ContainsKey(index))
      {
        return mNodes[index];
      }
      return null;
    }

    #endregion

    #region Agent Helper Funcs

    private uint _nextAgentId = 0;
    public uint NextAgentId
    {
      get
      {
        return ++_nextAgentId;
      }
    }

    protected Dictionary<uint, GridAgentAbstract<Coords>> mAgents;
    public GridAgentAbstract<Coords> GetAgent(uint index)
    {
      if (mNodes.ContainsKey(index))
      {
        return mAgents[index];
      }
      return null;
    }

    public abstract bool PlaceAgentInCell(GridAgentBase agent, uint cellId);

    public abstract uint CreateAgent(Coords pos, GridAgentBase.GridAgentBrain brain);

    //public abstract GridAgentAbstract<Coords> GetAgent(uint id);

    #endregion


    
    protected void InitBase()
    {
      mNodes = new Dictionary<uint, GridNode>();
      mAgents = new Dictionary<uint, GridAgentAbstract<Coords>>();
    }

  }

}