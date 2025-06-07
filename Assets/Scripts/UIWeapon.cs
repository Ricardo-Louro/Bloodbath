//Import necessary namespaces
using UnityEngine;

public class UIWeapon : MonoBehaviour
{
    //Set reference to the firing transform (position at the end of the model's barrel) used for the line renderer in the Weapon class
    [SerializeField] public Transform firingTransform;
    //Set reference to the weapon model used by the HealthSystem class to disable during death and re-enable upon respawn
    [SerializeField] public GameObject weaponModel;
}