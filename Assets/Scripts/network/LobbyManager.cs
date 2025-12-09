using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class LobbyManager : MonoBehaviourPunCallbacks
{
	public TMP_InputField roomNameInput;
	public TMP_InputField joinRoomNameInput;
	public GameObject roomListPrefab;
	public Transform roomListParent;
	public MainMenuManager menuManager;
	public TextMeshProUGUI playerListText;
	private List<RoomInfo> availableRooms = new List<RoomInfo>();
	public TextMeshProUGUI reconnectTimerText;
	public TextMeshProUGUI playerCountText;

	private float retryInterval = 5f;
	private bool isRetrying = false;

	void Awake()
	{
		PhotonNetwork.AutomaticallySyncScene = true;
	}
	void Start()
	{
		PhotonNetwork.ConnectUsingSettings();
		PhotonNetwork.KeepAliveInBackground = 60000f;
		if (!PhotonNetwork.IsConnectedAndReady)
		{
			menuManager.PrepareLoadStatus();
		}
	}
	void Update()
	{
		if (PhotonNetwork.IsConnectedAndReady)
		{
			int playersInRegion = PhotonNetwork.CountOfPlayers;
			playerCountText.text = $"{playersInRegion} / 20";
		}
		else
		{
			playerCountText.text = "";
		}
	}

	public override void OnConnectedToMaster()
	{
		PhotonNetwork.JoinLobby();
		menuManager.PrepareLoadStatus();
		isRetrying = false;
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
			PhotonNetwork.JoinOrCreateRoom(roomNameInput.text, roomOptions, TypedLobby.Default);
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
	public void ExitRoom()
	{
		PhotonNetwork.LeaveRoom();
	}
	public override void OnLeftRoom()
	{
		Debug.Log("Odadan çıkıldı");
		menuManager.EnterMainMenu();
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

	public override void OnDisconnected(DisconnectCause cause)
	{
		Debug.LogError($"Bağlantı koptu. Neden: {cause}");
		isRetrying = false;

		if (cause == DisconnectCause.MaxCcuReached)
		{
			Debug.LogError("SUNUCU DOLU! (Maksimum 20 CCU Limitine Ulaşıldı)");
			StartCoroutine(RetryConnectionCoroutine());
		}
		else if (cause != DisconnectCause.DisconnectByClientLogic)
		{
			StartCoroutine(RetryConnectionCoroutine());
		}
	}

	private IEnumerator RetryConnectionCoroutine()
	{
		if (isRetrying) yield break;
		menuManager.OpenReConnectPanel();
		isRetrying = true;
		float timer = retryInterval;
		while (isRetrying && !PhotonNetwork.IsConnected)
		{
			while (timer > 0)
			{
				timer -= Time.deltaTime;
				reconnectTimerText.text = ((int)timer).ToString();
				yield return null;
			}

			reconnectTimerText.text = "*connecting*";
			PhotonNetwork.ConnectUsingSettings();
			timer = retryInterval;
			yield return new WaitForSeconds(3f);
		}

		isRetrying = false;
	}
}
