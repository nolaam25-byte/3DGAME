using UnityEngine;

public class PhysicsObjectManipulator : MonoBehaviour
{
    private Rigidbody rigidbody;
    public Vector3 forceToAdd = Vector3.zero;

    [Space]
    [Header("Debug")]
    [Tooltip("Whether or not this script prints information to the debug console.")]
    public bool consoleLog = false;

    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    public void ChangeBodyTypeToDynamic()
    {
        rigidbody.isKinematic = false;
        if (consoleLog) Debug.Log("Changing RigidbodyType to Dynamic: " + gameObject.name);
    }

    public void ChangeBodyTypeToKinematic()
    {
        rigidbody.isKinematic = true;
        if (consoleLog) Debug.Log("Changing RigidbodyType to Kinematic: " + gameObject.name);
    }

    public void SetBodyDetectCollisions(bool enable)
    {
        rigidbody.detectCollisions = enable;
        if (consoleLog) Debug.Log("Changing Rigidbody Detect Collisions to " +  enable + ": " + gameObject.name);
    }

    public void ApplyForce()
    {
        rigidbody.AddForce(forceToAdd);
        if (consoleLog) Debug.Log("Applying Force to Object: " + gameObject.name);
    }

    public void ApplyRelativeForce()
    {
        rigidbody.AddRelativeForce(forceToAdd);
        if (consoleLog) Debug.Log("Applying Force to Object: " + gameObject.name);
    }

    public void ChangeOtherBodyTypeToDynamic(GameObject aObject)
    {
        aObject.GetComponent<Rigidbody>().isKinematic = false;
        if (consoleLog) Debug.Log("Changing " + aObject.name + " RigidbodyType to Dynamic: " + gameObject.name);
    }

    public void ChangeOtherBodyTypeToKinematic(GameObject aObject)
    {
        aObject.GetComponent<Rigidbody>().isKinematic = true;
        if (consoleLog) Debug.Log("Changing " + aObject.name + " RigidbodyType to Kinematic: " + gameObject.name);
    }

    public void SetOtherBodyDetectCollisionsTrue(GameObject aObject)
    {
        aObject.GetComponent<Rigidbody>().detectCollisions = true;
        if (consoleLog) Debug.Log("Changing Rigidbody Detect Collisions to true: " + gameObject.name);
    }

    public void SetOtherBodyDetectCollisionsFalse(GameObject aObject)
    {
        aObject.GetComponent<Rigidbody>().detectCollisions = false;
        if (consoleLog) Debug.Log("Changing Rigidbody Detect Collisions to false: " + gameObject.name);
    }


    public void ApplyForceToOtherObject(GameObject aObject)
    {
        aObject.GetComponent<Rigidbody>().AddForce(forceToAdd);
        if (consoleLog) Debug.Log("Applying Force to Object: " + gameObject.name);
    }

    public void ApplyRelativeForceToOtherObject(GameObject aObject)
    {
        aObject.GetComponent<Rigidbody>().AddRelativeForce(forceToAdd);
        if (consoleLog) Debug.Log("Applying Force to Object: " + gameObject.name);
    }
}
