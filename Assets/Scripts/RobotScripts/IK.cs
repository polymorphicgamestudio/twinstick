using UnityEngine;

// To use, place this script on the IK target object and plug in transforms for the end bone and pole
// [ExecuteInEditMode]
public class IK : MonoBehaviour {

	public Transform end; //bone that sticks to IK
	private Transform parent;
	private Transform grandParent;
    private Transform greatGrandParent; //used for determining pole and the rotation target of end
    
	public Vector3 poleOffset = Vector3.zero; //local offset from origin. direction to bend towards

    [Space (10)]
    [Header ("Rotation Offests")]
	public float upperRotation;//Rotation offsets
	public float lowerRotation;
    public Vector3 endRotation;

    //values for use in cos rule
    private float a;
	private float b;
	private float c;
	private Vector3 en;//Normal of plane we want our arm to be on



	//debug pole positions while out of playmode
	/*
	void OnDrawGizmos() {
		Gizmos.color = Color.yellow;
		Transform ggp = end.parent.parent.parent;
		Vector3 pole = ggp.position + ggp.forward * poleOffset.z + ggp.right * poleOffset.x + ggp.up * poleOffset.y;
		Gizmos.DrawSphere(pole, 0.02f);
	}*/


	private void Start() {
		parent = end.parent;
		grandParent = parent.parent;
        greatGrandParent = grandParent.parent;
	}
	void LateUpdate() {
		if (!parent || !grandParent)
			return;

		a = parent.localPosition.magnitude;
		b = end.localPosition.magnitude;
		c = Vector3.Distance(grandParent.position, transform.position);
		Vector3 polePosition = greatGrandParent.position + greatGrandParent.forward * poleOffset.z + greatGrandParent.right * poleOffset.x + greatGrandParent.up * poleOffset.y;

		en = Vector3.Cross(transform.position - grandParent.position, polePosition - grandParent.position);

        Debug.DrawLine(parent.position, polePosition);
        //Debug.Log("The angle is: " + CosAngle(a, b, c));
        //Debug.DrawLine(grandParent.position, transform.position);
        //Debug.DrawLine((grandParent.position + transform.position) / 2, parent.position);


        
        #region "Rotations"
        //upper
        grandParent.rotation = Quaternion.LookRotation(transform.position - grandParent.position, Quaternion.AngleAxis(upperRotation, parent.position - grandParent.position) * (en));
		grandParent.rotation *= Quaternion.Inverse(Quaternion.FromToRotation(Vector3.forward, parent.localPosition));
		grandParent.rotation = Quaternion.AngleAxis(-CosAngle(a, c, b), -en) * grandParent.rotation;
        //lower
		parent.rotation = Quaternion.LookRotation(transform.position - parent.position, Quaternion.AngleAxis(lowerRotation, end.position - parent.position) * (en));
		parent.rotation *= Quaternion.Inverse(Quaternion.FromToRotation(Vector3.forward, end.localPosition));
		//end
		end.rotation = transform.rotation * Quaternion.Euler(endRotation);
		//end.rotation = greatGrandParent.rotation * Quaternion.Euler(endRotation);
		#endregion
	}


	//find angles using the cosine rule 
	float CosAngle(float a, float b, float c) {
		if (!float.IsNaN(Mathf.Acos((-(c * c) + (a * a) + (b * b)) / (-2 * a * b)) * Mathf.Rad2Deg)) {
			return Mathf.Acos((-(c * c) + (a * a) + (b * b)) / (2 * a * b)) * Mathf.Rad2Deg;
		}
		else {
			return 1;
		}
	}
}




/* https://docs.unity3d.com/Manual/InverseKinematics.html?_ga=2.11356150.356617217.1611084843-1526741662.1504116627
 * Not using because non-humanoid model, but here is Unity's build-in way of handeling IKs
 * https://blogs.unity3d.com/2018/08/27/animation-c-jobs/?_ga=2.120586442.356617217.1611084843-1526741662.1504116627
 * Tried this method, but I found it too complicated fro me to make it work with multiple limbs
 * https://wirewhiz.com/how-to-code-two-bone-ik-in-unity/
 * What this is based off
 */