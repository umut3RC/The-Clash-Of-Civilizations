using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

[RequireComponent(typeof(Collider))]
public class HealAreaScript : MonoBehaviour
{
	[Tooltip("Saniyede kaç can iyileştirecek?")]
	public int healAmount = 1;

	[Tooltip("İyileştirme ne sıklıkla tetiklenecek (saniye)?")]
	public float healInterval = 1.0f;

	private string ownerTag;

	// --- DEĞİŞİKLİK BURADA ---
	// Liste artık 'HealthScript' değil, 'ArmyScript' tutacak.
	private List<ArmyScript> alliesInAura = new List<ArmyScript>();

	private PhotonView wizardPhotonView;

	void Awake()
	{
		wizardPhotonView = GetComponentInParent<PhotonView>();

		if (wizardPhotonView == null)
		{
			Debug.LogError("Heal Aura, bir PhotonView'a sahip parent (Büyücü) bulamadı!");
			this.enabled = false;
			return;
		}

		ownerTag = wizardPhotonView.gameObject.tag;
	}

	void OnEnable()
	{
		alliesInAura.Clear();

		// Yetki (Authority) hala Master Client'da.
		if (PhotonNetwork.IsMasterClient)
		{
			InvokeRepeating(nameof(HealAllies), healInterval, healInterval);
		}
	}

	void OnDisable()
	{
		if (PhotonNetwork.IsMasterClient)
		{
			CancelInvoke(nameof(HealAllies));
		}
	}

	// Tetikleyici alana bir collider girdiğinde
	void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag(ownerTag) && other.gameObject != wizardPhotonView.gameObject)
		{
			// --- DEĞİŞİKLİK BURADA ---
			// 'HealthScript' yerine 'ArmyScript' (veya miras alan) aranıyor.
			ArmyScript ally = other.GetComponent<ArmyScript>();

			if (ally != null && !alliesInAura.Contains(ally))
			{
				alliesInAura.Add(ally);
			}
		}
	}

	// Tetikleyici alandan bir collider çıktığında
	void OnTriggerExit(Collider other)
	{
		// --- DEĞİŞİKLİK BURADA ---
		ArmyScript ally = other.GetComponent<ArmyScript>();

		if (ally != null && alliesInAura.Contains(ally))
		{
			alliesInAura.Remove(ally);
		}
	}

	// Bu fonksiyon SADECE MASTER CLIENT'DA 1 saniyede bir çalışır
	private void HealAllies()
	{
		for (int i = alliesInAura.Count - 1; i >= 0; i--)
		{
			// --- DEĞİŞİKLİK BURADA ---
			ArmyScript ally = alliesInAura[i];

			if (ally != null)
			{
				// Müttefikin 'ArmyScript'i üzerinde 'PhotonView' bileşenini bul
				PhotonView allyPhotonView = ally.GetComponent<PhotonView>();
				if (allyPhotonView != null)
				{
					// Bulunan 'PhotonView' üzerinden 'RPC_Heal' fonksiyonunu çağır.
					// Bu fonksiyon artık ArmyScript'te mevcut.
					allyPhotonView.RPC("RPC_Heal", RpcTarget.All, healAmount);
				}
			}
			else
			{
				// Birlik ölmüş veya yok olmuş, listeden çıkar
				alliesInAura.RemoveAt(i);
			}
		}
	}
}