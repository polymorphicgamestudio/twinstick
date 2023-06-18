using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ShepProject
{

    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Animator))]
    public class SheepPhysicsMethods : MonoBehaviour
    {

        [SerializeField]
        private int sheepID;
        public int SheepID => sheepID;
        public bool Initialized => sheepID != -1;
        public float speed;

        [SerializeField]
        private Rigidbody rb;
        [SerializeField]
        private Animator animator;
        private NPCManager manager;

        public void SetInitialInfo(ushort sheepID, NPCManager manager)
        {

            this.manager = manager;
            this.sheepID = sheepID;

        }

        public void SetVelocity(Vector3 velocity)
        {
            rb.velocity = velocity;

        }

        public void UnsetKinematic()
        {
            rb.isKinematic = false;
        }

        public void SetSlimeDistance(float slimeDistance)
        {

            if (slimeDistance > 64)
            {
                animator.SetBool("Moving", false);
                animator.SetBool("Running", false);

                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

            }
            else if (slimeDistance > 36)
            {
                animator.SetBool("Moving", true);
                animator.SetBool("Running", false);

                rb.velocity = transform.forward * (speed /2f);

            }
            else if (slimeDistance > 4)
            {
                animator.SetBool("Moving", true);
                animator.SetBool("Running", true);

                rb.velocity = transform.forward * speed;

            }
            else
            {
                //dead sheep
                manager.OnSheepDeath((ushort)sheepID);

            }

        }

        public void WaveEndInfoSet()
        {


        }

    }


}