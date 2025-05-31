using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class PlayerScript : MonoBehaviour
{
	public GameObject spawnTargetVisulazer;
	public LayerMask raycastLayer;
	public GameObject testArmyPrefab;
	[SerializeField] int totalCoins = 0;
	[SerializeField] GameObject totalCoinText;

	private float coinTimer = 0f;

	bool selectedSpawn = false;
	string lastTargetArmyName = "";
	GameObject lastTargetArmy;

	void Start()
	{
		SetCoin(100);
		spawnTargetVisulazer.SetActive(false);
	}

	void Update()
	{
		coinTimer += Time.deltaTime;

		if (coinTimer >= 1f)
		{
			IncreaseCoin();
			coinTimer = 0f;
		}

		if (selectedSpawn)
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
			lastTargetArmy = Instantiate(testArmyPrefab);
			int _amount = lastTargetArmy.GetComponent<HeavySwordmanScript>().GetAmount();
			if (_amount >= totalCoins)
			{
				Destroy(lastTargetArmy);
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
}
