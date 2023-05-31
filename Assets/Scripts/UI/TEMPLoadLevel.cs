using UnityEngine;
using UnityEngine.SceneManagement;

public class TEMPLoadLevel : MonoBehaviour {

	public void LoadScene(int index) {
		SceneManager.LoadScene(index);
	}
}