using UnityEngine;

public class CameraFocuser : MonoBehaviour
{
    public Transform cameraAngle;

    public void LockCameraToPoint()
    {
        CameraFollow.instance.SetCameraTarget(transform);
        SetCameraAngle();
    }

    public void LockCameraToPlayer()
    {
        CameraFollow.instance.FocusCameraToPlayer();
    }

    public void SetCameraAngleToFixed()
    {
        CameraRotate.instance.SetCameraModeToFixed();
        SetCameraAngle();
    }

    public void SetCameraAngleToFree()
    {
        CameraRotate.instance.SetCameraModeToFree();
    }

    public void FocusCameraToObject(GameObject aObject)
    {
        CameraFollow.instance.SetCameraTarget(aObject.transform);
    }

    private void SetCameraAngle()
    {
        if (cameraAngle != null)
        {
            CameraRotate.instance.SetTargetPosition(cameraAngle.localPosition);
            CameraRotate.instance.SetTargetRotation(cameraAngle.localRotation);
        }
    }

    public void SetCameraMoveTime(float amount)
    {
        CameraFollow.instance.OverrideSmoothTime(amount);
    }

    public void ResetCameraMoveTime()
    {
        CameraFollow.instance.StopOverrideSmoothTime();
    }

    public void OnDrawGizmos()
    {
        if (cameraAngle != null)
        {
            Gizmos.color = Color.white;
            Matrix4x4 tempMat = Gizmos.matrix;
            Gizmos.matrix = cameraAngle.localToWorldMatrix;
            Gizmos.DrawFrustum(Vector3.zero, 60f, Vector3.Distance(cameraAngle.position,transform.position), 0f, 16f/9f);
            Gizmos.matrix = tempMat;
        }
    }
}
