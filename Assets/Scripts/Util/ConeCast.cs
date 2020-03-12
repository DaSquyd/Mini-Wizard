using System.Collections.Generic;
using UnityEngine;

public static class ConeCast
{
	public static RaycastHit[] ConeCastAll(Vector3 origin, Vector3 direction, float maxDistance, float coneAngle, LayerMask layerMask)
	{
		float maxRadius = maxDistance * Mathf.Sin(coneAngle / 2 * Mathf.Deg2Rad) / Mathf.Sin((90f - coneAngle / 2) * Mathf.Deg2Rad);

		RaycastHit[] sphereCastHits = Physics.SphereCastAll(origin - direction.normalized * maxRadius, maxRadius, direction, maxDistance, layerMask);
		List<RaycastHit> coneCastHitList = new List<RaycastHit>();

		Debug.DrawLine(origin, origin + direction.normalized * maxDistance + Vector3.Cross(Vector3.up, direction) * maxRadius, Color.red, Time.fixedDeltaTime);
		Debug.DrawLine(origin, origin + direction.normalized * maxDistance + Vector3.Cross(Vector3.down, direction) * maxRadius, Color.red, Time.fixedDeltaTime);

		if (sphereCastHits.Length > 0)
		{
			for (int i = 0; i < sphereCastHits.Length; i++)
			{
				Vector3 hitPoint = sphereCastHits[i].point;
				Vector3 directionToHit = hitPoint - origin;
				float angleToHit = Vector3.Angle(direction, directionToHit);
				bool secondRaycastHit = Physics.Raycast(origin, directionToHit.normalized, out RaycastHit info, maxDistance);


				if (angleToHit < coneAngle && secondRaycastHit && info.collider == sphereCastHits[i].collider)
				{
					//Debug.DrawRay(origin, directionToHit.normalized * info.distance, Color.blue, Time.fixedDeltaTime);
					coneCastHitList.Add(sphereCastHits[i]);
				}
			}
		}

		RaycastHit[] coneCastHits = new RaycastHit[coneCastHitList.Count];
		coneCastHits = coneCastHitList.ToArray();

		return coneCastHits;
	}
}