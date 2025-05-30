using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviourPunCallbacks
{
	public TMP_InputField roomNameInput;
	public TMP_InputField joinRoomNameInput;
	public TextMeshProUGUI lobbyListText;
	public MainMenuManager menuManager;
	public TextMeshProUGUI playerListText;
	private List<RoomInfo> availableRooms = new List<RoomInfo>();

	void Awake()
	{
		PhotonNetwork.AutomaticallySyncScene = true;
	}
	void Start()
	{
		PhotonNetwork.ConnectUsingSettings();
	}

	public override void OnConnectedToMaster()
	{
		PhotonNetwork.JoinLobby();
	}
	public override void OnJoinedLobby()
	{
		base.OnJoinedLobby();
		menuManager.StartPanels();
	}

	public override void OnRoomListUpdate(List<RoomInfo> roomList)
	{
		availableRooms = roomList;
		UpdateLobbyList();
	}

	void UpdateLobbyList()
	{
		lobbyListText.text = "";
		foreach (var room in availableRooms)
		{
			lobbyListText.text += $"{room.Name} - {room.PlayerCount}/{room.MaxPlayers}\n";
		}
	}
	public void CreateRoom()
	{
		if (!string.IsNullOrEmpty(roomNameInput.text) && roomNameInput.text.Length < 10)
		{
			RoomOptions roomOptions = new RoomOptions() { MaxPlayers = 2 };
			PhotonNetwork.CreateRoom(roomNameInput.text, roomOptions, TypedLobby.Default);
			menuManager.EnterRoom();
		}
	}
	public void JoinRoom()
	{
		PhotonNetwork.JoinRoom(joinRoomNameInput.text);
		menuManager.EnterRoom();
	}
	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		UpdatePlayerList(); // Odaya biri katıldığında liste güncellenir
	}

	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		UpdatePlayerList(); // Odayı biri terk ederse de liste güncellenir
	}
	public void UpdatePlayerList()
	{
		playerListText.text = "";
		foreach (Player player in PhotonNetwork.PlayerList)
		{
			playerListText.text += player.NickName + "\n";
		}
		menuManager.CheckStartGameButton();
	}
	public override void OnJoinedRoom()
	{
		UpdatePlayerList();
	}
}
