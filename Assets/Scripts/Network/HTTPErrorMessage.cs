using UnityEngine;

public class HTTPErrorMessage
{

    public long errorCode;
    public string message;

    public HTTPErrorMessage(long p_error, string p_message)
    {
        errorCode = p_error;
        message = p_message;
    }
}
