using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using InControl;
using Cinemachine;
using DG.Tweening;

public class PlayerController : Entity
{
	public static PlayerController instance;


	public PlayerSettings settings;
	public CameraSettings cameraSettings;
	public GameObject meshContainer;
	public ProjectileController fireballPrefab;
	public ProjectileController iceballPrefab;

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

	CinemachineVirtualCamera _vcam;
	Rigidbody _rb;

	Vector3 _move;
	Vector3 _targetMove;
	public Vector3 TargetMove
	{
		get
		{
			return _targetMove;
		}
		private set
		{
			if (value.magnitude > 1f)
				_targetMove = value.normalized;
			else
				_targetMove = value;
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
	Vector3 _meshTargetMove = Vector3.forward;

	[DebugDisplay("Velocity")] Vector3 _velocityDisplay;

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
	[DebugDisplay] WeaponState _weaponState;


	[DebugDisplay("Melee State")] MeleeState _meleeState;
	byte _currentMeleeAttack;
	bool _meleeAttackBuffer;
	float _meleeAttackTime;

	Vector3 _meleeEffectAmount;
	Tween _meleeEffectTween;

	bool _airMelee;

	float _shootCooldown;
	public byte Ammo
	{
		get;
		private set;
	}


	// Jump
	bool _jumpInput;
	bool _jumpInputChange;
	[DebugDisplay("Air Jumps")] byte _airJumps;
	float _jumpForgivenessTime;
	float _jumpTime;

	// Rotation
	float _pitch;
	float _yaw;
	[DebugDisplay]
	public Vector2 Rotation
	{
		get
		{
			return new Vector2(_pitch, _yaw);
		}

		private set
		{
			_pitch = value.x;
			_yaw = value.y;
		}
	}


	Vector3 _cameraCurrentVelocity;
	Vector3 _cameraBasePosition;

	[DebugDisplay("Camera")] CameraMode _cameraMode = CameraMode.Auto;
	[DebugDisplay("Cam CD")] float _cameraCooldownTime;

	CinemachineSmoothPath _path;

#if DEBUG
	float DebugCurrentPath
	{
		get
		{
			if (this != null)
				return _path.FindClosestPoint(transform.position, 0, 100, 10);

			return 0f;
		}
	}

	bool _debugMouseDisabled =
#if UNITY_EDITOR
		true;
#else
		false;
#endif
#endif

	protected sealed override void Start()
	{
		base.Start();
		instance = this;

		_vcam = GameManager.instance.playerVcam;

		_meleeEffectAmount = Vector3.right * _vcam.m_Lens.FieldOfView;

		_rb = GetComponent<Rigidbody>();

		_yaw = transform.eulerAngles.y;
		meshContainer.transform.rotation = Quaternion.Euler(Rotation);
		LastTargetMove = transform.forward;

		// TODO Find a more universal way of doing this.
		_path = FindObjectOfType<CinemachineSmoothPath>();

		MaxHealth = settings.maxHealth;
		Health = MaxHealth;

		_cameraBasePosition = transform.position;

		_meshRenderer = meshContainer.GetComponentInChildren<MeshRenderer>();

		_meshTargetMove = transform.forward;
		TurnMesh(false);

		CanAttack = true;
	}


	protected sealed override void Update()
	{
		base.Update();

		_vcam.m_Lens.FieldOfView = _meleeEffectAmount.x;

		MaxHealth = settings.maxHealth;

#if DEBUG
		if (Input.GetKeyDown(KeyCode.T))
		{
			ApplyDamageToEntity(this, 1);
		}

		if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha1))
		{
			_debugMouseDisabled = !_debugMouseDisabled;
		}
#endif
		_yaw = transform.eulerAngles.y;

		if (ActionInputManager.GetInputDown("Swap") && CanAttack)
		{
			if (_weaponState.Equals(WeaponState.Fireballs))
				_weaponState = WeaponState.Iceballs;
			else
				_weaponState = WeaponState.Fireballs;
		}

		MeleeUpdate();
		ShootUpdate();
		JumpUpdate();
		TurnMesh(true);
		UpdateCamera();

		_velocityDisplay = _rb.velocity;
	}

	private void MeleeUpdate()
	{
		if (_meleeState == MeleeState.Attack)
		{
			_rb.velocity = Vector3.Lerp(LastTargetMove * settings.attackCooldowns[_currentMeleeAttack - 1].force + (IsGrounded ? Vector3.zero : Vector3.down * settings.attackDownForce),
				IsGrounded ? Vector3.zero : Vector3.down * settings.attackDownForce,
				(settings.attackCooldowns[_currentMeleeAttack - 1].time - _meleeAttackTime) / settings.attackCooldowns[_currentMeleeAttack - 1].time);
		}

		if (_meleeAttackTime == 0)
		{

			switch (_meleeState)
			{
				case MeleeState.Attack:
					IsLocked = false;
					_meleeAttackBuffer = false;
					_rb.useGravity = true;
					if (settings.attackCooldowns.Length > _currentMeleeAttack)
					{
						_meleeState = MeleeState.Wait;
						_meleeAttackTime = settings.attackCooldowns[_currentMeleeAttack].wait;
					}
					else
					{
						_meleeState = MeleeState.Idle;
						_meleeAttackTime = settings.attackCooldowns[0].wait;
						_currentMeleeAttack = 0;
					}
					break;
				case MeleeState.Wait:
					_meleeState = MeleeState.PostAttack;
					_meleeAttackTime = settings.attackCooldowns[_currentMeleeAttack].comboForgiveness;
					break;
				case MeleeState.PostAttack:
					_meleeState = MeleeState.Idle;
					_currentMeleeAttack = 0;
					_meleeAttackBuffer = false;
					break;
			}

			IsLocked = false;
		}
		else
		{
			_meleeAttackTime = Mathf.MoveTowards(_meleeAttackTime, 0f, Time.deltaTime);
		}

		// Stops if lock state or can't attack
		if (IsLocked || !CanAttack)
			return;

		if (ActionInputManager.GetInputDown("Melee"))
		{
			if (_meleeState == MeleeState.Idle)
			{
				if (_meleeAttackTime == 0f)
					MeleeAttack();
			}
			else
				_meleeAttackBuffer = true;
		}
		if (_meleeState == MeleeState.PostAttack && _meleeAttackBuffer)
			MeleeAttack();
	}

	private void MeleeAttack()
	{
		if (_airMelee && _currentMeleeAttack == 0)
			return;

		_meleeEffectAmount = new Vector3(cameraSettings.fov, 0f, 0f);
		_meleeEffectTween = DOTween.Punch(() => _meleeEffectAmount, x => _meleeEffectAmount = x, Vector3.right * (cameraSettings.attackFov - cameraSettings.fov), 0.2f, 1, 1f);

		TurnMesh(false);

		if (!IsGrounded)
			_airMelee = true;

		_currentMeleeAttack++;

		_meleeAttackTime = settings.attackCooldowns[_currentMeleeAttack - 1].time;
		_meleeState = MeleeState.Attack;

		_rb.useGravity = false;
		IsLocked = true;
	}


	private void ShootUpdate()
	{
		if (Ammo == 0)
			return;

		if (_shootCooldown > 0f)
			return;
	}

	private void Shoot()
	{

	}


	private void JumpUpdate()
	{
		if (IsLocked)
			return;

		if (_jumpTime > 0f)
		{
			_jumpTime = Mathf.Max(0f, _jumpTime - Time.deltaTime);
			return;
		}

		if (ActionInputManager.GetInputDown("Jump"))
		{
			if (_jumpForgivenessTime > 0f)
			{
				Jump(false);
			}
#if DEBUG
			else if (_airJumps < settings.maxAirJumps || InputManager.ActiveDevice.LeftBumper)
#else
			else if (_airJumps < settings.maxAirJumps)
#endif
			{
				Jump(true);
				_airJumps++;
			}
		}
	}

	private void Jump(bool inAir)
	{
		_rb.velocity = new Vector3(_rb.velocity.x, inAir ? settings.airJumpStrength : settings.groundJumpStrength, _rb.velocity.z);
		_jumpTime = settings.jumpCooldown;
		IsGrounded = false;
	}

	private void TurnMesh(bool smoothed)
	{
		if (Mathf.Abs(TargetMove.magnitude) > 0f)
		{
			_meshTargetMove = TargetMove;
		}

		if (smoothed)
			_meshMove = Vector3.Slerp(_meshMove, _meshTargetMove, Time.deltaTime * settings.turnSpeed);
		else
			_meshMove = Vector3.RotateTowards(_meshMove, _meshTargetMove, Time.fixedDeltaTime * settings.attackTurnSpeed, 1f);

		float yaw = Mathf.Atan2(_meshMove.x, _meshMove.z) * Mathf.Rad2Deg;

		meshContainer.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
	}

	private void UpdateCamera()
	{
		if (_path == null)
			return;

		// Finds the closest on the track and sets the tangent (direction player should be facing)
		float posAlongPath = _path.FindClosestPoint(transform.position, 0, 100, 10);

		Vector3 pathPointTangent = _path.EvaluateTangent(posAlongPath);
		Vector3 pathPointPosition = _path.EvaluatePosition(posAlongPath);

		float distanceFromPath = Vector3.Distance(transform.position, pathPointPosition);

		// Sets the targeted Yaw when in auto mode
		float autoTangentYaw = Mathf.Atan2(pathPointTangent.x, pathPointTangent.z) * Mathf.Rad2Deg;
		float autoTowardsYaw = Mathf.Atan2(pathPointPosition.x - transform.position.x, pathPointPosition.z - transform.position.z) * Mathf.Rad2Deg;

		float activationPercent = Mathf.InverseLerp(settings.camera.activationMinDistance, settings.camera.activationMaxDistance, distanceFromPath);

		float autoYaw = Mathf.LerpAngle(autoTangentYaw, autoTowardsYaw, activationPercent);
		float autoPitch = Mathf.LerpAngle(settings.camera.trackAngle, settings.camera.distanceAngle, activationPercent);
#if UNITY_EDITOR
		Debug.DrawLine(pathPointPosition, pathPointPosition + pathPointTangent, Color.green, Time.deltaTime);
#endif

		// User input
#if DEBUG
		float yaw = ((ActionInputManager.GetInput("Look Right") - ActionInputManager.GetInput("Look Left")) * settings.cameraJoystickSpeed) + (_debugMouseDisabled ? 0f : (Input.GetAxisRaw("mouse x") * settings.cameraMouseSpeed));
		float pitch = ((ActionInputManager.GetInput("Look Up") - ActionInputManager.GetInput("Look Down")) * settings.cameraJoystickSpeed) + (_debugMouseDisabled ? 0f : (Input.GetAxisRaw("mouse y") * settings.cameraMouseSpeed));
#else
		float yaw = ((ActionInputManager.GetInput("Look Right") - ActionInputManager.GetInput("Look Left")) * settings.cameraJoystickSpeed) + (Input.GetAxisRaw("mouse x") * settings.cameraMouseSpeed);
		float pitch = ((ActionInputManager.GetInput("Look Up") - ActionInputManager.GetInput("Look Down")) * settings.cameraJoystickSpeed) + (Input.GetAxisRaw("mouse y") * settings.cameraMouseSpeed);
#endif

		Vector2 delta = Quaternion.RotateTowards(Quaternion.Euler(Rotation), Quaternion.Euler(autoPitch, autoYaw, 0f), 10f).eulerAngles;
		if (delta.x < 0f)
		{
			delta.x += 360;
		}


		float dist = Quaternion.Angle(Quaternion.Euler(Rotation), Quaternion.Euler(autoPitch, autoYaw, 0f));

		switch (_cameraMode)
		{
			// MANUAL
			case CameraMode.Manual:
				// Set mode to locked
				if (ActionInputManager.GetInputDown("Auto Camera"))
				{
					_cameraMode = CameraMode.SettingToAuto;
				}

				// If player moves camera, reset cooldown
				if (!Mathf.Approximately(yaw, 0f) || !Mathf.Approximately(pitch, 0f))
				{
					_cameraCooldownTime = settings.camera.autoCooldown;
				}

				// If in air, reset cooldown
				if (!IsGrounded)
				{
					_cameraCooldownTime = Mathf.MoveTowards(_cameraCooldownTime, settings.camera.autoCooldown, (settings.camera.autoCooldownResetAir + 1) * Time.deltaTime);
				}
				else if (_rb.velocity.magnitude > 0.1f)
				{
					_cameraCooldownTime = Mathf.MoveTowards(_cameraCooldownTime, settings.camera.autoCooldown, (settings.camera.autoCooldownResetMove + 1) * Time.deltaTime);
				}

				// Once cooldown is 0, switch to locked mode
				if (_cameraCooldownTime == 0f)
				{
					_cameraMode = CameraMode.Auto;
				}
				break;

			// SETTING TO AUTO
			case CameraMode.SettingToAuto:
				yaw = 0f;
				pitch = 0f;


				float distManual = Mathf.Min(dist, settings.camera.settingToAutoFineTuneAngle) / settings.camera.settingToAutoFineTuneAngle;
				Rotation = Quaternion.RotateTowards(Quaternion.Euler(Rotation), Quaternion.Euler(autoPitch, autoYaw, 0f), settings.camera.settingToAutoSpeed * distManual * Time.deltaTime).eulerAngles;
				transform.rotation = Quaternion.Euler(transform.eulerAngles.x, _yaw, transform.eulerAngles.z);

				if (dist < 5f)
				{
					_cameraMode = CameraMode.Auto;
					_cameraCooldownTime = settings.camera.settingToAutoCooldown;
				}
				break;

			// AUTO
			case CameraMode.Auto:
				if (_cameraCooldownTime > 0f)
				{
					yaw = 0f;
					pitch = 0f;
				}

				if (ActionInputManager.GetInputDown("Auto Camera"))
				{
					_cameraMode = CameraMode.SettingToAuto;
				}

				float distLocked = Mathf.Min(dist, settings.camera.autoFineTuneAngle) / settings.camera.autoFineTuneAngle;
				Rotation = Quaternion.RotateTowards(Quaternion.Euler(Rotation), Quaternion.Euler(autoPitch, autoYaw, 0f), settings.camera.autoSpeed * distLocked * Time.deltaTime).eulerAngles;
				transform.rotation = Quaternion.Euler(transform.eulerAngles.x, _yaw, transform.eulerAngles.z);

				// Check if player has changed camera                                  Checks buffer so it doesn't automatically activate right after setting to locked mode
				if ((!Mathf.Approximately(yaw, 0f) || !Mathf.Approximately(pitch, 0f)) && _cameraCooldownTime == 0)
				{
					_cameraMode = CameraMode.Manual;
				}
				break;
		}

		// Decrement _cameraCooldownTime by deltaTime
		if (_cameraCooldownTime > 0f)
		{
			_cameraCooldownTime = Mathf.MoveTowards(_cameraCooldownTime, 0f, Time.deltaTime);
		}


		float offsetPercent = Mathf.InverseLerp(cameraSettings.minOffsetPitch, cameraSettings.maxOffsetPitch, _pitch);
		float offset = Mathf.Lerp(cameraSettings.minOffset, cameraSettings.maxOffset, offsetPercent);

		Vector3 start = _cameraBasePosition + (Vector3.up * offset);
		Vector3 direction = Quaternion.Euler(Rotation) * Vector3.back;

		LayerMask layerMask = LayerMask.GetMask("Terrain");
		bool hit = Physics.SphereCast(start, cameraSettings.buffer, direction, out RaycastHit info, cameraSettings.maxDistance, layerMask);

		float distance = Mathf.Max(cameraSettings.minDistance, info.distance);

		if (!hit)
			distance = cameraSettings.maxDistance;

		Debug.DrawRay(start, direction * distance, Color.cyan);

		_pitch += pitch;

		while (_pitch > 180f)
			_pitch -= 360f;

		_pitch = Mathf.Clamp(_pitch, cameraSettings.minAngle, cameraSettings.maxAngle);

		_vcam.transform.rotation = Quaternion.Euler(Rotation);
		_vcam.transform.position = start + (direction * (distance - cameraSettings.buffer));

		transform.Rotate(new Vector3(0f, yaw, 0f));
	}

	private void FixedUpdate()
	{
		if (!IsGrounded)
			_jumpForgivenessTime -= Time.fixedDeltaTime;

		_jumpForgivenessTime = Mathf.Max(0f, _jumpForgivenessTime);

		_cameraBasePosition = Vector3.SmoothDamp(_cameraBasePosition, transform.position, ref _cameraCurrentVelocity, 1f / cameraSettings.cameraSpeed);
		//_cameraBasePosition = Vector3.MoveTowards(_cameraBasePosition, transform.position, Time.fixedDeltaTime * Vector3.Distance(_cameraBasePosition, transform.position) * cameraSettings.cameraSpeed);
		
		TargetUpdate();
		Movement();

		IsGrounded = false;

		_velocityDisplay = _rb.velocity;
	}

	private void TargetUpdate()
	{
		// Custom "conecast" to find all objects in range
		RaycastHit[] coneHit = ConeCast.ConeCastAll(transform.position, meshContainer.transform.forward, settings.targetingMaxDistance, settings.targetingMaxAngle, LayerMask.GetMask("Default"));

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
			float angleTowardsEnemy = Quaternion.Angle(meshContainer.transform.rotation, lookRotation);

			// Priority is a combination of the angle from the player and the distance
			// Lower priority = better
			float enemyPriority = angleTowardsEnemy + Vector3.Distance(transform.position, enemies[i].transform.position) * settings.targetingDistanceWeight;

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
				ProjectileController fireball = Instantiate(fireballPrefab, transform.position, targetLookRotation);
				fireball.Target = enemies[targetEnemyIndex].transform;
				fireball.Owner = this;
			}
			else
			{
				// If there was no enemy to target, send the fireball forward
				ProjectileController fireball = Instantiate(fireballPrefab, transform.position, meshContainer.transform.rotation);
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

		_move = Vector3.Lerp(_move, TargetMove, Time.fixedDeltaTime * (IsGrounded ? settings.groundAcceleration : settings.airAcceleration));

		_rb.velocity = (_move * settings.speed) + (Vector3.up * _rb.velocity.y);

	}

	void OnCollisionStay(Collision collision)
	{
		bool collisionIsGround = false;
		foreach (ContactPoint contactPoint in collision.contacts)
		{
			if (Vector3.Angle(Vector3.up, contactPoint.normal) < settings.maxSlope)
			{
				collisionIsGround = true;
			}
		}

		if (collisionIsGround)
		{
			IsGrounded = true;
			_airJumps = 0;
			_airMelee = false;
			_jumpForgivenessTime = settings.jumpForgiveness;
		}
	}


	private void LateUpdate()
	{
		if (this != instance)
		{
			Destroy(gameObject);
		}
	}
}
