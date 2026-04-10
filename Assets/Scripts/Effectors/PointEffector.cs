using UnityEngine;

public class PointEffector : MonoBehaviour
{
    public float radius = 5;
    public float appliedForce = 5;

    [Space]
    [Header("Gizmos")]
    public bool drawArea = true;
    public Color areaColor = new Color(1, 0, 1, 0.5f);

    // void OnTriggerStay(Collider c)
    // {
    //     c.attachedRigidbody.AddForce(
    //         (transform.position - c.transform.position) * appliedForce 
    //         * (Vector3.Distance(transform.position, c.transform.position) * distanceModifier)
    //         );
    // }

    void FixedUpdate()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider each in colliders)
        {
            Rigidbody rigidbody = each.GetComponent<Rigidbody>();

            if (rigidbody != null)
            {
                rigidbody.AddExplosionForce(appliedForce, transform.position, radius);
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        if (drawArea)
        {
            Gizmos.color = areaColor;
            Gizmos.DrawSphere(transform.position, radius);
        }
    }
}
