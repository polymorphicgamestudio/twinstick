using UnityEngine;
public class CreateMeshBounds : MonoBehaviour {
	void Start() {
		Mesh m = GetComponent<MeshFilter>().mesh;
		m.bounds = new Bounds(Vector3.up, Vector3.one);
	}
}