using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using GridsDeluxe;


namespace GridsDeluxe.Grid2D
{
  //[StructLayout(LayoutKind.Explicit)]
  public class GridCoords : Coords2DAbstract
  {

    #region statics

    static Vector3 _sXstep = new Vector3(2.0f, 0f, 0f);
    static Vector3 _sYstep = new Vector3(0f, 0f, 2f);

    public override int[] NeighborDirs
    {
      get { return new int[4] { 0, 1, 2, 3 }; }
    }

    #endregion

    #region Construction

    static GridCoords() { }

    public GridCoords()
      : base(0)
    {
    }

    public GridCoords(uint _index)
      : base(_index)
    {
    }

    public GridCoords(short X, short Y)
      : base(X, Y)
    {
    }

    #endregion

    #region Operators

    public static GridCoords operator +(GridCoords h1, GridCoords h2)
    {
      return new GridCoords((short)(h1.x + h2.x), (short)(h1.y + h2.y));
    }

    public static GridCoords operator -(GridCoords h1, GridCoords h2)
    {
      return new GridCoords((short)(h1.x - h2.x), (short)(h1.y - h2.y));
    }

    public static bool operator !=(GridCoords h1, GridCoords h2)
    {
      return h1.index != h2.index;
    }

    public static bool operator ==(GridCoords h1, GridCoords h2)
    {
      return h1.index == h2.index;
    }

    public override bool Equals(object obj)
    {
      return index == (obj as Coords2DAbstract).index;
    }

    #endregion

    #region Helpers

    public override bool isWithinRadius(int radius)
    {
      radius = Mathf.Abs(radius);
      if (Mathf.Abs(x) < radius &&
          Mathf.Abs(y) < radius)
        return true;
      return false;
    }

    public override Coords2DAbstract GetNeighbor(int dir)
    {
      switch ((Dir)dir)
      {
        case Dir.N:
          return new GridCoords(x, (short)(y + 1));
        case Dir.E:
          return new GridCoords((short)(x + 1), y);
        case Dir.S:
          return new GridCoords(x, (short)(y - 1));
        case Dir.W:
          return new GridCoords((short)(x - 1), y);
      }
      return new GridCoords(0);
    }

    public override Vector3 GetPosNormalized()
    {
      return (_sXstep * x + _sYstep * y);
    }


    #endregion


  }

}