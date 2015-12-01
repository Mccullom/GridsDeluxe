using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using GridsDeluxe;

namespace GridsDeluxe.HexGrid
{

  public class HexCoords : Coords2DAbstract
  {

    #region statics

    static Vector3 _sXstep = new Vector3(Mathf.Sqrt(3), 0f, 1f);
    static Vector3 _sYstep = new Vector3(0f, 0f, 2f);

    public override int[] NeighborDirs
    {
      get { return new int[6] { 0, 1, 2, 3, 4, 5 }; }
    }

    #endregion

    #region Data

    public short z
    {
      get { return (short)-(x + y); }
    }

    #endregion

    #region Construction

    public HexCoords()
      : base(0)
    { }

    public HexCoords(uint _index)
      : base(_index)
    {

    }

    public HexCoords(short X, short Y)
      : base(X, Y)
    {

    }

    #endregion

    #region Operators

    public static HexCoords operator +(HexCoords h1, HexCoords h2)
    {
      return new HexCoords((short)(h1.x + h2.x), (short)(h1.y + h2.y));
    }
    public static HexCoords operator -(HexCoords h1, HexCoords h2)
    {
      return new HexCoords((short)(h1.x - h2.x), (short)(h1.y - h2.y));
    }
    public static bool operator !=(HexCoords h1, HexCoords h2)
    {
      return h1.index != h2.index;
    }
    public static bool operator ==(HexCoords h1, HexCoords h2)
    {
      return h1.index == h2.index;
    }

    #endregion

    #region Helpers

    public override bool isWithinRadius(int radius)
    {
      radius = Mathf.Abs(radius);
      if (Mathf.Abs(x) < radius &&
         Mathf.Abs(y) < radius &&
         Mathf.Abs(z) < radius)
        return true;
      return false;
    }

    public override Coords2DAbstract GetNeighbor(int dir)
    {
      switch ((HexDir)dir)
      {
        case HexDir.N:
          return new HexCoords(x, (short)(y + 1));
        case HexDir.Eh:
          return new HexCoords((short)(x + 1), y);
        case HexDir.El:
          return new HexCoords((short)(x + 1), (short)(y - 1)) as Coords2DAbstract;
        case HexDir.S:
          return new HexCoords(x, (short)(y - 1)) as Coords2DAbstract;
        case HexDir.Wl:
          return new HexCoords((short)(x - 1), y) as Coords2DAbstract;
        case HexDir.Wh:
          return new HexCoords((short)(x - 1), (short)(y + 1)) as Coords2DAbstract;
      }
      return new HexCoords(uint.MaxValue) as Coords2DAbstract;
    }

    public override Vector3 GetPosNormalized()
    {
      return (_sXstep * x + _sYstep * y);
    }

    static public HexCoords GetCell(Vector3 pos)
    {
      //pos /= radius;

      float hX = (pos.x) / _sXstep.x;
      float hY = ((pos.z) - (hX * _sXstep.z)) / (_sYstep.z);

      short x = 0;
      short y = 0;
      if (hX < 0)
      {
        x = (short)Mathf.CeilToInt(hX - .5f);
      }
      else
      {
        x = (short)Mathf.FloorToInt(hX + .5f);
      }

      if (hY < 0)
      {
        y = (short)Mathf.CeilToInt(hY - .5f);
      }
      else
      {
        y = (short)Mathf.FloorToInt(hY + .5f);
      }
      return new HexCoords(x, y);
    }

    #endregion

  }

}