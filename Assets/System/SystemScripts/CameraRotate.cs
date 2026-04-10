using UnityEngine;

public class CameraRotate : MonoBehaviour
{
    public static CameraRotate instance;

    public enum CameraMode { Free, Fixed }
    public CameraMode cameraMode = CameraRotate.CameraMode.Free;

    [Header("Free Camera Variables")]
    public float cameraDistance = 10;
    public Vector3 cameraOffset = Vector3.zero;
    public LayerMask cameraLayerMask;
    private Transform playerTransform;

    [Header("Fixed Camera Variables")]
    public Vector3 targetPosition = Vector3.zero;
    public Quaternion freeTargetRotation = Quaternion.identity;
    public Quaternion fixedTargetRotation = Quaternion.identity;
    private Vector3 positionVelocity;
    private float moveTime = 0.3f;
    private float rotateTime = 25f;
    private float transitionTime = 1f;
    private float rotateStartTime = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        instance = this;
        targetPosition = transform.localPosition;
        freeTargetRotation = transform.localRotation;
        fixedTargetRotation = transform.localRotation;
    }

    void Start()
    {
        playerTransform = PlayerController.instance.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (cameraMode == CameraMode.Free)
        {
            DoFreeCameraMovement();
        }
        else
        {
            DoFixedCameraMovement();
        }
    }

    private void DoFixedCameraMovement()
    {
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, targetPosition, ref positionVelocity, moveTime);

        if ((Time.time - rotateStartTime) < rotateTime)
        {
            float rotateTimePerc = (Time.time - rotateStartTime) / rotateTime;
            transform.localRotation = Quaternion.Slerp(transform.localRotation, fixedTargetRotation, rotateTimePerc);
        }
        else
        {
            transform.localRotation = fixedTargetRotation;
        }
    }

    private void DoFreeCameraMovement()
    {
        float currentCameraDistance = cameraDistance;

        if (Physics.Raycast(playerTransform.position, -transform.forward, out RaycastHit hitInfo, cameraDistance, cameraLayerMask))
        {
            currentCameraDistance = hitInfo.distance;
        }


        if ((Time.time - rotateStartTime) < transitionTime)
        {
            float rotateTimePerc = (Time.time - rotateStartTime) / rotateTime;
            transform.localRotation = Quaternion.Slerp(transform.localRotation, freeTargetRotation, rotateTimePerc);

            Vector3 freeTargetPosition = cameraOffset - (transform.forward * currentCameraDistance);
            transform.localPosition = Vector3.Lerp(transform.localPosition, freeTargetPosition, rotateTimePerc);
        }
        else
        {
            transform.localRotation = freeTargetRotation;

            transform.localPosition = cameraOffset - (transform.forward * currentCameraDistance);
        }
    }

    public void SetTargetPosition(Vector3 newPosition)
    {
        targetPosition = newPosition;
    }

    public void SetTargetRotation(Quaternion newRotation)
    {
        fixedTargetRotation = newRotation;
        rotateStartTime = Time.time;
    }

    public void SetPlayerCameraAngle(Quaternion newRotation)
    {
        freeTargetRotation = newRotation;
    }

    public void SetCameraModeToFree()
    {
        cameraMode = CameraMode.Free;
        rotateStartTime = Time.time;
    }

    public void SetCameraModeToFixed()
    {
        cameraMode = CameraMode.Fixed;
    }

    public void SetCameraDistance(float aValue)
    {
        cameraDistance = aValue;
    }
}
