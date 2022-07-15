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
            for (int i = 0; i < maxTiles; i++)
            {
                Instantiate(tiles[(int)Random.Range(0, tiles.Count)].gameObject,
                    new Vector3(0, i + 2, 0), Quaternion.identity);
            }
        }

    }
}