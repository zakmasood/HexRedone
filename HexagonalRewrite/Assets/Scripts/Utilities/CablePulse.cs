using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class CablePulse : MonoBehaviour
{
    private LineRenderer lineRenderer;
    public Gradient baseGradient;        // Default gradient for the cable
    public Gradient pulseGradient;       // Gradient representing the pulse
    public float pulseSpeed = 1f;        // Speed of the pulse animation
    public float pulseDuration = 1f;     // Duration for which the pulse is visible

    private bool isPulsing = false;
    private float pulseTimer = 0f;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.colorGradient = baseGradient;
    }

    private void Update()
    {
        if (isPulsing)
        {
            // Animate the gradient
            float t = (pulseTimer / pulseDuration) * pulseSpeed;
            Gradient animatedGradient = AnimateGradient(baseGradient, pulseGradient, t);
            lineRenderer.colorGradient = animatedGradient;

            // Stop pulsing after the duration
            pulseTimer += Time.deltaTime;
            if (pulseTimer >= pulseDuration)
            {
                isPulsing = false;
                lineRenderer.colorGradient = baseGradient; // Reset to base gradient
            }
        }
    }

    public void TriggerPulse()
    {
        isPulsing = true;
        pulseTimer = 0f;
    }

    private Gradient AnimateGradient(Gradient baseGradient, Gradient pulseGradient, float t)
    {
        GradientColorKey[] baseColorKeys = baseGradient.colorKeys;
        GradientColorKey[] pulseColorKeys = pulseGradient.colorKeys;

        Gradient animatedGradient = new Gradient();
        GradientColorKey[] animatedColorKeys = new GradientColorKey[baseColorKeys.Length];

        for (int i = 0; i < baseColorKeys.Length; i++)
        {
            Color blendedColor = Color.Lerp(baseColorKeys[i].color, pulseColorKeys[i % pulseColorKeys.Length].color, Mathf.PingPong(t, 1));
            animatedColorKeys[i] = new GradientColorKey(blendedColor, baseColorKeys[i].time);
        }

        animatedGradient.SetKeys(animatedColorKeys, baseGradient.alphaKeys);
        return animatedGradient;
    }
}
