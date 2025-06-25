using UnityEngine;

[CreateAssetMenu(fileName = "NetworkSettings", menuName = "Scriptable Objects/NetworkSettings")]
public class NetworkSettings : ScriptableObject
{
    public string ServerAddress = "http://192.168.1.102:61100";
    public bool OfflineMode;
    [Range(1, 60)]
    public int RequestTimeout;
    public EndpointInfo Login;
    public EndpointInfo MyPicture;
    public EndpointInfo Question;
}
