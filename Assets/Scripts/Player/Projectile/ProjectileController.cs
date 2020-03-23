using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
	public ProjectileSettings settings;

	public Transform Target
	{
		get; set;
	}
	public Entity Owner
	{
		get; set;
	}

	float _speed;
	float _rotationSpeed;
	float _maxRotationAngle;
	float _damage;

	// Debug
	Color _color = Color.red;

	private void Start()
	{
		_speed = settings.speed;
		_rotationSpeed = settings.rotationSpeed;
		_maxRotationAngle = settings.maxRotationAngle;
		_damage = settings.damage;
		Destroy(gameObject, settings.lifetime);
	}

	private void Update()
	{
		Debug.DrawLine(transform.position, transform.position + transform.forward, _color);

		if (Target != null)
		{
			Quaternion lookRotation = Quaternion.LookRotation(Target.position - transform.position);

			if (Quaternion.Angle(transform.rotation, lookRotation) > _maxRotationAngle)
			{
				_color = Color.red;
			}
			else
			{
				_color = Color.green;
				transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, Time.deltaTime * _rotationSpeed);
			}
		}

		transform.Translate(transform.forward * _speed * Time.deltaTime, Space.World);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (Owner == null)
			return;

		if (other.gameObject.layer == 9)
		{
			Destroy(gameObject);
			return;
		}

		Entity entity = other.GetComponentInParent<Entity>();
		if (entity != null && !entity.Equals(Owner) && entity.tag != "Projectile")
		{
			Owner.ApplyDamageToEntity(entity, _damage);

			Destroy(gameObject);
			return;
		}
	}
}
