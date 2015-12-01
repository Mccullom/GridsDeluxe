using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GridsDeluxe;

namespace GridsDeluxe.HexGrid
{

  public abstract class HexGrid : GridBaseAbstract<HexCoords>
  {

    #region Child Classes

    public class HexNode : GridNode
    {
      public uint mId;
      public Vector3 mOffset;
      public GameObject mOpenMarker;
      public GameObject mClosedMarker;

      GridAgentAbstract<HexCoords> mAgent;
      public GridAgentAbstract<HexCoords> Agent
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

      public HexNode()
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
        Agent = agent as GridAgentAbstract<HexCoords>;
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

    #region Public Vars

    public GameObject selectionObject;
    public GameObject openMarker;
    public GameObject closedMarker;
    public GameObject walkableMarker;
    public GameObject pathMarker;

    public HexAgent AgentPrefab;

    #endregion

    #region Current Grid State


    float _mNodeRadius;
    public override float CellRadius
    {
      get { return _initialCellRadius; }
    }

    int mGridRadius;
    public override int GridRadius
    {
      get { return mGridRadius; }
    }

    GameObject mWalkableRoot;
    GameObject mPathRoot;

    float mZStepDist;
    //float m2NodeRadius;
    //float mInvNodeRadius;

    //float mNodeRadius
    //{
    //  get
    //  {
    //    //if (_mNodeRadius != _initialCellRadius)
    //    //{
    //    //  _mNodeRadius = _initialCellRadius;
    //    //  mInvNodeRadius = 1.0f / _mNodeRadius;
    //    //  m2NodeRadius = _mNodeRadius * 2.0f;
    //    //  mZStepDist = Mathf.Sqrt((m2NodeRadius * m2NodeRadius) - (_mNodeRadius * _mNodeRadius));
    //    //}
    //    return _mNodeRadius;
    //  }
    //}

    uint mHoverIndex;

    #endregion

    #region Nodes

    #endregion

    #region Agent Control

    public override uint CreateAgent(HexCoords pos, GridAgentBase.GridAgentBrain brain)
    {
      HexAgent agent = GameObject.Instantiate(AgentPrefab) as HexAgent;

      agent.transform.SetParent(transform);
      agent.Init(this, brain, pos);
      mAgents[agent.ID] = agent;
      PlaceAgentInCell(agent, pos.index);

      return agent.ID;
    }

    public override bool PlaceAgentInCell(GridAgentBase agent, uint cellId)
    {
      HexNode cell = GetNode(cellId) as HexNode;
      if (cell != null && cell.GetAgent() == null)
      {
        agent.cell = cellId;
        agent.transform.localPosition = GetCellPos(new HexCoords(agent.cell));
        cell.SetAgent(agent);
        return true;
      }
      return false;
    }

    #endregion

    #region Cell Helper Funcs

    public override Vector3 GetOffset(int dir)
    {
      switch ((HexDir)dir)
      {
        case HexDir.N:
          return new Vector3(0.0f, 0.0f, 2.0f * CellRadius);
        case HexDir.Eh:
          return new Vector3(CellRadius, 0.0f, mZStepDist);
        case HexDir.El:
          return new Vector3(CellRadius, 0.0f, -mZStepDist);
        case HexDir.S:
          return new Vector3(0.0f, 0.0f, -2.0f * CellRadius);
        case HexDir.Wl:
          return new Vector3(-CellRadius, 0.0f, mZStepDist);
        case HexDir.Wh:
          return new Vector3(-CellRadius, 0.0f, -mZStepDist);
      }

      return Vector3.up;
    }

    public override bool CellIsInsideGrid(HexCoords cell)
    {
      return cell.isWithinRadius(GridRadius);
    }

    public override Vector3 GetCellPos(HexCoords cell)
    {
      return cell.GetPosNormalized() * CellRadius;
    }

    public override HexCoords GetCell(Vector3 pos)
    {
      pos *= 1.0f / CellRadius;
      return HexCoords.GetCell(pos);
    }

    #endregion

    #region Node Funcs

    public override void CreateNodeAt(HexCoords coords)
    {
      Vector3 scale = 2.0f * new Vector3(CellRadius, CellRadius, CellRadius);

      HexNode node = new HexNode();

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

    #region Grid Resize

    public override HexCoords StepToInnerRing(HexCoords pos)
    {
      return pos.GetNeighbor((int)HexDir.Eh) as HexCoords;
    }
    public override HexCoords StepToOuterRing(HexCoords pos)
    {
      return pos.GetNeighbor((int)HexDir.Wl) as HexCoords;
    }

    public override int SetGridRadius(int rad)
    {
      mGridRadius = rad;
      return mGridRadius;
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

      mWalkableRoot = new GameObject();
      mPathRoot = new GameObject();
      mWalkableRoot.transform.SetParent(gameObject.transform);
      mPathRoot.transform.SetParent(gameObject.transform);

      _mNodeRadius = _initialCellRadius;
      mGridRadius = 0;
      float nodeRad2 = 2.0f * _mNodeRadius;
      mZStepDist = Mathf.Sqrt((nodeRad2 * nodeRad2) - (_mNodeRadius * _mNodeRadius));

      HexCoords rootPos = new HexCoords(0);
      float radius = CellRadius;
      Vector3 scale = 2.0f * new Vector3(CellRadius, CellRadius, CellRadius);

      CreateNodeAt(rootPos);

      mGridRadius = 1;

      FixupGridRadius();

      if (selectionObject != null)
      {
        //float expRadius = radius * 1.2f;
        selectionObject.transform.localScale = scale * 1.2f;
      }

      mHoverIndex = HexHelper.sInvalidCell;

      Init_postStart();
    }

    #endregion

    #region Hover index funcs

    //  public void HideHoverObject()
    //  {
    //    selectionObject.SetActive(false);
    //  }

    //  public void UpdateHoverObject()
    //  {
    //    if (selectionObject != null)
    //    {
    //      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    //      RaycastHit hit;
    //      if (groundPlane.Raycast(ray, out hit, 10000.0f))
    //      {
    //        Vector3 point = ray.GetPoint(hit.distance);
    //        HexCoords2 cell = HexCoords2.GetCell(point, mNodeRadius);
    //        if (cell.index != mHoverIndex)
    //        {
    //          mHoverIndex = cell.index;
    //          if (cell.isWithinRadius(_gridRadius))
    //          {
    //            selectionObject.SetActive(true);
    //            selectionObject.transform.position = cell.GetPos(mNodeRadius);
    //          }
    //          else
    //          {
    //            selectionObject.SetActive(false);
    //          }
    //        }
    //      }
    //      else
    //      {
    //        selectionObject.SetActive(false);
    //      }
    //    }
    //  }

    //  public int GetHoverIndex()
    //  {
    //    return mHoverIndex;
    //  }

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
        HexCoords cell = HexCoords.GetCell(point * (1.0f / CellRadius));
        if (cell.index != mHoverIndex)
        {
          mHoverIndex = cell.index;
        }
      }
      else
      {
        mHoverIndex = HexHelper.sInvalidCell;
      }

      Update_post();
    }

    #endregion

  }
}