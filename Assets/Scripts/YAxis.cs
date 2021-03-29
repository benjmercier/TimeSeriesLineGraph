using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class YAxis 
{
    public int labelCount = 10;
    [HideInInspector]
    public int tempLabelCount;
    public float gridlineWidth = 2f;
    public float edgeBuffer = 0.2f;

    [HideInInspector]
    public int currentValue;

    public float defaultMinValue = 75;
    public float defaultMaxValue = 150;

    [HideInInspector]
    public float minValue,
        maxValue,
        valueRange;    

    [HideInInspector]
    public float labelPosNormal,
        labelPos;

    [HideInInspector]
    public RectTransform labelRect,
        gridlineRect;

    [HideInInspector]
    public float graphPos;
}
