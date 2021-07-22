# Project Information
Technical Paper: https://seanemery1.github.io/PDF-Procedural-Roguelike/

Web Release: https://semery.itch.io/procedural-bone-dungeon

*Note: Game is GPU intensive due to realtime lighting, framerate may suffer on weaker hardware*

![Preview Image](https://seanemery1.github.io/PDF-Procedural-Roguelike/Images/previewBossfight.png)

# Setup Instrunctions
1. Download the latest beta version of Unity which supports the Universal Render Pipeline.
2. Clone repo, then compile the game to your desired platform on the Unity Editor.

# Procedural Level Generation Algorithms
1. Binary Space Partitioning (aka BSP) Algorithm
   - Used to segment a 2-D grid space to accommodate rooms of random sizes with adequate space for corridors between them.
2. Dwyer’s Delaunay Triangulation Algorithm (Divide and Conquer)
   - Used to convert each room’s center vertex into a Delaunay Triangulation graph/mesh which maximizes the minimum angles of all the triangles in the mesh.
3. Kruskal’s Greedy Minimum Spanning Tree Algorithm (Disjoint Set, Union-Find variation)
   - Used to find a minimum spanning tree of an undirected edge-weighted graph (the Delaunay Triangulation mesh meets this criteria).
4. Doubly-Chained, Breadth-First Search Algorithm
   - Used to find the diameter of the Minimum Spanning Tree (the longest distance between two vertices of a given graph). This information is then used to determine both the starting room and the final boss room of the game.
5. An Original, Work-in-progress, Lock and Key Placement Algorithm.
   - This algorithm attempts to generate locked rooms and corresponding key placements that would produce a valid, solvable path from the start of the dungeon to the final boss room.

# Controls
Main Menu/Pause Menu Navigation:

- Mouse only

Movement:
- W - Move Up
- A - Move Left
- S - Move Down
- D - Move Right

Alternative Movement:
- Up Arrow - Move Up
- Left Arrow  - Move Left
- Down Arrow - Move Down
- Right Arrow - Move Right

Abilities:
- J - Attack
- K - Open Door (only if the player has a key and is standing in front of the door)

Misc:
- Esc - Pause
