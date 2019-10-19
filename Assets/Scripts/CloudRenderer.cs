using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    public class CloudRenderer : MonoBehaviour
    {
        public GameObject[] Clouds;
        public int CloudCount;

        public void Start()
        {
            for (int i = 0; i < CloudCount; i++)
            {
                GameObject cl = Instantiate(Clouds[UnityEngine.Random.Range(0, Clouds.Length - 1)]);

                int randx = UnityEngine.Random.Range(-30, 1100);
                int randy = UnityEngine.Random.Range(30, -230);

                cl.transform.position = new Vector3(randx, -10.5f, randy);
            }
        }

    }
}
