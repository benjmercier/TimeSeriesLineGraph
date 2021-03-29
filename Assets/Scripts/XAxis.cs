using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class XAxis
{
    public int labelCount = 6;
    public float edgeBuffer = 1f;

    public DateTime minDateTime,
        maxDateTime;
    public TimeSpan totalTime;

    [HideInInspector]
    public int labelIndex;

    [HideInInspector]
    public float minLabelPos,
        maxLabelPos,
        minMaxLabelVariance,
        currentLabelSpread,
        currentLabelPos;

    [HideInInspector]
    public RectTransform labelRect,
        gridlineRect;

    [HideInInspector]
    public float graphPos;
}
