using UnityEngine;
using TMPro;  // Only necessary if using TextMeshPro

public class textScritp : MonoBehaviour
{
    public BicycleStateMachine stateMachine; // Reference to your state machine
    public TextMeshProUGUI stateText; // Reference to your TextMeshPro UI component

    void Update()
    {
        if (stateText != null && stateMachine != null)
        {
            stateText.text = "Current State: " + stateMachine.currentState.ToString();
        }
    }
}
