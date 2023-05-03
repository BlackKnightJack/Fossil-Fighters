using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerManager : MonoBehaviour
{
    // OnTriggerEnter is called the frame this object's collider intersects with a trigger collider
    void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent.GetComponent<EventPlayer>())
        {
            other.transform.parent.GetComponent<EventPlayer>().TriggerRangeChange(true);
        }
    }

    // OnTriggerEnter is called the frame this object's collider stops intersecting with a trigger collider
    void OnTriggerExit(Collider other)
    {
        if (other.transform.parent.GetComponent<EventPlayer>())
        {
            other.transform.parent.GetComponent<EventPlayer>().TriggerRangeChange(false);
        }
    }
}
