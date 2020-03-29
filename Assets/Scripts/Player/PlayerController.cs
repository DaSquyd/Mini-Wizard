using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using InControl;
using Cinemachine;
using DG.Tweening;

public class PlayerController : Entity
{
	public static PlayerController Instance;


	public PlayerSettings Settings;
	public CameraSettings CameraSettings;
	public GameObject MeshContainer;
	public ProjectileController FireballPrefab;
	public ProjectileController IceballPrefab;

	public enum MeleeState
	{
		Idle,
		Attack,
		Wait,
		PostAttack
	}

	// "WeaponState.IceSword == WeaponState.Fireballs" will output TRUE
	// This is for contextual convenience. DebugDisplay will always show the respective Fire weapon
	public enum WeaponState
	{
		IceSword = 0,
		Fireballs = 0,
		FireSword = 1,
		Iceballs = 1
	}

	public enum CameraMode
	{
		Manual,
		SettingToAuto,
		Auto
	}

	CinemachineVirtualCamera vcam;
	Rigidbody rb;

	[DebugDisplay]
	Vector3 move;
	Vector3 _targetMove;
	[DebugDisplay]
	public Vector3 TargetMove
	{
		get
		{
			return _targetMove;
		}
		private set
		{
			_targetMove = Vector3.ClampMagnitude(value, 1f);
		}
	}

	Vector3 _lastTargetMove;
	public Vector3 LastTargetMove
	{
		get
		{
			return _lastTargetMove.normalized;
		}
		private set
		{
			_lastTargetMove = value.normalized;
		}
	}

	Vector3 _meshMove;
	public Vector3 MeshMove
	{
		get
		{
			return _meshMove;
		}
		private set
		{
			_meshMove = Vector3.ClampMagnitude(value, 1f);
		}
	}

	Vector3 meshTargetMove = Vector3.forward;

	[DebugDisplay("Velocity")] Vector3 velocityDisplay;

	public bool IsGrounded
	{
		get;
		private set;
	}

	public bool IsLocked
	{
		get;
		private set;
	}

	public bool CanAttack
	{
		get;
		private set;
	}

	// Attack
	[DebugDisplay] WeaponState weaponState;


	[DebugDisplay("Melee State")] MeleeState meleeState;
	byte currentMeleeAttack;
	bool meleeAttackBuffer;
	float meleeAttackTime;

	Vector3 meleeEffectAmount;
	Tween meleeEffectTween;

	bool airMelee;

	float shootCooldown;
	public byte Ammo
	{
		get;
		private set;
	}


	// Jump
	bool jumpInput;
	bool jumpInputChange;
	[DebugDisplay("Air Jumps")] byte airJumps;
	float jumpForgivenessTime;
	float jumpCooldown;
	float airTime;
	[DebugDisplay] bool hasJumped;

	// Rotation
	float _cameraPitch;
	float _cameraYaw;
	[DebugDisplay]
	public Vector2 CameraRotation
	{
		get
		{
			return new Vector2(_cameraPitch, _cameraYaw);
		}

		private set
		{
			_cameraPitch = value.x;
			_cameraYaw = value.y;
		}
	}


	Vector3 cameraCurrentVelocity;
	Vector3 cameraBasePosition;

	[DebugDisplay("Camera")] CameraMode cameraMode = CameraMode.Auto;
	[DebugDisplay("Cam CD")] float cameraCooldownTime;

	CinemachineSmoothPath path;

	[DebugDisplay]
	Vector3 contactVelocity = new Vector3();

#if DEBUG
	float DebugCurrentPath
	{
		get
		{
			if (this != null)
				return path.FindClosestPoint(transform.position, 0, 100, 10);

			return 0f;
		}
	}

	bool debugMouseDisabled =
#if UNITY_EDITOR
		true;
#else
		false;
#endif
#endif

	protected sealed override void Start()
	{
		base.Start();
		Instance = this;

		vcam = GameManager.Instance.PlayerVcam;

		meleeEffectAmount = Vector3.right * vcam.m_Lens.FieldOfView;

		rb = GetComponent<Rigidbody>();

		_cameraYaw = transform.eulerAngles.y;
		MeshContainer.transform.rotation = Quaternion.Euler(CameraRotation);
		LastTargetMove = transform.forward;

		// TODO Find a more universal way of doing this.
		path = FindObjectOfType<CinemachineSmoothPath>();

		MaxHealth = Settings.MaxHealth;
		Health = MaxHealth;

		cameraBasePosition = transform.position;

		meshTargetMove = transform.forward;
		TurnMesh(false);

		CanAttack = true;
	}


	protected sealed override void Update()
	{
		base.Update();

		vcam.m_Lens.FieldOfView = meleeEffectAmount.x;

		MaxHealth = Settings.MaxHealth;

#if DEBUG
		if (Input.GetKeyDown(KeyCode.T))
		{
			ApplyDamage(this, 1);
		}

		if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha1))
		{
			debugMouseDisabled = !debugMouseDisabled;
		}
#endif
		_cameraYaw = transform.eulerAngles.y;

		if (ActionInputManager.GetInputDown("Swap") && CanAttack)
		{
			if (weaponState.Equals(WeaponState.Fireballs))
				weaponState = WeaponState.Iceballs;
			else
				weaponState = WeaponState.Fireballs;
		}

		MeleeUpdate();
		ShootUpdate();
		JumpUpdate();
		TurnMesh(true);
		UpdateCamera();

		velocityDisplay = rb.velocity;
	}

	private void MeleeUpdate()
	{
		if (meleeState == MeleeState.Attack)
		{
			rb.velocity = Vector3.Lerp(LastTargetMove * Settings.AttackCooldowns[currentMeleeAttack - 1].Force + (IsGrounded ? Vector3.zero : Vector3.down * Settings.AttackDownForce),
				IsGrounded ? Vector3.zero : Vector3.down * Settings.AttackDownForce,
				(Settings.AttackCooldowns[currentMeleeAttack - 1].Time - meleeAttackTime) / Settings.AttackCooldowns[currentMeleeAttack - 1].Time);
		}

		if (meleeAttackTime == 0)
		{

			switch (meleeState)
			{
				case MeleeState.Attack:
					IsLocked = false;
					meleeAttackBuffer = false;
					if (Settings.AttackCooldowns.Length > currentMeleeAttack)
					{
						meleeState = MeleeState.Wait;
						meleeAttackTime = Settings.AttackCooldowns[currentMeleeAttack].Wait;
					}
					else
					{
						meleeState = MeleeState.Idle;
						meleeAttackTime = Settings.AttackCooldowns[0].Wait;
						currentMeleeAttack = 0;
					}
					break;
				case MeleeState.Wait:
					meleeState = MeleeState.PostAttack;
					meleeAttackTime = Settings.AttackCooldowns[currentMeleeAttack].ComboForgiveness;
					break;
				case MeleeState.PostAttack:
					meleeState = MeleeState.Idle;
					currentMeleeAttack = 0;
					meleeAttackBuffer = false;
					break;
			}

			IsLocked = false;
		}
		else
		{
			meleeAttackTime = Mathf.MoveTowards(meleeAttackTime, 0f, Time.deltaTime);
		}

		// Stops if lock state or can't attack
		if (IsLocked || !CanAttack)
			return;

		if (ActionInputManager.GetInputDown("Melee"))
		{
			if (meleeState == MeleeState.Idle)
			{
				if (meleeAttackTime == 0f)
					MeleeAttack();
			}
			else
				meleeAttackBuffer = true;
		}
		if (meleeState == MeleeState.PostAttack && meleeAttackBuffer)
			MeleeAttack();
	}

	private void MeleeAttack()
	{
		if (airMelee && currentMeleeAttack == 0)
			return;

		meleeEffectAmount = new Vector3(CameraSettings.Fov, 0f, 0f);
		meleeEffectTween = DOTween.Punch(() => meleeEffectAmount, x => meleeEffectAmount = x, Vector3.right * (CameraSettings.AttackFov - CameraSettings.Fov), 0.2f, 1, 1f);

		TurnMesh(false);

		if (!IsGrounded)
			airMelee = true;

		currentMeleeAttack++;

		meleeAttackTime = Settings.AttackCooldowns[currentMeleeAttack - 1].Time;
		meleeState = MeleeState.Attack;

		IsLocked = true;
	}


	private void ShootUpdate()
	{
		if (Ammo == 0)
			return;

		if (shootCooldown > 0f)
			return;
	}

	private void Shoot()
	{

	}


	private void JumpUpdate()
	{
		if (IsLocked)
			return;

		if (jumpCooldown > 0f)
		{
			jumpCooldown = Mathf.Max(0f, jumpCooldown - Time.deltaTime);
			return;
		}

		if (ActionInputManager.GetInputDown("Jump"))
		{
			if (jumpForgivenessTime > 0f)
			{
				Jump(false);
			}
#if DEBUG
			else if (airJumps < Settings.MaxAirJumps || InputManager.ActiveDevice.LeftBumper)
#else
			else if (_airJumps < settings.maxAirJumps)
#endif
			{
				Jump(true);
				airJumps++;
			}
		}
	}

	private void Jump(bool inAir)
	{
		rb.velocity = new Vector3(rb.velocity.x, inAir ? Settings.AirJumpStrength : Settings.GroundJumpStrength, rb.velocity.z);
		jumpCooldown = Settings.JumpCooldown;
		IsGrounded = false;
		hasJumped = true;
	}

	private void TurnMesh(bool smoothed)
	{
		if (Mathf.Abs(TargetMove.magnitude) > 0f)
		{
			meshTargetMove = TargetMove;
		}

		if (smoothed)
			MeshMove = Vector3.Slerp(MeshMove, meshTargetMove, Time.deltaTime * Settings.TurnSpeed);
		else
			MeshMove = Vector3.RotateTowards(MeshMove, meshTargetMove, Time.fixedDeltaTime * Settings.AttackTurnSpeed, 1f);

		float yaw = Mathf.Atan2(MeshMove.x, MeshMove.z) * Mathf.Rad2Deg;

		MeshContainer.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
	}

	private void UpdateCamera()
	{
		float dist = 0f;
		float autoYaw = 0f;
		float autoPitch = 0f;
		if (path != null)
		{
			// Finds the closest on the track and sets the tangent (direction player should be facing)
			float posAlongPath = path.FindClosestPoint(transform.position, 0, 100, 10);

			Vector3 pathPointTangent = path.EvaluateTangent(posAlongPath);
			Vector3 pathPointPosition = path.EvaluatePosition(posAlongPath);

			float distanceFromPath = Vector3.Distance(transform.position, pathPointPosition);

			// Sets the targeted Yaw when in auto mode
			float autoTangentYaw = Mathf.Atan2(pathPointTangent.x, pathPointTangent.z) * Mathf.Rad2Deg;
			float autoTowardsYaw = Mathf.Atan2(pathPointPosition.x - transform.position.x, pathPointPosition.z - transform.position.z) * Mathf.Rad2Deg;

			float activationPercent = Mathf.InverseLerp(Settings.Camera.ActivationMinDistance, Settings.Camera.ActivationMaxDistance, distanceFromPath);

			autoYaw = Mathf.LerpAngle(autoTangentYaw, autoTowardsYaw, activationPercent);
			autoPitch = Mathf.LerpAngle(Settings.Camera.TrackAngle, Settings.Camera.DistanceAngle, activationPercent);
#if UNITY_EDITOR
			Debug.DrawLine(pathPointPosition, pathPointPosition + pathPointTangent, Color.green, Time.deltaTime);
#endif

			Vector2 delta = Quaternion.RotateTowards(Quaternion.Euler(CameraRotation), Quaternion.Euler(autoPitch, autoYaw, 0f), 10f).eulerAngles;
			if (delta.x < 0f)
			{
				delta.x += 360;
			}


			dist = Quaternion.Angle(Quaternion.Euler(CameraRotation), Quaternion.Euler(autoPitch, autoYaw, 0f));
		}

		// User input
#if DEBUG
		float yaw = ((ActionInputManager.GetInput("Look Right") - ActionInputManager.GetInput("Look Left")) * Settings.CameraJoystickSpeed) + (debugMouseDisabled ? 0f : (Input.GetAxisRaw("mouse x") * Settings.CameraMouseSpeed));
		float pitch = ((ActionInputManager.GetInput("Look Up") - ActionInputManager.GetInput("Look Down")) * Settings.CameraJoystickSpeed) + (debugMouseDisabled ? 0f : (Input.GetAxisRaw("mouse y") * Settings.CameraMouseSpeed));
#else
		float yaw = ((ActionInputManager.GetInput("Look Right") - ActionInputManager.GetInput("Look Left")) * settings.cameraJoystickSpeed) + (Input.GetAxisRaw("mouse x") * settings.cameraMouseSpeed);
		float pitch = ((ActionInputManager.GetInput("Look Up") - ActionInputManager.GetInput("Look Down")) * settings.cameraJoystickSpeed) + (Input.GetAxisRaw("mouse y") * settings.cameraMouseSpeed);
#endif

		if (path == null)
			cameraMode = CameraMode.Manual;

		switch (cameraMode)
		{
			// MANUAL
			case CameraMode.Manual:
				// Set mode to locked
				if (ActionInputManager.GetInputDown("Auto Camera") && path != null)
				{
					cameraMode = CameraMode.SettingToAuto;
				}

				// If player moves camera, reset cooldown
				if (!Mathf.Approximately(yaw, 0f) || !Mathf.Approximately(pitch, 0f))
				{
					cameraCooldownTime = Settings.Camera.AutoCooldown;
				}

				// If in air, reset cooldown
				if (!IsGrounded)
				{
					cameraCooldownTime = Mathf.MoveTowards(cameraCooldownTime, Settings.Camera.AutoCooldown, (Settings.Camera.AutoCooldownResetAir + 1) * Time.deltaTime);
				}
				else if (rb.velocity.magnitude > 0.1f)
				{
					cameraCooldownTime = Mathf.MoveTowards(cameraCooldownTime, Settings.Camera.AutoCooldown, (Settings.Camera.AutoCooldownResetMove + 1) * Time.deltaTime);
				}

				// Once cooldown is 0, switch to locked mode
				if (cameraCooldownTime == 0f && path != null)
				{
					cameraMode = CameraMode.Auto;
				}
				break;

			// SETTING TO AUTO
			case CameraMode.SettingToAuto:
				yaw = 0f;
				pitch = 0f;


				float distManual = Mathf.Min(dist, Settings.Camera.SettingToAutoFineTuneAngle) / Settings.Camera.SettingToAutoFineTuneAngle;
				CameraRotation = Quaternion.RotateTowards(Quaternion.Euler(CameraRotation), Quaternion.Euler(autoPitch, autoYaw, 0f), Settings.Camera.SettingToAutoSpeed * distManual * Time.deltaTime).eulerAngles;
				transform.rotation = Quaternion.Euler(transform.eulerAngles.x, _cameraYaw, transform.eulerAngles.z);

				if (dist < 5f)
				{
					cameraMode = CameraMode.Auto;
					cameraCooldownTime = Settings.Camera.SettingToAutoCooldown;
				}
				break;

			// AUTO
			case CameraMode.Auto:
				if (cameraCooldownTime > 0f)
				{
					yaw = 0f;
					pitch = 0f;
				}

				if (ActionInputManager.GetInputDown("Auto Camera"))
				{
					cameraMode = CameraMode.SettingToAuto;
				}

				float distLocked = Mathf.Min(dist, Settings.Camera.AutoFineTuneAngle) / Settings.Camera.AutoFineTuneAngle;
				CameraRotation = Quaternion.RotateTowards(Quaternion.Euler(CameraRotation), Quaternion.Euler(autoPitch, autoYaw, 0f), Settings.Camera.AutoSpeed * distLocked * Time.deltaTime).eulerAngles;
				transform.rotation = Quaternion.Euler(transform.eulerAngles.x, _cameraYaw, transform.eulerAngles.z);

				// Check if player has changed camera                                  Checks buffer so it doesn't automatically activate right after setting to locked mode
				if ((!Mathf.Approximately(yaw, 0f) || !Mathf.Approximately(pitch, 0f)) && cameraCooldownTime == 0)
				{
					cameraMode = CameraMode.Manual;
				}
				break;
		}

		// Decrement _cameraCooldownTime by deltaTime
		if (cameraCooldownTime > 0f)
		{
			cameraCooldownTime = Mathf.MoveTowards(cameraCooldownTime, 0f, Time.deltaTime);
		}


		float offsetPercent = Mathf.InverseLerp(CameraSettings.MinOffsetPitch, CameraSettings.MaxOffsetPitch, _cameraPitch);
		float offset = Mathf.Lerp(CameraSettings.MinOffset, CameraSettings.MaxOffset, offsetPercent);

		Vector3 start = cameraBasePosition + (Vector3.up * offset);
		Vector3 direction = Quaternion.Euler(CameraRotation) * Vector3.back;

		LayerMask layerMask = LayerMask.GetMask("Terrain");
		bool hit = Physics.SphereCast(start, CameraSettings.Buffer, direction, out RaycastHit info, CameraSettings.MaxDistance, layerMask);

		float distance = Mathf.Max(CameraSettings.MinDistance, info.distance);

		if (!hit)
			distance = CameraSettings.MaxDistance;

		Debug.DrawRay(start, direction * distance, Color.cyan);

		_cameraPitch += pitch;

		while (_cameraPitch > 180f)
			_cameraPitch -= 360f;

		_cameraPitch = Mathf.Clamp(_cameraPitch, CameraSettings.MinAngle, CameraSettings.MaxAngle);

		vcam.transform.rotation = Quaternion.Euler(CameraRotation);
		vcam.transform.position = start + (direction * (distance - CameraSettings.Buffer));

		transform.Rotate(new Vector3(0f, yaw, 0f));
	}

	private void FixedUpdate()
	{
		if (!IsGrounded)
		{
			jumpForgivenessTime -= Time.fixedDeltaTime;
			airTime += Time.fixedDeltaTime;
		}
		else
		{
			if (jumpCooldown == 0)
				hasJumped = false;
			airTime = 0;
		}

		jumpForgivenessTime = Mathf.Max(0f, jumpForgivenessTime);

		cameraBasePosition = Vector3.SmoothDamp(cameraBasePosition, transform.position, ref cameraCurrentVelocity, 1f / CameraSettings.CameraSpeed);
		//_cameraBasePosition = Vector3.MoveTowards(_cameraBasePosition, transform.position, Time.fixedDeltaTime * Vector3.Distance(_cameraBasePosition, transform.position) * cameraSettings.cameraSpeed);

		TargetUpdate();
		Movement();

		IsGrounded = false;

		velocityDisplay = rb.velocity;
	}

	private void TargetUpdate()
	{
		// Custom "conecast" to find all objects in range
		RaycastHit[] coneHit = ConeCast.ConeCastAll(transform.position, MeshContainer.transform.forward, Settings.TargetingMaxDistance, Settings.TargetingMaxAngle, LayerMask.GetMask("Default"));

		// Create a new list and only add enemies from coneHit
		List<RaycastHit> enemies = new List<RaycastHit>();
		for (int i = 0; i < coneHit.Length; i++)
		{
			if (coneHit[i].transform.tag == "Enemy")
			{
				Debug.DrawLine(transform.position, coneHit[i].transform.position, Color.green);

				enemies.Add(coneHit[i]);
			}
		}


		// Sets up default target
		int targetEnemyIndex = 0;
		float targetEnemyPriority = 360f;
		Quaternion targetLookRotation = new Quaternion();

		// Loop through all enemies
		for (int i = 0; i < enemies.Count; i++)
		{
			Quaternion lookRotation = Quaternion.LookRotation(enemies[i].transform.position - transform.position);
			float angleTowardsEnemy = Quaternion.Angle(MeshContainer.transform.rotation, lookRotation);

			// Priority is a combination of the angle from the player and the distance
			// Lower priority = better
			float enemyPriority = angleTowardsEnemy + Vector3.Distance(transform.position, enemies[i].transform.position) * Settings.TargetingDistanceWeight;

			// If the latest check is less than the current target, set as new target
			if (enemyPriority < targetEnemyPriority)
			{
				targetEnemyIndex = i;
				targetEnemyPriority = enemyPriority;
				targetLookRotation = lookRotation;
			}
		}

#if UNITY_EDITOR
		// Displays current target
		if (enemies.Count > targetEnemyIndex)
			Debug.DrawLine(transform.position, enemies[targetEnemyIndex].point, Color.yellow);
#endif

		// Player has clicked the shoot button
		if (ActionInputManager.GetFixedInputDown("Shoot"))
		{
			// If enemy exists...
			if (enemies.Count > targetEnemyIndex)
			{
#if UNITY_EDITOR
				Debug.DrawLine(transform.position, enemies[targetEnemyIndex].point, Color.yellow);
#endif
				ProjectileController fireball = Instantiate(FireballPrefab, transform.position, targetLookRotation);
				fireball.Target = enemies[targetEnemyIndex].transform;
				fireball.Owner = this;
			}
			else
			{
				// If there was no enemy to target, send the fireball forward
				ProjectileController fireball = Instantiate(FireballPrefab, transform.position, MeshContainer.transform.rotation);
				fireball.Owner = this;
			}
		}
	}

	private void Movement()
	{
		if (IsLocked)
			return;

		// Gets player input
		float x = ActionInputManager.GetInput("Right") - ActionInputManager.GetInput("Left");
		float z = ActionInputManager.GetInput("Forward") - ActionInputManager.GetInput("Back");

		TargetMove = transform.right * x + transform.forward * z;

		if (TargetMove.magnitude > 0f)
		{
			LastTargetMove = TargetMove.normalized;
		}

		if (TargetMove.magnitude > 1f)
		{
			TargetMove.Normalize();
		}

		//contactVelocity = Vector3.ClampMagnitude(contactVelocity, 1f);
		move = Vector3.Lerp(move, TargetMove, Time.fixedDeltaTime * (IsGrounded ? Settings.GroundAcceleration : Settings.AirAcceleration));
		move = new Vector3(move.x * 1 - contactVelocity.x, 0f, move.z * 1 - contactVelocity.z);

		rb.velocity = (move * Settings.Speed) + (Vector3.up * Mathf.Min(rb.velocity.y + Settings.Gravity * Time.fixedDeltaTime, (!IsGrounded && !hasJumped) ? 0.1f : 100f));
		Debug.Log($"{move * Settings.Speed} + {Vector3.up * rb.velocity.y} + {contactVelocity}");

		contactVelocity = new Vector3();
	}

	void OnCollisionStay(Collision collision)
	{
		bool collisionIsGround = false;
		foreach (ContactPoint contactPoint in collision.contacts)
		{
			if (Vector3.Angle(Vector3.up, contactPoint.normal) < Settings.MaxSlope && collision.gameObject.layer == 9)
			{
				collisionIsGround = true;
			}
		}

		move.x = Mathf.MoveTowards(move.x, 0f, Mathf.Abs(collision.impulse.x) * Time.fixedDeltaTime);
		move.z = Mathf.MoveTowards(move.z, 0f, Mathf.Abs(collision.impulse.z) * Time.fixedDeltaTime);

		print(move.x);
		print(move.z);

		//contactVelocity = Vector3.MoveTowards(contactVelocity, new Vector3(1f - Mathf.Abs(collision.impulse.normalized.x), 1f - Mathf.Abs(collision.impulse.normalized.y), 1f - Mathf.Abs(collision.impulse.normalized.z)), Time.fixedDeltaTime);

		if (collisionIsGround)
		{
			IsGrounded = true;
			airJumps = 0;
			airMelee = false;
			jumpForgivenessTime = Settings.JumpForgiveness;
		}
	}

	private void LateUpdate()
	{
		if (this != Instance)
		{
			Destroy(gameObject);
		}
	}
}
