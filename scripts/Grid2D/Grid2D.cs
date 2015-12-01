using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GridsDeluxe;

namespace GridsDeluxe.Grid2D
{

  public abstract class Grid2D : GridBaseAbstract<GridCoords>
  {
    #region Public Vars

    public GameObject selectionObject;
    public GameObject openMarker;
    public GameObject closedMarker;
    public GameObject walkableMarker;
    public GameObject pathMarker;

    public GridAgent AgentPrefab;

    #endregion

    #region Current Grid State

    uint mHoverIndex;

    float mNodeRadius;
    public override float CellRadius
    {
      get { return mNodeRadius; }
    }

    int mGridRadius;
    public override int GridRadius
    {
      get { return mGridRadius; }
    }

    #endregion

    #region Child Classes

    public class SquareNode : GridNode
    {
      public uint mId;
      public Vector3 mOffset;
      public GameObject mOpenMarker;
      public GameObject mClosedMarker;

      GridAgentAbstract<GridCoords> mAgent;
      public GridAgentAbstract<GridCoords> Agent
      {
        get { return mAgent; }
        set
        {
          mAgent = value;
        }
      }

      NodeState mState;
      public NodeState State
      {
        get { return mState; }
        set
        {
          mState = value;
          switch (mState)
          {
            case NodeState.open:
              if (mOpenMarker != null) mOpenMarker.SetActive(true);
              if (mClosedMarker != null) mClosedMarker.SetActive(false);
              break;
            case NodeState.closed:
              if (mOpenMarker != null) mOpenMarker.SetActive(false);
              if (mClosedMarker != null) mClosedMarker.SetActive(true);
              break;
          }
        }
      }

      public SquareNode()
      {
        mState = NodeState.open;
      }

      public int RoomId()
      {
        return 0;
      }

      public GridAgentBase GetAgent()
      {
        return Agent;
      }

      public bool SetAgent(GridAgentBase agent)
      {
        Agent = agent as GridAgentAbstract<GridCoords>;
        return Agent != null;
      }

      public void Destroy()
      {
        GameObject.DestroyImmediate(mClosedMarker);
        GameObject.DestroyImmediate(mOpenMarker);
        mClosedMarker = null;
        mOpenMarker = null;
      }
    }

    #endregion

    #region Grid Resize

    public override GridCoords StepToInnerRing(GridCoords pos)
    {
      pos.Set((short)(pos.x + 1), (short)(pos.y + 1));
      return pos;
    }
    public override GridCoords StepToOuterRing(GridCoords pos)
    {
      pos.Set((short)(pos.x - 1), (short)(pos.y - 1));
      return pos;
    }

    public override int SetGridRadius(int rad)
    {
      mGridRadius = rad;
      return mGridRadius;
    }
    #endregion

    #region Cell Helper funcs

    override public GridCoords GetCell(Vector3 posNormalized)
    {
      short x = GridCoords.SnapToInt(posNormalized.x);
      short y = GridCoords.SnapToInt(posNormalized.y);

      return new GridCoords(x, y);
    }

    public override Vector3 GetOffset(int dir)
    {
      switch ((Dir)dir)
      {
        case Dir.N:
          return new Vector3(0.0f, 0.0f, CellRadius * 2.0f);
        case Dir.E:
          return new Vector3(CellRadius * 2.0f, 0.0f, 0);
        case Dir.S:
          return new Vector3(0.0f, 0.0f, CellRadius * -2.0f);
        case Dir.W:
          return new Vector3(CellRadius * -2.0f, 0.0f, 0);
      }

      return Vector3.up;
    }

    public override bool CellIsInsideGrid(GridCoords cell)
    {
      return cell.isWithinRadius(GridRadius);
    }

    public override Vector3 GetCellPos(GridCoords cell)
    {
      return cell.GetPosNormalized() * CellRadius;
    }

    #endregion

    #region Node Funcs

    public override void CreateNodeAt(GridCoords coords)
    {
      Vector3 scale = 2.0f * new Vector3(CellRadius, CellRadius, CellRadius);

      SquareNode node = new SquareNode();

      node.mOffset = coords.GetPosNormalized() * CellRadius;
      node.mId = coords.index;

      node.mOpenMarker = GameObject.Instantiate(openMarker);
      node.mOpenMarker.transform.SetParent(gameObject.transform);
      node.mOpenMarker.transform.localScale = scale;
      node.mOpenMarker.transform.localPosition = node.mOffset;

      node.mClosedMarker = GameObject.Instantiate(closedMarker);
      node.mClosedMarker.transform.SetParent(gameObject.transform);
      node.mClosedMarker.transform.localScale = scale;
      node.mClosedMarker.transform.localPosition = node.mOffset;

      node.State = NodeState.open;

      mNodes[coords.index] = node;
    }

    #endregion

    #region Init

    public abstract void Init_preStart();

    public abstract void Init_postStart();

    // Use this for initialization
    void Start()
    {
      InitBase();

      Init_preStart();

      //mWalkableRoot = new GameObject();
      //mPathRoot = new GameObject();
      //mWalkableRoot.transform.SetParent(gameObject.transform);
      //mPathRoot.transform.SetParent(gameObject.transform);

      mNodeRadius = _initialCellRadius;
      mGridRadius = 0;

      GridCoords rootPos = new GridCoords(0);
      float radius = mNodeRadius; // this triggers calculating other values
      Vector3 scale = 2.0f * new Vector3(CellRadius, CellRadius, CellRadius);

      CreateNodeAt(rootPos);

      mGridRadius = 1;

      FixupGridRadius();

      if (selectionObject != null)
      {
        //float expRadius = radius * 1.2f;
        selectionObject.transform.localScale = scale * 1.2f;
      }

      mHoverIndex = GridHelper.sInvalidCell;

      Init_postStart();
    }

    #endregion

    #region Update

    public abstract void Update_pre();

    public abstract void Update_post();

    // Update is called once per frame
    void Update()
    {
      Update_pre();

      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      RaycastHit hit;
      if (_groundPlane.Raycast(ray, out hit, 10000.0f))
      {
        Vector3 point = ray.GetPoint(hit.distance);
        GridCoords cell = GetCell(point);// * (1.0f / CellRadius));
        if (cell.index != mHoverIndex)
        {
          mHoverIndex = cell.index;
        }
      }
      else
      {
        mHoverIndex = GridHelper.sInvalidCell;
      }

      Update_post();
    }

    #endregion

    #region Node Helper Funcs


    #endregion


    #region Agent Helper Funcs

    public override bool PlaceAgentInCell(GridAgentBase agent, uint cellId)
    {
      GridNode cell = GetNode(cellId) as GridNode;
      if (cell != null && cell.GetAgent() == null)
      {
        agent.cell = cellId;
        agent.transform.localPosition = GetCellPos(new GridCoords(agent.cell));
        cell.SetAgent(agent);
        return true;
      }
      return false;
    }

    public override uint CreateAgent(GridCoords pos, GridAgentBase.GridAgentBrain brain)
    {
      GridAgent agent = GameObject.Instantiate(AgentPrefab) as GridAgent;

      agent.transform.SetParent(transform);
      agent.Init(this, brain, pos);
      mAgents[agent.ID] = agent;
      PlaceAgentInCell(agent, pos.index);

      return agent.ID;
    }

    #endregion


  }

}