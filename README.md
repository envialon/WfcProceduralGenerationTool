# WfcProceduralGenerationTool

## Description <a id="description"></a>
This is my final project for university. I chose to develop it when I found out Maxim Gumin's [Wave Function Collapse](https://github.com/mxgmn/WaveFunctionCollapse), I thought it was such a cool algorithm and I couldn't take it out of my head :D. 

The project consist of my own implementation of a tile-based WFC generator, with a strong focus of making it as generalist as possible, and a custom editor in order to handle tilemap manipulation and to facilitate the interaction with the algorithm. The idea is that anyone can model a tileset on any 3D mesh modeling software and use this to generate tile-based levels/maps with WFC.

![generation example](/docs/imgs/example.gif)

The WFC Overlapping model was chosen because of its flexibility and minimal user input, since the tile adjacencies can be extracted from an sample map, instead of having to manually declare each one by hand. 

Another great functionality that WFC supports and has been implemented is being able to autocomplete maps: 

![autocomplete](/docs/imgs/autocomplete.gif)

## Table of contents <a id="ContentTable"></a>
1. [Description][def]
2. [Table of contents][def2]
3. [How to use][def3]
4. [Implementation Details][def4]
    1. [Tileset][def5]
    3. [WfcGenerator][def7]
    

## How to use <a id="HowTo"></a>
The generator and the tool itself are two separate monobehaviours (`WfcGenerator` and `WfcInterface`), in case of needing just the generator but not the functionality of editing the input/output tilemaps. 

To experiment with the tool you can just attach the `WfcInterface` script to an emtpy `GameObject`. In order to use the generator you must create your `TileSet` first, where you define the tile meshes and materials that you're goint to use. Then you can create your own `Tilemap` with your tiles from the scene window on editor mode. On the `WfcInterface` Inspector window the option to serialize and load already serialized input tilemaps is available.

Having an input map created (either making it by hand or loading a serialized one) you can then start generating output maps with the button on the editor, you can tweak parameters to change the behaviour of the WFC generation from the `WfcGenerator` and `WfcInterface` inspector window.

Example with two generations with the same input tilemap and parameters except for patternSize:

patternSize == 2           |  patternSize == 3
:-------------------------:|:-------------------------:
![PatternSize2](/docs/imgs/PatternSize2.png) |  ![PatternSize3](/docs/imgs/PatternSize3.png)



## Implementation Details <a id="Implementation"></a>

### Tileset and TileAttributes <a id="Tileset"></a>
A `TileSet` is a scriptable object with essentially just a list of `TileAttributes`. You should be able to create one right-clicking on the project window and navigating to Create>ScriptableObjects>TileSet.

`TileAttributes` is a struct that contains the `Mesh`, `Material` and `SymmetryType` of every Tile. The `SymmetryType` is an important attribute to get right for your tiles if you want to use the pattern rotation or reflection parameters to generate maps, undesired generation results are expected if your tiles are not correctly classified with their respective symmetry type, check [Wave Function Collapse](https://github.com/mxgmn/WaveFunctionCollapse) original repo for an explanation of tile symmetries.

Here's the two simple tilesets I've made: 

testTileSet        |         islandTileSet
:-------------------------:|:-------------------------:
![TestTileset](/docs/imgs/TestTileset.png) |  ![IslandTileset](/docs/imgs/IslandTileset.png)


### Tilemap <a id="Tilemap"></a>
A `Tilemap` basically contains a flattened array of encoded tiles as integers. It must not be confused with `SerializableTilemap` which is just a `ScriptableObject` wrapper for serializing input tilemaps from the `WfcInterface` inspector window.

In the following pic there are two tilemaps, the input map made by hand (small one) and the generated output map (the bigger one):

![IslandMap](/docs/imgs/island.png)

### WfcGenerator<a id="WfcGenerator"></a>
`WfcGenerator` was separated from the interface just in case it needs to be used to generate using already serialized input tilemaps, for example to use as a level generator. 

There's a code functionality not reflected on the editor tool that allows you to save generated maps as meshes but its not that well written and the meshes are unoptimized.





[def]: #description
[def2]: #ContentTable
[def3]: #HowTo
[def4]: #Implementation
[def5]: #Tileset
[def6]: #Tilemap
[def7]: #WfcGenerator
[def8]: #WfcInterface
