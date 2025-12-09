using UnityEngine;

public class OrientationManager : MonoBehaviour
{
	[Tooltip("Telefon dik tutulduğunda açılacak uyarı paneli")]
	public GameObject portraitWarningPanel;

	private const float minAspectRatio = 0.85f;

	void Update()
	{
		CheckOrientation();
	}

	void CheckOrientation()
	{

		float aspectRatio = (float)Screen.width / (float)Screen.height;

		bool shouldShowWarning = false;

		if (Application.isMobilePlatform)
		{
			if (aspectRatio < 1.0f)
			{
				shouldShowWarning = true;
			}
		}
		else
		{
			if (aspectRatio < minAspectRatio)
			{
				shouldShowWarning = true;
			}
		}

		if (shouldShowWarning)
		{
			if (!portraitWarningPanel.activeSelf)
			{
				portraitWarningPanel.SetActive(true);
				Time.timeScale = 0f;
			}
		}
		else
		{
			if (portraitWarningPanel.activeSelf)
			{
				portraitWarningPanel.SetActive(false);
				Time.timeScale = 1f;
			}
		}
	}
}