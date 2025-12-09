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
	public GameObject mainPanel;
	public GameObject pausePanel;
	public TextMeshProUGUI pauseTimerText;

	public GameObject winnerPanel;
	public TextMeshProUGUI winnerTimer;
	public GameObject losePanel;
	public TextMeshProUGUI loseTimer;

	public GameObject selectEmpirePanel;

	private bool isGamePaused = false;
	private bool isFinish = false;
	private float pauseDuration = 20f;
	private float pauseTimer = 0f;

	void Start()
	{
		if (PhotonNetwork.IsConnectedAndReady)
		{
			SpawnAndSetParent();
			OpenPanel(selectEmpirePanel);
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
	[PunRPC]
	void RPC_PauseGameForAll()
	{
		if (!isGamePaused)
		{
			isGamePaused = true;
			pauseTimer = pauseDuration;
			Time.timeScale = 0f;
			OpenPanel(pausePanel);
		}
	}

	void ResumeGame()
	{
		isGamePaused = false;
		Time.timeScale = 1f;
	}

	IEnumerator ReturnToMenu()
	{
		PhotonNetwork.LeaveRoom();
		yield return null;
	}

	void SpawnAndSetParent()
	{
		if (spawnPoint == null)
		{
			Debug.LogError("spawnPoint atanmamış!");
			return;
		}
		GameObject playerObj = PhotonNetwork.Instantiate(prefabName, spawnPoint.position, Quaternion.identity);
		PhotonView pv = playerObj.GetComponent<PhotonView>();

		if (pv.IsMine)
		{
			PlayerScript ps = playerObj.GetComponent<PlayerScript>();
			ps.enabled = true;
			ps.SetGameManager(this); ;
		}
		else
		{
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

	public IEnumerator WinGame()
	{
		OpenPanel(winnerPanel);
		isFinish = true;
		int duration = 5;

		while (duration > 0f)
		{
			winnerTimer.text = duration.ToString();
			duration--;
			yield return new WaitForSeconds(1f);
		}
		winnerTimer.text = "0";
		StartCoroutine(ReturnToMenu());
		winnerPanel.SetActive(false);
		yield return null;
	}
	public IEnumerator LoseGame()
	{
		OpenPanel(losePanel);
		isFinish = true;
		int duration = 5;

		while (duration > 0f)
		{
			loseTimer.text = duration.ToString();
			duration--;
			yield return new WaitForSeconds(1f);
		}
		loseTimer.text = "0";
		StartCoroutine(ReturnToMenu());
		losePanel.SetActive(false);
		yield return null;
	}


	public override void OnLeftRoom()
	{
		SceneManager.LoadScene("MainMenu");
	}

	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		Debug.Log($"Oyuncu ayrıldı: {otherPlayer.NickName}");

		if (isFinish)
			return;
		isFinish = true;
		if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
		{
			Player winner = PhotonNetwork.LocalPlayer;
			Debug.Log($"Kazanan oyuncu: {winner.NickName}");

			StartCoroutine(WinGame());
		}
	}

	public void OpenPanel(GameObject targetPanel)
	{
		mainPanel.SetActive(true);
		pausePanel.SetActive(false);
		winnerPanel.SetActive(false);
		losePanel.SetActive(false);
		selectEmpirePanel.SetActive(false);

		targetPanel.SetActive(true);
	}
}

