using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShepProject
{

    public class InputManager : SystemBase
    {

        public int currentActionSelection;
        public PlayerInputActions actions;

        public event EventTrigger<bool> pauseEvent;
        private bool paused;


        private void Awake()
        {

            actions = new PlayerInputActions();
            actions.Player.Enable();
            actions.UI.Enable();
            actions.Buildings.Enable();


            actions.UI.Pause.performed += PauseEvent;


        }


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