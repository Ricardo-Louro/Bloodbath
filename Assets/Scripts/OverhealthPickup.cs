using UnityEngine;

public class OverhealthPickup : Pickable
{
    [SerializeField] private int amount;
    protected override void Pickup(Collider collision)
    {
        HealthSystem healthSystem = collision.GetComponent<HealthSystem>();
        if (healthSystem != null)
        {
            healthSystem.GainOverhealth(amount);
        }
    }
}