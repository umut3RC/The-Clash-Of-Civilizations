using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class LobbyManager : MonoBehaviourPunCallbacks
{
	public TMP_InputField roomNameInput;
	public TMP_InputField joinRoomNameInput;
	public GameObject roomListPrefab;
	public Transform roomListParent;
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
		if (!PhotonNetwork.IsConnectedAndReady)
		{
			menuManager.PrepareLoadStatus();
		}
	}

	public override void OnConnectedToMaster()
	{
		// TypedLobby customLobby = new TypedLobby("default", LobbyType.Default);
		// PhotonNetwork.JoinLobby(customLobby);
		PhotonNetwork.JoinLobby();
		menuManager.PrepareLoadStatus();
	}
	public override void OnJoinedLobby()
	{
		base.OnJoinedLobby();
		Debug.Log("Lobiye girildi: " + PhotonNetwork.CurrentLobby.Name);
		menuManager.StartPanels();
		menuManager.PrepareLoadStatus();
	}

	public override void OnRoomListUpdate(List<RoomInfo> roomList)
	{
		availableRooms = roomList;
		Debug.Log(availableRooms.Count);
		UpdateLobbyList();
	}

	public void UpdateLobbyList()
	{
		Dictionary<string, GameObject> currentPrefabs = new Dictionary<string, GameObject>();
		foreach (Transform child in roomListParent)
		{
			RoomPrefabScript script = child.GetComponent<RoomPrefabScript>();
			if (script != null)
			{
				string roomName = script.GetRoomName();
				if (!currentPrefabs.ContainsKey(roomName))
				{
					currentPrefabs.Add(roomName, child.gameObject);
				}
				else
				{
					Destroy(child.gameObject);
				}
			}
			else
			{
				Destroy(child.gameObject);
			}
		}
		HashSet<string> validRoomNames = new HashSet<string>();

		foreach (RoomInfo room in availableRooms)
		{
			if (room.RemovedFromList || !room.IsVisible || !room.IsOpen)
			{
				continue;
			}
			validRoomNames.Add(room.Name);
			if (!currentPrefabs.ContainsKey(room.Name))
			{
				GameObject newPrefab = Instantiate(roomListPrefab, roomListParent);
				newPrefab.GetComponent<RoomPrefabScript>().SetRoomName(room.Name);
			}
		}
		List<GameObject> prefabsToDestroy = new List<GameObject>();
		foreach (var entry in currentPrefabs)
		{
			string prefabName = entry.Key;
			GameObject prefabObject = entry.Value;

			if (!validRoomNames.Contains(prefabName))
			{
				prefabsToDestroy.Add(prefabObject);
			}
		}

		foreach (GameObject prefab in prefabsToDestroy)
		{
			Destroy(prefab);
		}
	}
	public void CreateRoom()
	{
		if (!string.IsNullOrEmpty(roomNameInput.text) && roomNameInput.text.Length < 10)
		{
			RoomOptions roomOptions = new RoomOptions() { MaxPlayers = 2 };
			PhotonNetwork.CreateRoom(roomNameInput.text, roomOptions, TypedLobby.Default);
			menuManager.ClosePanels();
		}
	}
	public void JoinRoom()
	{
		PhotonNetwork.JoinRoom(joinRoomNameInput.text);
		menuManager.ClosePanels();
	}
	public void JoinRoomWithName(string roomName)
	{
		PhotonNetwork.JoinRoom(roomName);
		menuManager.ClosePanels();
	}
	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		UpdatePlayerList();
	}

	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		UpdatePlayerList();
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
		menuManager.EnterRoom();
	}
}
