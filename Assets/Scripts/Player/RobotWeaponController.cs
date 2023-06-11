using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ShepProject
{

    public class RobotWeaponController : MonoBehaviour
    {

        private ShepGM Inst;
        private RobotController controller;

        [SerializeField]
        private Transform bulletSpawn;

        public int currentWeapon;
        public WeaponBase[] weapons;


        private void Awake()
        {

            currentWeapon = 0;
            weapons[currentWeapon].EquipWeapon(bulletSpawn);


        }


        // Start is called before the first frame update
        void Start()
        {
            Inst = ShepGM.inst;
            controller = GetComponent<RobotController>();

            Inst.Input.actionSelectionChanged += ActionSelectionChanged;

            //Inst.actions.Player.Action.started += Shoot;
            //Inst.actions.Player.Action.canceled += ShootCanceled;
        }

        private void ActionSelectionChanged(int previousAction, int currentAction)
        {
            if (currentAction > 2)
            {
                currentWeapon = -1;
                return;
            }

            if (previousAction != currentAction)
            {

                //just doing this for now since we only have first weapon setup
                if (currentAction == 0)
                {
                    currentWeapon = currentAction;
                    weapons[currentWeapon].EquipWeapon(bulletSpawn);

                }
                else
                {
                    currentWeapon = -1;
                }
            }




        }

        private void Update()
        {

            if (currentWeapon == 0)
            {
                weapons[currentWeapon].Update();

                if (!weapons[currentWeapon].shooting)
                {

                    if (Inst.Input.MouseOverHUD())
                        return;
                }

                weapons[currentWeapon].shooting = Inst.Input.ActionSelected;

            }



        }


    }

}
