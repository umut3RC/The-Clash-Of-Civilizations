using UnityEngine;

public class OrientationManager : MonoBehaviour
{
	[Tooltip("Telefon dik tutulduğunda açılacak uyarı paneli")]
	public GameObject portraitWarningPanel;

	// Telefonlar için kritik oran. 1.0 karedir. 
	// 0.85 altı, genellikle dik tutulan telefon/tablet anlamına gelir.
	// Bu sayede kare monitörler (1.0) veya PC'de hafif daraltılmış pencereler takılmaz.
	private const float minAspectRatio = 0.85f;

	void Update()
	{
		CheckOrientation();
	}

	void CheckOrientation()
	{
		// En-Boy oranını hesapla (Genişlik / Yükseklik)
		float aspectRatio = (float)Screen.width / (float)Screen.height;

		bool shouldShowWarning = false;

		// --- MOBİL İÇİN KATI KURAL ---
		// Eğer cihaz gerçekten mobilse, kesinlikle yatay olmalı.
		// WebGL'de Application.isMobilePlatform tarayıcı "User Agent"ına bakar.
		if (Application.isMobilePlatform)
		{
			// Mobilde biraz daha katı olabiliriz, genişlik yükseklikten küçükse uyar.
			// Ama kareye yakın tabletler için yine de biraz esneklik (0.95) bırakabiliriz.
			if (aspectRatio < 1.0f)
			{
				shouldShowWarning = true;
			}
		}
		// --- PC İÇİN ESNEK KURAL ---
		else
		{
			// PC'de kare monitör (1.0) veya hafif dikey pencereler sorun olmamalı.
			// Sadece ekran "aşırı" derecede dikeyse (telefon gibi görünüyorsa) uyar.
			// 0.85 oranı; iPad'in dik halinden (0.75) bile daha geniştir, güvenli bir sınırdır.
			if (aspectRatio < minAspectRatio)
			{
				shouldShowWarning = true;
			}
		}

		// --- PANELİ YÖNET ---
		if (shouldShowWarning)
		{
			if (!portraitWarningPanel.activeSelf)
			{
				portraitWarningPanel.SetActive(true);
				Time.timeScale = 0f; // Oyunu durdur
			}
		}
		else
		{
			if (portraitWarningPanel.activeSelf)
			{
				portraitWarningPanel.SetActive(false);
				Time.timeScale = 1f; // Oyunu devam ettir
			}
		}
	}
}