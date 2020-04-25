using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
	public void EnableHitbox()
	{
		PlayerController.Instance.EnableSwordHit();
	}

	public void DisableHitbox()
	{
		PlayerController.Instance.DisableSwordHit();
	}

	private void OnTriggerEnter(Collider other)
	{
		PlayerController player = PlayerController.Instance;

		PlayerController.WeaponState weaponState = player.CurrentWeaponState;

		bool isEntity = other.TryGetComponent(out Entity entity);

		if (isEntity)
		{
			Vector3 direction = (player.MeshContainer.transform.forward * 5f + player.transform.position) - other.transform.position;

			if (player.CurrentMeleeAttack - 1 < 0)
				return;

			PlayerSettings.Attack attack = player.Settings.AttackData[player.CurrentMeleeAttack - 1];

			if (entity.TryGetComponent(out BatEnemy bat))
			{
				bat.stunAttack = player.CurrentMeleeAttack;
			}

			if (weaponState == PlayerController.WeaponState.FireSword)
			{
				entity.ApplyDamage(player, attack.Damage, direction, Entity.DamageType.Melee, Element.Fire);
			}
			else
			{
				entity.ApplyDamage(player, attack.Damage, direction, Entity.DamageType.Melee, Element.Ice);
			}
		}
	}
}
