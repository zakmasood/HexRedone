using System.Collections;
using UnityEngine;
using TMPro;

public class FloatingTextManager : MonoBehaviour
{
    public static FloatingTextManager Instance; // Singleton instance
    public GameObject floatingTextPrefab; // Assign via Inspector
    private GameObject currentFloatingText;
    private TextMeshPro textMesh;
    private Camera mainCamera;

    private Coroutine activeCoroutine; // Tracks the currently active coroutine

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    private void Start()
    {
        mainCamera = Camera.main;

        // Create a single floating text object in the scene
        currentFloatingText = Instantiate(floatingTextPrefab);
        textMesh = currentFloatingText.GetComponent<TextMeshPro>();
        currentFloatingText.SetActive(false); // Hide by default
    }

    private void Update()
    {
        if (currentFloatingText.activeSelf)
        {
            // Ensure the floating text always faces the camera
            Vector3 directionToCamera = (mainCamera.transform.position - currentFloatingText.transform.position).normalized;
            currentFloatingText.transform.forward = -directionToCamera; // Text faces camera
        }
    }

    public void ShowText(Vector3 position, string text, float duration = 2f)
    {
        // Stop any active coroutines to prevent race conditions
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
        }

        // Move and activate the floating text
        currentFloatingText.transform.position = position; // Position the text in world space
        currentFloatingText.SetActive(true);
        textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, 1f); // Reset alpha
        textMesh.text = text;

        // Start the obfuscation and reveal effect
        activeCoroutine = StartCoroutine(ObfuscateAndReveal(text, duration));
    }

    public void HideText(float duration = 2f)
    {
        if (!currentFloatingText.activeSelf) return;

        // Stop any active coroutines to prevent race conditions
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
        }

        // Start the fade-out effect
        activeCoroutine = StartCoroutine(FadeOutAndHide(duration));
    }

    private IEnumerator ObfuscateAndReveal(string resourceType, float duration)
    {
        char[] finalChars = resourceType.ToCharArray();
        char[] garbledChars = new char[finalChars.Length];

        for (int i = 0; i < garbledChars.Length; i++)
        {
            garbledChars[i] = GetRandomAsciiChar();
        }
        textMesh.text = new string(garbledChars);

        for (float elapsed = 0; elapsed < duration; elapsed += Time.deltaTime)
        {
            float progress = elapsed / duration;

            for (int i = 0; i < finalChars.Length; i++)
            {
                if (Random.value < progress) { garbledChars[i] = finalChars[i]; }
                else { garbledChars[i] = GetRandomAsciiChar(); }
            }

            textMesh.text = new string(garbledChars);
            yield return null;
        }

        textMesh.text = resourceType;
    }

    private IEnumerator FadeOutAndHide(float duration)
    {
        for (float elapsed = 0; elapsed < duration; elapsed += Time.deltaTime)
        {
            float alpha = 1f - (elapsed / duration);
            textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, alpha);
            yield return null;
        }

        currentFloatingText.SetActive(false);
    }

    private char GetRandomAsciiChar() { return (char)Random.Range(33, 126); }
}
