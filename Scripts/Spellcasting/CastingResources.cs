using System;
using System.Collections;
using System.Collections.Generic;

public class CastParam {
  public enum ECastParamType : int { BOOL, INT, FLOAT, VECTOR2, VECTOR3, COLOR, STRING, NODE2D, BEHAVIOR, TARGET, SPELL };
  public string name;
  public ECastParamType paramType;
  public CastParam(string n, ECastParamType p) { this.name = n; this.paramType = p; }
  public CastParam Refactor(string n, ECastParamType p) { this.name = n; this.paramType = p; return this; }
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

  private static unsafe IntPtr ConvertParam<T>(ref T param)
  {
    TypedReference refer = __makeref(param);
    return *(IntPtr*)&refer;
  }

  public void Add<T>(String paramName, CastParam.ECastParamType type, ref T value)
  {
    this.Add(new CastParam("CASTING_POSITION", CastParam.ECastParamType.VECTOR2), ConvertParam<T>(ref value));
  }
  public void Add(String paramName, CastParam.ECastParamType type)
  {
    this.Add(new CastParam("CASTING_POSITION", CastParam.ECastParamType.VECTOR2), IntPtr.Zero);
  }

  public bool ContainsKey(string n, CastParam.ECastParamType p)
  {
    return ContainsKey(new CastParam(n, p));
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