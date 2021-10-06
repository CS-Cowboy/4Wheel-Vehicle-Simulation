using UnityEngine;

namespace com.braineeeeDevs.gunRun
{
    [System.Serializable]
    /// <summary>
    /// A class to represent and tie together wheel collider, meshes, and functionality.
    /// </summary>
    [RequireComponent(typeof(Calculus))]

    [RequireComponent(typeof(Calculus))]
    public class Wheel : VehicleComponent
    {
        public WheelCollider wheelCollider;
        public MeshRenderer mesh;
        public Calculus wheelCalculus, slipCalculus;
        public float slip, tireDiameterInMeters, inertia, drivingForce, antiRollForce = 1f, adjustedTorque;
        public uint wheelNumber = 0;
        public bool isTurning = false;
        public float AngularVelocity
        {
            get
            {
                return wheelCalculus.Velocity;
            }
        }
        public float AngularAcceleration
        {
            get
            {
                return wheelCalculus.Acceleration;
            }
        }

        public float SwayBarForce
        {
            set
            {
                antiRollForce = value > 0f ? value : -1f * value;
            }
        }

        public float SteerAngle
        {
            set
            {
                wheelCollider.steerAngle = value;
            }
        }
        public override void Start()
        {
            base.Start();
            owner = GetComponentInParent<GroundVehicle>();
            mesh = GetComponent<MeshRenderer>();
            wheelCalculus = GetComponent<Calculus>();
            slipCalculus = GetComponent<Calculus>();
        }
        /// <summary>
        /// Drives the given wheel. Divides the given engine torque to individual wheel torque range.
        /// </summary>
        /// <param name="inputTorque">The engine torque.</param>
        /// <param name="steerAngle">The angle to steer at (if steerable).</param>
        public override void Operate(float inputTorque)
        {
            if (owner != null && wheelCollider != null)
            {
                adjustedTorque = ComputeSlip(inputTorque);
                wheelCollider.motorTorque = adjustedTorque;
                wheelCollider.brakeTorque = 0f;
                ApplyPose();
                wheelCalculus.Compute(mesh.transform.localEulerAngles.z * Mathf.Deg2Rad);
                owner.RBPhysics.AddForceAtPosition(Vector3.up * -antiRollForce, transform.parent.position);
                isTurning = Mathf.Abs(AngularVelocity) >= 1f;
                Debug.Log(wheelCalculus.Velocity);
            }
        }
        public void ApplyBrake()
        {
            wheelCollider.brakeTorque = ((owner.traits.massInKg + wheelCollider.mass) * owner.traits.brakingForce);
            wheelCollider.motorTorque = 0f;
        }
        /// Computes the wheel's slip and speed.
        /// </summary>
        protected float ComputeSlip(float inputTorque)
        {
            var rOmega = Mathf.Clamp(AngularVelocity * wheelCollider.radius, wheelCollider.radius, owner.traits.topSpeed * wheelCollider.radius);
            inertia = Mathf.Clamp(AngularVelocity, 1f, owner.traits.topSpeed) * Mathf.Pow(wheelCollider.radius, 2f) * wheelCollider.mass;
            slip = (rOmega - owner.RBPhysics.velocity.magnitude) / rOmega;
            var a = (owner.Acceleration.magnitude / Mathf.Clamp(owner.Velocity.magnitude, 1f, float.MaxValue)) * (slip - 1f);
            var b = (owner.Velocity.magnitude / (inertia * wheelCollider.radius * Mathf.Clamp(Mathf.Pow(wheelCalculus.Velocity, 2f), 1f, float.MaxValue))) * (inputTorque - wheelCollider.radius * (drivingForce));
            drivingForce = a + b;
            //  Debug.Log(string.Format("NonLinearSlip for wheel {0}:=>> {1} ", wheelNumber, nonLinearSlip));
            return inputTorque;
        }

        /// <summary>
        /// Applies the wheel's position and rotation changes to the wheel mesh.
        /// </summary>
        protected void ApplyPose()
        {
            Vector3 wheelPos = new Vector3();
            Quaternion wheelRot = new Quaternion();
            wheelCollider.GetWorldPose(out wheelPos, out wheelRot);
            transform.parent.rotation = wheelRot * Quaternion.Euler(0f, 180f, 0f);
        }

    }
}