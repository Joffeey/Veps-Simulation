using UnityEngine;
using System.Collections;
using System.Diagnostics;

public class BicycleStateMachine : MonoBehaviour
{
    public enum State { Available, Reserved, Placed, Lifting, Parked, Lowering, Completed }
    public State currentState = State.Available;

    public delegate void MovementComplete();
    public event MovementComplete OnMovementComplete;


    public Transform slider; // Transform of the slider
    public LEDStripControl ledControl; // Reference to the LEDStripControl script
    public float bottomPosition = 0;
    public float topPosition = 3.1f;
    public float speed = 5f;
    public float stateDuration = 5f; // Duration for each state
    public Rigidbody rearWheelBody;
    public Rigidbody frontWheelBody;
    public float forceMagnitude = 5f;

    public Transform bicycle;
    public Vector3 offCameraPosition;
    public Vector3 onCameraPosition;
    public float rollSpeed = 2;
    public FixedJoint sliderJoint;
    bool isMoving = false;

    private float stateTimer; // Timer to keep track of time spent in the current state
    private bool isBlinking; // Flag to manage blinking states
    private float blinkInterval = 0.5f; // Blinking interval in seconds
    private float blinkTimer; // Timer to control blinking



    void Start()
    {
        UnityEngine.Debug.Log("Bottom Position: " + bottomPosition);
        UnityEngine.Debug.Log("Top Position: " + topPosition);
        stateTimer = stateDuration; // Initialize timer to transition after 5 seconds
    }

    void FixedUpdate()
    {
        if (currentState == State.Reserved || currentState == State.Completed)
        {
            Vector3 targetPosition = currentState == State.Reserved ? onCameraPosition : offCameraPosition;
            bicycle.position = Vector3.MoveTowards(bicycle.position, targetPosition, rollSpeed * Time.fixedDeltaTime);
        }
    }

    void Update()
    {
        // Handle any ongoing visual effects like blinking LEDs
        HandleBlinking();

        // Handle specific actions based on the current state
        switch (currentState)
        {
            case State.Lifting:
                // Attempt to move the slider to the top position
                MoveSlider(topPosition);
                // Check if the slider has reached the top position
                if (Mathf.Approximately(slider.position.y, topPosition))
                {
                    TransitionToNextState();
                }
                break;
            case State.Lowering:
                // Attempt to move the slider to the bottom position
                MoveSlider(bottomPosition);
                // Check if the slider has reached the bottom position
                if (Mathf.Approximately(slider.position.y, bottomPosition))
                {
                    TransitionToNextState();
                }
                break;
        }

        // Handle state transitions based on timing, ensuring no ongoing movements
        if (!isMoving && stateTimer > 0)
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0)
            {
                TransitionToNextState();
                // Reset the state timer for the next state duration
                stateTimer = stateDuration;
            }
        }
    }

    void MoveSlider(float targetY)
    {
        Vector3 targetPosition = new Vector3(slider.position.x, targetY, slider.position.z);
        slider.position = Vector3.MoveTowards(slider.position, targetPosition, speed * Time.deltaTime);
        UnityEngine.Debug.Log("Moving to " + targetY + "; Current position: " + slider.position.y);
    }

    private void HandleBlinking()
    {
        if (isBlinking)
        {
            blinkTimer -= Time.deltaTime;
            if (blinkTimer <= 0)
            {
                blinkTimer = blinkInterval;
                ledControl.ChangeLEDColor(ledControl.CurrentColor == "yellow" ? "off" : "yellow");
            }
        }
    }

    private void TransitionToNextState()
    {
        currentState = currentState switch
        {
            State.Available => State.Reserved,
            State.Reserved => State.Placed,
            State.Placed => State.Lifting,
            State.Lifting => State.Parked,
            State.Parked => State.Lowering,
            State.Lowering => State.Completed,
            State.Completed => State.Available,
            _ => currentState
        };

        OnStateEnter(currentState);
    }

    private void OnStateEnter(State newState)
    {
        switch (newState)
        {
            case State.Available:
                ledControl.ChangeLEDColor("green");
                isBlinking = false;
                break;
            case State.Reserved:
                OnMovementComplete += TransitionToPlaced;
                ledControl.ChangeLEDColor("yellow");
                isBlinking = false;
                StartCoroutine(MoveBicycle(onCameraPosition));
                break;
            case State.Placed:
                // Ensure joint is correctly configured when the bicycle is in place
                if (sliderJoint == null)
                {
                    sliderJoint = slider.gameObject.AddComponent<FixedJoint>();
                    sliderJoint.connectedBody = frontWheelBody.GetComponent<Rigidbody>();
                }
                else
                {
                    sliderJoint.connectedBody = frontWheelBody.GetComponent<Rigidbody>();
                }
                ledControl.ChangeLEDColor("green");
                isBlinking = false;
                break;
            case State.Lifting:
                ledControl.ChangeLEDColor("yellow");
                isBlinking = true;
                MoveSlider(topPosition);
                break;
            case State.Parked:
                ledControl.ChangeLEDColor("red");
                isBlinking = false;
                break;
            case State.Lowering:
                ledControl.ChangeLEDColor("yellow");
                isBlinking = true;
                MoveSlider(bottomPosition);
                break;
            case State.Completed:
                if (sliderJoint != null)
                {
                    sliderJoint.connectedBody = null;
                }
                StartCoroutine(MoveBicycle(offCameraPosition));
                ledControl.ChangeLEDColor("green");
                isBlinking = false;
                break;
        }
    }

    private void TransitionToPlaced()
    {
        OnMovementComplete -= TransitionToPlaced; // Unsubscribe to prevent multiple calls
        currentState = State.Placed;
        OnStateEnter(currentState); // Manually trigger the next state entry
    }

    IEnumerator MoveBicycle(Vector3 targetPosition)
    {
        isMoving = true;
        float wheelDiameter = 0.96f; // The actual diameter of your bicycle's wheels.
        float wheelCircumference = Mathf.PI * wheelDiameter;

        // Determine the movement direction relative to the bicycle's forward direction
        float directionMultiplier = (targetPosition - bicycle.position).z > 0 ? 1 : -1; // Assuming z is the forward direction

        while (Vector3.Distance(bicycle.position, targetPosition) > 0.05f)
        {
            Vector3 oldPosition = bicycle.position;
            bicycle.position = Vector3.MoveTowards(bicycle.position, targetPosition, rollSpeed * Time.fixedDeltaTime);
            float distanceMoved = Vector3.Distance(bicycle.position, oldPosition);
            float rotationAngle = (distanceMoved / wheelCircumference) * 360 * directionMultiplier;

            // Apply rotation to both wheels
            frontWheelBody.transform.Rotate(Vector3.right, rotationAngle, Space.World);
            rearWheelBody.transform.Rotate(Vector3.right, rotationAngle, Space.World);

            yield return new WaitForFixedUpdate();
        }

        // Stop the bicycle exactly at the target position
        bicycle.position = targetPosition;
        frontWheelBody.angularVelocity = rearWheelBody.angularVelocity = Vector3.zero;
        frontWheelBody.transform.localRotation = rearWheelBody.transform.localRotation = Quaternion.identity;

        isMoving = false;

        // Invoke the movement completion event
        OnMovementComplete?.Invoke();

        // Reattach the bicycle to the slider if needed
        if (targetPosition == onCameraPosition && sliderJoint == null)
        {
            sliderJoint = slider.gameObject.AddComponent<FixedJoint>();
            sliderJoint.connectedBody = frontWheelBody;
        }
    }
}
