using System.Collections;
using System.Collections.Generic;
using TriangleNet.Geometry;
using TriangleNet.Topology;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PopulateDungeon : MonoBehaviour
{
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

    Dictionary<Vertex, int> distances;
    HashSet<Vertex> vertices;
    int[] distancesArr;
    Vertex[] verticesArr;

    public void PopulateMap()
    {
        mapData = GetComponent<ProceduralMapGenerator>();
        distances = mapData.distances;
        vertices = mapData.vertices;
        OrientMap();
        SpawnPlayer();
        SpawnSkeletons();
        SpawnKeysAndDoors();
        SpawnSkeletrax();
        SpawnHealth();
        SpawnSpikes();
    }
    //roomBounds.xMin + 2, roomBounds.xMax - 2), Random.Range(roomBounds.yMin + 2, roomBounds.yMax - 4)
    void SpawnSpikes()
    {
        foreach (Vertex vertex in vertices)
        {
            RectInt tempRoom = mapData.FindRoom(vertex);
            bool bossRoom = tempRoom.Equals(mapData.FindRoom(mapData.end));
            if (!tempRoom.Equals(mapData.FindRoom(mapData.start)))
            {
                for (int x = tempRoom.xMin + 1; x <= tempRoom.xMax - 1; x++)
                {
                    for (int y = tempRoom.yMin + 2; y <= tempRoom.yMax - 3; y++)
                    {
                        if (bossRoom)
                        {
                            if (!(x <= tempRoom.center.x + 2 && x >= tempRoom.center.x - 2 && y <= tempRoom.center.y + 2 && y >= tempRoom.center.y - 2))
                            {
                                if (Random.Range(0, 100) < 3)
                                {
                                    spikeGrid.SetTile(new Vector3Int(x, y, 0), spike);
                                }
                            }
                        } else
                        {
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
    void SpawnHealth()
    {
        for(int i = 0; i < distancesArr.Length; i++)
        {
            RectInt roomBounds = mapData.FindRoom(verticesArr[i]);
            if (!roomBounds.Equals(mapData.end))
            {
                if (Random.Range(0,9)< distancesArr[i])
                {
                    Instantiate(health, new Vector3(Random.Range(roomBounds.xMin + 2, roomBounds.xMax - 2) + 0.125f, Random.Range(roomBounds.yMin + 2, roomBounds.yMax - 4) + 0.1875f), Quaternion.identity);
                }
                
            }
        }
    }
    void SpawnPlayer()
    {
        //Instantiate(player, new Vector3((float) mapData.start.x, (float) mapData.start.y, 0), Quaternion.identity);
        player.transform.position = new Vector3((float)mapData.start.x, (float)mapData.start.y, 0);
    }
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
        verticesArr = verticesList.ToArray();
        distancesArr = distancesList.ToArray();
        System.Array.Sort(distancesArr, verticesArr);
        for (int i = 0; i < distancesArr.Length; i++)
        {
            Debug.Log("Dist: " + distancesArr[i] + " Vert: " + verticesArr[i]);
        }
    }
    void SpawnSkeletons()
    {
        RectInt roomBounds;
        foreach (Vertex vertex in vertices)
        {
            roomBounds = mapData.FindRoom(vertex);
            //if (!mapData.FindRoom(vertex).Equals(mapData.start) )
            //{
                
            for (int i = 0; i < distances[vertex]; i++)
            {
                if (mapData.FindRoom(vertex).Equals(mapData.FindRoom(mapData.end)))
                {
                    Instantiate(skeletonHard, new Vector3(Random.Range(roomBounds.xMin + 2, roomBounds.xMax - 2), Random.Range(roomBounds.yMin + 2, roomBounds.yMax - 4)), Quaternion.identity);
                    Instantiate(skeletonHard, new Vector3(Random.Range(roomBounds.xMin + 2, roomBounds.xMax - 2), Random.Range(roomBounds.yMin + 2, roomBounds.yMax - 4)), Quaternion.identity);
                    //Instantiate(skeletonHard, new Vector3(Random.Range(roomBounds.xMin + 2, roomBounds.xMax - 2), Random.Range(roomBounds.yMin + 2, roomBounds.yMax - 4)), Quaternion.identity);

                } else
                {
                    if (Random.Range(0, 100) < 80)
                    {
                        Instantiate(skeleton, new Vector3(Random.Range(roomBounds.xMin + 2, roomBounds.xMax - 2), Random.Range(roomBounds.yMin + 2, roomBounds.yMax - 4)), Quaternion.identity);
                    }
                    if (i < 3)
                    {
                        Instantiate(skeleton, new Vector3(Random.Range(roomBounds.xMin + 2, roomBounds.xMax - 2), Random.Range(roomBounds.yMin + 2, roomBounds.yMax - 4)), Quaternion.identity);
                    }
                }


            }
            //}
       
        }
    }
    void SpawnSkeletrax()
    {
        RectInt room = mapData.FindRoom(mapData.end);
        var boss = Instantiate(skeletrax, new Vector3(room.center.x, room.center.y), Quaternion.identity);
        boss.GetComponent<SkeletraxAI>().room = room;
    }
    void SpawnKeysAndDoors()
    {
        Queue<Vertex> q = new Queue<Vertex>();
        List<Vertex> visited = new List<Vertex>();
        distances = new Dictionary<Vertex, int>();
        Vertex start = mapData.start;
        SpawnKey(mapData.FindRoom(start), 0f);
        SpawnDoors(start, verticesArr[1]);
        HashSet<Vertex> lockedRooms = new HashSet<Vertex>();
        HashSet<Vertex> keyRooms = new HashSet<Vertex>();
        keyRooms.Add(verticesArr[0]);
        //lockedRooms.Add(verticesArr[1]);
        
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
                        if (verticesArr[i+1].Equals(mapData.end))
                        {
                            if (!endIsLocked)
                            {
                                SpawnKey(mapData.FindRoom(verticesArr[i]), 0f);
                                SpawnDoors(doorHereRoom, verticesArr[i + 1]);
                                keyRooms.Add(verticesArr[i]);
                                //lockedRooms.Add(verticesArr[i + 1]);
                                endIsLocked = true;
                            }
                            
                        }
                        else if (verticesArr[i].Equals(mapData.end))
                        {
                            if (!endIsLocked)
                            {
                                SpawnKey(mapData.FindRoom(verticesArr[i + 1]), 0f);
                                SpawnDoors(doorHereRoom, verticesArr[i]);
                                keyRooms.Add(verticesArr[i + 1]);
                                //lockedRooms.Add(verticesArr[i]);
                                endIsLocked = true;
                            }
                            
                        }
                        else if (neighbors0.Count < neighbors1.Count)
                        {
                            Debug.Log("Door: " + doorHereRoom.x + " " + doorHereRoom.y + " -> " + verticesArr[i + 1].x + " " + verticesArr[i + 1].y);
                            Debug.Log("Key: " + doorHereRoom.x + " " + doorHereRoom.y + " -> " + verticesArr[i].x + " " + verticesArr[i].y);
                            SpawnKey(mapData.FindRoom(verticesArr[i]), 0f);
                            SpawnDoors(doorHereRoom, verticesArr[i + 1]);
                            keyRooms.Add(verticesArr[i]);
                            lockedRooms.Add(verticesArr[i + 1]);
                        }
                        else if (neighbors0.Count >= neighbors1.Count)
                        {
                            Debug.Log("Door: " + doorHereRoom.x + " " + doorHereRoom.y + " -> " + verticesArr[i].x + " " + verticesArr[i].y);
                            Debug.Log("Key: " + doorHereRoom.x + " " + doorHereRoom.y + " -> " + verticesArr[i + 1].x + " " + verticesArr[i + 1].y);
                            SpawnKey(mapData.FindRoom(verticesArr[i + 1]), 0f);
                            SpawnDoors(doorHereRoom, verticesArr[i]);
                            keyRooms.Add(verticesArr[i + 1]);
                            lockedRooms.Add(verticesArr[i]);
                        }
                    }
                    
                }
            }
        }
        if (!endIsLocked)
        {
            //SpawnKey(mapData.FindRoom(verticesArr[i + 1]), 0f);
            foreach (Edge edge in mapData.mstEdges)
            {
                Vertex v0 = mapData.mesh.vertices[edge.P0];
                Vertex v1 = mapData.mesh.vertices[edge.P1];
                if (mapData.FindRoom(v0).Equals(mapData.FindRoom(mapData.end)) || mapData.FindRoom(v1).Equals(mapData.FindRoom(mapData.end)))
                {
                    if (v0.Equals(verticesArr[verticesArr.Length - 1]))
                    {
                        SpawnDoors(v1, v0);
                       // lockedRooms.Add(v0);
                    } else
                    {
                        SpawnDoors(v0, v1);
                        //lockedRooms.Add(v1);
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
        //Debug.Log()
        if (yMid <= room1.yMax - 3 && yMid <= room2.yMax - 3 && yMid >= room1.yMin + 2 && yMid >= room2.yMin + 2)
        {
            if (x1 < x2)
            {
                //for (int y = room1.yMin; y < room1)
                SpawnRightDoor(room1.xMax, yMid);
            }
            else
            {
                SpawnLeftDoor(room1.xMin, yMid);

            }
            //SpawnRight/Left
            //CreateHorTunnel(Mathf.Min(room1.xMax, room2.xMax), Mathf.Max(room1.xMin, room2.xMin), yMid, room1, room2);

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

            //CreateVirTunnel(Mathf.Min(room1.yMax, room2.yMax), Mathf.Max(room1.yMin, room2.yMin), xMid, room1, room2);
        }
        else
        {
            Debug.Log("else");
            Vector2 vector = new Vector2(room2.x - room1.x, room2.y - room1.y);
            float angle = Mathf.Atan2(room2.y - room1.y, room2.x - room1.x)*Mathf.Rad2Deg;
            Debug.Log("angle: " + angle);
            bool found = false;
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
            //look for hallway ts;
        }

    }
    void SpawnKey(RectInt roomBounds, float offset)
    {
        Instantiate(key, new Vector3(roomBounds.center.x + offset, roomBounds.center.y + 1.5f + offset), Quaternion.identity);
    }
    //for (int x = 0; x< this.mapWidth; x++)
    //    {
    //        for (int y = 0; y< this.mapHeight; y++)
    //        {
    //            if (hallwaysT[x, y] == 1 || level[x, y] == 1 || hallways[x, y] == 1)//level[x,y]==1||hallways[x, y] == 1 ||
    //            {
    //                dungeonGrid.SetTile(new Vector3Int(x, y, 0), this.dungeon);
    //                //minimapGrid.SetTile(new Vector3Int(x, y, 0), this.dungeon);
    //                Debug.Log(x);
    //            }
    //        }
    //    }
    // hor

    //        this.hallways[x, y + 1] = 1;
    //        this.hallways[x, y] = 1;
    //        this.hallways[x, y - 1] = 1;
    //        this.hallways[x, y - 2] = 1;

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
    // X
    // X
    // Y
    // X
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

    // YXYY
    //this.hallways[x - 2, y] = 1;
    //this.hallways[x - 1, y] = 1;
    //this.hallways[x, y] = 1;
    //this.hallways[x + 1, y] = 1;
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
    //ver
        
  
