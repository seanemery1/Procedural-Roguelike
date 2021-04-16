using System.Collections;
using System.Collections.Generic;
using TriangleNet.Geometry;
using TriangleNet.Topology;
using UnityEngine;
using UnityEngine.Tilemaps;

// Populates the dungeon map after it has been generated
public class PopulateDungeon : MonoBehaviour
{
    // Declaring/initializing all the various map elements that will be placed procedurally
    public GameObject player;
    public GameObject skeletonHard;
    public GameObject skeleton;
    public GameObject skeletrax;
    public GameObject key;
    public GameObject health;
    public AnimatedTile spike;
    public RuleTile door;
    public Tilemap doorGrid;
    public Tilemap spikeGrid;
    ProceduralMapGenerator mapData;

    // Miscalaneous variables used only by algorithms
    Dictionary<Vertex, int> distances;
    HashSet<Vertex> vertices;
    int[] distancesArr;
    Vertex[] verticesArr;

    // Initialzing some variables and calling various methods
    public void PopulateMap()
    {
        mapData = GetComponent<ProceduralMapGenerator>();
        distances = mapData.distances;
        vertices = mapData.vertices;

        // Populates the dungeon with each item at a time.
        OrientMap();
        SpawnPlayer();
        SpawnSkeletons();
        SpawnKeysAndDoors();
        SpawnSkeletrax();
        SpawnHealth();
        SpawnSpikes();
    }

    // Spikes have a chance on spawning in every room (except for the starting room)
    void SpawnSpikes()
    {
        // For each vertex/room
        foreach (Vertex vertex in vertices)
        {
            RectInt tempRoom = mapData.FindRoom(vertex);
            bool bossRoom = tempRoom.Equals(mapData.FindRoom(mapData.end));

            // If not the starting room where the player spawns
            if (!tempRoom.Equals(mapData.FindRoom(mapData.start)))
            {
                // Double for loop to loop through each tile in a room
                for (int x = tempRoom.xMin + 1; x <= tempRoom.xMax - 1; x++)
                {
                    for (int y = tempRoom.yMin + 2; y <= tempRoom.yMax - 3; y++)
                    {
                        if (bossRoom)
                        {
                            // If inside a boss room, do not spawn spikes if it is too close to the center (where the boss will spawn)
                            if (!(x <= tempRoom.center.x + 2 && x >= tempRoom.center.x - 2 && y <= tempRoom.center.y + 2 && y >= tempRoom.center.y - 2))
                            {
                                // 3% chance of having a spike spawn on a tile
                                if (Random.Range(0, 100) < 3)
                                {
                                    spikeGrid.SetTile(new Vector3Int(x, y, 0), spike);
                                }
                            }
                        } else
                        {
                            // 2% chance of having a spike spawn on a tile
                            if (Random.Range(0, 100) < 2)
                            {
                                spikeGrid.SetTile(new Vector3Int(x, y, 0), spike);
                            }
                        }
                        
                    }
                }
            }
            
        }
    }

    // Health/one heart has an increasing chance of spawning the closer we get to the boss room (the further away we get from the spawn/start room)
    void SpawnHealth()
    {
        // For each room
        for(int i = 0; i < distancesArr.Length; i++)
        {
            RectInt roomBounds = mapData.FindRoom(verticesArr[i]);
            // If the room is not a boss room, there is a chance to spawn a heart piece
            if (!roomBounds.Equals(mapData.FindRoom(mapData.end)))
            {
                if (Random.Range(0,100) < 10)
                {
                    Instantiate(health, new Vector3(Random.Range(roomBounds.xMin + 2, roomBounds.xMax - 2) + 0.125f, Random.Range(roomBounds.yMin + 2, roomBounds.yMax - 4) + 0.1875f), Quaternion.identity);
                }
                
            }
        }
    }

    // Spawn (aka teleport) player in the center of the starting room
    void SpawnPlayer()
    {
        player.transform.position = new Vector3((float)mapData.start.x, (float)mapData.start.y, 0);
    }

    // Method that produce a corresponding pair of lists that's sorted in order
    // For example: distances[index] corresponds to the distance of vertex[index]
    void OrientMap()
    {
        //bool inverse = true;
        List<Vertex> verticesList = new List<Vertex>();
        List<int> distancesList = new List<int>();

        foreach (Vertex vertex in vertices)
        {
            if (vertex.Equals(mapData.start))
            {
                if (distances[vertex] == 0)
                {
                    //inverse = false;
                }
            }
            verticesList.Add(vertex);
            distancesList.Add(distances[vertex]);
        }
        // Deprecated method to swap the spawn/boss room by comparing the two sizes (and making the spawn room the smallest one between the two)
        //if (inverse)
        //{
        //    int index = 0;
        //    foreach (Vertex vertex in vertices)
        //    {

        //        distances[vertex] = mapData.maxDistance - distances[vertex];
        //        distancesList[index] = mapData.maxDistance - distancesList[index];
        //        index++;
        //    }
        //}

        // Sorting both arrays by ascending distances
        verticesArr = verticesList.ToArray();
        distancesArr = distancesList.ToArray();
        System.Array.Sort(distancesArr, verticesArr);
        for (int i = 0; i < distancesArr.Length; i++)
        {
            Debug.Log("Dist: " + distancesArr[i] + " Vert: " + verticesArr[i]);
        }
    }

    // Method that spawns skeletons in each room in an almost linear fashing
    void SpawnSkeletons()
    {
        // For each room
        RectInt roomBounds;
        foreach (Vertex vertex in vertices)
        {
            roomBounds = mapData.FindRoom(vertex);
            // Attempt to spawn x skeletons where x is the distance/number of rooms awaay from spawn
            for (int i = 0; i < distances[vertex]; i++)
            {
                // If boss room, spawn harder variants of the skeleton mob
                if (mapData.FindRoom(vertex).Equals(mapData.FindRoom(mapData.end)))
                {
                    Instantiate(skeletonHard, new Vector3(Random.Range(roomBounds.xMin + 2, roomBounds.xMax - 2), Random.Range(roomBounds.yMin + 2, roomBounds.yMax - 4)), Quaternion.identity);
                    Instantiate(skeletonHard, new Vector3(Random.Range(roomBounds.xMin + 2, roomBounds.xMax - 2), Random.Range(roomBounds.yMin + 2, roomBounds.yMax - 4)), Quaternion.identity);

                } else
                {
                    // There is an 80% chance for each skeleton to spawn
                    if (Random.Range(0, 100) < 80)
                    {
                        Instantiate(skeleton, new Vector3(Random.Range(roomBounds.xMin + 2, roomBounds.xMax - 2), Random.Range(roomBounds.yMin + 2, roomBounds.yMax - 4)), Quaternion.identity);
                    }
                    // If it's one of the first 2 rooms away from spawn, guarantee one skeleton spawn
                    if (i <= 2)
                    {
                        Instantiate(skeleton, new Vector3(Random.Range(roomBounds.xMin + 2, roomBounds.xMax - 2), Random.Range(roomBounds.yMin + 2, roomBounds.yMax - 4)), Quaternion.identity);
                    }
                }


            }
            //}
       
        }
    }

    // Method spawns the final boss, Skeletrax in the center of the room furthest away from the spawn room
    void SpawnSkeletrax()
    {
        RectInt room = mapData.FindRoom(mapData.end);
        var boss = Instantiate(skeletrax, new Vector3(room.center.x, room.center.y), Quaternion.identity);
        boss.GetComponent<SkeletraxAI>().room = room;
    }

    // Method that uses a pseudo-BFS algorithm to create solvable lock and key puzzles (in terms of dungeon navigation)
    void SpawnKeysAndDoors()
    {
        Vertex start = mapData.start;
        SpawnKey(mapData.FindRoom(start), 0f);
        SpawnDoors(start, verticesArr[1]);
        HashSet<Vertex> lockedRooms = new HashSet<Vertex>();
        HashSet<Vertex> keyRooms = new HashSet<Vertex>();
        keyRooms.Add(verticesArr[0]);
        bool endIsLocked = false;
        for (int i = 1; i < distancesArr.Length - 1; i++)
        {
            if (i + 1 < distancesArr.Length)
            {
                if (distancesArr[i] == distancesArr[i + 1])
                {

                    HashSet<Vertex> neighbors0 = new HashSet<Vertex>();
                    HashSet<Vertex> neighbors1 = new HashSet<Vertex>();
                    foreach (Edge edge in mapData.mstEdges)
                    {
                        Vertex v0 = mapData.mesh.vertices[edge.P0];
                        Vertex v1 = mapData.mesh.vertices[edge.P1];

                        if (v0.Equals(verticesArr[i]) || v1.Equals(verticesArr[i]))
                        {
                            neighbors0.Add(v0);
                            neighbors0.Add(v1);
                        }
                        if (v0.Equals(verticesArr[i + 1]) || v1.Equals(verticesArr[i + 1]))
                        {
                            neighbors1.Add(v0);
                            neighbors1.Add(v1);
                        }
                    }
                    Vertex doorHereRoom = null;
                    foreach (Vertex neighbor in neighbors0)
                    {
                        if (neighbors1.Contains(neighbor))
                        {
                            doorHereRoom = neighbor;
                            break;
                        }
                    }
                    if (doorHereRoom != null)
                    {
                        bool bossPath = CheckIfPathContainsBoss(doorHereRoom, verticesArr[i]);
                        if (bossPath)
                        {
                            if(!endIsLocked)
                            {
                                Vertex placeKeyHere = FurthestRoomBFS(doorHereRoom, verticesArr[i + 1]);
                                SpawnKey(mapData.FindRoom(placeKeyHere), 0f);
                                SpawnDoors(doorHereRoom, verticesArr[i]);
                                keyRooms.Add(verticesArr[i + 1]);

                                if (verticesArr[i].Equals(mapData.end))
                                {
                                    endIsLocked = true;
                                }
                                else
                                {
                                    lockedRooms.Add(verticesArr[i]);
                                }                        
                            }
                        } else
                        {
                            if(!endIsLocked)
                            {
                                Vertex placeKeyHere = FurthestRoomBFS(doorHereRoom, verticesArr[i]);
                                SpawnKey(mapData.FindRoom(placeKeyHere), 0f);
                                SpawnDoors(doorHereRoom, verticesArr[i + 1]);
                                keyRooms.Add(verticesArr[i]);
                                if (verticesArr[i + 1].Equals(mapData.end))
                                {
                                    endIsLocked = true;
                                } else
                                {
                                    lockedRooms.Add(verticesArr[i + 1]);
                                }      
                            }
                        }  
                    }

                }
            }
        }
        if (!endIsLocked)
        {
            foreach (Edge edge in mapData.mstEdges)
            {
                Vertex v0 = mapData.mesh.vertices[edge.P0];
                Vertex v1 = mapData.mesh.vertices[edge.P1];
                if (mapData.FindRoom(v0).Equals(mapData.FindRoom(mapData.end)) || mapData.FindRoom(v1).Equals(mapData.FindRoom(mapData.end)))
                {
                    if (mapData.FindRoom(v0).Equals(mapData.FindRoom(mapData.end)))
                    {
                        SpawnDoors(v1, v0);
              
                    }
                    else
                    {
                        SpawnDoors(v0, v1);
                    }

                }
            }
            bool extraKeyPlaced = false;
            while (!extraKeyPlaced)
            {
                foreach (Vertex vertex in lockedRooms)
                {
                    if (Random.Range(0, 100) < 25)
                    {
                        SpawnKey(mapData.FindRoom(vertex), -1.5f);
                        extraKeyPlaced = true;
                        break;
                    }
                }
            }
        }

    }
    // Breadth-first search method that traverses the graph to locate the furthest room away.
    // Method will place a key in that furthest room.
    Vertex FurthestRoomBFS(Vertex splitRoom, Vertex keyPlacementContender)
    {
        Queue<Vertex> q = new Queue<Vertex>();
        List<Vertex> visited = new List<Vertex>();
        visited.Add(splitRoom);
        Dictionary<Vertex, int> distances = new Dictionary<Vertex, int>();
        Vertex source = keyPlacementContender;
        foreach (Vertex vertex in vertices)
        {
            distances.Add(vertex, 0);
            if (source == null)
            {
                source = vertex;
            }

        }
        visited.Add(source);
        q.Enqueue(source);

        while (q.Count != 0)
        {
            Vertex v = q.Peek();
            foreach (Edge neighborEdge in mapData.mstEdges)
            {
                Vertex v0 = mapData.mesh.vertices[neighborEdge.P0];
                Vertex v1 = mapData.mesh.vertices[neighborEdge.P1];
                if (v0.Equals(v) || v1.Equals(v))
                {
                    Vertex neighbor = null;
                    if (!v0.Equals(v))
                    {
                        Debug.Log("Start/End v0");
                        neighbor = v0;
                    }
                    else if (!v1.Equals(v))
                    {
                        Debug.Log("Start/End v1");
                        neighbor = v1;
                    }
                    else
                    {
                    }
                    if (!visited.Contains(neighbor))
                    {
                        q.Enqueue(neighbor);

                        visited.Add(neighbor);
                        distances[neighbor] = distances[v] + 1;
                    }

                }
            }
            q.Dequeue();
        }
        foreach (Vertex vertex in vertices)
        {
            if (distances[vertex] > distances[source])
            {
                Debug.Log("Start/End distance: " + distances[vertex]);
                source = vertex;
            }
        }
        return source;
    }

    // Breadth-first search method that traverses the graph to see if a boss room exists at the end of that branch.
    bool CheckIfPathContainsBoss(Vertex splitRoom, Vertex roomToBoss)
    {
        Queue<Vertex> q = new Queue<Vertex>();
        List<Vertex> visited = new List<Vertex>();
        visited.Add(splitRoom);
        Dictionary<Vertex, int> distances = new Dictionary<Vertex, int>();
        Vertex source = roomToBoss;
        foreach (Vertex vertex in vertices)
        {
            distances.Add(vertex, 0);
            if (source == null)
            {
                source = vertex;
            }

        }
        visited.Add(source);
        q.Enqueue(source);

        while (q.Count != 0)
        {
            Vertex v = q.Peek();
            //Debug.Log("Start/End peek v " + v.x + " " + v.y);
            foreach (Edge neighborEdge in mapData.mstEdges)
            {
                Vertex v0 = mapData.mesh.vertices[neighborEdge.P0];
                Vertex v1 = mapData.mesh.vertices[neighborEdge.P1];
                if (v0.Equals(v) || v1.Equals(v))
                {
                    Vertex neighbor = null;
                    if (!v0.Equals(v))
                    {
                        Debug.Log("Start/End v0");
                        neighbor = v0;
                    }
                    else if (!v1.Equals(v))
                    {
                        Debug.Log("Start/End v1");
                        neighbor = v1;
                    }
                    else
                    {
                    }
                    if (!visited.Contains(neighbor))
                    {
                        q.Enqueue(neighbor);

                        visited.Add(neighbor);
                        distances[neighbor] = distances[v] + 1;
                    }

                }
            }
            q.Dequeue();
        }
        foreach (Vertex vertex in visited)
        {

            if (mapData.FindRoom(vertex).Equals(mapData.FindRoom(mapData.end)))
            {
                return true;
            }

        }
        return false;
    }
    // Method used to spawn doors between two rooms. Needs major refactoring.
    void SpawnDoors(Vertex placeDoorHere, Vertex nextRoom)
    {
        Debug.Log("Spawn doors: " + placeDoorHere.x + " " + placeDoorHere.y);
        int x1 = Mathf.RoundToInt((float)placeDoorHere.x);
        int y1 = Mathf.RoundToInt((float)placeDoorHere.y);

        int x2 = Mathf.RoundToInt((float)nextRoom.x);
        int y2 = Mathf.RoundToInt((float)nextRoom.y);

        RectInt room1 = mapData.FindRoom(placeDoorHere);
        RectInt room2 = mapData.FindRoom(nextRoom);

        int xMid = Mathf.RoundToInt((x1 + x2) / 2);
        int yMid = Mathf.RoundToInt((y1 + y2) / 2);
        Debug.Log("yMid: " + yMid + " y1Max " + room1.yMax + " y1Min" + room1.yMin + " y2Max " + room2.yMax + " y2Min" + room2.yMin);
        if (yMid <= room1.yMax - 3 && yMid <= room2.yMax - 3 && yMid >= room1.yMin + 2 && yMid >= room2.yMin + 2)
        {
            if (x1 < x2)
            {
                SpawnRightDoor(room1.xMax, yMid);
            }
            else
            {
                SpawnLeftDoor(room1.xMin, yMid);

            }
        }
        else if (xMid <= room1.xMax - 3 && xMid <= room2.xMax - 3 && xMid >= room1.xMin + 2 && xMid >= room2.xMin + 2)
        {
            if (y1 < y2)
            {// Place Top
                SpawnTopDoor(xMid, room1.yMax);
            }
            else
            {
                SpawnBotDoor(xMid, room1.yMin);
            }
        }
        else
        {
            float angle = Mathf.Atan2(room2.y - room1.y, room2.x - room1.x)*Mathf.Rad2Deg;
            Debug.Log("angle: " + angle);
            bool found = false;
            // Checks which corner the door should be in. Needs refactoring.
            if (angle < 90f && angle >= 0f)
            {
                if (!found)
                {
                    for (int x = room1.xMin; x <= room1.xMax; x++)
                    {
                        if (mapData.hallwaysT[x, room1.yMax] == 1)
                        {
                            SpawnTopDoor(x + 2, room1.yMax);
                            found = true;
                            break;
                        }
                    }
                }

                if (!found)
                {
                    for (int y = room1.yMin; y <= room1.yMax; y++)
                    {
                        if (mapData.hallwaysT[room1.xMax, y] == 1)
                        {
                            SpawnRightDoor(room1.xMax, y + 2);
                            found = true;
                            break;
                        }
                    }
                }
                if (!found)
                {
                    for (int y = room1.yMin; y <= room1.yMax; y++)
                    {
                        if (mapData.hallways[room1.xMax, y] == 1)
                        {
                            SpawnRightDoor(room1.xMax, y + 2);
                            found = true;
                            break;
                        }
                    }
                }
                if (!found)
                {
                    for (int x = room1.xMin; x <= room1.xMax; x++)
                    {
                        if (mapData.hallways[x, room1.yMax] == 1)
                        {
                            SpawnTopDoor(x + 2, room1.yMax);
                            break;
                        }
                    }
                }

                // scan xMax and Ymax;
            }
            else if (angle <= 180f &&angle > 90f)
            {

                if (!found)
                {
                    for (int y = room1.yMin; y <= room1.yMax; y++)
                    {
                        if (mapData.hallwaysT[room1.xMin, y] == 1)
                        {
                            SpawnLeftDoor(room1.xMin, y + 2);
                            found = true;
                            break;
                        }
                    }
                }

                if (!found)
                {
                    for (int x = room1.xMin; x <= room1.xMax; x++)
                    {
                        if (mapData.hallwaysT[x, room1.yMax] == 1)
                        {
                            SpawnTopDoor(x + 2, room1.yMax);
                            found = true;
                            break;
                        }
                    }
                }
                if (!found)
                {
                    for (int x = room1.xMin; x <= room1.xMax; x++)
                    {
                        if (mapData.hallways[x, room1.yMax] == 1)
                        {
                            SpawnTopDoor(x + 2, room1.yMax);
                            found = true;
                            break;
                        }
                    }
                }
                if (!found)
                {
                    for (int y = room1.yMin; y <= room1.yMax; y++)
                    {
                        if (mapData.hallways[room1.xMin, y] == 1)
                        {
                            SpawnLeftDoor(room1.xMin, y + 2);
                            break;
                        }
                    }
                }
                //scan yMin and xMax
            }
            else if (angle >= -180f && angle < -90f)
            {
                //scan xMin and Ymin
                if (!found)
                {
                    for (int x = room1.xMin; x <= room1.xMax; x++)
                    {
                        if (mapData.hallwaysT[x, room1.yMin] == 1)
                        {
                            SpawnBotDoor(x + 2, room1.yMin);
                            found = true;
                            break;
                        }
                    }
                }
                
                if (!found)
                {
                    for (int y = room1.yMin; y <= room1.yMax; y++)
                    {
                        if (mapData.hallwaysT[room1.xMin, y] == 1)
                        {
                            SpawnLeftDoor(room1.xMin, y + 2);
                            found = true;
                            break;
                        }
                    }
                }
                if (!found)
                {
                    for (int y = room1.yMin; y <= room1.yMax; y++)
                    {
                        if (mapData.hallways[room1.xMin, y] == 1)
                        {
                            SpawnLeftDoor(room1.xMin, y + 2);
                            found = true;
                            break;
                        }
                    }
                }
                if (!found)
                {
                    for (int x = room1.xMin; x <= room1.xMax; x++)
                    {
                        if (mapData.hallways[x, room1.yMin] == 1)
                        {
                            SpawnBotDoor(x + 2, room1.yMin);
                            break;
                        }
                    }
                }
            }  
            else
            {
                if (!found)
                {
                    for (int y = room1.yMin; y <= room1.yMax; y++)
                    {
                        if (mapData.hallwaysT[room1.xMax, y] == 1)
                        {
                            SpawnRightDoor(room1.xMax, y + 2);
                            found = true;
                            break;
                        }
                    }
                }
                
                if (!found)
                {
                    for (int x = room1.xMin; x <= room1.xMax; x++)
                    {
                        if (mapData.hallwaysT[x, room1.yMin] == 1)
                        {
                            SpawnBotDoor(x + 2, room1.yMin);
                            found = true;
                            break;
                        }
                    }
                }
                if (!found)
                {
                    for (int x = room1.xMin; x <= room1.xMax; x++)
                    {
                        if (mapData.hallways[x, room1.yMin] == 1)
                        {
                            SpawnBotDoor(x + 2, room1.yMin);
                            found = true;
                            break;
                        }
                    }
                }
                if (!found)
                {
                    for (int y = room1.yMin; y <= room1.yMax; y++)
                    {
                        if (mapData.hallways[room1.xMax, y] == 1)
                        {
                            SpawnRightDoor(room1.xMax, y + 2);
                            break;
                        }
                    }
                }
                // scan yMin and xMax;
            }
        }
    }
    
    // Instantiate a key around the room's center with a slight offset.
    void SpawnKey(RectInt roomBounds, float offset)
    {
        Instantiate(key, new Vector3(roomBounds.center.x + offset, roomBounds.center.y + 1.5f + offset), Quaternion.identity);
    }

    // Spawn door tilesets with the correct offset in the appropriate configuration to make a right door at a hallway entrance.
    void SpawnRightDoor(int x, int y)
    {
        Debug.Log("Right door");
        Debug.Log("Right " + x + " " + y);
        doorGrid.SetTile(new Vector3Int(x, y - 2, 0), door);
        doorGrid.SetTile(new Vector3Int(x, y - 1, 0), door);
        doorGrid.SetTile(new Vector3Int(x, y, 0), door);
        doorGrid.SetTile(new Vector3Int(x + 1, y - 2, 0), door);
        doorGrid.SetTile(new Vector3Int(x + 1, y - 1, 0), door);
        //doorGrid.SetTile(new Vector3Int(x + 1, y , 0), door);
    }

    // Spawn door tilesets with the correct offset in the appropriate configuration to make a left door at a hallway entrance.
    void SpawnLeftDoor(int x, int y)
    {
        Debug.Log("Left door");
        Debug.Log("Left " + x + " " + y);
        doorGrid.SetTile(new Vector3Int(x, y - 2, 0), door);
        doorGrid.SetTile(new Vector3Int(x, y - 1, 0), door);
        doorGrid.SetTile(new Vector3Int(x, y, 0), door);
        doorGrid.SetTile(new Vector3Int(x - 1, y - 2, 0), door);
        doorGrid.SetTile(new Vector3Int(x - 1, y - 1, 0), door);
        //doorGrid.SetTile(new Vector3Int(x - 1, y, 0), door);
    }

    // Spawn door tilesets with the correct offset in the appropriate configuration to make a top door at a hallway entrance.
    void SpawnTopDoor(int x, int y)
    {
        Debug.Log("Top door");
        Debug.Log("Top " + x + " " + y);
        doorGrid.SetTile(new Vector3Int(x, y - 2, 0), door);
        //doorGrid.SetTile(new Vector3Int(x - 1, y - 2, 0), door);
        doorGrid.SetTile(new Vector3Int(x, y - 1, 0), door);
        doorGrid.SetTile(new Vector3Int(x - 1, y - 1, 0), door);
        doorGrid.SetTile(new Vector3Int(x, y, 0), door);
        doorGrid.SetTile(new Vector3Int(x - 1, y, 0), door);
    }

    // Spawn door tilesets with the correct offset in the appropriate configuration to make a bottom door at a hallway entrance.
    void SpawnBotDoor(int x, int y)
    {
        Debug.Log("Bot door");
        Debug.Log("Bot " + x + " " + y);
        //doorGrid.SetTile(new Vector3Int(x, y - 1, 0), door);
        doorGrid.SetTile(new Vector3Int(x - 1, y - 1, 0), door);
        doorGrid.SetTile(new Vector3Int(x, y, 0), door);
        doorGrid.SetTile(new Vector3Int(x - 1, y, 0), door);
        doorGrid.SetTile(new Vector3Int(x, y + 1, 0), door);
        doorGrid.SetTile(new Vector3Int(x - 1, y + 1, 0), door);
    }
}
        
  
