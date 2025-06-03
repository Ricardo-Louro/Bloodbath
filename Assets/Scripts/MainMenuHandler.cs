using UnityEngine;

public class MainMenuHandler : MonoBehaviour
{
    [SerializeField] private GameObject firstOptions;
    [SerializeField] private GameObject hostOptions;
    [SerializeField] private GameObject joinOptions;

    public void DisplayHostOptions()
    {
        firstOptions.SetActive(false);
        hostOptions.SetActive(true);
    }

    public void DisplayJoinOptions()
    {
        firstOptions.SetActive(false);
        joinOptions.SetActive(true);
    }
}
