using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GridsDeluxe;

namespace GridsDeluxe
{

  public abstract class Search<Coords>
    where Coords : Coords2DAbstract, new()
  {
    bool mFinished;
    public bool Finished
    {
      get { return mFinished; }
      set { mFinished = value; }
    }

    GridBaseAbstract<Coords> mGrid;
    public GridBaseAbstract<Coords> Grid
    {
      get { return mGrid; }
    }

    protected List<uint> mResults;

    public Search(GridBaseAbstract<Coords> grid)
    {
      mFinished = false;
      mGrid = grid;
      mResults = new List<uint>();
    }

    // return false if init fails
    public abstract bool Init();

    // return true if we can step again
    public abstract bool Step();

    public virtual void RunToEnd()
    {
      while (Step()) { }
      Finished = true;
    }

    public virtual bool GetResults(out List<uint> results)
    {
      results = mResults;
      return Finished;
    }

    // weight returned is additional cost to enter this node
    public virtual bool IsTransitionValid(Coords from, Coords to, out float weight)
    {
      weight = 0.0f;
      return true;
    }

    public virtual bool IsNodeValidEndpoint(Coords end)
    {
      GridNode node = Grid.GetNode(end.index);

      if (node == null)
        return false;
      if (node.State == NodeState.closed)
        return false;
      if (node.GetAgent() != null)
      {
        return false;
      }

      return true;
    }
  }
  #region Search Classes


  public class DepthSearch<Coords> : Search<Coords>
    where Coords : Coords2DAbstract, new()
  {
    Queue<uint> mOpenList;
    Coords mStart;
    int mRadius;
    public int Radius
    {
      get { return mRadius; }
    }

    public DepthSearch(GridBaseAbstract<Coords> grid, Coords start, int radius)
      : base(grid)
    {
      mStart = start;
      mRadius = radius;
      mOpenList = new Queue<uint>();
      mOpenList.Enqueue(start.index);
    }

    public override bool Init()
    {
      return true;
    }

    public override bool Step()
    {
      if (!Finished)
      {
        uint id = mOpenList.Dequeue();
        Coords node = new Coords();
        node.Set(id);
        Coords delta = node - mStart as Coords;
        if (delta.isWithinRadius(mRadius))
        {
          // this one is within the radius
          mResults.Add(node.index);
          // add neighbors if not in open list or results
          //for (HexDir n = HexDir.N; n < HexDir.Count; ++n)
          foreach(int d in node.NeighborDirs)
          {
            Coords neighbor = node.GetNeighbor(d) as Coords;
            if (!mOpenList.Contains(neighbor.index) && !mResults.Contains(neighbor.index))
            {
              mOpenList.Enqueue(neighbor.index);
            }
          }
        }
        Finished = mOpenList.Count <= 0;
      }
      return !Finished;
    }
  }

  public class GetLineTo<Coords> : Search<Coords>
    where Coords : Coords2DAbstract, new()
  {
    protected struct openNode
    {
      public float f
      {
        get { return g + h; }
      }
      public float g; // distance from start
      public float h; // est dist to end
      public uint index;
      public uint fromIndex;

      public openNode(uint i)
      {
        g = 0;
        h = 0;
        fromIndex = i;
        index = i;
      }

      public static bool operator <(openNode n1, openNode n2)
      {
        return n1.f < n2.f;
      }
      public static bool operator >(openNode n1, openNode n2)
      {
        return n1.f > n2.f;
      }
      public static bool operator ==(openNode n1, openNode n2)
      {
        return n1.index == n2.index;
      }
      public static bool operator !=(openNode n1, openNode n2)
      {
        return n1.index != n2.index;
      }

    }

    protected LinkedList<openNode> mOpenList;
    protected Dictionary<uint, openNode> mClosedList;

    protected Coords mStart;
    protected Coords mEnd;

    public GetLineTo(GridBaseAbstract<Coords> grid, Coords start, Coords end)
      : base(grid)
    {
      mStart = start;
      mEnd = end;
      mOpenList = new LinkedList<openNode>();
      mClosedList = new Dictionary<uint, openNode>();
      Init();
    }

    public override bool Init()
    {
      mResults.Clear();
      mOpenList.Clear();
      mClosedList.Clear();

      openNode node = new openNode(mStart.index);

      node.fromIndex = mStart.index;
      node.g = 0;
      node.h = (mEnd.GetPosNormalized() - mStart.GetPosNormalized()).magnitude;
      mOpenList.AddFirst(node);

      return true;
    }

    bool BuildPath()
    {
      uint index = mEnd.index;
      while (mClosedList.ContainsKey(index))
      {
        openNode node = mClosedList[index];
        if (index == mStart.index)
        {
          mResults.Reverse();
          return true;
        }
        mResults.Add(index);
        index = node.fromIndex;
      }
      return false;
    }

    void PushOpenList(openNode node)
    {
      if (mClosedList.ContainsKey(node.index))
      {
        if (node.f < mClosedList[node.index].f)
        {
          mClosedList.Remove(node.index);
          mOpenList.AddFirst(node);
        }
      }
      else if (mOpenList.Contains(node))
      {
        LinkedListNode<openNode> listNode = mOpenList.Find(node);
        if (node.f < listNode.Value.f)
        {
          listNode.Value = node;
        }
      }
      else
      {
        mOpenList.AddFirst(node);
      }
    }

    void PushClosedList(openNode node)
    {
      while (mOpenList.Contains(node))
      {
        mOpenList.Remove(node);
      }

      if (!mClosedList.ContainsKey(node.index) || node.f < mClosedList[node.index].f)
      {
        mClosedList[node.index] = node;
      }
    }

    public override bool Step()
    {
      if (!Finished)
      {
        // sort nodes by f value
        openNode nodeData = mOpenList.OrderBy(x => x.f).First<openNode>();
        mOpenList.Remove(nodeData);

        Coords node = new Coords();
        node.Set(nodeData.index);

        PushClosedList(nodeData);

        if (node == mEnd)
        {
          Finished = true;
          BuildPath();
          mOpenList.Clear();
        }
        else
        {
          // add neighbors if not in open list or results
          foreach (int d in node.NeighborDirs)
          {
            Coords neighbor = node.GetNeighbor(d) as Coords;

            float weight;
            if (IsTransitionValid(node, neighbor, out weight))
            {
              //bool add = true;
              openNode neighborData = new openNode(neighbor.index);

              neighborData.fromIndex = nodeData.index;
              neighborData.g = nodeData.g + (2 * 2) + weight;
              neighborData.h = (mEnd.GetPosNormalized() - neighbor.GetPosNormalized()).magnitude;

              PushOpenList(neighborData);
            }
          }
          Finished = mOpenList.Count <= 0;
        }
      }
      return !Finished;
    }
  }

  #endregion

  #region AgentSearch classes

  public class FindPath<Coords> : GetLineTo<Coords>
    where Coords : Coords2DAbstract, new()
  {

    #region private vars

    GridAgentAbstract<Coords> mAgent;

    #endregion

    public FindPath(GridBaseAbstract<Coords> grid, Coords start, Coords end, GridAgentAbstract<Coords> avatar)
      : base(grid, start, end)
    {
      mAgent = avatar;
      if (!IsNodeValidEndpoint(end))
      {
        Finished = true;
      }
    }

    public override bool IsTransitionValid(Coords from, Coords to, out float weight)
    {
      weight = 0.0f;
      GridNode node = Grid.GetNode(to.index);

      if (node == null)
        return false;
      if (node.State == NodeState.closed)
        return false;
      if (node.GetAgent() != null)
      {
        if (node.GetAgent().Brain != null && !node.GetAgent().Brain.CanPassThrough(mAgent.Brain))
        {
          return false;
        }
      }

      return true;
    }
    //public override bool IsNodeValidEndpoint(HexCoords2 end)
    //{
    //  HexNode node = Grid.GetNode(end.index);

    //  if (node == null)
    //    return false;
    //  if (node.State == HexNodeState.closed)
    //    return false;
    //  if (node.Agent != null)
    //  {
    //    return false;
    //  }

    //  return true;
    //}

  }

  public class FindReachableNodes<Coords> : DepthSearch<Coords>
    where Coords : Coords2DAbstract, new()
  {
    GridAgentAbstract<Coords> mAgent;

    // Node id to shortest distance 
    Dictionary<uint, int> mOpenNodes;
    Dictionary<uint, int> mResultNodes;

    public FindReachableNodes(GridBaseAbstract<Coords> grid, Coords start, int radius, GridAgentAbstract<Coords> avatar)
      : base(grid, start, radius)
    {
      mAgent = avatar;
      mOpenNodes = new Dictionary<uint, int>();
      mResultNodes = new Dictionary<uint, int>();
      mOpenNodes[start.index] = 0;
    }

    void PushOpen(uint id, int range)
    {

      if (mResultNodes.ContainsKey(id))
      {
        if (mResultNodes[id] > range)
        {
          mResultNodes.Remove(id);
        }
      }

      if (mOpenNodes.ContainsKey(id))
      {
        if (mOpenNodes[id] > range)
        {
          mOpenNodes[id] = range;
        }
      }
      else
      {
        mOpenNodes[id] = range;
      }
    }

    void PushClosed(uint id, int range)
    {
      if (mOpenNodes.ContainsKey(id))
      {
        if (mOpenNodes[id] >= range)
        {
          mOpenNodes.Remove(id);
        }
        else
        {
          // on the open list with a lower valueF:\Code\Unity\HexHeros\Assets\_scripts\States.cs
          // don't add to the closed list
          return;
        }
      }

      if (mResultNodes.ContainsKey(id))
      {
        if (mResultNodes[id] > range)
        {
          mResultNodes[id] = range;
        }
      }
      else
      {
        mResultNodes[id] = range;
      }

    }

    void BuildResults()
    {
      mResults.AddRange(mResultNodes.Keys);
    }

    Dictionary<uint, int> GetResultsWithDepth()
    {
      return mResultNodes;
    }

    public override bool IsTransitionValid(Coords from, Coords to, out float weight)
    {
      weight = 0.0f;
      GridNode node = Grid.GetNode(to.index);

      if (node == null)
        return false;
      if (node.State == NodeState.closed)
        return false;
      if (node.GetAgent() != null)
      {
        if (node.GetAgent().Brain != null && !node.GetAgent().Brain.CanPassThrough(mAgent.Brain))
        {
          return false;
        }
      }
      return true;
    }

    public override bool IsNodeValidEndpoint(Coords end)
    {
      GridNode node = Grid.GetNode(end.index);

      if (node == null)
        return false;
      if (node.State == NodeState.closed)
        return false;
      if (node.GetAgent() != null)
      {
        return false;
      }

      return true;
    }

    public override bool Step()
    {
      if (!Finished)
      {
        uint id = mOpenNodes.Keys.First();
        int range = mOpenNodes[id];
        mOpenNodes.Remove(id);

        PushClosed(id, range);

        Coords node = new Coords();
        node.Set(id);

        ++range;
        if (range <= Radius)
        {
          // add neighbors if not in open list or results
          foreach (int d in node.NeighborDirs)
          {
            Coords neighbor = node.GetNeighbor(d) as Coords;

            float weight;
            if (IsTransitionValid(node, neighbor, out weight))
            {
              if (range != Radius || IsNodeValidEndpoint(neighbor))
              {
                PushOpen(neighbor.index, range);
              }
            }
          }
        }
        Finished = mOpenNodes.Count <= 0;
        if (Finished)
        {
          BuildResults();
        }
      }
      return !Finished;
    }
  }


  #endregion

}
