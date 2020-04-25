using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using InControl;
using Cinemachine;
using DG.Tweening;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : Entity
{
	public static PlayerController Instance;

	[Header("Settings")]
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

	public CinemachineVirtualCamera Vcam
	{
		get; set;
	}
	public Rigidbody Rigidbody
	{
		get; private set;
	}

	[Header("Movement")]
	[DebugDisplay]
	Vector3 move;
	Vector3 _targetMove;
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

#if UNITY_EDITOR
	[DebugDisplay("Velocity")] Vector3 velocityDisplay;
#endif

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

	bool damaged = false;

	public bool CanAttack
	{
		get;
		private set;
	}

	[Header("Attacks")]
	public BoxCollider FireSwordBoxCollider;
	public BoxCollider IceSwordBoxCollider;
	public Animation FireSwordAnimation;
	public Animation IceSwordAnimation;
	public GameObject FireballGroup;
	public GameObject IceballGroup;
	public GameObject[] Fireballs;
	public GameObject[] Iceballs;

	[Header("Audio")]
	public AudioSource JumpAudioSource;
	public AudioEvent JumpSFX;
	public AudioEvent AirJump1SFX;
	public AudioEvent AirJump2SFX;

	public AudioSource SwordAudioSource;
	public AudioEvent SwordSwing1SFX;
	public AudioEvent SwordSwing2SFX;
	public AudioEvent SwordStabSFX;

	public AudioSource ShootAudioSource;
	public AudioEvent ShootSFX;

	// Attack
	public WeaponState CurrentWeaponState {
		get; private set;
	}


	[DebugDisplay("Melee State")] MeleeState meleeState;
	[DebugDisplay]
	public byte CurrentMeleeAttack
	{
		get; private set;
	}
	bool meleeAttackBuffer;
	float meleeAttackTime;

	Vector3 meleeEffectAmount;
	Tween meleeEffectTween;

	bool airMelee;

	float shootCooldown = 1f;
	public byte Ammo
	{
		get;
		private set;
	}

	// Jump
	bool jumpInput;
	bool jumpInputChange;
	byte airJumps;
	float jumpForgivenessTime;
	float jumpCooldown;
	float airTime;
	bool hasJumped;

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

	CameraMode cameraMode = CameraMode.Auto;
	float cameraCooldownTime;

	CameraPath path;

	Vector3 contactVelocity = new Vector3();

#if DEBUG
	float DebugCurrentPath
	{
		get
		{
			if (this != null)
				return path.smoothPath.FindClosestPoint(transform.position, 0, 100, 10);

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

		Vcam = GameManager.Instance.PlayerVcam;

		meleeEffectAmount = Vector3.right * Vcam.m_Lens.FieldOfView;

		Rigidbody = GetComponent<Rigidbody>();

		_cameraYaw = transform.eulerAngles.y;
		MeshContainer.transform.rotation = Quaternion.Euler(CameraRotation);
		LastTargetMove = transform.forward;

		// TODO Find a more universal way of doing this.
		path = FindObjectOfType<CameraPath>();

		MaxHealth = Settings.MaxHealth;
		Health = MaxHealth;

		cameraBasePosition = transform.position;

		meshTargetMove = transform.forward;
		TurnMesh(false);

		CanAttack = true;

		Ammo = 3;
	}


	protected sealed override void Update()
	{
		if (GameManager.Instance.IsPaused)
			return;

		base.Update();

		Vcam.m_Lens.FieldOfView = meleeEffectAmount.x;

		MaxHealth = Settings.MaxHealth;

#if DEBUG
		if (Input.GetKeyDown(KeyCode.T))
		{
			ApplyDamage(this, 1, Vector3.zero, DamageType.Other, Element.None);
		}

		if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha1))
		{
			debugMouseDisabled = !debugMouseDisabled;
		}
#endif
		_cameraYaw = transform.eulerAngles.y;

		if (ActionInputManager.GetInputDown("Swap") && CanAttack && meleeAttackTime == 0f)
		{
			if (CurrentWeaponState.Equals(WeaponState.Fireballs))
			{
				CurrentWeaponState = WeaponState.Iceballs;

				FireSwordAnimation.gameObject.SetActive(true);
				IceSwordAnimation.gameObject.SetActive(false);
				FireballGroup.SetActive(false);
				IceballGroup.SetActive(true);
			}
			else
			{
				CurrentWeaponState = WeaponState.Fireballs;

				FireSwordAnimation.gameObject.SetActive(false);
				IceSwordAnimation.gameObject.SetActive(true);
				FireballGroup.SetActive(true);
				IceballGroup.SetActive(false);
			}

			meleeAttackTime = 0.25f;

			meleeState = MeleeState.Idle;
			CurrentMeleeAttack = 0;
		}

		MeleeUpdate();
		ShootUpdate();
		JumpUpdate();
		TurnMesh(true);
		UpdateCamera();

		if (Ammo < 3)
		{
			if (shootCooldown > 0f)
			{
				shootCooldown = Mathf.MoveTowards(shootCooldown, 0f, Time.deltaTime);
			}
			else
			{
				Ammo++;
				shootCooldown = 1f;
			}
		}

		if (Ammo < 3)
		{
			Fireballs[2].SetActive(false);
			Iceballs[2].SetActive(false);
		}
		else
		{
			Fireballs[2].SetActive(true);
			Iceballs[2].SetActive(true);
		}

		if (Ammo < 2)
		{
			Fireballs[1].SetActive(false);
			Iceballs[1].SetActive(false);
		}
		else
		{
			Fireballs[1].SetActive(true);
			Iceballs[1].SetActive(true);
		}

		if (Ammo < 1)
		{
			Fireballs[0].SetActive(false);
			Iceballs[0].SetActive(false);
		}
		else
		{
			Fireballs[0].SetActive(true);
			Iceballs[0].SetActive(true);
		}
	}

	public void EnableSwordHit()
	{
		if (CurrentWeaponState == WeaponState.FireSword)
			FireSwordBoxCollider.enabled = true;
		if (CurrentWeaponState == WeaponState.IceSword)
			IceSwordBoxCollider.enabled = true;
	}
	public void DisableSwordHit()
	{
		FireSwordBoxCollider.enabled = false;
		IceSwordBoxCollider.enabled = false;
	}

	private void MeleeUpdate()
	{
		if (meleeState == MeleeState.Attack)
		{
			Rigidbody.velocity = Vector3.Lerp(LastTargetMove * Settings.AttackData[CurrentMeleeAttack - 1].Force + (IsGrounded ? Vector3.zero : Vector3.down * Settings.AttackDownForce),
				IsGrounded ? Vector3.zero : Vector3.down * Settings.AttackDownForce,
				(Settings.AttackData[CurrentMeleeAttack - 1].Time - meleeAttackTime) / Settings.AttackData[CurrentMeleeAttack - 1].Time);
		}

		if (meleeAttackTime == 0)
		{

			switch (meleeState)
			{
				case MeleeState.Attack:
					IsLocked = false;
					meleeAttackBuffer = false;
					if (Settings.AttackData.Length > CurrentMeleeAttack)
					{
						meleeState = MeleeState.Wait;
						meleeAttackTime = Settings.AttackData[CurrentMeleeAttack].Wait;
					}
					else
					{
						meleeState = MeleeState.Idle;
						meleeAttackTime = Settings.AttackData[0].Wait;
						CurrentMeleeAttack = 0;
					}
					break;
				case MeleeState.Wait:
					meleeState = MeleeState.PostAttack;
					meleeAttackTime = Settings.AttackData[CurrentMeleeAttack].ComboForgiveness;
					break;
				case MeleeState.PostAttack:
					meleeState = MeleeState.Idle;
					CurrentMeleeAttack = 0;
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
		if (IsLocked || !CanAttack || damaged)
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
		if (airMelee && CurrentMeleeAttack == 0)
			return;

		meleeEffectAmount = new Vector3(CameraSettings.Fov, 0f, 0f);
		meleeEffectTween = DOTween.Punch(() => meleeEffectAmount, x => meleeEffectAmount = x, Vector3.right * (CameraSettings.AttackFov - CameraSettings.Fov), 0.2f, 1, 1f);

		TurnMesh(false);

		if (!IsGrounded)
			airMelee = true;

		CurrentMeleeAttack++;

		if (CurrentMeleeAttack == 1)
		{
			SwordSwing1SFX.Play(SwordAudioSource);
		}
		else if (CurrentMeleeAttack == 2)
		{
			SwordSwing2SFX.Play(SwordAudioSource);
		}
		else if (CurrentMeleeAttack == 3)
		{
			SwordStabSFX.Play(SwordAudioSource);
		}

		if (CurrentWeaponState == WeaponState.FireSword)
		{
			FireSwordAnimation.Stop();

			if (CurrentMeleeAttack == 1)
				FireSwordAnimation.Play("FireSwordSwing");
			else if (CurrentMeleeAttack == 2)
				FireSwordAnimation.Play("FireSwordSwing2");
			else if (CurrentMeleeAttack == 3)
				FireSwordAnimation.Play("FireSwordStab");
		}
		else if (CurrentWeaponState == WeaponState.IceSword)
		{
			IceSwordAnimation.Stop();

			if (CurrentMeleeAttack == 1)
				IceSwordAnimation.Play("IceSwordSwing");
			else if (CurrentMeleeAttack == 2)
				IceSwordAnimation.Play("IceSwordSwing2");
			else if (CurrentMeleeAttack == 3)
				IceSwordAnimation.Play("IceSwordStab");
		}

		meleeAttackTime = Settings.AttackData[CurrentMeleeAttack - 1].Time;
		meleeState = MeleeState.Attack;

		IsLocked = true;
	}


	private void ShootUpdate()
	{
		if (Ammo == 0)
			return;

		if (ActionInputManager.GetInputDown("Shoot"))
			Shoot();
	}

	private void Shoot()
	{
		ShootSFX.Play(ShootAudioSource);

		ProjectileController projectile;

		if (CurrentWeaponState == WeaponState.Fireballs)
		{
			projectile = Instantiate(FireballPrefab, Fireballs[Ammo - 1].transform.position, MeshContainer.transform.rotation);
		}
		else
		{
			projectile = Instantiate(IceballPrefab, Iceballs[Ammo - 1].transform.position, MeshContainer.transform.rotation);
		}
		projectile.Owner = this;

		// If enemy exists...
		if (enemies.Count > targetEnemyIndex)
		{
#if UNITY_EDITOR
			Debug.DrawLine(transform.position, enemies[targetEnemyIndex].point, Color.yellow);
#endif
			projectile.Target = enemies[targetEnemyIndex].transform;
		}

		Ammo--;
		shootCooldown = 1f;
	}


	private void JumpUpdate()
	{
		if (damaged || IsLocked)
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
			else if (airJumps < Settings.MaxAirJumps)
#endif
			{
				Jump(true);
				airJumps++;
			}
		}
	}

	private void Jump(bool inAir)
	{
		Rigidbody.velocity = new Vector3(Rigidbody.velocity.x, inAir ? Settings.AirJumpStrength : Settings.GroundJumpStrength, Rigidbody.velocity.z);
		jumpCooldown = Settings.JumpCooldown;
		IsGrounded = false;
		hasJumped = true;

		if (!inAir)
		{
			JumpSFX.Play(JumpAudioSource);
		}
		else
		{
			if (airJumps == 0)
			{
				AirJump1SFX.Play(JumpAudioSource);
			}
			else
			{
				AirJump2SFX.Play(JumpAudioSource);
			}
		}
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
			float posAlongPath = path.smoothPath.FindClosestPoint(transform.position, 0, 100, 10);

			Vector3 pathPointTangent = path.smoothPath.EvaluateTangent(posAlongPath);
			Vector3 pathPointPosition = path.smoothPath.EvaluatePosition(posAlongPath);

			float distanceFromPath = Vector3.Distance(transform.position, pathPointPosition);

			// Sets the targeted Yaw when in auto mode
			float autoTangentYaw = Mathf.Atan2(pathPointTangent.x, pathPointTangent.z) * Mathf.Rad2Deg;
			float autoTowardsYaw = Mathf.Atan2(pathPointPosition.x - transform.position.x, pathPointPosition.z - transform.position.z) * Mathf.Rad2Deg;

			float positionAmount = Mathf.InverseLerp(Mathf.FloorToInt(posAlongPath), Mathf.CeilToInt(posAlongPath), posAlongPath);
			float minActivation = Mathf.Lerp(path.min[Mathf.FloorToInt(posAlongPath)], path.min[Mathf.CeilToInt(posAlongPath)], positionAmount);
			float maxActivation = Mathf.Lerp(path.max[Mathf.FloorToInt(posAlongPath)], path.max[Mathf.CeilToInt(posAlongPath)], positionAmount);
			float activationPercent = Mathf.InverseLerp(minActivation, maxActivation, distanceFromPath);

			autoYaw = Mathf.LerpAngle(autoTangentYaw, autoTowardsYaw, activationPercent);
			autoPitch = Mathf.LerpAngle(path.pitch[Mathf.FloorToInt(posAlongPath)], path.pitch[Mathf.CeilToInt(posAlongPath)], positionAmount);
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
		float yaw = ((ActionInputManager.GetInput("Look Right") - ActionInputManager.GetInput("Look Left")) * Settings.CameraJoystickSpeed) + (Input.GetAxisRaw("mouse x") * Settings.CameraMouseSpeed);
		float pitch = ((ActionInputManager.GetInput("Look Up") - ActionInputManager.GetInput("Look Down")) * Settings.CameraJoystickSpeed) + (Input.GetAxisRaw("mouse y") * Settings.CameraMouseSpeed);
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
				else if (Rigidbody.velocity.magnitude > 0.1f)
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

		Vcam.transform.rotation = Quaternion.Euler(CameraRotation);
		Vcam.transform.position = start + (direction * (distance - CameraSettings.Buffer));

		transform.Rotate(new Vector3(0f, yaw, 0f));
	}

	private void FixedUpdate()
	{
		if (GameManager.Instance.IsPaused)
			return;

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

#if UNITY_EDITOR
		velocityDisplay = Rigidbody.velocity;
#endif
	}

	List<RaycastHit> enemies;
	int targetEnemyIndex;
	float targetEnemyPriority;
	Quaternion targetLookRotation;
	private void TargetUpdate()
	{
		// Custom "conecast" to find all objects in range
		RaycastHit[] coneHit = ConeCast.ConeCastAll(transform.position, MeshContainer.transform.forward, Settings.TargetingMaxDistance, Settings.TargetingMaxAngle, LayerMask.GetMask("Default", "Terrain", "Enemy"));

		// Create a new list and only add enemies from coneHit
		enemies = new List<RaycastHit>();
		for (int i = 0; i < coneHit.Length; i++)
		{
			if (coneHit[i].transform.tag == "Enemy")
			{
				Debug.DrawLine(transform.position, coneHit[i].transform.position, Color.green);

				enemies.Add(coneHit[i]);
			}
		}


		targetEnemyIndex = 0;
		targetEnemyPriority = 360f;
		targetLookRotation = new Quaternion();

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
	}

	private void Movement()
	{
		if (damaged || IsLocked)
		{
			Rigidbody.velocity = new Vector3(Rigidbody.velocity.x, Mathf.Min(Rigidbody.velocity.y + Settings.Gravity * Time.fixedDeltaTime), Rigidbody.velocity.z);
			return;
		}

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

		Rigidbody.velocity = (move * Settings.Speed) + (Vector3.up * Mathf.Min(Rigidbody.velocity.y + Settings.Gravity * Time.fixedDeltaTime, (!IsGrounded && !hasJumped) ? 0.1f : 100f));
		//Debug.Log($"{move * Settings.Speed} + {Vector3.up * rb.velocity.y} + {contactVelocity}");

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
	protected override void OnDeath()
	{
		base.OnDeath();
		GameManager.Instance.Lose();
	}

	protected override void OnReceiveDamage(Entity attacker, int amount, Vector3 direction, DamageType type, Element sourceElement)
	{
		Rigidbody.velocity = new Vector3(direction.x * 5f, 1f, direction.z * 5f);
		CurrentMeleeAttack = 0;
		meleeState = MeleeState.Idle;
		StartCoroutine(Invuln());
	}

	IEnumerator Invuln()
	{
		Invincible = true;
		damaged = true;
		yield return new WaitForSeconds(Settings.StunTime);
		damaged = false;

		yield return new WaitForSeconds(Settings.InvulnerabilityTime - Settings.StunTime);
		Invincible = false;
	}
}
