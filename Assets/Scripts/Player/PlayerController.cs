using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;

public class PlayerController : MonoBehaviour
{
	// Static Variables
	public static PlayerController current;
	public new static Camera camera;


	// Public Variables
	public PlayerSettings settings;
	public CameraSettings cameraSettings;

	// Private Variables
	// CharacterController _controller;
	Rigidbody _rb;
	GameObject _meshObject;

	Vector3 _move;
	Vector3 _meshMove;
	Vector3 _meshTargetMove = Vector3.forward;

	[DebugDisplay] Vector3 _velocityDisplay;

	[DebugDisplay] bool _isGrounded;

	bool _jumpInput;
	bool _jumpInputChange;
	bool _airJump;
	[DebugDisplay] float _jumpForgivenessTime;
	float _jumpTime;

	float _yaw;
	float _pitch;

	bool _debugMouseDisabled;

	private void Start()
	{
		current = this;
		camera = Camera.main;

		//_controller = GetComponent<CharacterController>();
		_rb = GetComponent<Rigidbody>();
		_meshObject = GetComponentInChildren<MeshRenderer>().gameObject;
	}

	private void Update()
	{
		InputDevice inputDevice = InputManager.ActiveDevice;

		if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha1))
		{
			_debugMouseDisabled = !_debugMouseDisabled;
		}

		_yaw = transform.rotation.eulerAngles.y;

		JumpInput(inputDevice);
		UpdateCamera(inputDevice);

		_velocityDisplay = _rb.velocity;
	}

	private void FixedUpdate()
	{
		if (!_isGrounded)
		{
			_jumpForgivenessTime -= Time.fixedDeltaTime;
		}

		_jumpForgivenessTime = Mathf.Max(0f, _jumpForgivenessTime);


		InputDevice inputDevice = InputManager.ActiveDevice;

		Movement(inputDevice);

		_isGrounded = false;


		_velocityDisplay = _rb.velocity;
	}

	void OnCollisionStay(Collision collision)
	{
		bool isGround = false;
		foreach (ContactPoint contactPoint in collision.contacts)
		{
			if (Vector3.Angle(Vector3.up, contactPoint.normal) < settings.maxSlope)
			{
				//isGround = collision.collider.tag == "Ground";
				isGround = true;
			}
		}

		if (isGround)
		{
			_isGrounded = true;
			_airJump = false;
			_jumpForgivenessTime = settings.jumpForgiveness;
		}
	}

	private void Movement(InputDevice inputDevice)
	{
		float x = Mathf.Clamp(inputDevice.LeftStickX + Input.GetAxis("Horizontal"), -1f, 1f);
		float z = Mathf.Clamp(inputDevice.LeftStickY + Input.GetAxis("Vertical"), -1f, 1f);

		Vector3 _targetMove = transform.right * x + transform.forward * z;

		if (Mathf.Abs(_targetMove.magnitude) > 1f)
		{
			_targetMove.Normalize();
		}

		if (Mathf.Abs(_targetMove.magnitude) > 0f)
		{
			_meshTargetMove = _targetMove;
		}

		_move = Vector3.Lerp(_move, _targetMove, Time.fixedDeltaTime * (_isGrounded ? settings.groundAcceleration : settings.airAcceleration));
		_meshMove = Vector3.Slerp(_meshMove, _meshTargetMove, Time.fixedDeltaTime * settings.turnSpeed);

		float yaw = Mathf.Atan2(_meshMove.x, _meshMove.z) * Mathf.Rad2Deg;

		_meshObject.transform.rotation = Quaternion.Euler(0f, yaw, 0f);

		_rb.velocity = (_move * settings.speed) + (Vector3.up * _rb.velocity.y);

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
			Jump();
			Debug.Log(_jumpForgivenessTime);
		}
		else if (!_airJump || inputDevice.LeftBumper)
		{
			Jump();
			_airJump = true;
		}
	}

	private void Jump()
	{
		_rb.velocity = new Vector3(_rb.velocity.x, settings.jumpStrength, _rb.velocity.z);
		_jumpTime = settings.jumpCooldown;
		_isGrounded = false;
		Debug.Log("Jump");
	}

	private void UpdateCamera(InputDevice inputDevice)
	{
		float yaw = (inputDevice.RightStickX * settings.cameraJoystickSpeed) + (_debugMouseDisabled ? 0f : (Input.GetAxis("mouse x") * settings.cameraMouseSpeed));
		float pitch = (inputDevice.RightStickY * settings.cameraJoystickSpeed) + (_debugMouseDisabled ? 0f : (Input.GetAxis("mouse y") * settings.cameraMouseSpeed));

		float xzLen = Mathf.Cos(_pitch * Mathf.Deg2Rad);

		float x = xzLen * Mathf.Sin(-_yaw * Mathf.Deg2Rad);
		float y = Mathf.Sin(_pitch * Mathf.Deg2Rad);
		float z = xzLen * -Mathf.Cos(_yaw * Mathf.Deg2Rad);


		Vector3 start = transform.position + (Vector3.up * cameraSettings.verticalOffset);
		Vector3 direction = new Vector3(x, y, z);

		Physics.Raycast(start, direction, out RaycastHit hit, cameraSettings.maxDistance);

		float distance = Mathf.Max(cameraSettings.minDistance, hit.distance);

		if (!hit.collider)
		{
			distance = cameraSettings.maxDistance;
		}

		Debug.DrawRay(start, direction * (distance - cameraSettings.buffer));

		_pitch += pitch;

		_pitch = Mathf.Clamp(_pitch, cameraSettings.minAngle, cameraSettings.maxAngle);

		camera.transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);

		camera.transform.position = start + (direction * (distance - cameraSettings.buffer));

		transform.Rotate(new Vector3(0f, yaw, 0f));
	}

	private void LateUpdate()
	{
		if (this != current)
		{
			Destroy(gameObject, 3f);
		}
	}
}
