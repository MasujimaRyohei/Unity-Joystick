using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualInput
{
    public class Example : MonoBehaviour
    {

        public GameObject leftCubeObject;
        public GameObject rightCubeObject;

        private Joystick leftJoystick;
        private Joystick rightJoystick;

        // Use this for initialization
        void Start()
        {
            leftJoystick = Joystick.GetJoystick(0);
            rightJoystick = Joystick.GetJoystick(1);
        }

        // Update is called once per frame
        void Update()
        {
            leftCubeObject.transform.Rotate(leftJoystick.AxisRaw);
            rightCubeObject.transform.Rotate(rightJoystick.AxisRaw);
        }
    }
}