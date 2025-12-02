using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class WizardSupportScript : ArmyScript
{
	public GameObject healAuraObject;
	public float auraDuration = 5.0f;
	public float auraCooldown = 5.0f;

	// DEĞİŞİKLİK BURADA: 'override' kelimesini ekledik
	public override void Start()
	{
		// 1. ÖNCE ArmyScript'in (Babanın) Start fonksiyonunu çalıştır.
		// Bunu yazmazsan büyücün hareket edemez, canı dolmaz, animasyon çalışmaz.
		base.Start();

		// 2. SONRA Büyücüye özel (Aura) kodları çalıştır.
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
		// Büyücü hayatta olduğu sürece bu döngü devam eder
		while (true)
		{
			// --- 1. AURA AÇIK ---

			// Diğer oyunculara (ve kendimize) aurayı AÇ komutu gönder
			photonView.RPC(nameof(RPC_SetAuraActive), RpcTarget.All, true);

			// 5 saniye (açık) bekle
			yield return new WaitForSeconds(auraDuration);

			// --- 2. AURA KAPALI ---

			// Diğer oyunculara (ve kendimize) aurayı KAPAT komutu gönder
			photonView.RPC(nameof(RPC_SetAuraActive), RpcTarget.All, false);

			// 5 saniye (kapalı) bekle
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
