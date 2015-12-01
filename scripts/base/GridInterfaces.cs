using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;


namespace GridsDeluxe
{
  public enum NodeState
  {
    open,
    closed,
  }

  public interface GridNode
  {
    int RoomId();
    GridAgentBase GetAgent();
    bool SetAgent(GridAgentBase agent);

    void Destroy();

    NodeState State
    {
      get;
      set;
    }
  }

  public interface GridNodeFactory
  {
    GridNode CreateNode(uint index);
  }

  public interface GridSizeProvider
  {
    float GetCellRadius();
    short GetGridRadius();
  }

}
