using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float Acceleration = 50;
    public float AccSprintMultiplier = 4;
    public float LookSensitivity = 1;
    public float DampingCoefficient = 5;
    public bool FocusOnEnable = true;

    public float BobSpeed = 14f;
    public float BobAmount = 0.1f;

    private Vector3 _velocity;
    private float _defaultPosY = 0;
    private float _timer = 0;

    private static bool Focused
    {
        get => Cursor.lockState == CursorLockMode.Locked;
        set
        {
            Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !value;
        }
    }

    private void Start()
    {
        _defaultPosY = transform.localPosition.y;
    }

    private void OnEnable()
    {
        Debug.Log("Entering OnEnable");
        if (FocusOnEnable) Focused = true;
        Debug.Log("Exiting OnEnable");
    }

    private void OnDisable()
    {
        Debug.Log("Entering OnDisable");
        Focused = false;
        Debug.Log("Exiting OnDisable");
    }

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

    private Vector3 GetAccelerationVector()
    {
        Debug.Log("Entering GetAccelerationVector");

        Vector3 moveInput = default;

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
