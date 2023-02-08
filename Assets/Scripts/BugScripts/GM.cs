using UnityEngine;

public class GM : MonoBehaviour {
	[HideInInspector] public static Colors cm; // set in relevant script's Awake function
}


/*
[HideInInspector] public static BoundsManager bounds; // set in relevant script's Awake function

static List<Transform>[] lists;
public enum ID {
	Player,
	Alien,
	PlayerProjectile,
	AlienProjectile,
	Asteroid
}

void Awake() {
	InitializeLists();
}

#region Lists Functions
void InitializeLists() {
	int numberOfIDs = System.Enum.GetNames(typeof(ID)).Length;
	lists = new List<Transform>[numberOfIDs];
	for (int i = 0; i < numberOfIDs; i++) {
		lists[i] = new List<Transform>();
	}
}

public static void Add(Transform transform, ID id) {
	lists[(int)id]?.Add(transform);
}

public static void Remove(Transform transform, ID id) {
	lists[(int)id]?.Remove(transform);
}

public static int GetCount(ID id) {
	return lists[(int)id].Count;
}

public static Transform GetRandom(ID id) {
	return lists[(int)id][Random.Range(0, GetCount(id))];
}

public static Transform GetNearest(ID id, Vector3 point) {
	if (GetCount(id) == 0) return null;
	float nearestSquareDistance = float.MaxValue;
	Transform nearestObject = null;
	for (int i = 0; i < GetCount(id); i++) {
		float sDist = Vector3.SqrMagnitude(
			point - lists[(int)id][i].position
		);
		if (sDist < nearestSquareDistance) {
			nearestSquareDistance = sDist;
			nearestObject = lists[(int)id][i];
		}
	}
	return nearestObject;
}
#endregion
*/
