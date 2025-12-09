using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class WizardSupportScript : ArmyScript
{
	public GameObject healAuraObject;
	public float auraDuration = 5.0f;
	public float auraCooldown = 5.0f;

	public override void Start()
	{
		base.Start();

		if (healAuraObject != null)
		{
			healAuraObject.SetActive(false);
		}

		if (photonView.IsMine)
		{
			StartCoroutine(AuraToggleCoroutine());
		}
	}
	private IEnumerator AuraToggleCoroutine()
	{
		while (true)
		{
			photonView.RPC(nameof(RPC_SetAuraActive), RpcTarget.All, true);

			yield return new WaitForSeconds(auraDuration);
			photonView.RPC(nameof(RPC_SetAuraActive), RpcTarget.All, false);
			yield return new WaitForSeconds(auraCooldown);
		}
	}
	[PunRPC]
	private void RPC_SetAuraActive(bool isActive)
	{
		if (healAuraObject != null)
		{
			healAuraObject.SetActive(isActive);
		}
	}
}
