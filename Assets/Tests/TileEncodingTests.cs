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
        Tile tile = new Tile(12, 3);
        int encoded = InputReader.EncodeTile(tile.id, tile.rotation, false);
        Tile decoded = InputReader.DecodeTile(encoded);
        Assert.IsTrue(tile.id == decoded.id);
    }
    [Test]
    public void TestTileRotationEncoding()
    {
        Tile tile = new Tile(12, 3);
        int encoded = InputReader.EncodeTile(tile.id, tile.rotation, false);
        Tile decoded = InputReader.DecodeTile(encoded);
        Assert.IsTrue(tile.rotation == decoded.rotation);
    }
}
