using UnityEngine;

public class HealthPickup : Pickable
{
    [SerializeField] private int amount;
    protected override void Pickup(Collider collision)
    {
        HealthSystem healthSystem = collision.GetComponent<HealthSystem>();
        if (healthSystem != null)
        {
            healthSystem.GainHealth(amount);
        }
    }
}