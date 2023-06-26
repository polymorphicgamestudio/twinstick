using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ShepProject
{

    public class InputManager : SystemBase
    {

        private int previousActionSelection;
        private int currentActionSelection;
        private PlayerInputActions actions;

        public PlayerInputActions Actions => actions;

        public event EventTrigger<int, int> actionSelectionChanged;
        public event EventTrigger<bool> pauseEvent;
        private bool paused;
        private bool actionSelected;
         

        [SerializeField]
        private Canvas canvas;
        [SerializeField]
        private EventSystem eventSystem;
        //private GraphicRaycaster graphicRaycaster;

        [SerializeField]
        private GraphicRaycaster[] raycasters;

        private PointerEventData pointerEventData;
        private List<RaycastResult> raycastResults;

        public bool ActionSelected => actionSelected;

        public int PreviousActionSelection => previousActionSelection;
        public int CurrentActionSelection => currentActionSelection;


        private void Awake()
        {
            previousActionSelection = 0;
            currentActionSelection = 0;


            pointerEventData = new PointerEventData(eventSystem);
            raycastResults = new List<RaycastResult>(20);

            actions = new PlayerInputActions();
            actions.Player.Enable();
            actions.UI.Enable();
            actions.Buildings.Enable();

            #region Input Callbacks

            actions.UI.Pause.performed += PauseEvent;

            actions.UI.SlotOne.performed += SlotOne;
            actions.UI.SlotTwo.performed += SlotTwo;
            actions.UI.SlotThree.performed += SlotThree;
            actions.UI.SlotFour.performed += SlotFour;
            actions.UI.SlotFive.performed += SlotFive;
            actions.UI.SlotSix.performed += SlotSix;
            actions.UI.SlotSeven.performed += SlotSeven;
            actions.UI.SlotEight.performed += SlotEight;
            actions.UI.SlotNine.performed += SlotNine;
            actions.UI.SlotTen.performed += SlotTen;

            actions.Player.ActionSelectionDown.started += SelectionToLeft;
            actions.Player.ActionSelectionDown.started += SelectionToRight;
            actions.Player.Action.started += ActionStarted;
            actions.Player.Action.canceled += ActionCanceled;

            #endregion

            if (canvas == null)
            {
                Debug.LogError("Canvas not set in Input Manager, needed for towers and shooting!");

            }
            else
            {
                //graphicRaycaster = canvas.GetComponent<GraphicRaycaster>();

            }




        }

        public void ManualUpdate()
        {

            for (int i = 0; i < raycasters.Length; i++)
            {

                raycastResults.Clear();
                pointerEventData.position = actions.Player.MousePosition.ReadValue<Vector2>();
                raycasters[i].Raycast(pointerEventData, raycastResults);

                if (raycastResults.Count > 0)
                    break;

            }



        }

        private void SetSelectedAction(int actionIndex)
        {
            currentActionSelection = actionIndex;
            ActionSelectionChanged();
        }

        private void ActionSelectionChanged()
        {

            if (currentActionSelection != previousActionSelection)
            {
                actionSelectionChanged?.Invoke(previousActionSelection, currentActionSelection);
            }

            previousActionSelection = currentActionSelection;
        }


        public void SetupSlotButtons(Button[] buttons)
        {

            for (int i = 0; i < buttons.Length; i++)
            {
                int val = i;
                buttons[i].onClick.AddListener(delegate() { SetSelectedAction(val); });
            }
            

        }

        public bool MouseOverHUD()
        {

            return raycastResults.Count > 0;
        }



        #region Slot Callbacks


        #region Keyboard

        private void SlotOne(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            SetSelectedAction(0);
        }

        private void SlotTwo(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            SetSelectedAction(1);
        }

        private void SlotThree(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            SetSelectedAction(2);
        }

        private void SlotFour(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            SetSelectedAction(3);
        }

        private void SlotFive(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            SetSelectedAction(4);
        }

        private void SlotSix(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            SetSelectedAction(5);
        }

        private void SlotSeven(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            SetSelectedAction(6);
        }

        private void SlotEight(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            SetSelectedAction(7);
        }

        private void SlotNine(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            SetSelectedAction(8);
        }

        private void SlotTen(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            SetSelectedAction(9);
        }

        #endregion

        #region Gamepad



        private void ActionStarted(InputAction.CallbackContext obj)
        {


            if (MouseOverHUD())
                return;

            actionSelected = true;

        }

        private void ActionCanceled(InputAction.CallbackContext obj)
        {

            actionSelected = false;

        }



        private void SelectionToLeft(InputAction.CallbackContext obj)
        {

            if (currentActionSelection > 0)
            {
                currentActionSelection--;
            }


        }

        private void SelectionToRight(InputAction.CallbackContext obj)
        {

            if (currentActionSelection < 9)
                currentActionSelection++;


        }



        #endregion

        #endregion

        private void PauseEvent(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {

            paused = !paused;
            pauseEvent.Invoke(paused);
        
        }

        private void OnDestroy()
        {

            actions.Dispose();

        }



    }


}