using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using TMPro;

public class CountrySelectManager : MonoBehaviourPunCallbacks
{
	[Header("UI Referansları")]
	public GameObject selectionPanel; // Kapatılacak olan Ülke Seçim Paneli
	public TextMeshProUGUI statusText;           // "Rakip Bekleniyor" yazacak text (Opsiyonel)
	public Button[] countryButtons;   // Tıklandığında kilitlenecek butonlar
	public GameObject empirePanel;

	// Oyuncunun hazır olup olmadığını takip eden anahtar kelime
	private const string PLAYER_READY_PROP = "IsReady";
	private const string PLAYER_COUNTRY_PROP = "Country";

	void Start()
	{
		// Panel açık başladığında durum metnini sıfırla
		// if (statusText != null) statusText.text = "Lütfen bir ülke seçiniz.";
	}

	// --- UI BUTONLARINDAN ÇAĞRILACAK FONKSİYON ---
	// Butonların OnClick olayına bunu bağlayın ve parametre olarak ülke adı (örn: "Rome") gönderin.
	public void OnCountrySelected(string countryName)
	{
		// 1. Oyuncunun seçim yaptığını Photon'a bildir (Custom Properties)
		Hashtable props = new Hashtable
		{
			{ PLAYER_COUNTRY_PROP, countryName },
			{ PLAYER_READY_PROP, true } // Oyuncu artık hazır
        };

		PhotonNetwork.LocalPlayer.SetCustomProperties(props);

		// 2. UI Güncellemesi (Oyuncuya geri bildirim ver)
		if (statusText != null)
			statusText.text = "Waiting for the opponent...";

		// 3. Oyuncu tekrar seçim yapamasın diye butonları kilitle
		foreach (Button btn in countryButtons)
		{
			btn.interactable = false;
		}

		// Not: Paneli BURADA KAPATMIYORUZ. Aşağıdaki callback kapatacak.
	}

	// --- PHOTON CALLBACKLERİ ---

	// Bir oyuncunun özelliği değiştiğinde Photon bunu otomatik çağırır.
	public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
	{
		// Eğer değişen özellik "IsReady" ise kontrol et
		if (changedProps.ContainsKey(PLAYER_READY_PROP))
		{
			CheckIfAllPlayersReady();
		}
	}

	// Odaya yeni biri girdiğinde veya çıktığında da kontrol etmek iyi olur
	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		CheckIfAllPlayersReady();
	}

	// --- KONTROL MEKANİZMASI ---

	private void CheckIfAllPlayersReady()
	{
		// 1. Odada 2 kişi var mı? (Veya oyununuz kaç kişilikse)
		// Eğer test için tek kişi giriyorsanız bu satırı '>= 1' yapabilirsiniz.
		if (PhotonNetwork.PlayerList.Length < 2)
		{
			// if (statusText != null && PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(PLAYER_READY_PROP))
			// statusText.text = "Diğer oyuncunun odaya girmesi bekleniyor...";
			return;
		}

		// 2. Odadaki TÜM oyuncuları gez
		bool allReady = true;
		foreach (Player p in PhotonNetwork.PlayerList)
		{
			// Oyuncunun özelliklerinde "IsReady" var mı ve true mu?
			object isReady;
			if (p.CustomProperties.TryGetValue(PLAYER_READY_PROP, out isReady))
			{
				if (!(bool)isReady)
				{
					allReady = false;
					break;
				}
			}
			else
			{
				// Oyuncu henüz özellik bile atamamış (seçim yapmamış)
				allReady = false;
				break;
			}
		}

		// 3. Eğer HERKES hazırsa oyunu başlat
		if (allReady)
		{
			StartGame();
		}
	}

	private void StartGame()
	{
		Debug.Log("Tüm oyuncular hazır! Oyun Başlıyor.");

		// Paneli kapat
		if (selectionPanel != null)
		{
			selectionPanel.SetActive(false);
			empirePanel.SetActive(false);
		}

		// (İsteğe bağlı) Eğer burada başka başlatma kodlarınız varsa çağırabilirsiniz.
		// Örneğin: GameManager.Instance.StartTimer();
	}
}