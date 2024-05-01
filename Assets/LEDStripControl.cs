using UnityEngine;

public class LEDStripControl : MonoBehaviour
{
    public Material ledOffMaterial;
    public Material ledGreenMaterial;
    public Material ledYellowMaterial;
    public Material ledRedMaterial;
    private Renderer[] ledRenderers; // Array to hold renderers of all child LEDs
    private string currentColor = "off"; // Track current color, initially off

    public string CurrentColor => currentColor; // Public getter for currentColor

    void Start()
    {
        // Initialize the array with the renderers of all child objects
        ledRenderers = GetComponentsInChildren<Renderer>();
        ChangeLEDColor("off"); // Start with LEDs off
    }

    public void ChangeLEDColor(string color)
    {
        currentColor = color; // Update the current color
        Material selectedMaterial = ledOffMaterial; // Default to off material

        switch (color)
        {
            case "green":
                selectedMaterial = ledGreenMaterial;
                break;
            case "yellow":
                selectedMaterial = ledYellowMaterial;
                break;
            case "red":
                selectedMaterial = ledRedMaterial;
                break;
        }

        // Apply the selected material to all child LEDs
        foreach (Renderer ledRenderer in ledRenderers)
        {
            ledRenderer.material = selectedMaterial;
        }
    }
}
