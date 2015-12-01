using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

namespace GridsDeluxe
{

  [StructLayout(LayoutKind.Explicit)]
  struct Coords2DCore
  {
    [FieldOffset(0)]
    public short x;
    [FieldOffset(2)]
    public short y;

    [FieldOffset(0)]
    public uint index;
  }

  public abstract class Coords2DAbstract
  {

#region Data

  Coords2DCore mCoords;

  public short x
  {
    get { return mCoords.x; }
  }

  public short y
  {
    get { return mCoords.y; }
  }

  public uint index
  {
    get { return mCoords.index; }
  }

  public static short MaxRadius
  {
    get { return short.MaxValue; }
  }

#endregion

#region Neighbors

  public abstract int[] NeighborDirs
  {
    get;
  }

#endregion

#region Construction

  static Coords2DAbstract() { }

  public Coords2DAbstract(uint idx)
  {
    mCoords = new Coords2DCore();
    mCoords.index = idx;
  }

  public Coords2DAbstract(short X, short Y)
  {
    mCoords = new Coords2DCore();
    mCoords.x = X;
    mCoords.y = Y;
  }

  public void Set(uint idx)
  {
    mCoords.index = idx;
  }

  public void Set(short X, short Y)
  {
    mCoords.x = X;
    mCoords.y = Y;
  }

#endregion

#region Internal Helpers

  public static short SnapToInt(float val)
  {
    if (val < 0)
    {
      return (short)Mathf.FloorToInt(Mathf.Min(0, val + .5f));
    }
    else
    {
      return (short)Mathf.CeilToInt(Mathf.Max(0, val - .5f));
    }
  }

#endregion

    public abstract Coords2DAbstract GetNeighbor(int dir);

    public abstract Vector3 GetPosNormalized();

    //public abstract Coords2DAbstract GetCell(Vector3 posNormalized);

    public abstract bool isWithinRadius(int radius);

#region Operators
  // Operators for + and - should be overridden for derived types

    public static Coords2DAbstract operator +(Coords2DAbstract c1, Coords2DAbstract c2)
    {
      throw new System.NotImplementedException();
    }

    public static Coords2DAbstract operator -(Coords2DAbstract c1, Coords2DAbstract c2)
    {
      throw new System.NotImplementedException();
    }

    public static bool operator !=(Coords2DAbstract c1, Coords2DAbstract c2)
    {
      return c1.index != c2.index;
    }

    public static bool operator ==(Coords2DAbstract c1, Coords2DAbstract c2)
    {
      return c1.index == c2.index;
    }

    public override bool Equals(object obj)
    {
      return index == (obj as Coords2DAbstract).index;
    }

#endregion



  }

}