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

	void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag(ownerTag) && other.gameObject != wizardPhotonView.gameObject)
		{
			ArmyScript ally = other.GetComponent<ArmyScript>();

			if (ally != null && !alliesInAura.Contains(ally))
			{
				alliesInAura.Add(ally);
			}
		}
	}

	void OnTriggerExit(Collider other)
	{
		ArmyScript ally = other.GetComponent<ArmyScript>();

		if (ally != null && alliesInAura.Contains(ally))
		{
			alliesInAura.Remove(ally);
		}
	}
	private void HealAllies()
	{
		for (int i = alliesInAura.Count - 1; i >= 0; i--)
		{
			ArmyScript ally = alliesInAura[i];

			if (ally != null)
			{
				PhotonView allyPhotonView = ally.GetComponent<PhotonView>();
				if (allyPhotonView != null)
				{
					allyPhotonView.RPC("RPC_Heal", RpcTarget.All, healAmount);
				}
			}
			else
			{
				alliesInAura.RemoveAt(i);
			}
		}
	}
}