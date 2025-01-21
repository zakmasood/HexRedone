using UnityEngine;

public class DrillAnimator : MonoBehaviour
{
    private Animator animator;
    public bool IsLowered { get; private set; } = false; // Tracks if the drill is lowered
    public float LoweringDuration = 7.5f; // Duration of the lowering animation

    private void Start() { animator = GetComponent<Animator>(); }

    public void LowerDrill() { if (!IsLowered) { animator.SetTrigger("LowerDrill"); IsLowered = true; } }

    public void StartDrilling() { animator.SetTrigger("StartDrilling"); }

    public void StopDrilling() { animator.SetTrigger("StopDrilling"); IsLowered = false; }
}
