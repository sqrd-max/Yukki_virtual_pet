using UnityEngine;

public class TextBlock : MonoBehaviour
{
    public GameObject fbxModel;

    void Start()
    {
        if (fbxModel != null)
        {
            Debug.Log("FBX model found. Current scale: " + fbxModel.transform.localScale);
            
            fbxModel.transform.localScale = new Vector3(-200.0f, 200.0f, 200.0f);
        }
        else
        {
            Debug.LogError("FBX model is not assigned.");
        }
    }
}
