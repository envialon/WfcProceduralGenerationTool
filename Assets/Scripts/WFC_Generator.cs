using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace WFC_Procedural_Generator_Framework
{
    public class WFC_Generator : MonoBehaviour
    {
        public TileSet tileSet;
        public float tileSize;
        AdyacencyRulesGenerator adyacencyRulesGenerator;


        void Start()
        {
            //generate rules 
            adyacencyRulesGenerator = new AdyacencyRulesGenerator(tileSet, tileSize);

            //wave collapsing
            //creating the new mesh 

        }
    }
}