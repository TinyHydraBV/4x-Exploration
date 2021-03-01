# 4x-Exploration
unity version: 2020.2.6f1

Creating a basic 4X game using hexes.
Currently almost everything in terms of map generation, and camera controls, are editable in the inspector using scripts attached to game objects present in the _SCENE_ hierarchy.
This is not true of some of the debug/developer ref functionality. Right now if you want to endable/disable visible coordinates for the hexes, go to Assets > Prefabs > HexPrefab and access the HexCoordLabel child, then toggle the Mesh Renderer according to your preference.
/// Goals ///

In Progress:
1. Create an interactable hex map with world wrapping and different terrain types

	- Lookup project porcupine for example to help migrating the terrain definitions to a config file (JSON?) & map generation to LUA

2. Add turn based mechanics

To Do:

3. Add units that can move through the map interacting with each other and the world

4. Add cities and improvements, and the ability to manage these

5. Add AI characters with chat (using ink?)

6. Add syncronous turn multiplayer

// the hope here is to create a basic framework for building a larger 4x game