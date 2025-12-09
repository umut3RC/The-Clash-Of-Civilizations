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
	public GameObject empiresPanel;
	public GameObject reconnectPanel;
	public GameObject startGameButton;
	float loadValue = 3f;
	public Image loadImage;

	void Start()
	{
		if (PhotonNetwork.IsConnectedAndReady || !string.IsNullOrWhiteSpace(PhotonNetwork.NickName))
		{
			EnterMainMenu();
		}
		else
		{
			OpenPanel(loadingPanel);
			PrepareLoadStatus();
		}
	}

	public void PrepareLoadStatus()
	{
		loadImage.fillAmount = (3f - loadValue) / 3;
		loadValue--;
	}

	public void StartPanels()
	{
		if (PhotonNetwork.IsConnectedAndReady && !string.IsNullOrWhiteSpace(PhotonNetwork.NickName))
		{
			EnterMainMenu();
		}
		else
		{
			OpenPanel(userNamePanel);
		}
	}
	public void SetPlayerName()
	{
		if (PhotonNetwork.IsConnected && !string.IsNullOrEmpty(userNameInput.text) && userNameInput.text.Length < 10)
		{
			PhotonNetwork.NickName = userNameInput.text;
			EnterMainMenu();
		}
	}
	public void EnterMainMenu()
	{
		OpenPanel(menuPanel);
	}
	public void OpenReConnectPanel()
	{
		OpenPanel(reconnectPanel);
	}
	public void EnterRoom()
	{
		OpenPanel(roomPanel);
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
		reconnectPanel.SetActive(false);
	}
	public void OpenPanel(GameObject panel)
	{
		userNamePanel.SetActive(false);
		menuPanel.SetActive(false);
		roomPanel.SetActive(false);
		loadingPanel.SetActive(false);
		reconnectPanel.SetActive(false);
		panel.SetActive(true);
	}

}
