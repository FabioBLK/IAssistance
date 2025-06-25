using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using KKSpeech;

public class RecordingCanvas : MonoBehaviour
{
  [SerializeField] private NetworkSettings networkSettings;
  [SerializeField] private Button startRecordingButton;
  [SerializeField] private Text resultText;
  [SerializeField] private Text apiText;

  private WebRequester _webRequester;

  void Start()
  {
    _webRequester = new WebRequester(this, networkSettings);
    
    if (SpeechRecognizer.ExistsOnDevice())
    {
      SpeechRecognizerListener listener = GameObject.FindObjectOfType<SpeechRecognizerListener>();
      listener.onAuthorizationStatusFetched.AddListener(OnAuthorizationStatusFetched);
      listener.onAvailabilityChanged.AddListener(OnAvailabilityChange);
      listener.onErrorDuringRecording.AddListener(OnError);
      listener.onErrorOnStartRecording.AddListener(OnError);
      listener.onFinalResults.AddListener(OnFinalResult);
      listener.onPartialResults.AddListener(OnPartialResult);
      listener.onEndOfSpeech.AddListener(OnEndOfSpeech);
      SpeechRecognizer.RequestAccess();
    }
    else
    {
      resultText.text = "Sorry, but this device doesn't support speech recognition";
      //startRecordingButton.enabled = false;
    }
  }

  public void OnFinalResult(string result)
  {
    startRecordingButton.GetComponentInChildren<Text>().text = "Start Recording";
    resultText.text = result;
    startRecordingButton.enabled = true;

    Debug.LogWarning($"-----QUESTION IS {result}");
    SendQuestionToApi(result);
  }

  private void SendQuestionToApi(string question)
  {
    _webRequester.AskQuestion(question, OnAskQuestionSuccess, OnAskQuestionFailure);
  }

  private void OnAskQuestionSuccess(string obj)
  {
    Debug.LogWarning($"Success getting answer from Api - {obj}");
    AiResponse response = JsonUtility.FromJson<AiResponse>(obj);
    apiText.text = response.response;
  }

  private void OnAskQuestionFailure(HTTPErrorMessage obj)
  {
    Debug.LogError($"Failed to get question from the api {obj.errorCode} - {obj.message}");
  }

  public void OnPartialResult(string result)
  {
    resultText.text = result;
  }

  public void OnAvailabilityChange(bool available)
  {
    startRecordingButton.enabled = available;
    if (!available)
    {
      resultText.text = "Speech Recognition not available";
    }
    else
    {
      resultText.text = "Faça sua pergunta! :-)";
    }
  }

  public void OnAuthorizationStatusFetched(AuthorizationStatus status)
  {
    switch (status)
    {
      case AuthorizationStatus.Authorized:
        startRecordingButton.enabled = true;
        break;
      default:
        startRecordingButton.enabled = false;
        resultText.text = "Cannot use Speech Recognition, authorization status is " + status;
        break;
    }
  }

  public void OnEndOfSpeech()
  {
    startRecordingButton.GetComponentInChildren<Text>().text = "";
  }

  public void OnError(string error)
  {
    Debug.LogError(error);
    startRecordingButton.GetComponentInChildren<Text>().text = "";
    startRecordingButton.enabled = true;
  }

  public void OnStartRecordingPressed()
  {
    apiText.text = string.Empty;
    
  #if UNITY_EDITOR
    SendQuestionToApi(resultText.text);
    return;
  #endif
    
    if (SpeechRecognizer.IsRecording())
    {
#if UNITY_IOS && !UNITY_EDITOR
			SpeechRecognizer.StopIfRecording();
			startRecordingButton.GetComponentInChildren<Text>().text = "Stopping";
			startRecordingButton.enabled = false;
#elif UNITY_ANDROID && !UNITY_EDITOR
			SpeechRecognizer.StopIfRecording();
			startRecordingButton.GetComponentInChildren<Text>().text = "Start Recording";
#endif
    }
    else
    {
      SpeechRecognizer.StartRecording(true);
      startRecordingButton.GetComponentInChildren<Text>().text = "Parar gravação";
      resultText.text = "Faça sua pergunta! :-)";
    }
  }
}
