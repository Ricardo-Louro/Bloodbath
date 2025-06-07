using UnityEngine;

public class HealthPickup : Pickable
{
    //Set the amount of health this pickable should recover
    [SerializeField] private int amount;

    protected override void Pickup(Collider collision)
    {
        //Try and extract the HealthSystem of the object it collided with
        HealthSystem healthSystem = collision.GetComponent<HealthSystem>();
        //If it does have a HealthSystem... (this means it is a Player)
        if (healthSystem != null)
        {
            //Recover the Player's health by the specified amount.
            healthSystem.GainHealth(amount);
        }
    }
}