using System.Collections.Generic;
using UnityEngine;


namespace WFC_Procedural_Generator_Framework
{
    public class WFCollapse : MonoBehaviour
    {
        public List<Tile> tiles = new List<Tile>();
        public int maxTiles = 10;
        void Start()
        {
            RandomGenerate();
        }

        public void RandomGenerate()
        {
    
        }

    }
}