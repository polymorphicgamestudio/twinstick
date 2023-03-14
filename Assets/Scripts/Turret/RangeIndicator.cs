using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeIndicator : MonoBehaviour {
	[SerializeField] LayerMask layerMask;
	[SerializeField] Transform center;
	[SerializeField] float viewDistance = 10f;
	Mesh mesh;

	private void Start() {
		mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;
	}

	private void Update() {

		Vector3 origin = center.position;
		int rayCount = 91;
		float rayAngle = 0f;
		float angleIncrease = 360.0f / (rayCount - 1);
		

		Vector3[] verts = new Vector3[rayCount + 2]; // +1 for origin and +1 for ray at 0 rayAngle
		Vector2[] uv = new Vector2[verts.Length];
		int[] tris = new int[rayCount * 3];

		verts[0] = origin;
		int vertIndex = 1;
		int triIndex = 0;

		for (int i = 0; i < rayCount; i++) {
			Vector3 vertex;
			RaycastHit hit;
			Physics.Raycast(origin, GetVectorFromAngle(rayAngle), out hit, viewDistance, layerMask);
			if (hit.collider == null)
				vertex = origin + GetVectorFromAngle(rayAngle) * viewDistance;
			else
				vertex = hit.point;



			verts[vertIndex] = vertex;

			
			//for now only run when i>0   later conncet this first tri to the last position to close circle?
			if (i > 0) {
				tris[triIndex] = 0;
				tris[triIndex + 1] = vertIndex - 1;
				tris[triIndex + 2] = vertIndex;
				triIndex += 3;
			}


			vertIndex++;
			rayAngle -= angleIncrease;
		}
		


		mesh.vertices = verts;
		mesh.uv = uv;
		mesh.triangles = tris;
	}

	Vector3 GetVectorFromAngle(float angle) {
		// angle = 0 -> 360
		float angleRad = angle * Mathf.Deg2Rad;
		return new Vector3(Mathf.Cos(angleRad), 0, Mathf.Sin(angleRad));
	}
	float GetAngleFromVector(Vector3 dir) {
		dir.Normalize();
		float n = Mathf.Atan2(dir.x,dir.z) * Mathf.Rad2Deg;
		if (n < 0) n += 360;
		return n;
	}
}


//https://youtu.be/CSeUMTaNFYk