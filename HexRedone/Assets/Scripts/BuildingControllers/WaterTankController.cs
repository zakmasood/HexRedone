using UnityEngine;

public class FluidTank : MonoBehaviour
{
    public float maxCapacity = 100f;
    public float currentCapacity = 0f;
    public float inputRate = 5f; // Gallons per second
    public float outputRate = 3f;
    public string fluidType; // Oil, Water, etc.

    private float tickInterval = 20f; // Global tick interval
    private float timeSinceLastTick = 0f;

    void Update()
    {
        timeSinceLastTick += Time.deltaTime;

        if (timeSinceLastTick >= tickInterval)
        {
            ProcessFluid();

            timeSinceLastTick = 0f;
        }
    }

    public void SetInputRate(float rate)
    {
        inputRate = rate;
    }

    public void SetOutputRate(float rate)
    {
        outputRate = rate;
    }

    public void AddFluid(string type, float amount)
    {
        if (currentCapacity + amount <= maxCapacity)
        {
            currentCapacity += amount;
            fluidType = type;
        }
        else
        {
            // Handle overflow, Debugging for now ( FIX LATER. )
            Debug.LogWarning("Tank overflow!");
        }
    }

    public void RemoveFluid(float amount)
    {
        if (currentCapacity - amount >= 0)
        {
            currentCapacity -= amount;
        }
        else
        {
            // Handle underflow, Debugging for now ( FIX LATER. )
            Debug.LogWarning("Not enough fluid in the tank!");
        }
    }

    void ProcessFluid()
    {
        // Example function to simulate processing or using the fluid
        if (!string.IsNullOrEmpty(fluidType))
        {
            Debug.Log($"Processing {fluidType} . . .");
        }
    }
}