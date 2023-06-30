using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using WFC_Model;
public class TileEncodingTests
{
    [Test]
    public void TestTileIdEncoding()
    {
        Dictionary<int, SymmetryType> dic = new Dictionary<int, SymmetryType>();
        dic.Add(12, SymmetryType.T);
        Tile tile = new Tile(12, 3, SymmetryType.T);
        int encoded = Tile.EncodeTile(tile);
        Tile decoded = Tile.DecodeTile(encoded, dic);
        Assert.IsTrue(tile.id == decoded.id);
    }
    [Test]
    public void TestTileRotationEncoding()
    {
        Dictionary<int, SymmetryType> dic = new Dictionary<int, SymmetryType>();
        dic.Add(12, SymmetryType.T);
        Tile tile = new Tile(12, 3, SymmetryType.T);
        int encoded = Tile.EncodeTile(tile);
        Tile decoded = Tile.DecodeTile(encoded, dic);
        Assert.IsTrue(tile.rotation == decoded.rotation);
    }


    [Test]
    public void TestTileReflectionEncoding()
    {
        Dictionary<int, SymmetryType> dic = new Dictionary<int, SymmetryType>();
        dic.Add(12, SymmetryType.T);

        Tile tile = new Tile(12, 3, SymmetryType.T);
        Tile tile2 = new Tile(12, 3, SymmetryType.T, true);

        int encoded2 = Tile.EncodeTile(tile2);
        Tile decoded2 = Tile.DecodeTile(encoded2, dic);

        Assert.IsTrue(decoded2.reflected == tile2.reflected);

        int encoded = Tile.EncodeTile(tile);
        Tile decoded = Tile.DecodeTile(encoded, dic);

        Assert.IsTrue(decoded.reflected == tile.reflected);

    }
}
