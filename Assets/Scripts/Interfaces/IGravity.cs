using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGravity
{
    bool IsStatic { get; set; }
    float Gravity { get; }
}