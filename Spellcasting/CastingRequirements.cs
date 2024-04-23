using System;
using System.Collections;
using System.Collections.Generic;

enum param_type : int {
  BOOL,
  INT,
  FLOAT,
  GAMEOBJECT,
  VECTOR2,
  VECTOR3       
};
 
public struct CastingRequirements
{
    Dictionary<param_type, CastingComponent> 
}