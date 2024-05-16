using Unity.VisualScripting;
using UnityEngine;

public class ToggleChat : MonoBehaviour
{
    public GameObject Inventory;
    
    void Start()
    {
        Inventory.gameObject.SetActive(false);
    }

    public void Switch()
    {
        if (Inventory.activeSelf == false) Inventory.SetActive(true);
        else Inventory.SetActive(false);
    }
}
