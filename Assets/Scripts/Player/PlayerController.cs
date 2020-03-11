using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;

using Cinemachine;

public class PlayerController : MonoBehaviour
{
	// Static Variables
	public static PlayerController current;
	//public new static Camera camera;

	// Public Variables
	public PlayerSettings settings;
	public CameraSettings cameraSettings;
	public GameObject meshContainer;
	public CinemachineVirtualCamera vcam;

	public enum AttackState
	{
		Idle,
		Attack,
		Wait,
		PostAttack
	}

	public enum CameraMode
	{
		Manual,
		SettingToAuto,
		Auto
	}

	// Private Variables
	// CharacterController _controller;
	Rigidbody _rb;

	Vector3 _move;
	Vector3 _targetMove;
	Vector3 _lastTargetMove;
	Vector3 _meshMove;
	Vector3 _meshTargetMove = Vector3.forward;

	[DebugDisplay] Vector3 _velocityDisplay;

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

	AttackState _attackState;

	byte _currentAttack;
	bool _attackBuffer;
	float _swordAttackTime;

	bool _airAttack;

	bool _jumpInput;
	bool _jumpInputChange;
	[DebugDisplay] byte _airJumps;
	float _jumpForgivenessTime;
	float _jumpTime;

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

	[DebugDisplay] CameraMode _cameraMode = CameraMode.Auto;
	[DebugDisplay] float _cameraCooldownTime;

	[DebugDisplay] CinemachineSmoothPath _path;

	// DEBUG
	bool _debugMouseDisabled;

	private void Start()
	{
		current = this;
#if FALSE
		camera = Camera.main;
#endif

		//_controller = GetComponent<CharacterController>();
		_rb = GetComponent<Rigidbody>();

		_yaw = transform.rotation.eulerAngles.y;
		_lastTargetMove = new Vector3(Mathf.Sin(_yaw * Mathf.Deg2Rad), 0f, Mathf.Cos(_yaw * Mathf.Deg2Rad));

		_path = FindObjectOfType<CinemachineSmoothPath>();


	}

	private void Update()
	{
		if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha1))
		{
			_debugMouseDisabled = !_debugMouseDisabled;
		}

		_yaw = transform.rotation.eulerAngles.y;

		InputDevice inputDevice = InputManager.ActiveDevice;


		if (_attackState == AttackState.Attack)
		{
			_rb.velocity = Vector3.Lerp(_lastTargetMove * settings.attackCooldowns[_currentAttack - 1].force + (IsGrounded ? Vector3.zero : Vector3.down * settings.attackDownForce),
				IsGrounded ? Vector3.zero : Vector3.down * settings.attackDownForce,
				(settings.attackCooldowns[_currentAttack - 1].time - _swordAttackTime) / settings.attackCooldowns[_currentAttack - 1].time);
		}

		if (_swordAttackTime == 0)
		{
			switch (_attackState)
			{
				case AttackState.Attack:
					IsLocked = false;
					_attackBuffer = false;
					_rb.useGravity = true;
					if (settings.attackCooldowns.Length > _currentAttack)
					{
						_attackState = AttackState.Wait;
						_swordAttackTime = settings.attackCooldowns[_currentAttack].wait;
					}
					else
					{
						_attackState = AttackState.Idle;
						_swordAttackTime = settings.attackCooldowns[0].wait;
						_currentAttack = 0;
					}
					break;
				case AttackState.Wait:
					_attackState = AttackState.PostAttack;
					_swordAttackTime = settings.attackCooldowns[_currentAttack].comboForgiveness;
					break;
				case AttackState.PostAttack:
					_attackState = AttackState.Idle;
					_currentAttack = 0;
					_attackBuffer = false;
					break;
			}

			IsLocked = false;
		}
		else
		{
			_swordAttackTime = Mathf.MoveTowards(_swordAttackTime, 0f, Time.deltaTime);
		}


		if (!IsLocked)
		{
			SwordAttackInput(inputDevice);
			JumpInput(inputDevice);
		}

		TurnMesh(true);
		UpdateCamera(inputDevice);

		_velocityDisplay = _rb.velocity;
	}

	private void SwordAttackInput(InputDevice inputDevice)
	{
		if (inputDevice.Action3.WasPressed)
		{
			if (_attackState == AttackState.Idle)
			{
				if (_swordAttackTime == 0f)
					SwordAttack();
			}
			else
				_attackBuffer = true;
		}
		if (_attackState == AttackState.PostAttack && _attackBuffer)
		{
			SwordAttack();
		}
	}

	private void SwordAttack()
	{
		if (_airAttack && _currentAttack == 0)
			return;

		TurnMesh(false);

		if (!IsGrounded)
			_airAttack = true;


		_currentAttack++;

		_swordAttackTime = settings.attackCooldowns[_currentAttack - 1].time;
		_attackState = AttackState.Attack;

		//_rb.velocity = _lastTargetMove * settings.attackCooldowns[_currentAttack - 1]._force;
		_rb.useGravity = false;
		IsLocked = true;
	}

	private void JumpInput(InputDevice inputDevice)
	{
		_jumpInput = false;
		if (Input.GetAxis("Jump") > 0.2f && !_jumpInputChange)
		{
			_jumpInput = true;
			_jumpInputChange = true;
		}

		if (Input.GetAxis("Jump") <= 0.2f && _jumpInputChange)
		{
			_jumpInputChange = false;
		}

		if (_jumpTime > 0f)
		{
			_jumpTime = Mathf.Max(0f, _jumpTime - Time.deltaTime);
			return;
		}

		if (!inputDevice.Action1.WasPressed && !_jumpInput)
			return;

		if (_jumpForgivenessTime > 0f)
		{
			Jump(false);
		}
		else if (_airJumps < settings.maxAirJumps || inputDevice.LeftBumper)
		{
			Jump(true);
			_airJumps++;
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
		if (Mathf.Abs(_targetMove.magnitude) > 0f)
		{
			_meshTargetMove = _targetMove;
		}

		if (smoothed)
			_meshMove = Vector3.Slerp(_meshMove, _meshTargetMove, Time.deltaTime * settings.turnSpeed);
		else
			_meshMove = Vector3.RotateTowards(_meshMove, _meshTargetMove, Time.fixedDeltaTime * settings.attackTurnSpeed, 1f);

		float yaw = Mathf.Atan2(_meshMove.x, _meshMove.z) * Mathf.Rad2Deg;

		meshContainer.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
	}

	private void UpdateCamera(InputDevice inputDevice)
	{
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


		// User input
		float yaw = (inputDevice.RightStickX * settings.cameraJoystickSpeed) + (_debugMouseDisabled ? 0f : (Input.GetAxisRaw("mouse x") * settings.cameraMouseSpeed));
		float pitch = (inputDevice.RightStickY * settings.cameraJoystickSpeed) + (_debugMouseDisabled ? 0f : (Input.GetAxisRaw("mouse y") * settings.cameraMouseSpeed));

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
				if (inputDevice.RightStickButton.WasPressed)
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
					_cameraCooldownTime = settings.camera.autoCooldown;
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
				transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, _yaw, transform.rotation.eulerAngles.z);

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

				float distLocked = Mathf.Min(dist, settings.camera.autoFineTuneAngle) / settings.camera.autoFineTuneAngle;
				Rotation = Quaternion.RotateTowards(Quaternion.Euler(Rotation), Quaternion.Euler(autoPitch, autoYaw, 0f), settings.camera.autoSpeed * distLocked * Time.deltaTime).eulerAngles;
				transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, _yaw, transform.rotation.eulerAngles.z);

				// Check if player has changed camera									Checks buffer so it doesn't automatically activate right after setting to locked mode
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

		float xzLen = Mathf.Cos(_pitch * Mathf.Deg2Rad);

		float x = xzLen * Mathf.Sin(-_yaw * Mathf.Deg2Rad);
		float y = Mathf.Sin(_pitch * Mathf.Deg2Rad);
		float z = xzLen * -Mathf.Cos(_yaw * Mathf.Deg2Rad);


		float offsetPercent = Mathf.InverseLerp(cameraSettings.minOffsetPitch, cameraSettings.maxOffsetPitch, _pitch);

		float offset = Mathf.Lerp(cameraSettings.minOffset, cameraSettings.maxOffset, offsetPercent);


		Vector3 start = transform.position + (Vector3.up * offset);
		Vector3 direction = new Vector3(x, y, z);

		LayerMask mask = LayerMask.GetMask("Terrain");

		bool contact = Physics.SphereCast(start, cameraSettings.buffer, direction, out RaycastHit hit, cameraSettings.maxDistance, mask);
		
		float distance = Mathf.Max(cameraSettings.minDistance, hit.distance);

		if (!contact)
			distance = cameraSettings.maxDistance;

		Debug.DrawRay(start, direction * distance, Color.cyan);

		_pitch += pitch;

		while (_pitch > 180f)
			_pitch -= 360f;

		_pitch = Mathf.Clamp(_pitch, cameraSettings.minAngle, cameraSettings.maxAngle);

#if FALSE
		camera.transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);

		camera.transform.position = start + (direction * (distance - cameraSettings.buffer));
#endif
		vcam.transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
		vcam.transform.position = start + (direction * (distance - cameraSettings.buffer));

		transform.Rotate(new Vector3(0f, yaw, 0f));
	}


	private void FixedUpdate()
	{
		if (!IsGrounded)
		{
			_jumpForgivenessTime -= Time.fixedDeltaTime;
		}

		_jumpForgivenessTime = Mathf.Max(0f, _jumpForgivenessTime);


		InputDevice inputDevice = InputManager.ActiveDevice;

		if (!IsLocked)
			Movement(inputDevice);

		IsGrounded = false;

		_velocityDisplay = _rb.velocity;
	}

	private void Movement(InputDevice inputDevice)
	{
		float x = Mathf.Clamp(inputDevice.LeftStickX + Input.GetAxis("Horizontal"), -1f, 1f);
		float z = Mathf.Clamp(inputDevice.LeftStickY + Input.GetAxis("Vertical"), -1f, 1f);

		_targetMove = transform.right * x + transform.forward * z;

		if (_targetMove.magnitude > 0f)
		{
			_lastTargetMove = _targetMove.normalized;
		}

		if (_targetMove.magnitude > 1f)
		{
			_targetMove.Normalize();
		}

		_move = Vector3.Lerp(_move, _targetMove, Time.fixedDeltaTime * (IsGrounded ? settings.groundAcceleration : settings.airAcceleration));

		_rb.velocity = (_move * settings.speed) + (Vector3.up * _rb.velocity.y);

	}

	void OnCollisionStay(Collision collision)
	{
		bool collisionIsGround = false;
		foreach (ContactPoint contactPoint in collision.contacts)
		{
			if (Vector3.Angle(Vector3.up, contactPoint.normal) < settings.maxSlope)
			{
				//isGround = collision.collider.tag == "Ground";
				collisionIsGround = true;
			}
		}

		if (collisionIsGround)
		{
			IsGrounded = true;
			_airJumps = 0;
			_airAttack = false;
			_jumpForgivenessTime = settings.jumpForgiveness;
		}
	}

	private void LateUpdate()
	{
		if (this != current)
		{
			Destroy(gameObject, 3f);
		}
	}
}
