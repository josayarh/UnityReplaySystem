using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct SphereFrameData
{
    public byte[] id;
    public float[] position;
    public float[] rotation;
}

[Serializable]
public struct SphereDiffFrameData
{
    public float[] position;
    public float[] rotation;
}