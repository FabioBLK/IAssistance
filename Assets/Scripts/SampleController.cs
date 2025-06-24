using UnityEngine;

public class SampleController : MonoBehaviour
{
    [SerializeField] private NetworkSettings _networkSettings; 
    
    private WebRequester _webRequester;
    
    void Start()
    {
        _webRequester = new WebRequester(this, _networkSettings);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
