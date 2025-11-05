using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class RoomPrefabScript : MonoBehaviour
{
	public TextMeshProUGUI roomNameText;
	LobbyManager lobbyManager;

	void Start()
	{
		if (lobbyManager == null)
		{
			lobbyManager = GameObject.Find("MenuManager").GetComponent<LobbyManager>();
		}
	}

	public void JoinRoom()
	{
		if (lobbyManager != null)
		{
			lobbyManager.JoinRoomWithName(roomNameText.text);
		}
	}

	public string GetRoomName()
	{
		return roomNameText.text;
	}
	public void SetRoomName(string name)
	{
		roomNameText.text = name;
	}
}
