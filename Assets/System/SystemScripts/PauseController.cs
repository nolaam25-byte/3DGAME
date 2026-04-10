using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class PauseController : MonoBehaviour
{
    public GameObject pauseContainer;
    private bool isPaused = false;
    private bool pausedThisPress = false;

    private bool holdingPause = false;
    private float holdPauseStartTime = 0;
    private readonly float quitHoldTime = 2f;

    public TMP_Text pauseText;
    public Slider lookSensitivity;

    void Start()
    {
        pauseContainer.SetActive(false);
         #if UNITY_WEBGL
        if(pauseText != null)
        {
            pauseText.text = "<b>Paused</b>";
        }
        #endif
    }

    void Update()
    {
        if(Input.GetButtonDown("Pause"))
        {
            if (!isPaused)
            {
                Pause();
                pausedThisPress = true;
            }
            else
            {
                holdingPause = true;
                holdPauseStartTime = Time.realtimeSinceStartup;
            }
        }

        if(Input.GetButtonUp("Pause"))
        {
            holdingPause = false;
            if (!pausedThisPress)
            {
                if (isPaused) UnPause();
            }
            else
            {
                pausedThisPress = false;
            }
        }

        if (holdingPause && Time.realtimeSinceStartup - holdPauseStartTime > quitHoldTime)
        {
            Debug.Log("Quit!");
            Application.Quit();
        }
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        pauseContainer.SetActive(true);
        AudioController.GetMixer().SetFloat("pauseVolume", -15);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(lookSensitivity.gameObject);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void UnPause()
    {
        isPaused = false;
        Time.timeScale = 1.0f;
        pauseContainer.SetActive(false);
        AudioController.GetMixer().SetFloat("pauseVolume", 0);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OnLookSensitivitySet(System.Single value) {
        PlayerController.instance.lookSpeed = value;
        // Debug.Log(PlayerController.instance.lookSpeed);
    }

    public void OnInveryYAxis(bool enable) {
        PlayerController.instance.invertYAxis = enable;
    }
}
