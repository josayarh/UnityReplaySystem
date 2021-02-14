using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Bot : MonoBehaviour
{
    protected Guid id;
    protected List<String> frameSteps;
    
    public virtual void FixedUpdate()
    {
        uint frameNumber = GameObjectStateManager.Instance.FrameNumber;
        if ( frameNumber < frameSteps.Count)
        {
            if (frameNumber == 0)
            {
                LoadFrame(frameSteps[(int)frameNumber]);
            }
            else
            {
                LoadDiffFrame(frameSteps[(int)frameNumber]);
            }
        }
    }

    public abstract void LoadFrame(string binarySave);
    public abstract void LoadDiffFrame(string binarySave);
    
    public List<string> FrameSteps
    {
        set
        {
            frameSteps = value;
            LoadFrame(value[0]);
        }
    }

    public Guid Id
    {
        get => id;
    }
}
