using Photon.Pun;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
	public LobbyManager lobyManager;
	public GameObject userNamePanel;
	public TMP_InputField userNameInput;
	public GameObject menuPanel;
	public GameObject roomPanel;
	public GameObject startGameButton;

	void Start()
	{
		userNamePanel.SetActive(false);
		menuPanel.SetActive(false);
		roomPanel.SetActive(false);
	}
	public void StartPanels()
	{
		userNamePanel.SetActive(true);
		menuPanel.SetActive(false);
		roomPanel.SetActive(false);
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
		if (PhotonNetwork.PlayerList.Length > 0 && PhotonNetwork.IsMasterClient)
			startGameButton.GetComponent<Button>().interactable = true;
	}


}
