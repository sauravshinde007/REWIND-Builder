using System;
using System.Collections.Generic;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public class MechanicAttribute : Attribute {

    public string Tag {get;}

    public float Min {get;}
    public float Max {get;}

    public MechanicAttribute(string tag){
        Tag = tag;
        Min = 0f;
        Max = 1f;
    }
    public MechanicAttribute(string tag, float min, float max){
        Tag = tag;
        Min = min;
        Max = max;
    }

    public int DefaultIndex = -1;
    public bool Dropdown = false;

    public MechanicAttribute(string tag, int defaultIndex)
    {
        Dropdown = true;
        Tag = tag;
        if (defaultIndex == -1)
        {
            Debug.Log("-1 index not allowed");
            return;
        }
        DefaultIndex = defaultIndex;
    }
}