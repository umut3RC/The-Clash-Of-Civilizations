using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
public class PlayerScript : MonoBehaviour
{
	public GameObject spawnTargetVisulazer;
	public LayerMask raycastLayer;
	[SerializeField] int totalCoins = 0;
	[SerializeField] GameObject totalCoinText;

	private float coinTimer = 0f;

	bool selectedSpawn = false;
	

	void Start()
	{
		SetCoin(100);
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
			}
			if (Input.GetMouseButtonDown(0))
			{
				Debug.Log("Spawnlanik gari");
				SelectArmy();
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
	public void SelectArmy()
	{
		selectedSpawn = !selectedSpawn;
	}
}
