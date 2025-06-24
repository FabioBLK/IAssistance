using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;
using KKSpeech;

namespace KKSpeech {
	public class SpeechRecognitionLanguageDropdown : MonoBehaviour {

		private Dropdown dropdown;
		private List<LanguageOption> languageOptions;

		void Start () {
			Debug.LogWarning("------ Starting Script ------");
			dropdown = GetComponent<Dropdown>();
			dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
			dropdown.ClearOptions();

			var speech = GameObject.FindObjectOfType<SpeechRecognizerListener>();
			Debug.LogWarning($"------ Find Speech = {speech.name} ------");
			speech.onSupportedLanguagesFetched.AddListener(OnSupportedLanguagesFetched);

			Debug.LogWarning($"------ Get Supported Languages ------");
			SpeechRecognizer.GetSupportedLanguages();
		}

		// remember to add ExampleScene to Build Settings!
		public void GoToRecordingScene() {
			SceneManager.LoadScene("ExampleScene");
		}

		void OnDropdownValueChanged(int index) {
			LanguageOption languageOption = languageOptions[index];

			SpeechRecognizer.SetDetectionLanguage(languageOption.id);
		}

		void OnSupportedLanguagesFetched(List<LanguageOption> languages) {
			Debug.LogWarning($"------ FETCHED with languages {languages.Count} ------");
			List<Dropdown.OptionData> dropdownOptions = new List<Dropdown.OptionData>();

			foreach (LanguageOption langOption in languages) {
				Debug.LogWarning($"------ adding language {langOption.displayName} ------");
				dropdownOptions.Add(new Dropdown.OptionData(langOption.displayName));
			}

			dropdown.AddOptions(dropdownOptions);

			languageOptions = languages;
		} 

	}
}

