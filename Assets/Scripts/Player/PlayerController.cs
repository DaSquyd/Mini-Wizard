using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
	// Static Variables
	public static PlayerController current;
	public new static Camera camera;


	// Public Variables
	public PlayerSettings settings;


	// Private Variables
	CharacterController _controller;
	GameObject _meshObject;

	Vector3 _move;
	Vector3 _targetMove;
	Vector3 _meshMove;
	Vector3 _meshTargetMove = Vector3.forward;

	public Vector3 _velocity;

	public bool _jumpInput;
	public bool _jumpInputChange;
	public bool _airJump;
	public float _jumpTime;

	float _yaw;
	float _pitch;

	private void Start()
	{
		current = this;
		camera = Camera.main;

		_controller = GetComponent<CharacterController>();
		_meshObject = GetComponentInChildren<MeshRenderer>().gameObject;
	}

	private void Update()
	{
		InputDevice inputDevice = InputManager.ActiveDevice;

		_yaw = transform.rotation.eulerAngles.y;

		Movement(inputDevice);
		Gravity();
		JumpInput(inputDevice);
		UpdateCamera(inputDevice);
	}

	void OnCollisionStay(Collision collision)
	{
		bool isGround = false;
		foreach (ContactPoint contactPoint in collision.contacts)
		{
			if (Vector3.Angle(Vector3.up, contactPoint.normal) < _controller.slopeLimit)
			{
				isGround = collision.collider.tag == "Ground";
			}
		}

		if (isGround)
		{
			Debug.Log(collision.impulse);
			GetComponent<Rigidbody>().AddForce(collision.impulse, ForceMode.Impulse);
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

		_move = Vector3.Lerp(_move, _targetMove, Time.deltaTime * settings.acceleration);
		_meshMove = Vector3.Slerp(_meshMove, _meshTargetMove, Time.deltaTime * settings.acceleration);

		float yaw = Mathf.Atan2(_meshMove.x, _meshMove.z) * Mathf.Rad2Deg;

		_meshObject.transform.rotation = Quaternion.Euler(0f, yaw, 0f);

		_controller.Move(_move * settings.speed * Time.deltaTime);

	}

	private void Gravity()
	{
		_velocity.y = Mathf.Max(_velocity.y + (settings.gravity * Time.deltaTime), -settings.maxFallSpeed);

		_controller.Move(_velocity * Time.deltaTime);

		if (_controller.isGrounded)
		{
			_velocity.y = -2f;
			_airJump = false;
		}
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

		if (_controller.isGrounded)
		{
			Jump();
		}
		else if (!_airJump || inputDevice.LeftBumper)
		{
			Jump();
			_airJump = true;
		}
	}

	private void Jump()
	{
		_velocity.y = settings.jumpStrength;
		_jumpTime = settings.jumpCooldown;
	}

	private void UpdateCamera(InputDevice inputDevice)
	{
		float yaw = (inputDevice.RightStickX * settings.cameraJoystickSpeed) + (Input.GetAxis("mouse x") * settings.cameraMouseSpeed);
		float pitch = (inputDevice.RightStickY * settings.cameraJoystickSpeed) + (Input.GetAxis("mouse y") * settings.cameraMouseSpeed);

		float xzLen = Mathf.Cos(_pitch * Mathf.Deg2Rad);

		float x = xzLen * Mathf.Sin(-_yaw * Mathf.Deg2Rad);
		float y = Mathf.Sin(_pitch * Mathf.Deg2Rad);
		float z = xzLen * -Mathf.Cos(_yaw * Mathf.Deg2Rad);


		Vector3 start = transform.position + (Vector3.up * 1f);
		Vector3 direction = new Vector3(x, y, z);

		Physics.Raycast(start, direction, out RaycastHit hit, settings.cameraMaxDistance);

		float distance = Mathf.Max(settings.cameraMinDistance, hit.distance);

		if (!hit.collider)
		{
			distance = settings.cameraMaxDistance;
		}

		Debug.DrawRay(start, direction * distance);

		_pitch += pitch;

		_pitch = Mathf.Clamp(_pitch, settings.cameraMinAngle, settings.cameraMaxAngle);

		camera.transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);

		camera.transform.position = start + (direction * distance);

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
