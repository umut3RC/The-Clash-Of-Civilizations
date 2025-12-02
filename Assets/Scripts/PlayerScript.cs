using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
public class PlayerScript : MonoBehaviourPunCallbacks
{
	public GameObject spawnTargetVisulazer;
	public LayerMask raycastLayer;
	public GameObject myCanvas;
	public GameObject myCamera;
	GameObject myVillageCamera;
	public GameObject[] myBuildings;
	public GameObject myVillageCamera_origin;
	public GameObject myVillageCamera_other;
	public GameObject villageGround;
	public GameObject[] battleStuff;
	public GameObject[] villageStuff;
	public TextMeshProUGUI[] panelInfoTexts;//username hp coin username hp coin
	public GameObject[] myVillage;
	public GameObject[] armyButtons;
	public GameObject[] villageButtons;
	public GameObject[] villageBuildingInfos;
	[SerializeField] private int totalCoins = 0;
	int coinPlus = 1;
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
		// SetCoin(1000);
		// SetHealth(100);
		panelInfoTexts[0].text = PhotonNetwork.NickName;
		panelInfoTexts[3].text = PhotonNetwork.NickName;
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
	// void Start()
	// {
	// 	if (!photonView.IsMine)
	// 	{
	// 		return;
	// 	}

	// 	// --- 1. TAKIM VE KAMERA AYARLARI ---
	// 	if (PhotonNetwork.IsMasterClient)
	// 	{
	// 		armyLayer = "Army1"; // Master Client her zaman Army1
	// 		myVillageCamera = myVillageCamera_origin;
	// 	}
	// 	else
	// 	{
	// 		armyLayer = "Army2"; // Misafir her zaman Army2
	// 		myVillageCamera = myVillageCamera_other;
	// 		villageGround.transform.position += new Vector3(55, 0, 0);

	// 		// Eğer misafirsen raycast zeminini de güncelle (önceki kodlardan)
	// 		raycastLayer = LayerMask.GetMask("Ground 2");
	// 	}

	// 	// --- 2. KATMAN (LAYER) SENKRONİZASYONU ---
	// 	// Bu RPC fonksiyonun içinde "SetLayerRecursively" olduğunu varsayıyoruz.
	// 	// Böylece Player objesi ve altındaki TÜM binalar doğru takıma geçer.
	// 	photonView.RPC("RPC_SetLayerAndTag", RpcTarget.AllBuffered, armyLayer);

	// 	// --- 3. BİNALARIN (KULELERİN) BAŞLATILMASI ---
	// 	InitializeBuildings();

	// 	// --- 4. UI VE DİĞER AYARLAR ---
	// 	// SetCoin(1000); // İstersen açabilirsin
	// 	// SetHealth(100); // İstersen açabilirsin
	// 	panelInfoTexts[0].text = PhotonNetwork.NickName;
	// 	panelInfoTexts[3].text = PhotonNetwork.NickName;

	// 	spawnTargetVisulazer.SetActive(false);

	// 	if (PhotonNetwork.IsConnectedAndReady)
	// 	{
	// 		PrepareMine();
	// 	}
	// }

	// Binaları (Kuleleri) savaşa hazırlayan yeni fonksiyon
	void InitializeBuildings()
	{
		// Ben Army1 isem düşman Army2'dir (veya tam tersi)
		string myEnemyTag = (armyLayer == "Army1") ? "Army2" : "Army1";

		foreach (GameObject building in myBuildings)
		{
			if (building == null) continue;

			// Binada TowerScript var mı? (Tüm saldırı yapan binalarda olmalı)
			TowerScript tower = building.GetComponent<TowerScript>();

			if (tower != null)
			{
				// Kuleye düşmanın kim olduğunu söyle
				// (TowerScript artık ArmyScript'ten miras aldığı için bu fonksiyona sahip)
				tower.SetEnemyTag(myEnemyTag);
			}

			// Not: Binaların Layer'ını ayarlamaya gerek yok, 
			// yukarıdaki RPC_SetLayerAndTag tüm child objeleri (binaları) zaten ayarladı.
		}
	}

	void Update()
	{
		if (!photonView.IsMine)
		{
			return;
		}
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
		totalCoins += coinPlus;
		panelInfoTexts[2].text = totalCoins.ToString();
		panelInfoTexts[5].text = totalCoins.ToString();
	}
	public void DecreaseCoin(int _amount)
	{
		totalCoins -= _amount;
		panelInfoTexts[2].text = totalCoins.ToString();
		panelInfoTexts[5].text = totalCoins.ToString();
	}

	private void SetCoin(int c)
	{
		totalCoins = c;
		panelInfoTexts[2].text = totalCoins.ToString();
		panelInfoTexts[5].text = totalCoins.ToString();
	}
	private void SetHealth(int c)
	{
		health = c;
		panelInfoTexts[1].text = totalCoins.ToString();
		panelInfoTexts[4].text = totalCoins.ToString();
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

		lastTargetArmy = null;
	}
	private void PrepareMine()
	{
		if (!PhotonNetwork.IsMasterClient)
		{
			transform.rotation *= Quaternion.Euler(0, 180f, 0);
			transform.localPosition = new Vector3(28, 0, 0);

			raycastLayer = LayerMask.GetMask("Ground 2");
			armyLayer = "Army2";
		}
		if (photonView.IsMine)
		{
			armyLayer = "Army1";
			myCamera.SetActive(true);
			myCanvas.SetActive(true);
			InitializeBuildings();
		}
		isReady = true;
	}

	public void SetGameManager(GameRoomConnectionManager manager)
	{
		gameManager = manager;

		photonView.ViewID.ToString();
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
		foreach (GameObject obj in villageBuildingInfos)
		{
			obj.SetActive(false);
		}
	}

	public void buildStructure(string bname)
	{
		if (!photonView.IsMine)
		{
			return;
		}
		string[] splitedString = bname.Split(",");
		string stcName = splitedString[0];
		int stcCoast = int.Parse(splitedString[1]);
		int strcIndex = -1;
		int armyIndex = -1;

		switch (stcName)
		{
			case "barracks":
				strcIndex = 0;
				armyIndex = 0;
				break;
			case "archer":
				strcIndex = 1;
				armyIndex = 1;
				break;
			case "wizard":
				strcIndex = 2;
				armyIndex = 3;
				break;
			case "smith":
				strcIndex = 3;
				armyIndex = 6;
				break;
			case "farm":
				strcIndex = 4;
				coinPlus += 3;
				break;
			case "library":
				strcIndex = 5;
				Debug.Log("Open Search!");
				break;
			case "barn":
				strcIndex = 6;
				armyIndex = 2;
				break;
			default:
				strcIndex = -1;
				armyIndex = -1;
				break;
		}

		if (strcIndex >= 0 && stcCoast <= totalCoins)
		{

			DecreaseCoin(stcCoast);
			villageButtons[strcIndex].SetActive(false);
			if (armyIndex >= 0)
				armyButtons[armyIndex].SetActive(true);

			photonView.RPC("RPC_ActivateBuilding", RpcTarget.All, strcIndex);

			CheckKing();
		}
		else
		{
			Debug.Log("There is a problem");
		}
	}
	[PunRPC]
	public void RPC_ActivateBuilding(int index)
	{

		if (index >= 0 && index < myVillage.Length)
		{

			myVillage[index].SetActive(true);
		}
	}
	void CheckKing()
	{
		int i = 0;
		foreach (GameObject strc in myVillage)
		{
			if (strc.activeSelf)
			{
				i++;
			}
		}

		if (i == myVillage.Length)
		{
			Debug.Log("Hail the king!");
			armyButtons[4].SetActive(true);
		}
	}

	// [PunRPC]
	// public void RPC_DestroyTowerByIndex(int towerIndex)
	// {
	// 	if (towerIndex >= 0 && towerIndex < myBuildings.Length)
	// 	{
	// 		TowerScript tower = myBuildings[towerIndex].GetComponent<TowerScript>();
	// 		if (tower != null)
	// 		{
	// 			tower.DestroyTowerLocally();
	// 		}
	// 	}
	// }

	// [PunRPC]
	// public void RPC_DealDamageToTower(int towerIndex, int damage)
	// {
	// 	if (towerIndex < 0 || towerIndex >= myBuildings.Length) return;

	// 	TowerScript targetTower = myBuildings[towerIndex].GetComponent<TowerScript>();
	// 	if (targetTower != null)
	// 	{
	// 		targetTower.DecreaseHp(damage);
	// 	}
	// }
	[PunRPC]
	public void RPC_DestroyTowerByIndex(int towerIndex)
	{
		if (towerIndex >= 0 && towerIndex < myBuildings.Length)
		{
			TowerScript tower = myBuildings[towerIndex].GetComponent<TowerScript>();
			if (tower != null)
			{
				tower.DestroyTowerLocally();
			}

			// --- YENİ EKLENEN KISIM BAŞLANGICI ---

			// Eğer yıkılan bina 0. index ise (Ana Bina)
			if (towerIndex == 0)
			{
				Debug.Log("Ana bina yıkıldı! Oyun Bitiyor...");

				// Bu kararı sadece Master Client vermeli ki komut bir kere gönderilsin
				if (PhotonNetwork.IsMasterClient)
				{
					// Kaybeden oyuncunun (bu scriptin sahibinin) ActorNumber'ını herkese gönder
					int loserActorNumber = photonView.Owner.ActorNumber;

					// RPC_FinishGame fonksiyonunu herkes için çağır
					photonView.RPC("RPC_FinishGame", RpcTarget.All, loserActorNumber);
				}
			}
			// --- YENİ EKLENEN KISIM SONU ---
		}
	}

	[PunRPC]
	public void RPC_DealDamageToTower(int towerIndex, int damage)
	{
		if (towerIndex < 0 || towerIndex >= myBuildings.Length) return;

		TowerScript targetTower = myBuildings[towerIndex].GetComponent<TowerScript>();
		if (targetTower != null)
		{
			targetTower.TakeDamage(damage);
		}
	}

	[PunRPC]
	public void RPC_FinishGame(int loserActorNumber)
	{

		if (gameManager == null)
		{
			gameManager = FindObjectOfType<GameRoomConnectionManager>();
		}

		if (gameManager == null)
		{
			Debug.Log("Missing manager! UI güncellenemedi.");
			return;
		}

		if (PhotonNetwork.LocalPlayer.ActorNumber == loserActorNumber)
		{
			StartCoroutine(gameManager.LoseGame());
		}
		else
		{
			StartCoroutine(gameManager.WinGame());
		}
	}

}
