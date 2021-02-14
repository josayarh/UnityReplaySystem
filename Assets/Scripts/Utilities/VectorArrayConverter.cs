using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorArrayConverter
{
    public static float[] vector3ToArray(Vector3 vec)
    {
        float[] tab = new float[3];
        for (int i = 0; i < 3; i++)
        {
            tab[i] = vec[i];
        }

        return tab;
    }

    public static Vector3 arrayToVector3(float[] array)
    {
        return new Vector3(array[0],array[1],array[2]);
    }
}
