using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidHelper : MonoBehaviour
{

    private const int DirectionCount = 300;

    // Cache Fibonacci sphere directions for every agent to reuse
    public static readonly Vector3[] Directions;

    static BoidHelper()
    {
        Directions = new Vector3[DirectionCount];

        float goldenRatio = (1f + Mathf.Sqrt(5f)) / 2f;
        float angleIncrement = Mathf.PI * 2f * goldenRatio;

        for(int i = 0; i < DirectionCount; i++)
        {
            float t = (float)i / DirectionCount;

            float inclination = Mathf.Acos(1f - 2f * t);
            float azimuth = angleIncrement * i;

            float x = Mathf.Sin(inclination) * Mathf.Cos(azimuth);
            float y = Mathf.Sin(inclination) * Mathf.Sin(azimuth);
            float z = Mathf.Cos(inclination);

            Directions[i] = new Vector3(x,y,z);
        }
    }

    void Start()
    {

    }

    void Update()
    {

    }
}
