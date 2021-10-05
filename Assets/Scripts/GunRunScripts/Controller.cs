using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace com.braineeeeDevs.gunRun
{
    /// <summary>
    /// The tie-in controller for controlling GroundVehicles.
    /// </summary>
    public class Controller : MonoBehaviour
    {
        public GroundVehicle puppet;
        public Vector3 mousePosition, drive_and_steering;
        protected bool applyBrakes, toggleDriveState = false;
        /// <summary>
        /// Sets the vehicle under control.
        /// </summary>
        /// <param name="p">The vehicle to take control of.</param>
        public void SetPuppet(GroundVehicle p)
        {
            this.puppet = p;
        }
        /// <summary>
        /// Retrieves inputs from the Unity InputManager. (The new InputSystem sucks).
        /// </summary>
        public void GetInputs()
        {
            applyBrakes = Input.GetAxisRaw("Jump") > 0f;
            toggleDriveState = applyBrakes && Input.GetAxisRaw("DriveShifter") > 0f;
            drive_and_steering = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0f);
            mousePosition = new Vector3(Input.mousePosition.x - Screen.width * 0.5f, Input.mousePosition.y - Screen.height * 0.5f, Input.GetAxisRaw("Mouse ScrollWheel"));
        }
        /// <summary>
        /// Applies the input to the vehicle under control.
        /// </summary>
        void ApplyInputsTo()
        {
            puppet.applyingBrakes = applyBrakes;
            puppet.SteeringAndDrive = new Vector3(drive_and_steering.x, drive_and_steering.y, 0f);
            if (toggleDriveState)
            {
                puppet.Shift();
            }
            
        }

        private void Update()
        {
            GetInputs();
        }


        void FixedUpdate()
        {
            if (puppet != null)
            {
                ApplyInputsTo();
            }
        }

    }
}
