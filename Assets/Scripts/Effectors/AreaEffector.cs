using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AreaEffector : MonoBehaviour
{
    public Vector3 appliedForce = Vector3.zero;
    private Collider collider;

    [Space]
    [Header("Gizmos")]
    public bool drawArea = true;
    public Color areaColor = new Color(1, 0, 1, 0.5f);

    void OnTriggerStay(Collider c)
    {
        c.attachedRigidbody.AddForce(appliedForce);
    }

    private void OnDrawGizmos()
    {
        if (drawArea)
        {
            if (GetComponent<Collider>() == null) collider = GetComponent<Collider>();
            if (GetComponent<Collider>() != null && GetComponent<Collider>() is BoxCollider)
            {
                Gizmos.color = areaColor;
                Vector3 worldCenter = GetComponent<Collider>().transform.TransformPoint((GetComponent<Collider>() as BoxCollider).center);
                Vector3 worldExtents = GetComponent<Collider>().transform.TransformVector((GetComponent<Collider>() as BoxCollider).size);
                Gizmos.DrawCube(worldCenter, worldExtents);
            }
        }
    }
}
