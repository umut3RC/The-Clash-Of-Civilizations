using UnityEngine;
using TMPro;


public class WebGLKeyboardFix : MonoBehaviour
{
	private TMP_InputField inputField;

	void Start()
	{
		inputField = GetComponent<TMP_InputField>();

		inputField.onSelect.AddListener(OnInputSelected);
		inputField.onDeselect.AddListener(OnInputDeselected);
	}

	public void OnInputSelected(string text)
	{

#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLInput.captureAllKeyboardInput = false;
#endif
	}

	public void OnInputDeselected(string text)
	{

#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLInput.captureAllKeyboardInput = true;
#endif
	}
}