using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private Camera camera;

    void Start()
    {
        camera = FindObjectOfType<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (camera != null)
        {
            transform.localRotation = Quaternion.Euler(0, camera.gameObject.transform.eulerAngles.y, 0);
        }
    }
}
