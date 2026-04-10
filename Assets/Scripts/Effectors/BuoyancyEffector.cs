using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BuoyancyEffector : MonoBehaviour
{
    public float buoyancy = 20;
    public float damping = 20;
    private Collider collider;
    private Dictionary<Collider, float> dampingMap = new Dictionary<Collider,float>();

    void Start()
    {
        collider = GetComponent<Collider>();
    }

    void OnTriggerEnter(Collider c)
    {
        dampingMap.Add(c,c.attachedRigidbody.linearDamping);
    }

    void OnTriggerStay(Collider c)
    {
        // calculate buoyancy (upwards force) and drag:
        float bottom=c.transform.position.y-c.bounds.extents.y;
        float surface=transform.position.y+collider.bounds.extents.y;
        float depth=surface-bottom;
        float submerged=Mathf.Clamp01(depth/(c.bounds.extents.y*2)); // how much of the object is below the water surface? (range 0 to 1)
        c.attachedRigidbody.linearDamping=damping;
        c.attachedRigidbody.AddForce((Vector3.up*submerged*buoyancy)/c.attachedRigidbody.mass);
    }

    void OnTriggerExit(Collider c)
    {
        if(dampingMap.TryGetValue(c,out float oldDamp))
        {
            c.attachedRigidbody.linearDamping = oldDamp;
            dampingMap.Remove(c);
        }
        else
        {
            c.attachedRigidbody.linearDamping = 0;
        }
    }
}