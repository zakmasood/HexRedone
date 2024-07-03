/**
 * @file CameraController.cs
 * @brief Controls the camera movement and behavior in the game.
 *
 * This script handles the camera movement, including acceleration, damping, and bobbing effects.
 */

using UnityEngine;

/**
 * @class CameraController
 * @brief Manages the camera's position, rotation, and input handling.
 *
 * This class allows the camera to move and look around based on player input. It supports acceleration, sprinting, and a bobbing effect when moving.
 */
public class CameraController : MonoBehaviour
{
    /// Acceleration factor for camera movement.
    public float Acceleration = 50;

    /// Multiplier applied when sprinting.
    public float AccSprintMultiplier = 4;

    /// Sensitivity of the camera look movement.
    public float LookSensitivity = 1;

    /// Damping coefficient for smoothing camera movement.
    public float DampingCoefficient = 5;

    /// Determines whether the camera should focus when enabled.
    public bool FocusOnEnable = true;

    /// Speed of the bobbing effect.
    public float BobSpeed = 14f;

    /// Amount of bobbing applied to the camera.
    public float BobAmount = 0.1f;

    private Vector3 _velocity; ///< Current velocity of the camera.
    private float _defaultPosY = 0; ///< Default Y position of the camera.
    private float _timer = 0; ///< Timer used for the bobbing effect.

    /**
     * @brief Checks if the cursor is locked and visible.
     * 
     * Locks or unlocks the cursor based on its state.
     */
    private static bool Focused
    {
        get => Cursor.lockState == CursorLockMode.Locked;
        set
        {
            Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !value;
        }
    }

    /**
     * @brief Initializes the default Y position of the camera.
     */
    private void Start()
    {
        _defaultPosY = transform.localPosition.y;
    }

    /**
     * @brief Called when the script is enabled.
     *
     * Optionally focuses the camera when it is enabled.
     */
    private void OnEnable()
    {
        Debug.Log("Entering OnEnable");
        if (FocusOnEnable) Focused = true;
        Debug.Log("Exiting OnEnable");
    }

    /**
     * @brief Called when the script is disabled.
     *
     * Unfocuses the camera when it is disabled.
     */
    private void OnDisable()
    {
        Debug.Log("Entering OnDisable");
        Focused = false;
        Debug.Log("Exiting OnDisable");
    }

    /**
     * @brief Updates the camera position and rotation every frame.
     *
     * Handles input for moving and rotating the camera, applies damping and bobbing effects.
     */
    private void Update()
    {
        Debug.Log("Entering Update");

        if (Focused)
        {
            UpdateInput();
        }
        else if (Input.GetMouseButtonDown(0))
        {
            Focused = true;
            UpdateInput();
        }

        _velocity = Vector3.Lerp(_velocity, Vector3.zero, DampingCoefficient * Time.deltaTime);
        transform.position += _velocity * Time.deltaTime;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
        {
            _timer += Time.deltaTime * BobSpeed;
            transform.localPosition = new Vector3(transform.localPosition.x, _defaultPosY + Mathf.Sin(_timer) * BobAmount, transform.localPosition.z);
        }
        else
        {
            _timer = 0;
            transform.localPosition = new Vector3(transform.localPosition.x, Mathf.Lerp(transform.localPosition.y, _defaultPosY, Time.deltaTime * BobSpeed), transform.localPosition.z);
        }

        Debug.Log("Exiting Update");
    }

    /**
     * @brief Handles user input for accelerating and rotating the camera.
     */
    private void UpdateInput()
    {
        Debug.Log("Entering UpdateInput");

        _velocity += GetAccelerationVector() * Time.deltaTime;

        Vector2 mouseDelta = LookSensitivity * new Vector2(Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y"));
        Quaternion rotation = transform.rotation;
        Quaternion horiz = Quaternion.AngleAxis(mouseDelta.x, Vector3.up);
        Quaternion vert = Quaternion.AngleAxis(mouseDelta.y, Vector3.right);
        transform.rotation = horiz * rotation * vert;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Focused = false;
        }

        Debug.Log("Exiting UpdateInput");
    }

    /**
     * @brief Computes the acceleration vector based on user input.
     *
     * Handles movement keys and applies appropriate acceleration.
     *
     * @return The acceleration vector.
     */
    private Vector3 GetAccelerationVector()
    {
        Debug.Log("Entering GetAccelerationVector");

        Vector3 moveInput = default;

        /**
         * @brief Adds movement to the acceleration vector based on a key press.
         * 
         * @param key The key to check for input.
         * @param dir The direction vector to add.
         */
        void AddMovement(KeyCode key, Vector3 dir)
        {
            Debug.Log($"Entering AddMovement for key: {key}");
            if (Input.GetKey(key))
            {
                moveInput += dir;
            }
            Debug.Log($"Exiting AddMovement for key: {key}");
        }

        AddMovement(KeyCode.W, Vector3.forward);
        AddMovement(KeyCode.S, Vector3.back);
        AddMovement(KeyCode.D, Vector3.right);
        AddMovement(KeyCode.A, Vector3.left);
        AddMovement(KeyCode.Space, Vector3.up);
        AddMovement(KeyCode.LeftControl, Vector3.down);

        Vector3 direction = transform.TransformVector(moveInput.normalized);

        Debug.Log("Exiting GetAccelerationVector");

        if (Input.GetKey(KeyCode.LeftShift))
        {
            return direction * (Acceleration * AccSprintMultiplier);
        }
        return direction * Acceleration;
    }
}