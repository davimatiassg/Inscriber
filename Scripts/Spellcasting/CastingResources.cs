using System;
using System.Collections;
using System.Collections.Generic;

public class CastParam {
  public enum ECastParamType : int { BOOL, INT, FLOAT, VECTOR2, VECTOR3, COLOR, STRING, NODE2D, PHYSICAL_BEHAVIOR, TARGET, SPELL };
  public string name;
  public string description;
  public ECastParamType paramType;
  public CastParam(string n, ECastParamType p) { this.name = n; this.paramType = p; }
  public CastParam(string n, ECastParamType p, string d) : this(n, p) { this.description = d; }
  public CastParam Refactor(string n, ECastParamType p) { this.name = n; this.paramType = p; return this; }
  

  public static bool operator ==(CastParam p1, CastParam p2)
  {
    return p1.name == p2.name && p1.paramType == p2.paramType;
  }
  public static bool operator !=(CastParam p1, CastParam p2)
  {
    return p1.name != p2.name || p1.paramType != p2.paramType;
  }
}


public class CastParamComparer : Comparer<CastParam> {
  public override int Compare(CastParam? x, CastParam? y) { return string.Compare(x.name, y.name); }
}
public class CastingResources : SortedDictionary<CastParam, Object>
{

  public CastingResources() : base(new CastParamComparer()) {}

  public CastingResources(IDictionary<CastParam,Object> d) :  base(d, new CastParamComparer()) {}

  public void Merge(CastingResources r)
  {
    foreach(KeyValuePair<CastParam, Object> c in r ) 
    { 
      if(ContainsKey(c.Key)) continue;
      this.Add(c.Key, c.Value); 
    }
  }

  public void MergeOverwrite(CastingResources r)
  {
    foreach(KeyValuePair<CastParam, Object> c in r ) 
    {
      if(ContainsKey(c.Key))  this[c.Key] = c.Value;
      else                    Add(c.Key, c.Value); 
    }
  }
  
  public static CastingResources Merge(CastingResources[] datas)
  {
    CastingResources r = new CastingResources();
    foreach(CastingResources d in datas ) { 
    foreach(KeyValuePair<CastParam, Object> c in d ) { 
      r.Add(c.Key, c.Value); 
    }
    }
    return r;
  }


  public void Add<T>(string paramName, CastParam.ECastParamType type, Object value)
  {
    this.Add(new CastParam(paramName, type), value);
  }
  public void Add(string paramName, CastParam.ECastParamType type)
  {
    this.Add(new CastParam(paramName, type), null);
  }
  public bool ContainsKey(string n, CastParam.ECastParamType p)
  {
    return ContainsKey(new CastParam(n, p));
  }

  public static CastingResources operator +(CastingResources r1, CastingResources r2)
  {
    CastingResources r = new CastingResources(r1);
    foreach(KeyValuePair<CastParam, Object> c in r2 )
    {
      r.Add(c.Key, c.Value);
    }
    return r;
  }

  public static CastingResources operator -(CastingResources r1, CastingResources r2)
  {
    CastingResources r = new CastingResources(r1);
    foreach(KeyValuePair<CastParam, Object> c in r2 )
    {
      r.Remove(c.Key);
    }
    return r;
  }

  public static bool operator >=(CastingResources r1, CastingResources r2)
  {
    if(r1.Count < r2.Count) return false;
    foreach(KeyValuePair<CastParam, Object> p in r2)
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