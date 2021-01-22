using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleAnimTest : MonoBehaviour
{
    public GameObject Go;
    public float Time;
    public AnimationClip Clip;

    private void OnValidate()
    {
        Sample();
    }

    [ContextMenu(nameof(Sample))]
    public void Sample()
    {
        Clip.SampleAnimation(Go, Time % Clip.length);
    }
}
