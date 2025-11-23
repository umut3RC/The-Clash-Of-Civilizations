using UnityEngine;

public class OrientationManager : MonoBehaviour
{
	[Tooltip("Telefon dik tutulduğunda açılacak uyarı paneli")]
	public GameObject portraitWarningPanel;

	void Update()
	{
		// WebGL'de en güvenli yöntem Aspect Ratio (En-Boy Oranı) kontrolüdür.

		// Eğer Genişlik < Yükseklik ise (Cihaz DİK tutuluyor demektir)
		if (Screen.width < Screen.height)
		{
			// Uyarı panelini göster
			if (!portraitWarningPanel.activeSelf)
			{
				portraitWarningPanel.SetActive(true);
				Time.timeScale = 0f; // Oyunu durdur (İsteğe bağlı)
			}
		}
		// Eğer Genişlik >= Yükseklik ise (Cihaz YAN tutuluyor demektir)
		else
		{
			// Uyarı panelini gizle
			if (portraitWarningPanel.activeSelf)
			{
				portraitWarningPanel.SetActive(false);
				Time.timeScale = 1f; // Oyunu devam ettir
			}
		}
	}
}