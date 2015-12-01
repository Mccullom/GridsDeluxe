using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GridsDeluxe;

namespace GridsDeluxe
{
  public abstract class GridAgentBase : MonoBehaviour
  {
    #region Brain

    #region Agent Brain interface

    public interface GridAgentBrain
    {
      void SetAgent(GridAgentBase agent);

      bool CanPassThrough(GridAgentBrain other);
    }

    #endregion

    GridAgentBrain mBrain;
    public GridAgentBrain Brain
    {
      get { return mBrain; }
    }

    public void SetBrain(GridAgentBrain brain)
    {
      if (mBrain != null)
      {
        mBrain.SetAgent(null);
      }
      mBrain = brain;
      if (mBrain != null)
      {
        mBrain.SetAgent(this);
      }
    }

    #endregion

    #region Cell

    public abstract uint cell
    {
      get;
      set;
    }

    #endregion
  }

  public class GridAgentAbstract<Coords> : GridAgentBase
    where Coords : Coords2DAbstract, new()
  {

    #region Public Vars

    uint mId;
    public uint ID
    {
      get { return mId; }
    }

    GridBaseAbstract<Coords> mGrid;
    public GridBaseAbstract<Coords> Grid
    {
      get { return mGrid; }
    }


    public int movementRange;
    public float speed;

    Coords mCell;
    public override uint cell
    {
      get { return mCell.index; }
      set
      {
        if (mCell != null)
        {
          GridNode node = Grid.GetNode(mCell.index);
          if (node != null)
          {
            node.SetAgent(null);
          }
        }
        mCell.Set(value);
        if (mCell != null)
        {
          GridNode node = Grid.GetNode(mCell.index);
          if (node != null)
          {
            node.SetAgent(this);
          }
        }
      }
    }

    #endregion

    #region State vars

    bool mWasMoving;
    bool mIsMoving;
    public bool IsMoving
    {
      get { return mIsMoving; }
    }

    LinkedList<uint> mPath;

    #endregion

    #region Movement

    public void Move(List<uint> path)
    {
      mPath = new LinkedList<uint>(path.ToArray());
    }

    #endregion

    #region Initialization

    public bool Init(GridBaseAbstract<Coords> grid, Coords pos)
    {
      mGrid = grid;
      cell = pos.index;
      if (Grid != null && cell != uint.MaxValue)
      {
        mId = Grid.NextAgentId;
        transform.SetParent(Grid.transform);
        transform.position = Grid.GetCellPos(pos);
        return true;
      }
      return false;
    }

    public void Init(GridBaseAbstract<Coords> grid, GridAgentBrain brain, Coords pos)
    {
      Init(grid, pos);
      SetBrain(brain);
    }

    // Use this for initialization
    void Start()
    {
      mWasMoving = false;
      mIsMoving = false;
      //if(cell == 0)
      //cell = 0;
    }

    public void SetMaterial(Material material)
    {
      Transform capsule = transform.FindChild("character");
      if (capsule != null)
      {
        capsule.gameObject.GetComponentInChildren<MeshRenderer>().material = material;
      }
    }

    #endregion

    #region Helper Funcs

    public void TurnToFace(Coords tgt)
    {
      tgt = new Coords();
      tgt.Set(mPath.First.Value);
      Vector3 facing = Grid.GetCellPos(tgt) - transform.position;
      facing.Normalize();
      transform.rotation = Quaternion.LookRotation(facing);
    }

    #endregion

    #region Update

    void UpdateMovement()
    {
      mIsMoving = mPath != null && mPath.Count > 0;
      if (IsMoving)
      {
        Coords tgt = new Coords();
        tgt.Set(mPath.First.Value);
        if (!mWasMoving)
        {
          // turn to face next hex when we start moving
          TurnToFace(tgt);
        }
        Vector3 delta = Vector3.MoveTowards(transform.position, Grid.GetCellPos(tgt), speed * Time.deltaTime);
        if ((transform.position - delta).sqrMagnitude <= Mathf.Epsilon * Mathf.Epsilon)
        {
          // we are here, pop from the list
          mPath.RemoveFirst();
          cell = tgt.index; // set this as our current location
          if (mPath.Count != 0)
          {
            TurnToFace(tgt);
          }
          else
          {
            Grid.PlaceAgentInCell(this, tgt.index);
          }
        }
        transform.position = delta;
      }
      mWasMoving = mIsMoving;
    }

    // Update is called once per frame
    void Update()
    {
      UpdateMovement();
    }

    #endregion

  }
}