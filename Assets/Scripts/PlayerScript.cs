using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UIElements;
public class PlayerScript : MonoBehaviourPunCallbacks
{
	public GameObject spawnTargetVisulazer;
	public LayerMask raycastLayer;
	public GameObject testArmyPrefab;
	public GameObject myCanvas;
	public GameObject myCamera;
	GameObject myVillageCamera;
	public GameObject myVillageCamera_origin;
	public GameObject myVillageCamera_other;
	public GameObject villageGround;
	public GameObject[] battleStuff;
	public GameObject[] villageStuff;
	public TextMeshProUGUI[] panelInfoTexts;//username hp coin
	public Transform[] myBuildings;
	public Transform[] enemyBuildings;
	public GameObject[] myVillage;
	[SerializeField] private int totalCoins = 0;
	private float coinTimer = 0f;
	bool selectedSpawn = false;
	string lastTargetArmyName = "";
	GameObject lastTargetArmy;
	bool isReady = false;
	string armyLayer = "Army1";
	GameRoomConnectionManager gameManager;
	int health;

	void Start()
	{
		if (!photonView.IsMine)
		{
			return;
		}
		SetCoin(100);
		SetHealth(100);
		panelInfoTexts[0].text = photonView.name;
		if (!PhotonNetwork.IsMasterClient)
		{
			myVillageCamera = myVillageCamera_other;
			villageGround.transform.position += new Vector3(54, 0, 0);
		}
		else
		{
			myVillageCamera = myVillageCamera_origin;
		}
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
		panelInfoTexts[2].text = totalCoins.ToString();
	}
	public void DecreaseCoin(int _amount)
	{
		totalCoins -= _amount;
		panelInfoTexts[2].text = totalCoins.ToString();
	}

	private void SetCoin(int c)
	{
		totalCoins = c;
		panelInfoTexts[2].text = totalCoins.ToString();
	}
	private void SetHealth(int c)
	{
		health = c;
		panelInfoTexts[1].text = totalCoins.ToString();
	}
	public void SelectArmy(string targetArmy)
	{
		lastTargetArmyName = targetArmy;
		selectedSpawn = true;
		spawnTargetVisulazer.SetActive(true);
		if (selectedSpawn && lastTargetArmyName != null)
		{
			lastTargetArmy = PhotonNetwork.Instantiate(targetArmy, Vector3.zero, Quaternion.identity);

			int _amount = lastTargetArmy.GetComponent<ArmyScript>().GetAmount();
			if (_amount >= totalCoins)
			{
				PhotonNetwork.Destroy(lastTargetArmy);
				selectedSpawn = false;
				spawnTargetVisulazer.SetActive(false);
			}
			else
			{
				ArmyScript _target = lastTargetArmy.GetComponent<ArmyScript>();

				lastTargetArmy.GetComponent<PhotonView>().RPC("RPC_SetLayerAndTag", RpcTarget.AllBuffered, armyLayer);

				_target.SetEnemyTag(armyLayer == "Army1" ? "Army2" : "Army1");
				// _target.SetEnemyBuildings(gameManager.GetEnemyBuildings(photonView.IsMine));
				DecreaseCoin(_amount);
			}
		}
	}

	private void SpawnArmy()
	{
		selectedSpawn = false;
		spawnTargetVisulazer.SetActive(false);

		ArmyScript armyScript = lastTargetArmy.GetComponent<ArmyScript>();

		armyScript.GetComponent<PhotonView>().RPC("RPC_StartArmy", RpcTarget.AllBuffered);
		// armyScript.SetEnemyBuildings(enemyBuildings);

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
	public Transform[] GetMyBuildings()
	{
		return (myBuildings);
	}
	[PunRPC]
	public void RPC_RequestEnemyBuildings(int armyViewID)
	{
		PhotonView armyView = PhotonView.Find(armyViewID);
		if (armyView != null)
		{
			Transform[] buildings = GetMyBuildings();
			int[] buildingIDs = new int[buildings.Length];
			for (int i = 0; i < buildings.Length; i++)
			{
				PhotonView buildingView = buildings[i].GetComponent<PhotonView>();
				buildingIDs[i] = buildingView.ViewID;
			}

			armyView.RPC("RPC_SetEnemyBuildings", armyView.Owner, buildingIDs);
		}
	}

	public void SetGameManager(GameRoomConnectionManager manager)
	{
		gameManager = manager;

		// ViewID ile buildings'i kayıt ettir
		photonView.ViewID.ToString(); // sadece tetikleyici
		gameManager.photonView.RPC("RegisterPlayerBuildings", RpcTarget.AllBuffered, photonView.ViewID);

		// enemyBuildings'i belirle
		SetEnemyBuildings();
	}

	void SetEnemyBuildings()
	{
		if (PhotonNetwork.IsMasterClient)
		{
			enemyBuildings = gameManager.otherBuildings; // Diğer oyuncunun binaları
		}
		else
		{
			enemyBuildings = gameManager.clientBuildings; // MasterClient'ın binaları
		}
	}
	public void ChangeCamera()
	{
		bool _status = !myCamera.activeSelf;

		myCamera.SetActive(_status);
		myVillageCamera.SetActive(!_status);

		foreach (GameObject obj in battleStuff)
		{
			obj.SetActive(_status);
		}
		foreach (GameObject obj in villageStuff)
		{
			obj.SetActive(!_status);
		}
	}

	public void buildStructure(string bname)
	{
		string[] splitedString = bname.Split(",");
		string stcName = splitedString[0];
		int stcCoast = int.Parse(splitedString[1]);
		int index = -1;

		switch (stcName)
		{
			case "barracks":
				index = 0;
				break;
			case "archer":
				index = 1;
				break;
			default:
				index = -1;
				break;
		}
		if (index >= 0 && stcCoast <= totalCoins)
		{
			myVillage[index].SetActive(true);
			DecreaseCoin(stcCoast);
		}
	}
}
