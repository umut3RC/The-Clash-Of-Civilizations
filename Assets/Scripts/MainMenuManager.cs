using Photon.Pun;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
	public LobbyManager lobyManager;
	public GameObject userNamePanel;
	public GameObject loadingPanel;
	public TMP_InputField userNameInput;
	public GameObject menuPanel;
	public GameObject roomPanel;
	public GameObject startGameButton;
	float loadValue = 3f;
	public Image loadImage;

	void Start()
	{
		ClosePanels();
		loadingPanel.SetActive(true);
		PrepareLoadStatus();
	}

	public void PrepareLoadStatus()
	{
		// loadingPanel.SetActive(true);
		loadImage.fillAmount = (3f - loadValue) / 3;
		loadValue--;
	}

	public void StartPanels()
	{
		userNamePanel.SetActive(true);
		menuPanel.SetActive(false);
		roomPanel.SetActive(false);
		loadingPanel.SetActive(false);
	}
	public void EnterMainMenu()
	{
		if (PhotonNetwork.IsConnected && !string.IsNullOrEmpty(userNameInput.text) && userNameInput.text.Length < 10)
		{
			PhotonNetwork.NickName = userNameInput.text;
			userNamePanel.SetActive(false);
			menuPanel.SetActive(true);
		}
	}
	public void EnterRoom()
	{
		userNamePanel.SetActive(false);
		menuPanel.SetActive(false);
		roomPanel.SetActive(true);
		lobyManager.UpdatePlayerList();
	}

	public void StartGame()
	{
		if (PhotonNetwork.IsMasterClient)
		{
			PhotonNetwork.CurrentRoom.IsOpen = false;
			PhotonNetwork.CurrentRoom.IsVisible = false;

			PhotonNetwork.LoadLevel("GameScene");
		}
	}
	public void CheckStartGameButton()
	{
		if (PhotonNetwork.PlayerList.Length > 1 && PhotonNetwork.IsMasterClient)
			startGameButton.GetComponent<Button>().interactable = true;
	}
	public void ClosePanels()
	{
		userNamePanel.SetActive(false);
		menuPanel.SetActive(false);
		roomPanel.SetActive(false);
		loadingPanel.SetActive(false);
	}

}
