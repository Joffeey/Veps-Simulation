using UnityEngine;
using System.Collections;

public class LiftBicycle : MonoBehaviour
{
    public Transform bicycle;  // Assign the bicycle transform in the Inspector
    public Transform liftEndpoint;  // Assign the endpoint transform in the Inspector
    public float delayBeforeLifting = 5.0f;  // Delay in seconds before lifting starts
    public float liftDuration = 5.0f;  // Duration of the lift in seconds

    private float startTime;
    private Vector3 originalPosition;

    void Start()
    {
        startTime = Time.time;
        originalPosition = bicycle.position;
    }

    void Update()
    {
        if (Time.time > startTime + delayBeforeLifting)
        {
            float t = (Time.time - (startTime + delayBeforeLifting)) / liftDuration;
            t = Mathf.Clamp01(t);  // Ensure t is between 0 and 1
            bicycle.position = Vector3.Lerp(originalPosition, liftEndpoint.position, t);
        }
    }
}