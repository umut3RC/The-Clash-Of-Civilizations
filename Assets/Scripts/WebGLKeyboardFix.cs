using UnityEngine;
using TMPro; // TextMeshPro kullanıyorsanız
			 // using UnityEngine.UI; // Eski InputField kullanıyorsanız

public class WebGLKeyboardFix : MonoBehaviour
{
	private TMP_InputField inputField;

	void Start()
	{
		inputField = GetComponent<TMP_InputField>();

		// Input alanına tıklandığında tetiklenecek event'i ekle
		inputField.onSelect.AddListener(OnInputSelected);
		inputField.onDeselect.AddListener(OnInputDeselected);
	}

	// Input alanına tıklandığında
	public void OnInputSelected(string text)
	{
		// Unity'nin klavye yakalamasını kapat, tarayıcıya izin ver
#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLInput.captureAllKeyboardInput = false;
#endif
	}

	// Input alanından çıkıldığında
	public void OnInputDeselected(string text)
	{
		// Tekrar Unity'nin kontrolüne ver (Oyun kontrolleri çalışsın diye)
#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLInput.captureAllKeyboardInput = true;
#endif
	}
}