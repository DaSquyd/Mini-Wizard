using System.Collections.Generic;
using UnityEngine;

public static class ConeCast
{
	public static RaycastHit[] ConeCastAll(Vector3 origin, Vector3 direction, float maxDistance, float coneAngle, LayerMask layerMask)
	{
		float maxRadius = maxDistance * Mathf.Sin(coneAngle / 2 * Mathf.Deg2Rad) / Mathf.Sin((90f - coneAngle / 2) * Mathf.Deg2Rad);

		RaycastHit[] sphereCastHits = Physics.SphereCastAll(origin - direction.normalized * maxRadius, maxRadius, direction, maxDistance, layerMask);
		List<RaycastHit> coneCastHitList = new List<RaycastHit>();

#if UNITY_EDITOR
		Debug.DrawLine(origin, origin + direction.normalized * maxDistance + Vector3.Cross(Vector3.up, direction).normalized * maxRadius, Color.red);
		Debug.DrawLine(origin, origin + direction.normalized * maxDistance + Vector3.Cross(Vector3.down, direction).normalized * maxRadius, Color.red);
		Debug.DrawLine(origin, origin + direction.normalized * maxDistance, Color.red);
#endif

		if (sphereCastHits.Length > 0)
		{
			for (int i = 0; i < sphereCastHits.Length; i++)
			{
				Vector3 hitPoint = sphereCastHits[i].point;
				Vector3 directionToHit = sphereCastHits[i].transform.position - origin;

				if (direction == Vector3.zero || directionToHit == Vector3.zero)
					continue;

				Quaternion originDirection = Quaternion.LookRotation(direction);
				Quaternion towardsTargetDirection = Quaternion.LookRotation(directionToHit);

				float angleToHit = Quaternion.Angle(originDirection, towardsTargetDirection);
				bool secondRaycastHit = Physics.Raycast(origin, sphereCastHits[i].transform.position - origin, out RaycastHit info, maxDistance);

				if (angleToHit < coneAngle && secondRaycastHit && info.collider == sphereCastHits[i].collider)
				{
					coneCastHitList.Add(sphereCastHits[i]);
				}
			}
		}

		RaycastHit[] coneCastHits = new RaycastHit[coneCastHitList.Count];
		coneCastHits = coneCastHitList.ToArray();

		return coneCastHits;
	}
}