using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace WFC_Procedural_Generator_Framework
{
    [System.Serializable]
    public class Tile : MonoBehaviour
    {
        public GameObject gameObject;
        public string xPos;
        public string yPos;
        public string zPos;
        public string xNeg;
        public string yNeg;
        public string zNeg;
    }
}