using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour {

	float smoothTime = 0;
	public TextMeshProUGUI counter;
	
	void Start() {
		Application.targetFrameRate = 999;
	}
	void Update() {
		smoothTime = Mathf.Lerp(smoothTime, 1f / Time.unscaledDeltaTime,Time.deltaTime);
		counter.text = "FPS: " + smoothTime.ToString("000");
    }
}