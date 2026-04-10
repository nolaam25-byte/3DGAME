using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class ColorManipulator : MonoBehaviour
{
    private Renderer renderer;

    [Header("Starting Values")]
    [Tooltip("Whether or not to set the color on game start.")]
    public bool setStartingColor = false;
    [Tooltip("Color to set on game start.")]
    public Color startingColor = Color.white;

    [Space]

    [Tooltip("How long to fade between colors. A value of 0 will set instantly.")]
    public float colorChangeTime = 5f;

    private Color originalColor = Color.white;
    [Tooltip("Colors that this component can change a sprite renderer's color to.  When choosing via an event, choose the index of the color, starting with 0.")]
    public Color[] presetColors;

    [Space]
    [Header("Other Object Values")]
    [Tooltip("When changing a triggered object's hue, saturation, lightness, or alpha, the trigger'd objects values will change to this amount.")]
    public float changeAmount = 0;

    [Space]
    [Header("Debug")]
    [Tooltip("Whether or not this script prints information to the debug console.")]
    public bool consoleLog = false;

    private Color targetColor = Color.white;
    private float startingColorChangeTime;

    void Awake()
    {
        renderer = GetComponent<Renderer>();
        if (setStartingColor)
        {
            renderer.material.color = startingColor;
            originalColor = startingColor;
        }
        else
        {
            originalColor = renderer.material.color;
        }
        targetColor = originalColor;
    }

    private void SetColor(Color aColor)
    {
        targetColor = aColor;
        startingColorChangeTime = Time.time;
    }

    public void Update()
    {
        float percent = (Time.time - startingColorChangeTime) / colorChangeTime;
        renderer.material.color = Color.Lerp(renderer.material.color, targetColor, percent);
    }

    public void ChangeColorPreset(int color)
    {
        SetColor(presetColors[color]);
    }

    public void ResetColor()
    {
        SetColor(originalColor);
    }

    public void AdjustColorHue(float amount)
    {
        Color.RGBToHSV(renderer.material.color, out float hue, out float saturation, out float value);
        SetColor(Color.HSVToRGB(hue + amount, saturation, value));
    }

    public void AdjustColorSaturation(float amount)
    {
        Color.RGBToHSV(renderer.material.color, out float hue, out float saturation, out float value);
        SetColor(Color.HSVToRGB(hue, saturation + amount, value));
    }

    public void AdjustColorValue(float amount)
    {
        Color.RGBToHSV(renderer.material.color, out float hue, out float saturation, out float value);
        SetColor(Color.HSVToRGB(hue, saturation, value + amount));
    }

    public void SetAlpha(float amount)
    {
        Color color = renderer.material.color;
        color.a = amount;
        SetColor(color);
    }

    public void AdjustAlpha(float amount)
    {
        Color color = renderer.material.color;
        color.a += amount;
        SetColor(color);
    }

    public void RandomColor()
    {
        SetColor(new Color(Random.Range(0F, 1F), Random.Range(0, 1F), Random.Range(0, 1F)));
    }

    public void SetColorChangeTime(float aValue)
    {
        colorChangeTime = aValue;
    }


    // ------- Other Object Methods ------

    private Renderer GetRenderer(GameObject aObject)
    {
        Renderer renderer = aObject.GetComponent<Renderer>();
        if (renderer == null)
        {
            renderer = aObject.GetComponentInChildren<Renderer>();
        }
        return renderer;
    }

    public void OtherChangeColorPreset(GameObject aObject)
    {
        GetRenderer(aObject).material.color = presetColors[0];
    }

    public void OtherAdjustColorHue(GameObject aObject)
    {
        Color.RGBToHSV(GetRenderer(aObject).material.color, out float hue, out float saturation, out float value);
        GetRenderer(aObject).material.color = Color.HSVToRGB(hue + changeAmount, saturation, value);
    }

    public void OtherAdjustColorSaturation(GameObject aObject)
    {
        Color.RGBToHSV(GetRenderer(aObject).material.color, out float hue, out float saturation, out float value);
        GetRenderer(aObject).material.color = Color.HSVToRGB(hue, saturation + changeAmount, value);
    }

    public void OtherAdjustColorValue(GameObject aObject)
    {
        Color.RGBToHSV(GetRenderer(aObject).material.color, out float hue, out float saturation, out float value);
        GetRenderer(aObject).material.color = Color.HSVToRGB(hue, saturation, value + changeAmount);
    }

    public void OtherSetAlpha(GameObject aObject)
    {
        Color color = GetRenderer(aObject).material.color;
        color.a = changeAmount;
        GetRenderer(aObject).material.color = color;
    }

    public void OtherAdjustAlpha(GameObject aObject)
    {
        Color color = GetRenderer(aObject).material.color;
        color.a += changeAmount;
        GetRenderer(aObject).material.color = color;
    }

    public void OtherRandomColor(GameObject aObject)
    {
        GetRenderer(aObject).material.color = new Color(Random.Range(0F, 1F), Random.Range(0, 1F), Random.Range(0, 1F));
    }
}
