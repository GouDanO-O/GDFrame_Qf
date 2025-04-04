// Designed by KINEMATION, 2023

using UnityEngine;

namespace KINEMATION.KAnimationCore.Runtime.Core
{
    public struct FloatSpringState
    {
        public float Velocity;
        public float Error;

        public void Reset()
        {
            Error = Velocity = 0f;
        }
    }

    public class KSpringMath
    {
        public static float FloatSpringInterp(float current, float target, float speed, float criticalDamping,
            float stiffness, float scale, ref FloatSpringState state)
        {
            var interpSpeed = Mathf.Min(Time.deltaTime * speed, 1f);

            if (!Mathf.Approximately(interpSpeed, 0f))
            {
                var damping = 2 * Mathf.Sqrt(stiffness) * criticalDamping;
                var error = target * scale - current;
                var errorDeriv = error - state.Error;
                state.Velocity += error * stiffness * interpSpeed + errorDeriv * damping;
                state.Error = error;

                var value = current + state.Velocity * interpSpeed;
                return value;
            }

            return current;
        }
    }
}