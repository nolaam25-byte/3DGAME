using UnityEngine;
using UnityEditor;

public class EditorTools : MonoBehaviour
{
    [MenuItem("GameKit//Warp Player To Editor Camera %w")]
    public static void WarpPlayerToEditorCamera()
    {
        if (Application.isPlaying)
        {
            Transform player = PlayerController.instance.transform;
            Transform camera = FindObjectOfType<CameraFollow>().transform;

            Physics.queriesHitTriggers = false;
            if (Physics.Raycast(SceneView.lastActiveSceneView.camera.transform.position, SceneView.lastActiveSceneView.camera.transform.forward, out RaycastHit hitInfo, 60))
            {
                player.position = hitInfo.point;
            }
            else
            {
                player.position =
                    SceneView.lastActiveSceneView.camera.transform.position
                    + (SceneView.lastActiveSceneView.camera.transform.forward * 15);
            }


            camera.position = SceneView.lastActiveSceneView.camera.transform.position;
        }
        else Debug.Log("Can only jump player to editor camera in play mode!");
    }
}
