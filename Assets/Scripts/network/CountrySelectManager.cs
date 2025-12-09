using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using TMPro;

public class CountrySelectManager : MonoBehaviourPunCallbacks
{
	[Header("UI Referansları")]
	public GameObject selectionPanel;
	public TextMeshProUGUI statusText;
	public Button[] countryButtons;
	public GameObject empirePanel;

	private const string PLAYER_READY_PROP = "IsReady";
	private const string PLAYER_COUNTRY_PROP = "Country";

	void Start()
	{
	}

	public void OnCountrySelected(string countryName)
	{
		Hashtable props = new Hashtable
		{
			{ PLAYER_COUNTRY_PROP, countryName },
			{ PLAYER_READY_PROP, true }
		};

		PhotonNetwork.LocalPlayer.SetCustomProperties(props);

		if (statusText != null)
			statusText.text = "Waiting for the opponent...";


		foreach (Button btn in countryButtons)
		{
			btn.interactable = false;
		}
	}

	public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
	{

		if (changedProps.ContainsKey(PLAYER_READY_PROP))
		{
			CheckIfAllPlayersReady();
		}
	}

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		CheckIfAllPlayersReady();
	}

	private void CheckIfAllPlayersReady()
	{

		if (PhotonNetwork.PlayerList.Length < 2)
		{

			return;
		}


		bool allReady = true;
		foreach (Player p in PhotonNetwork.PlayerList)
		{

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

				allReady = false;
				break;
			}
		}

		if (allReady)
		{
			StartGame();
		}
	}

	private void StartGame()
	{
		Debug.Log("Tüm oyuncular hazır! Oyun Başlıyor.");

		if (selectionPanel != null)
		{
			selectionPanel.SetActive(false);
			empirePanel.SetActive(false);
		}
	}
}