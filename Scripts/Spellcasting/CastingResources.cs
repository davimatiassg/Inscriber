using System;
using System.Collections;
using System.Collections.Generic;
 
public class CastParam {
  public enum ECastParamType : int {
  BOOL,
  INT,
  FLOAT,
  GAMEOBJECT,
  VECTOR2,
  VECTOR3,
  COLOR
};
  public string name;
  public ECastParamType paramType;
}

public class CastParamComparer : Comparer<CastParam> {
  public override int Compare(CastParam? x, CastParam? y) { return string.Compare(x.name, y.name); }
}
public class CastingResources : SortedDictionary<CastParam, IntPtr>
{

  public CastingResources() : base(new CastParamComparer()) {}

  public CastingResources(IDictionary<CastParam,IntPtr> d) :  base(d, new CastParamComparer()) {}

  public void Merge(CastingResources r)
  {
    foreach(KeyValuePair<CastParam, IntPtr> c in r ) { this.Add(c.Key, c.Value); }
  }
  
  public static CastingResources Merge(CastingResources[] datas)
  {
    CastingResources r = new CastingResources();
    foreach(CastingResources d in datas ) { 
    foreach(KeyValuePair<CastParam, IntPtr> c in d ) { 
      r.Add(c.Key, c.Value); 
    }
    }
    return r;
  }

  public static CastingResources operator +(CastingResources r1, CastingResources r2)
  {
    CastingResources r = new CastingResources(r1);
    foreach(KeyValuePair<CastParam, IntPtr> c in r2 )
    {
      r.Add(c.Key, c.Value);
    }
    return r;
  }

  public static bool operator >=(CastingResources r1, CastingResources r2)
  {
    if(r1.Count < r2.Count) return false;
    foreach(KeyValuePair<CastParam, IntPtr> p in r2)
    {
      if(!r1.ContainsKey(p.Key)) return false; 
    }
    return true;
  }

  public static bool operator <=(CastingResources r1, CastingResources r2)
  {
    return r2 >= r1;
  }
}