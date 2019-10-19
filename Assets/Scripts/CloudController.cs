using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    public class CloudController : MonoBehaviour
    {
        public float MovementSpeed;
        public void Update()
        {
            transform.Translate(Vector3.right * Time.deltaTime * MovementSpeed);
        }
    }
}
