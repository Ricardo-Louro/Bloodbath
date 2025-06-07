//Import necessary namespaces
using UnityEngine;

public class MainMenuHandler : MonoBehaviour
{
    //Set reference to the Join and Host buttons
    [SerializeField] private GameObject firstOptions;
    //Set reference to the Join Code display
    [SerializeField] private GameObject hostOptions;
    //Set reference to the Input Field where the Join Code is inserted
    [SerializeField] private GameObject joinOptions;

    //Toggle the components when you click the Host button
    public void DisplayHostOptions()
    {
        //Hide the Join and Host button
        firstOptions.SetActive(false);
        //Show the Join Code display
        hostOptions.SetActive(true);
    }

    //Toggle the components when you click the Join button
    public void DisplayJoinOptions()
    {
        //Hide the Join and Host button
        firstOptions.SetActive(false);
        //Show the Input Field where the Join Code is inserted
        joinOptions.SetActive(true);
    }
}
