using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject cameraTransformer; // The object to rotate
    public Camera mainCamera; // The camera to adjust zoom
    public float rotationStep = 30f; // Degrees to rotate
    public float zoomSpeed = 1f; // Speed of zooming
    public float maxZoom = 10f; // Maximum orthographic size
    public float minZoom = 1f; // Minimum orthographic size
    public float rotationLerpSpeed = 7f; // Speed of rotation interpolation
    public float zoomLerpSpeed = 15f; // Speed of zoom interpolation

    private float targetYRotation; // Target Y-axis rotation
    private float targetZoom; // Target zoom level for the camera

    private void Start()
    {
        // Initialize target values
        if (cameraTransformer != null)
        {
            targetYRotation = cameraTransformer.transform.eulerAngles.y;
        }
        if (mainCamera != null)
        {
            targetZoom = mainCamera.orthographicSize;
        }
    }

    private void Update()
    {
        // Rotate the cameraTransformer when Q or R is pressed
        if (Input.GetKeyDown(KeyCode.Q))
        {
            RotateCamera(rotationStep);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            RotateCamera(-rotationStep);
        }

        // Zoom with the scroll wheel
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0f)
        {
            AdjustZoom(-scrollInput * zoomSpeed);
        }

        // Smoothly interpolate the rotation and zoom
        SmoothRotate();
        SmoothZoom();
    }

    private void RotateCamera(float angle)
    {
        if (cameraTransformer != null)
        {
            // Update the target Y-axis rotation
            targetYRotation += angle;
        }
    }

    private void AdjustZoom(float delta)
    {
        if (mainCamera != null && mainCamera.orthographic)
        {
            // Update the target zoom level
            targetZoom = Mathf.Clamp(targetZoom + delta, minZoom, maxZoom);
        }
    }

    private void SmoothRotate()
    {
        if (cameraTransformer != null)
        {
            // Smoothly interpolate only the Y-axis rotation
            Vector3 currentRotation = cameraTransformer.transform.eulerAngles;
            float smoothYRotation = Mathf.LerpAngle(currentRotation.y, targetYRotation, Time.deltaTime * rotationLerpSpeed);

            cameraTransformer.transform.rotation = Quaternion.Euler(30, smoothYRotation, 0);
        }
    }

    private void SmoothZoom()
    {
        if (mainCamera != null && mainCamera.orthographic)
        {
            // Smoothly interpolate to the target zoom level
            mainCamera.orthographicSize = Mathf.Lerp(
                mainCamera.orthographicSize,
                targetZoom,
                Time.deltaTime * zoomLerpSpeed
            );
        }
    }
}
