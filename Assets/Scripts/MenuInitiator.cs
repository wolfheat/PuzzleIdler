using UnityEngine;

public class MenuInitiator : MonoBehaviour
{
    [SerializeField] private GameObject[] activate; 
    [SerializeField] private GameObject[] deactivate; 

    void Start()
    {
        foreach (var item in activate) {
            item.SetActive(true);
        }
        foreach (var item in deactivate) {
            item.SetActive(false);
        }    
    }
}
