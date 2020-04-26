using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : Entity
{
	public BossSettings Settings;

	protected override void Start()
	{
		base.Start();

		MaxHealth = Settings.MaxHealth;
		Health = MaxHealth;
	}
}
