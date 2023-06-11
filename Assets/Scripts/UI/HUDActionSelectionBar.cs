using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace ShepProject {

	public class HUDActionSelectionBar : MonoBehaviour 
	{

		[SerializeField] 
        RectTransform selectionIndicator;
        private float distanceChange;

        [SerializeField]
        private Button[] buttons;


        private void Start()
        {
            ShepGM.inst.Input.actionSelectionChanged += SetSelection;
            ShepGM.inst.Input.SetupSlotButtons(buttons);

        }

        private void Update()
        {
            if (math.abs(distanceChange) < 0.1f)
                return;

            selectionIndicator.anchoredPosition += new Vector2(((distanceChange * Time.deltaTime) * 10), 0);
            distanceChange -= ((distanceChange * Time.deltaTime) * 10);
        }


        public void SetSelection(int previousAction, int currentAction) 
		{

            distanceChange += ((currentAction - previousAction) * 141);

        }



    }
}