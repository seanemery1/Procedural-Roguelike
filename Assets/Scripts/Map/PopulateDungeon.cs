using System.Collections;
using System.Collections.Generic;
using TriangleNet.Geometry;
using TriangleNet.Topology;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PopulateDungeon : MonoBehaviour
{
    public GameObject player;
    public GameObject skeleton;
    public GameObject key;
    public RuleTile door;
    public Tilemap doorGrid;
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
    }
    void SpawnPlayer()
    {

        //Instantiate(player, new Vector3((float) mapData.start.x, (float) mapData.start.y, 0), Quaternion.identity);
        player.transform.position = new Vector3((float)mapData.start.x, (float)mapData.start.y, 0);
    }
    void OrientMap()
    {
        bool inverse = true;
        List<Vertex> verticesList = new List<Vertex>();
        List<int> distancesList = new List<int>();

        foreach (Vertex vertex in vertices)
        {
            if (vertex.Equals(mapData.start))
            {
                if (distances[vertex] == 0)
                {
                    inverse = false;
                }
            }
            verticesList.Add(vertex);
            distancesList.Add(distances[vertex]);
        }
        if (inverse)
        {
            int index = 0;
            foreach (Vertex vertex in vertices)
            {

                distances[vertex] = mapData.maxDistance - distances[vertex];
                distancesList[index] = mapData.maxDistance - distancesList[index];
                index++;
            }
        }
        verticesArr = verticesList.ToArray();
        distancesArr = distancesList.ToArray();
        System.Array.Sort(distancesArr, verticesArr);
    }
    void SpawnSkeletons()
    {
        RectInt roomBounds;
        foreach (Vertex vertex in vertices)
        {
            if (!mapData.FindRoom(vertex).Equals(mapData.start) || !mapData.FindRoom(vertex).Equals(mapData.end))
            {
                roomBounds = mapData.FindRoom(vertex);
                for (int i = 0; i < distances[vertex]; i++)
                {
                    if (Random.Range(0, 100) < 80)
                    {
                        Instantiate(skeleton, new Vector3(Random.Range(roomBounds.xMin + 2, roomBounds.xMax - 2), Random.Range(roomBounds.yMin + 2, roomBounds.yMax - 4)), Quaternion.identity);
                    }
                }
            }
            {

            }
        }
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
        for (int i = 1; i < distancesArr.Length; i++)
        {
            if (i + 2 < distancesArr.Length)
            {
                if (distancesArr[i + 1] == distancesArr[i + 2])
                {
                    HashSet<Vertex> neighbors = new HashSet<Vertex>();
                    foreach (Edge edge in mapData.mstEdges)
                    {
                        Vertex v0 = mapData.mesh.vertices[edge.P0];
                        Vertex v1 = mapData.mesh.vertices[edge.P1];
                        if (v0.Equals(verticesArr[i + 1]) || v1.Equals(verticesArr[i + 1]))
                        {
                            neighbors.Add(v0);
                            neighbors.Add(v1);
                        }
                    }
                    if (i+2== distancesArr.Length - 1)
                    {
                        SpawnKey(mapData.FindRoom(verticesArr[i + 1]), 0f);
                        SpawnDoors(verticesArr[i], verticesArr[i + 2]);
                        keyRooms.Add(verticesArr[i + 1]);
                        lockedRooms.Add(verticesArr[i + 2]);
                        endIsLocked = true;
                    }
                    else if (neighbors.Count <= 2)
                    {
                        SpawnKey(mapData.FindRoom(verticesArr[i + 1]), 0f);
                        SpawnDoors(verticesArr[i], verticesArr[i + 2]);
                        keyRooms.Add(verticesArr[i + 1]);
                        lockedRooms.Add(verticesArr[i + 2]);
                    }
                    else
                    {
                        SpawnKey(mapData.FindRoom(verticesArr[i + 2]), 0f);
                        SpawnDoors(verticesArr[i], verticesArr[i + 1]);
                        keyRooms.Add(verticesArr[i + 2]);
                        lockedRooms.Add(verticesArr[i + 1]);
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
                if (v0.Equals(verticesArr[verticesArr.Length-1]) || v1.Equals(verticesArr[verticesArr.Length - 1]))
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
                        SpawnKey(mapData.FindRoom(vertex), 1.5f);
                        extraKeyPlaced = true;
                        break;
                    }
                }
            }
            
        }



    }

    void SpawnDoors(Vertex placeDoorHere, Vertex nextRoom)
    {

        int x1 = Mathf.RoundToInt((float)placeDoorHere.x);
        int y1 = Mathf.RoundToInt((float)placeDoorHere.y);

        int x2 = Mathf.RoundToInt((float)nextRoom.x);
        int y2 = Mathf.RoundToInt((float)nextRoom.y);

        RectInt room1 = mapData.FindRoom(placeDoorHere);
        RectInt room2 = mapData.FindRoom(nextRoom);

        int xMid = Mathf.RoundToInt((x1 + x2) / 2);
        int yMid = Mathf.RoundToInt((y1 + y2) / 2);

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
            Vector2 vector = new Vector2(room2.x - room1.x, room2.y - room1.y);
            float angle = Vector2.SignedAngle(Vector2.up, vector);
            bool found = false;
            if (angle < -90f)
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
                        if (mapData.hallwaysT[room1.xMin, room1.yMin] == 1)
                        {
                            SpawnLeftDoor(room1.xMin, y + 1);
                            found = true;
                            break;
                        }
                    }
                }
                if (!found)
                {
                    for (int y = room1.yMin; y <= room1.yMax; y++)
                    {
                        if (mapData.hallways[room1.xMin, room1.yMin] == 1)
                        {
                            SpawnLeftDoor(room1.xMin, y + 1);
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
            }
            else if (angle < 0f)
            {
                
                if (!found)
                {
                    for (int y = room1.yMin; y <= room1.yMax; y++)
                    {
                        if (mapData.hallwaysT[room1.xMin, room1.yMin] == 1)
                        {
                            SpawnLeftDoor(room1.xMin, y + 1);
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
                        if (mapData.hallways[room1.xMin, room1.yMin] == 1)
                        {
                            SpawnLeftDoor(room1.xMin, y + 1);
                            break;
                        }
                    }
                }
                //scan yMin and xMax
            }
            else if (angle < 90f)
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
                        if (mapData.hallwaysT[room1.xMax, room1.yMin] == 1)
                        {
                            SpawnRightDoor(room1.xMax, y + 1);
                            found = true;
                            break;
                        }
                    }
                }
                if (!found)
                {
                    for (int y = room1.yMin; y <= room1.yMax; y++)
                    {
                        if (mapData.hallways[room1.xMax, room1.yMin] == 1)
                        {
                            SpawnRightDoor(room1.xMax, y + 1);
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

                // scan xMax and Ymax;
            }
            else
            {
                if (!found)
                {
                    for (int y = room1.yMin; y <= room1.yMax; y++)
                    {
                        if (mapData.hallwaysT[room1.xMax, room1.yMin] == 1)
                        {
                            SpawnRightDoor(room1.xMax, y + 1);
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
                        if (mapData.hallways[room1.xMax, room1.yMin] == 1)
                        {
                            SpawnRightDoor(room1.xMax, y + 1);
                            break;
                        }
                    }
                }
                // scan yMax and xMin;
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
        doorGrid.SetTile(new Vector3Int(x, y - 2, 0), door);
        doorGrid.SetTile(new Vector3Int(x, y - 1, 0), door);
        //doorGrid.SetTile(new Vector3Int(x, y, 0), door);
        doorGrid.SetTile(new Vector3Int(x + 1, y - 2, 0), door);
        doorGrid.SetTile(new Vector3Int(x + 1, y - 1, 0), door);
        doorGrid.SetTile(new Vector3Int(x + 1, y , 0), door);
    }
    // X
    // X
    // Y
    // X
    void SpawnLeftDoor(int x, int y)
    {
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
        doorGrid.SetTile(new Vector3Int(x, y - 2, 0), door);
        //doorGrid.SetTile(new Vector3Int(x - 1, y - 2, 0), door);
        doorGrid.SetTile(new Vector3Int(x, y - 1, 0), door);
        doorGrid.SetTile(new Vector3Int(x - 1, y - 1, 0), door);
        doorGrid.SetTile(new Vector3Int(x, y, 0), door);
        doorGrid.SetTile(new Vector3Int(x - 1, y, 0), door);
    }

    void SpawnBotDoor(int x, int y)
    {
        //doorGrid.SetTile(new Vector3Int(x, y - 1, 0), door);
        doorGrid.SetTile(new Vector3Int(x - 1, y - 1, 0), door);
        doorGrid.SetTile(new Vector3Int(x, y, 0), door);
        doorGrid.SetTile(new Vector3Int(x - 1, y, 0), door);
        doorGrid.SetTile(new Vector3Int(x, y + 1, 0), door);
        doorGrid.SetTile(new Vector3Int(x - 1, y + 1, 0), door);
    }

}
    //ver
        
  
