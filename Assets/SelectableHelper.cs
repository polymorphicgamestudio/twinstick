using UnityEngine.UI;

public class SelectableHelper : Selectable{
	public bool isHighlighted {
		get {return IsHighlighted();}
	}
	public bool isPressed {
		get {return IsPressed();}
	}
}