using UnityEngine;

public class VirtualUIController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

        Debug.Log(Application.platform);
        this.gameObject.SetActive(false);
        this.enabled = false;

#if UNITY_ANDROID || UNITY_IOS || UNITY_WEBGL
        this.gameObject.SetActive(true);
        this.enabled = true;
#endif
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
