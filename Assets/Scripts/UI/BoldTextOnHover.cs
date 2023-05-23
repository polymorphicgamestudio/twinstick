using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class BoldTextOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

	TextMeshProUGUI text;

	void Start () {
		text = GetComponentInChildren<TextMeshProUGUI>();
	}

	public void OnPointerEnter(PointerEventData eventData) {
		SetBold();
	}
	public void OnPointerExit(PointerEventData eventData) {
		SetRegular();
	}


	public void SetBold() {
		text.fontStyle = FontStyles.Bold;
		Debug.Log("BOLD! " + text.text);
	}
	public void SetRegular() {
		text.fontStyle = FontStyles.Normal;
		text.color = Color.white;
	}
}