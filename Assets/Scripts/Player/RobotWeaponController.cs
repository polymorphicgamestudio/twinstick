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

        // Start is called before the first frame update
        void Start()
        {
            Inst = ShepGM.inst;
            controller = GetComponent<RobotController>();
            currentWeapon = -1;

            Inst.actions.Player.Action.started += Shoot;
            Inst.actions.Player.Action.canceled += ShootCanceled;

            Inst.actions.Buildings.BuildingOne.performed += UnequipWeapon;
            Inst.actions.Buildings.BuildingTwo.performed += UnequipWeapon;
            Inst.actions.Buildings.BuildingThree.performed += UnequipWeapon;
            Inst.actions.Buildings.BuildingFour.performed += UnequipWeapon;
            Inst.actions.Buildings.BuildingFive.performed += UnequipWeapon;
            Inst.actions.Buildings.BuildingSix.performed += UnequipWeapon;
            Inst.actions.Buildings.BuildingSeven.performed += UnequipWeapon;



            Inst.actions.Player.WeaponOne.performed += WeaponOne;
            //inst.actions.Player.WeaponTwo.performed += WeaponTwo;
            //inst.actions.Player.WeaponThree.performed += WeaponThree;

        }


        private void Update()
        {

            if (currentWeapon == 0)
                weapons[currentWeapon].Update();


        }

        private void UnequipWeapon(InputAction.CallbackContext obj)
        {
            currentWeapon = -1;
        }

        private void WeaponOne(InputAction.CallbackContext obj)
        {
            if (currentWeapon == 0)
                return;

            currentWeapon = 0;
            weapons[currentWeapon].EquipWeapon(bulletSpawn);

        }

        //private void WeaponTwo(InputAction.CallbackContext obj)
        //{
        //    currentWeapon = 1;

        //}

        //private void WeaponThree(InputAction.CallbackContext obj)
        //{
        //    currentWeapon = 2;
        //}

        private void Shoot(InputAction.CallbackContext obj)
        {
            if (currentWeapon == 0)
                weapons[currentWeapon].shooting = true;

        }

        private void ShootCanceled(InputAction.CallbackContext obj)
        {
            if (currentWeapon == 0)
                weapons[currentWeapon].shooting = false;

        }



    }

}
