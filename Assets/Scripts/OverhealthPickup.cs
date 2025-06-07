//Import necessary namespaces
using UnityEngine;

public class OverhealthPickup : Pickable
{
    //Set the value for the amount of overhealth to be restored
    [SerializeField] private int amount;
    protected override void Pickup(Collider collision)
    {
        //Obtain a reference to the collided object's HealthSystem
        HealthSystem healthSystem = collision.GetComponent<HealthSystem>();
        //If this is a valid reference (only Players would have valid references to HealthSystems)
        if (healthSystem != null)
        {
            //Regain health equal to the amount. This amount cannot exceed the Max Overhealth but can exceed the lower limit of Max Health
            healthSystem.GainOverhealth(amount);
        }
    }
}