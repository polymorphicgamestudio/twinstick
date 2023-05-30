using ShepProject;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public class HUDControllerNavigation : MonoBehaviour {

	[SerializeField] Selectable[] selectables;
	[SerializeField] RectTransform selectionIndicator;
	GameObject selected;
	Vector2 selectionPosition;
	float selectionWidth;
	bool usingController = true;
	bool selectionChanged = false;

	[HideInInspector] public UnityEvent eventValueChange = new UnityEvent();


	void Start() {
		StartCoroutine(nameof(_InitializeNextFrame));
	}
	void Update() {
		SetActiveOnMouseHover();
		CheckForSelectionChange();
		if (selectionChanged) 
			NewSelectionIndicatorPositionandSize();
		selectionIndicator.anchoredPosition = Vector2.Lerp(selectionIndicator.anchoredPosition, selectionPosition, 20f * Time.deltaTime);
	}
	void NewSelectionIndicatorPositionandSize() {
		RectTransform selectiontransform = selected.GetComponent<RectTransform>();
		selectionPosition = selectiontransform.anchoredPosition;
		selectionWidth = selectiontransform.sizeDelta.x;
	}
	void SetActiveOnMouseHover() {
		if (usingController) return;
		foreach (Selectable s in selectables) {
			if (s.GetComponent<SelectableHelper>().isHighlighted)
				s.Select();
		}
	}
	void CheckForSelectionChange() {
		if (!EventSystem.current.currentSelectedGameObject)
			selectables[0].Select();
		GameObject newSelection = EventSystem.current.currentSelectedGameObject;
		selectionChanged = selected != newSelection;
		selected = newSelection;
	}
	void InitializeControllerSelection() {
		if (EventSystem.current.currentSelectedGameObject) return;
		selectables[0].Select();
		selected = EventSystem.current.currentSelectedGameObject;
	}
	private void Navigate_performed(InputAction.CallbackContext context) {
		if (usingController) return;
		InitializeControllerSelection();
		usingController = true;
	}
	private void MouseDelta_performed(InputAction.CallbackContext context) {
		if (!usingController) return;
		usingController = false;
	}
	private IEnumerator _InitializeNextFrame() {
		yield return null;
		selectables[0].Select();
		selected = EventSystem.current.currentSelectedGameObject;
		NewSelectionIndicatorPositionandSize();
		ShepGM.inst.actions.UI.Navigate.performed += Navigate_performed;
		ShepGM.inst.actions.Player.MouseDelta.performed += MouseDelta_performed;
	}
}