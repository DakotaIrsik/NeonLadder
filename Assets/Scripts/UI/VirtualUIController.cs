using UnityEngine;

public class VirtualUIController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        this.gameObject.SetActive(false);

#if UNITY_ANDROID || UNITY_IOS || UNITY_WEBGL
        this.gameObject.SetActive(true);
#endif
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
