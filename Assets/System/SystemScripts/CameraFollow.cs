using UnityEngine;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow instance;
    private PlayerController playerControl;
    private Transform target;
    private Transform player;

    public bool followAlongXAxis = true;
    public bool followAlongYAxis = true;
    public bool followAlongZAxis = true;


    [Tooltip("How smoothly the camera follows the player left/right")]
    public float xSmoothTime = 0.2f;
    [Tooltip("How smoothly the camera follows the player upwards")]
    public float yJumpSmoothTime = 0.8f;
    [Tooltip("How smoothly the camera follows the player downwards")]
    public float yFallSmoothTime = 0.1f;
    [Tooltip("How far left/right the camera shifts in the direction the player is looking")]
    public float lookDirectionOffset = 2;

    private bool overrideSmoothTime = false;
    private float overrideSmoothTimeAmount = 1;

    private float fallThreshold = 2f;
    private float overJumpThreshold = 10f;

    private float xRefVelocity;
    private float yRefVelocity;
    private float zRefVelocity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (instance != null)
        {
            Debug.Log("WARNING: More than one camera detected!");
        }
        instance = this;

        playerControl = PlayerController.instance;
        player = playerControl.transform;
        target = player;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (target != null)
        {
            var targetPosition = target.position;
            float currentYSmoothTime = xSmoothTime;
            float currentXSmoothTime = xSmoothTime;
            if(overrideSmoothTime)
            {
                currentXSmoothTime = overrideSmoothTimeAmount;
                currentYSmoothTime = overrideSmoothTimeAmount;
            }
            else if (target == player)
            {
                if (playerControl != null)
                {
                    if (playerControl.IsFalling() && target.position.y < (transform.position.y - fallThreshold)) currentYSmoothTime = yFallSmoothTime;
                    else if(target.position.y > (transform.position.y + overJumpThreshold)) currentYSmoothTime = yFallSmoothTime;
                    else currentYSmoothTime = yJumpSmoothTime;
                }
            }

            Vector3 newPosition = transform.position;
            if(followAlongXAxis) newPosition.x = Mathf.SmoothDamp(transform.position.x, targetPosition.x, ref xRefVelocity, currentXSmoothTime);
            if(followAlongZAxis) newPosition.z = Mathf.SmoothDamp(transform.position.z, targetPosition.z, ref zRefVelocity, currentXSmoothTime);
            if(followAlongYAxis) newPosition.y = Mathf.SmoothDamp(transform.position.y, targetPosition.y, ref yRefVelocity, currentYSmoothTime);
            transform.position = newPosition;
        }
    }

    public void SetCameraTarget(Transform aTarget)
    {
        target = aTarget;
    }

    public void FocusCameraToPlayer()
    {
        target = player;
    }

    public void SetFollowAlongXAxis(bool enable)
    {
        followAlongXAxis = enable;
    }

    public void SetFollowAlongYAxis(bool enable)
    {
        followAlongYAxis = enable;
    }

    public void SetFollowAlongZAxis(bool enable)
    {
        followAlongZAxis = enable;
    }

    public void OverrideSmoothTime(float aAmount)
    {
        overrideSmoothTime = true;
        overrideSmoothTimeAmount = aAmount;
    }
    
    public void StopOverrideSmoothTime()
    {
        overrideSmoothTime = false;
    }
}
