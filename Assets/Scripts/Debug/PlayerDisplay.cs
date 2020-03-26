using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDisplay : Entity
{
	protected sealed override void Start()
	{
		base.Start();
		MaxHealth = 10;
		Health = 10;
		//Invincible = true;
	}
}
