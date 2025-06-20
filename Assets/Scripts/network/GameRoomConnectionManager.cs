using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class GameRoomConnectionManager : MonoBehaviourPunCallbacks
{
	public Transform spawnPoint;
	public string prefabName = "player room manager prefab";
	public Transform[] clientBuildings;
	public Transform[] otherBuildings;
	public GameObject pausePanel;
	public TextMeshProUGUI pauseTimerText;
	private bool isGamePaused = false;
	private float pauseDuration = 20f;
	private float pauseTimer = 0f;

	void Start()
	{
		if (PhotonNetwork.IsConnectedAndReady)
		{
			SpawnAndSetParent();
		}
	}
	void Awake()
	{
		if (spawnPoint == null)
		{
			GameObject found = GameObject.Find("MAP");
			if (found != null) spawnPoint = found.transform;
		}
	}

	void Update()
	{
		// if (!photonView.IsMine)
		// {
		// 	return;
		// }
		if (isGamePaused)
		{
			// Time.timeScale 0 olduğunda Time.deltaTime da 0 olacağı için Unscaled kullan
			pauseTimer -= Time.unscaledDeltaTime;
			pauseTimerText.text = pauseTimer.ToString();
			if (pauseTimer <= 0f)
			{
				ResumeGame();
			}
		}
	}

	// void OnApplicationFocus(bool hasFocus)
	// {
	// 	if (!hasFocus && PhotonNetwork.IsConnected && photonView.IsMine && !isGamePaused)
	// 	{
	// 		// Odağı kaybetti, herkese bildir
	// 		photonView.RPC("RPC_PauseGameForAll", RpcTarget.AllBuffered);
	// 	}
	// }
	[PunRPC]
	void RPC_PauseGameForAll()
	{
		if (!isGamePaused)
		{
			isGamePaused = true;
			pauseTimer = pauseDuration;
			Time.timeScale = 0f;
			pausePanel.SetActive(true);
		}
	}

	void ResumeGame()
	{
		isGamePaused = false;
		Time.timeScale = 1f;
		pausePanel.SetActive(true);
	}


	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		Debug.Log($"Oyuncu ayrıldı: {otherPlayer.NickName}");

		// Kalan oyuncuyu bul
		if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
		{
			Player winner = PhotonNetwork.LocalPlayer;
			Debug.Log($"Kazanan oyuncu: {winner.NickName}");

			StartCoroutine(ReturnToMenu());
		}
	}

	IEnumerator ReturnToMenu()
	{
		yield return new WaitForSeconds(5f);
		// "MainMenu" sahnesine veya index 0 olan sahneye dön
		SceneManager.LoadScene("MainMenu");
		// Alternatif: SceneManager.LoadScene(0);
	}

	void SpawnAndSetParent()
	{
		if (spawnPoint == null)
		{
			Debug.LogError("spawnPoint atanmamış!");
			return;
		}
		GameObject playerObj = PhotonNetwork.Instantiate(prefabName, spawnPoint.position, Quaternion.identity);

		// PhotonView üzerinden sahiplik kontrolü yap
		PhotonView pv = playerObj.GetComponent<PhotonView>();

		if (pv.IsMine)
		{
			PlayerScript ps = playerObj.GetComponent<PlayerScript>();
			ps.enabled = true;
			ps.SetGameManager(this);
			// playerObj.GetComponent<PlayerScript>().SetGameManager(this);
		}
		else
		{
			// İstersen diğer oyuncuların PlayerScript'ini tamamen kapat
			playerObj.GetComponent<PlayerScript>().enabled = false;
		}


		photonView.RPC("SetParentForObject", RpcTarget.AllBuffered, playerObj.GetComponent<PhotonView>().ViewID);
	}


	[PunRPC]
	void SetParentForObject(int viewID)
	{
		PhotonView targetView = PhotonView.Find(viewID);
		if (targetView != null)
		{
			targetView.transform.SetParent(spawnPoint);
		}
	}
	public Transform[] GetEnemyBuildings(bool isClient)
	{
		if (isClient)
		{
			return (otherBuildings);
		}
		else
		{
			return (clientBuildings);
		}
	}
	[PunRPC]
	public void RegisterPlayerBuildings(int viewID)
	{
		PhotonView view = PhotonView.Find(viewID);
		if (view == null) return;

		PlayerScript player = view.GetComponent<PlayerScript>();
		if (player == null) return;

		if (view.IsMine)
		{
			clientBuildings = player.GetMyBuildings();
		}
		else
		{
			otherBuildings = player.GetMyBuildings();
			Debug.Log("Other here");
		}
	}
}

