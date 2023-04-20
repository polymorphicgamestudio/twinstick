using UnityEngine;

public class RangeIndicator : MonoBehaviour {
	[SerializeField] LayerMask layerMask;
	[SerializeField] float viewDistance = 10f;
	Mesh mesh;

	private void Start() {
		mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;
	}

	private void Update() {
		CalculateMesh();
	}

	void CalculateMesh() {
		int rayCount = 91;
		float rayAngle = 0f;

		Vector3[] verts = new Vector3[rayCount + 2]; // +1 for origin and +1 for ray at 0 rayAngle
		Vector2[] uv = new Vector2[verts.Length];
		int[] tris = new int[rayCount * 3];

		int vertIndex = 1;
		int triIndex = 0;
		
		for (int i = 0; i < rayCount; i++) {
			Vector3 vertex;
			RaycastHit hit;
			Physics.Raycast(transform.position, VectorFromAngle(rayAngle), out hit, viewDistance, layerMask);
			if (hit.collider == null)
				vertex = VectorFromAngle(rayAngle) * viewDistance;
			else
				vertex = hit.point - transform.position;

			verts[vertIndex] = vertex;

			if (i > 0) {
				tris[triIndex] = 0;
				tris[triIndex + 1] = vertIndex - 1;
				tris[triIndex + 2] = vertIndex;
				triIndex += 3;
			}

			uv[vertIndex] = new Vector2((float)vertIndex / verts.Length, 1);

			vertIndex++;
			rayAngle -= 360.0f / (rayCount - 1);
		}
		uv[0] = new Vector2(0.5f, 0);


		mesh.vertices = verts;
		mesh.uv = uv;
		mesh.triangles = tris;
		mesh.bounds = new Bounds(transform.position, Vector3.one * 100);
		//mesh.RecalculateBounds();
	}

	Vector3 VectorFromAngle(float angle) {
		// angle = 0 -> 360
		float angleRad = angle * Mathf.Deg2Rad;
		return new Vector3(Mathf.Cos(angleRad), 0, Mathf.Sin(angleRad));
	}
	float AngleFromVector(Vector3 dir) {
		dir.Normalize();
		float n = Mathf.Atan2(dir.x,dir.z) * Mathf.Rad2Deg;
		if (n < 0) n += 360;
		return n;
	}
}


//https://youtu.be/CSeUMTaNFYk