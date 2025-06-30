using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class WebRequester
{
    private NetworkSettings m_settings;

    private MonoBehaviour m_executer;

    private IEnumerator m_friendPicturerequest;
    
    public string GetPath(EndpointInfo p_endpoint) {
        return string.Format("{0}{1}", m_settings.ServerAddress, p_endpoint.Path);
    }
    
    public WebRequester(MonoBehaviour p_executer, NetworkSettings p_settings)
    {
        m_executer = p_executer;
        m_settings = p_settings;
    }

    public void AskQuestion(string question, Action<string> p_success, Action<HTTPErrorMessage> p_failure)
    {
        string body = $"{{\"question\":\"{question}\"}}";
        IEnumerator request = NetworkRequestAsync(m_settings.Question, body, string.Empty, new Dictionary<string, string>(), p_success, p_failure);
        m_executer.StartCoroutine(request);
    }
    
    public void AskQuestionAudio(string question, Action<byte[]> p_dataSuccess, Action<HTTPErrorMessage> p_failure)
    {
        string body = $"{{\"question\":\"{question}\"}}";
        IEnumerator request = NetworkRequestAsync(m_settings.QuestionAudio, body, string.Empty, new Dictionary<string, string>(), null, p_failure, true, p_dataSuccess);
        m_executer.StartCoroutine(request);
    }
    
    public void LoginAsync(string p_user, string p_passwd, Action<string> p_success, Action<HTTPErrorMessage> p_failure) {
        string body = string.Format("{{\"email\":\"{0}\",\"password\":\"{1}\"}}", p_user, p_passwd);
        //Debug.Log(body);
        IEnumerator request = NetworkRequestAsync(m_settings.Login, body, string.Empty, new Dictionary<string, string>(), p_success, p_failure);
        m_executer.StartCoroutine(request);
    }
    
    public void UploadUserPicture(string p_jwt, byte[] p_data, Action<string> p_success, Action<HTTPErrorMessage> p_failure) {
        Dictionary<string, string> headers = new Dictionary<string, string>()
        {
            {"Authorization", p_jwt}
        };
        IEnumerator request = PictureUpload(p_data, headers, string.Empty, p_success, p_failure);
        m_executer.StartCoroutine(request);
    }
    
    public void UploadActivityPicture(string p_jwt, byte[] p_data, string p_groupPasswd, string p_activityId, Action<string> p_success, Action<HTTPErrorMessage> p_failure)
    {
        Dictionary<string, string> headers = new Dictionary<string, string>()
        {
            {"Authorization", p_jwt}
        };

        string args = string.Format("/{0}_{1}", p_groupPasswd, p_activityId);

        IEnumerator request = PictureUpload(p_data, headers, args, p_success, p_failure);
        m_executer.StartCoroutine(request);
    }
    
    public void DummyLoading(bool p_status, Action<string> p_success, Action<HTTPErrorMessage> p_failure) {
        IEnumerator request = DummyRequest(p_status, p_success, p_failure);
        m_executer.StartCoroutine(request);
    }

    /// <summary>
    /// Stops all running requests.
    /// </summary>
    public void StopAllRequests() {
        m_executer.StopAllCoroutines();
    }

    private IEnumerator DummyRequest(bool p_status, Action<string> p_success, Action<HTTPErrorMessage> p_failure) {
        Debug.LogWarning("Dummy Wait Start");
        yield return new WaitForSeconds(5);
        Debug.LogWarning("Dummy Wait Completed");
        if (p_status) {
            p_success("OK");
        }
        else {
            p_failure(new HTTPErrorMessage(400, "Dummy Failed"));
        }
    }

    private IEnumerator NetworkRequestAsync(
        EndpointInfo p_endpoint, string p_body, string p_args, 
        Dictionary<string, string> p_extraHeaders,
        Action<string> p_success, Action<HTTPErrorMessage> p_failure,
        bool p_pictures = false, Action<byte[]>p_successData = null)
    {
        string networkPath = string.Format("{0}{1}{2}", m_settings.ServerAddress, p_endpoint.Path, p_args);
        Debug.LogWarning(networkPath);
        UnityWebRequest webRequest = new UnityWebRequest(networkPath, p_endpoint.ApiVerb)
        {
            downloadHandler = new DownloadHandlerBuffer()
        };

        foreach (var header in p_extraHeaders) {
            webRequest.SetRequestHeader(header.Key, header.Value);
        }
        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.timeout = m_settings.RequestTimeout;

        if (!string.IsNullOrEmpty(p_body)) {
            byte[] myData = Encoding.UTF8.GetBytes(p_body);
            webRequest.uploadHandler = new UploadHandlerRaw(myData);
        }

        yield return webRequest.SendWebRequest();

        if (webRequest.isNetworkError)
        {
            //Error Callback
            if (!ReferenceEquals(p_failure, null)) {
                HTTPErrorMessage httpError = new HTTPErrorMessage(webRequest.responseCode, webRequest.error);
                p_failure(httpError);
            }
        }
        else
        {
            //Debug.LogWarning(webRequest.responseCode);
            //Debug.LogWarning(webRequest.isHttpError);
            if (webRequest.isHttpError || (webRequest.responseCode != 200 && webRequest.responseCode != 201)) {
                //Error Callback
                if (!ReferenceEquals(p_failure, null))
                {
                    HTTPErrorMessage httpError = new HTTPErrorMessage(webRequest.responseCode, webRequest.error);
                    p_failure(httpError);
                }
            }
            else {
                //Success Callback
                if (!ReferenceEquals(p_success, null))
                {
                    string result = webRequest.downloadHandler.text;
                    if (webRequest.responseCode == 201) {
                        result = webRequest.responseCode.ToString();
                    }
                    p_success(result);
                }
                else if (p_pictures && !ReferenceEquals(p_successData, null)) {
                    //Debug.Log(webRequest.downloadHandler.data);
                    byte[] result = webRequest.downloadHandler.data;
                    p_successData(result);
                }
            }
        }
    }

    private IEnumerator PictureUpload(byte[] p_data, Dictionary<string, string> p_extraHeaders, string p_args, Action<string> p_success, Action<HTTPErrorMessage> p_failure) {
        yield return new WaitForSeconds(1);

        List<IMultipartFormSection> formData = new List<IMultipartFormSection>
        {
            new MultipartFormFileSection("filepond", p_data, "filename.jpg", "image/jpg")
        };

        string networkPath = string.Format("{0}{1}{2}", m_settings.ServerAddress, m_settings.MyPicture.Path, p_args);
        //Debug.LogWarning(networkPath);

        UnityWebRequest webRequest = UnityWebRequest.Post(networkPath, formData);
        webRequest.chunkedTransfer = false;////ADD THIS LINE
        foreach (var header in p_extraHeaders)
        {
            webRequest.SetRequestHeader(header.Key, header.Value);
        }
        webRequest.timeout = m_settings.RequestTimeout;

        yield return webRequest.SendWebRequest();

        if (webRequest.isNetworkError)
        {
            //Error Callback
            if (!ReferenceEquals(p_failure, null))
            {
                HTTPErrorMessage httpError = new HTTPErrorMessage(webRequest.responseCode, webRequest.error);
                p_failure(httpError);
            }
        }
        else
        {
            //Debug.Log(webRequest.responseCode);
            if (webRequest.isHttpError || webRequest.responseCode != 200)
            {
                //Error Callback
                if (!ReferenceEquals(p_failure, null))
                {
                    HTTPErrorMessage httpError = new HTTPErrorMessage(webRequest.responseCode, webRequest.error);
                    p_failure(httpError);
                }
            }
            else
            {
                //Success Callback
                if (!ReferenceEquals(p_success, null))
                {
                    string result = webRequest.downloadHandler.text;
                    p_success(result);
                }
            }
        }
    }
}