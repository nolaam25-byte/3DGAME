using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RotateObject : MonoBehaviour
{
    public Vector3 rotateDirection = Vector3.zero;
    private Rigidbody rigidbody;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        Quaternion deltaRotation = Quaternion.Euler(rotateDirection * Time.fixedDeltaTime);
        rigidbody.MoveRotation(rigidbody.rotation * deltaRotation);
    }

    public void SetRotateDirectionX(float value)
    {
        rotateDirection = new Vector3(value, rotateDirection.y, rotateDirection.z);
    }

    public void SetRotateDirectionY(float value)
    {
        rotateDirection = new Vector3(rotateDirection.x, value, rotateDirection.z);
    }

    public void SetRotateDirectionZ(float value)
    {
        rotateDirection = new Vector3(rotateDirection.x, rotateDirection.y, value);
    }
}
