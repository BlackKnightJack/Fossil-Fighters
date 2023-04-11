using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleList : MonoBehaviour
{
    public int radius;

    // Start is called before the first frame update
    void Start()
    {
        //Make sure it's at least one
        if (radius < 1) radius = 1;

        //Cycle through each value between 1 and 45
        for(int theta = 0; theta <= 90; theta++)
        {
            print(theta + ": " + new Vector2Int((int)(Mathf.Cos(Mathf.Deg2Rad * theta) * radius), (int)(Mathf.Sin(Mathf.Deg2Rad * theta) * radius)));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
