using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
public class PlayerScript : MonoBehaviourPunCallbacks
{
	public GameObject spawnTargetVisulazer;
	public LayerMask raycastLayer;
	public GameObject testArmyPrefab;
	public GameObject myCanvas;
	public GameObject myCamera;
	public GameObject myCastle;
	[SerializeField] int totalCoins = 0;
	[SerializeField] GameObject totalCoinText;

	private float coinTimer = 0f;

	bool selectedSpawn = false;
	string lastTargetArmyName = "";
	GameObject lastTargetArmy;
	bool isReady = false;
	string armyLayer = "Army1";

	void Start()
	{
		SetCoin(100);
		spawnTargetVisulazer.SetActive(false);
		if (PhotonNetwork.IsConnectedAndReady)
		{
			PrepareMine();
		}
	}

	void Update()
	{
		coinTimer += Time.deltaTime;

		if (coinTimer >= 1f)
		{
			IncreaseCoin();
			coinTimer = 0f;
		}

		if (isReady && selectedSpawn)
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out RaycastHit hit, 100f, raycastLayer))
			{
				spawnTargetVisulazer.transform.position = hit.point;
				if (lastTargetArmy != null)
				{
					lastTargetArmy.transform.position = spawnTargetVisulazer.transform.position;
				}
			}
			if (Input.GetMouseButtonDown(0))
			{
				SpawnArmy();
			}
		}
	}

	public void IncreaseCoin()
	{
		totalCoins++;
		totalCoinText.GetComponent<TextMeshProUGUI>().text = totalCoins.ToString();
	}

	private void SetCoin(int c)
	{
		totalCoins = c;
		totalCoinText.GetComponent<TextMeshProUGUI>().text = totalCoins.ToString();
	}
	public void SelectArmy(string targetArmy)
	{
		lastTargetArmyName = targetArmy;
		selectedSpawn = true;
		spawnTargetVisulazer.SetActive(true);
		if (selectedSpawn && lastTargetArmyName != null)
		{
			// lastTargetArmy = Instantiate(testArmyPrefab);
			lastTargetArmy = PhotonNetwork.Instantiate("HeavySwordman", Vector3.zero, Quaternion.identity);
			lastTargetArmy.layer = LayerMask.NameToLayer(armyLayer);

			int _amount = lastTargetArmy.GetComponent<HeavySwordmanScript>().GetAmount();
			if (_amount >= totalCoins)
			{
				PhotonNetwork.Destroy(lastTargetArmy);
				selectedSpawn = false;
				spawnTargetVisulazer.SetActive(false);
			}
		}
	}

	private void SpawnArmy()
	{
		selectedSpawn = false;
		spawnTargetVisulazer.SetActive(false);
		lastTargetArmy = null;
	}
	private void PrepareMine()
	{
		if (photonView.IsMine)
		{
			myCamera.SetActive(true);
			myCanvas.SetActive(true);
		}
		if (!PhotonNetwork.IsMasterClient)
		{
			transform.rotation *= Quaternion.Euler(0, 180f, 0);
			transform.localPosition = new Vector3(28, 0, 0);

			raycastLayer = LayerMask.GetMask("Ground 2");
			armyLayer = "Army2";
		}
		isReady = true;
	}
}
