using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
	public ProjectileSettings Settings;

	public Transform Target;
	public Entity Owner;

	public Element Element;

	float speed;
	float rotationSpeed;
	float maxRotationAngle;
	int damage;

	// Debug
	Color color = Color.red;

	private void Start()
	{
		speed = Settings.Speed;
		rotationSpeed = Settings.RotationSpeed;
		maxRotationAngle = Settings.MaxRotationAngle;
		damage = Settings.Damage;
		Destroy(gameObject, Settings.Lifetime);
	}

	private void Update()
	{
		Debug.DrawLine(transform.position, transform.position + transform.forward, color);

		if (Target != null)
		{
			Quaternion lookRotation = Quaternion.LookRotation(Target.position - transform.position);

			if (Quaternion.Angle(transform.rotation, lookRotation) > maxRotationAngle)
			{
				color = Color.red;
			}
			else
			{
				color = Color.green;
				transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
			}
		}

		transform.Translate(transform.forward * speed * Time.deltaTime, Space.World);
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
			entity.ApplyDamage(Owner, damage, Element);

			Destroy(gameObject);
			return;
		}
	}
}
