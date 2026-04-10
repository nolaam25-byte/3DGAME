using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MoveTowardsTarget : MonoBehaviour
{
    public bool targetIsPlayer = true;
    public Transform alternativeTarget;

    private Transform playerTransform;
    private Rigidbody rigidbody;

    public float moveSpeed = 15;
    public float maxSpeed = 50;

    public bool moveInXDirection = true;
    public bool moveInYDirection = true;
    public bool moveInZDirection = true;

    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        playerTransform = PlayerController.instance.transform;
    }

    void FixedUpdate()
    {
        Vector3 moveDirection = Vector3.zero;
        Transform target = alternativeTarget;

        if(targetIsPlayer)
        {
            target = playerTransform;
        }

        moveDirection = (target.position - transform.position).normalized;
        moveDirection = new Vector3(
            moveInXDirection ? moveDirection.x : 0,
            moveInYDirection ? moveDirection.y : 0,
            moveInZDirection ? moveDirection.z : 0
        );

        rigidbody.AddForce(Vector3.ClampMagnitude(moveDirection * moveSpeed,maxSpeed));
    }
}
