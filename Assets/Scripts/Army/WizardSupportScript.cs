using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class WizardSupportScript : ArmyScript
{
	public override void AttackTarget()
	{
		if (canAttack && target != null)
		{
			ArmyScript enemy = target.GetComponent<ArmyScript>();
			if (enemy != null)
			{
				animator.SetTrigger("attack");
				enemy.photonView.RPC("TakeDamage", RpcTarget.AllBuffered, damage);
			}
			else
			{
				UpdateTarget();
			}
		}
	}
}
