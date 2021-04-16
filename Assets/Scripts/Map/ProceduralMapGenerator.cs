using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Tilemaps;
using TriangleNet.Geometry;
using TriangleNet.Topology;

public class ProceduralMapGenerator : MonoBehaviour
{
    // Parameters that modify the dungeon's size (maximum grid size and maximum/minimum room size)
    [SerializeField]
    public int maxLeafSize = 30;
    [SerializeField]
    public int maxRoomSize = 23;
    [SerializeField]
    public int mapHeight = 60;
    [SerializeField]
    public int mapWidth = 80;
    [SerializeField]
    public int offSetBorder = 0;
    [SerializeField]
    public int maxWeightShortcut = 5;
    [SerializeField]
    public int shortcutChance = 50;
    [SerializeField]
    public bool drawDelaunay = true;
    [SerializeField]
    public bool drawMST = true;
    [SerializeField]
    public bool drawDelaunayShortcuts = true;
    [SerializeField]
    public bool drawBSP = true;
    [SerializeField]
    private TileBase dungeon;
    [SerializeField]
    private Tilemap dungeonGrid;
    [SerializeField]
    private Tilemap minimapGrid;

    // Variables that stores level info
    public int[,] level;
    public int[,] hallways;
    public int[,] hallwaysT;
    public List<Edge> mstEdges;
    public List<Edge> delaunayEdges;
    public Dictionary<Vector3, Vector3> bspDebug;
    private List<Edge> meshEdges;
    private List<Edge> allEdges;
    private List<Edge> tEdges;
    private List<Leaf> leafs;

    // Variables for identifiying Delaunay edge reincorporation candidates
    private float[] weightsArrSorted;
    private Edge[] edgesArrSorted;
    private int indexMaxMSTWeight;
    public Dictionary<Vertex, int> distances;
    private List<RectInt> rooms;
    public HashSet<Vertex> vertices;
    public int maxDistance;
    private DisjointSet ds;
    public TriangleNet.Mesh mesh;
    public Vertex start;
    public Vertex end;
    PopulateDungeon populateDungeon;

    // Method that initializes some of the variables and sequences the appropriate methods that ultimately creates a procedurally generated dungeon.
    void Start()
    {
        rooms = new List<RectInt>();
    
        level = generateBSP(this.mapWidth, this.mapHeight);
        mesh = DelaunayTriangulation();
        meshEdges = new List<Edge>();
        
        foreach (Edge edge in this.mesh.Edges)
        {
            meshEdges.Add(edge);
        }

        FindMST();
        FindGraphDiameter();
        FilterDelaunayCandidates();

        this.hallways = new int[mapWidth, mapHeight];
        this.hallwaysT = new int[mapWidth, mapHeight];
        this.createHallways(this.mapWidth, this.mapHeight, this.allEdges);

        this.dungeonGrid = GetComponentInParent<Tilemap>();
        this.paintGrid();

        populateDungeon = GetComponent<PopulateDungeon>();
        populateDungeon.PopulateMap();
    }

    // Method that scans a list of every room to see if a vertex is contained within that room.
    // If a match is found, the room dimensions/coordinates are returned in the form of RectInt object.
    public RectInt FindRoom(Vertex vertex)
    {
        foreach (RectInt room in rooms)
        {
            if (room.Contains(new Vector2Int((int)vertex.x, (int)vertex.y)))
            {
                return room;
            }
        }
        return new RectInt(0, 0, 0, 0);
    }
    // Method to locate the pair of vertices with the longest length between them.
    // Runs a breadth-first serach algorithm twice.
    void FindGraphDiameter()
    {
        Vertex temp1 = BreadthFirstSearch(null);
        Vertex temp2 = BreadthFirstSearch(temp1);
        start = temp1;
        end = temp2;
  
    }
    // Breadth first search method that returns the vertex that's the furthest from the input vertex.
    Vertex BreadthFirstSearch(Vertex newSource)
    {
        Queue<Vertex> q = new Queue<Vertex>();
        List<Vertex> visited = new List<Vertex>();
        distances = new Dictionary<Vertex, int>();
        Vertex source = newSource;
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
            foreach (Edge neighborEdge in mstEdges)
            {
                Vertex v0 = mesh.vertices[neighborEdge.P0];
                Vertex v1 = mesh.vertices[neighborEdge.P1];
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
                maxDistance = distances[vertex];
            }
        }
        return source;
    }
    // Places tiles on the world-space wherever there is a 1 on the binary matrix/2-D array.
    void paintGrid()
    {
        for (int x = 0; x < this.mapWidth; x++)
        {
            for (int y = 0; y < this.mapHeight; y++)
            {
                if (hallwaysT[x, y] == 1 || level[x, y] == 1 || hallways[x, y] == 1)
                {
                    dungeonGrid.SetTile(new Vector3Int(x, y, 0), this.dungeon);
                }
            }
        }
    }
    // Kruskal's Algorithm for finding an MST
    void FindMST()
    {
        if (mesh != null)
        {
            List<Edge> edges = new List<Edge>();
            List<float> weights = new List<float>();
            foreach (Edge edge in mesh.Edges)
            {
                Vertex v0 = mesh.vertices[edge.P0];
                Vertex v1 = mesh.vertices[edge.P1];
                edges.Add(edge);
                weights.Add(Mathf.Sqrt((((float)v0.x - (float)v1.x) * ((float)v0.x - (float)v1.x)) + (((float)v0.y - (float)v1.y) * ((float)v0.y - (float)v1.y))));
            }
            weightsArrSorted = weights.ToArray();
            edgesArrSorted = edges.ToArray();
            System.Array.Sort(weightsArrSorted, edgesArrSorted);
            this.vertices = new HashSet<Vertex>();
            for (int i = 0; i < weightsArrSorted.Length; i++)
            {
                Vertex v0 = mesh.vertices[edgesArrSorted[i].P0];
                Vertex v1 = mesh.vertices[edgesArrSorted[i].P1];
                Debug.Log(weightsArrSorted[i] + "- x0: " + (float)v0.x + " y0: " + (float)v0.y + " x1: " + (float)v1.x + " y1: " + (float)v1.y);
                this.vertices.Add(v0);
                this.vertices.Add(v1);
            }

            mstEdges = new List<Edge>();

            ds = new DisjointSet();
            ds.MakeSet(this.vertices);
            indexMaxMSTWeight = 0;
            while (mstEdges.Count != rooms.Count - 1)
            {
                Edge nextEdge = edgesArrSorted[indexMaxMSTWeight];

                Vertex x = ds.Find(mesh.vertices[nextEdge.P0]);
                Vertex y = ds.Find(mesh.vertices[nextEdge.P1]);
                Debug.Log(weightsArrSorted[indexMaxMSTWeight] + " - MST - x0: " + (float)x.x + " y0: " + (float)x.y + " x1: " + (float)y.x + " y1: " + (float)y.y);
                indexMaxMSTWeight++;

                if (x != y)
                {
                    mstEdges.Add(nextEdge);
                    ds.Union(x, y);
                }
            }
        }
    }

    // Finds and selects viable unused Delaunay edge candidates to be reincorporated into the MST.
    void FilterDelaunayCandidates()
    {
        
        delaunayEdges = new List<Edge>();
        allEdges = new List<Edge>(mstEdges);
        for (int i = 0; i < Mathf.Min(indexMaxMSTWeight + maxWeightShortcut, edgesArrSorted.Length); i++)
        {
            if (!allEdges.Contains(edgesArrSorted[i]))
            {
                bool triangle = false;
                bool startEnd = false;
                foreach (Edge edge1 in allEdges)
                {
                    foreach (Edge edge2 in allEdges)
                    {
                        if (!edge1.Equals(edge2))
                        {
                            HashSet<Vertex> hash = new HashSet<Vertex>();
                            hash.Add(mesh.vertices[edge1.P0]);
                            hash.Add(mesh.vertices[edge1.P1]);
                            hash.Add(mesh.vertices[edge2.P0]);
                            hash.Add(mesh.vertices[edge2.P1]);
                            hash.Add(mesh.vertices[edgesArrSorted[i].P0]);
                            hash.Add(mesh.vertices[edgesArrSorted[i].P1]);
                            if (hash.Count == 3)
                            {
                                triangle = true;
                            }
                            hash = new HashSet<Vertex>();
                        }
                    }
                }
                if (mesh.vertices[edgesArrSorted[i].P0].Equals(start)
                    || mesh.vertices[edgesArrSorted[i].P0].Equals(end)
                    || mesh.vertices[edgesArrSorted[i].P1].Equals(start)
                    || mesh.vertices[edgesArrSorted[i].P1].Equals(end))
                {
                    Debug.Log("Start/End: flag is true");
                    startEnd = true;
                }
                if (Random.Range(0, 100) < shortcutChance && !triangle && !startEnd)
                {
                    allEdges.Add(edgesArrSorted[i]);
                    delaunayEdges.Add(edgesArrSorted[i]);
                }
            }
        }
    }

    // Method using the built in Triangle.Net Dwyer's DelaunayTriangulation algorithm.
    TriangleNet.Mesh DelaunayTriangulation()
    {
        Polygon polygon = new Polygon();
        for (int i = 0; i < rooms.Count; i++)
        {
            polygon.Add(new Vertex(rooms[i].center.x, rooms[i].center.y));
        }

        TriangleNet.Meshing.ConstraintOptions options = new TriangleNet.Meshing.ConstraintOptions()
        {
            ConformingDelaunay = false
        };

        return (TriangleNet.Mesh)polygon.Triangulate(options);
    }

    // Creates a binary tree node that stores information for the binary seperation algorithm.
    int[,] generateBSP(int mapWidth, int mapHeight)
    {
        level = new int[mapWidth + offSetBorder * 3, mapHeight + offSetBorder * 3];
        leafs = new List<Leaf>();

        Leaf root = new Leaf(0, 0, mapWidth, mapHeight);
        leafs.Add(root);

        bool splitSuccess = true;

        while (splitSuccess)
        {
            splitSuccess = false;
            for (int i = 0; i < this.leafs.Count; i++)
            {
                if (leafs[i].child1 == null && leafs[i].child2 == null)
                {
                    if (leafs[i].width > maxLeafSize || leafs[i].height > Mathf.RoundToInt(maxLeafSize * 0.75f) || Random.Range(0, 100) < 0)
                    {
                        if (leafs[i].SplitLeaf())
                        {
                            leafs.Add(leafs[i].child1);
                            leafs.Add(leafs[i].child2);
                            splitSuccess = true;

                        }
                    }
                }
            }
        }
        bspDebug = new Dictionary<Vector3, Vector3>();
        bspDebug.Add(new Vector3(0, 0), new Vector3(0, 0 + mapHeight));
        bspDebug.Add(new Vector3(0, 0 + mapHeight), new Vector3(0 + mapWidth, 0 + mapHeight));
        bspDebug.Add(new Vector3(0 + mapWidth, 0 + mapHeight), new Vector3(0 + mapWidth, 0));
        bspDebug.Add(new Vector3(0 + mapWidth, 0), new Vector3(0, 0));
        root.CreateRooms(this);
        foreach (Leaf leaf in leafs)
        {
            try
            {
                bspDebug.Add(leaf.split0, leaf.split1);
            } catch
            {
                bspDebug[leaf.split0] = leaf.split1;
            }
            

        }
        bspDebug[new Vector3(0, 0)] = new Vector3(0, 0 + mapHeight);
        bspDebug[new Vector3(0 + mapWidth, 0)] = new Vector3(0, 0);
        return this.level;


    }
    // Method that stores the room coordinates in each leaf on a 2-D array.
    public void CreateRoom(RectInt room)
    {
        this.rooms.Add(room);
        for (int x = room.min.x; x <= room.max.x; x++)
        {
            for (int y = room.min.y; y <= room.max.y; y++)
            {
                this.level[x, y] = 1;
            }
        }
    }

    // Method/algorithm that determiens what type of hallway should be generated between the two rooms
    // Determines wheter a vertical hallway, horizontal hallway or an L-shaped hallway (horizontal and vertical) is most suitable.
    void createHallways(int mapWidth, int mapHeight, List<Edge> edges)
    {
        tEdges = new List<Edge>();
        foreach (Edge edge in edges)
        {
            int x1 = Mathf.RoundToInt((float)mesh.vertices[edge.P0].x);
            int y1 = Mathf.RoundToInt((float)mesh.vertices[edge.P0].y);

            int x2 = Mathf.RoundToInt((float)mesh.vertices[edge.P1].x);
            int y2 = Mathf.RoundToInt((float)mesh.vertices[edge.P1].y);

            RectInt room1 = new RectInt(0, 0, 0, 0);
            RectInt room2 = new RectInt(0, 0, 0, 0);

            foreach (RectInt room in rooms)
            {
                if (x1 > room.xMin && x1 < room.xMax && y1 > room.yMin && y1 < room.yMax)
                {
                    room1 = room;
                }

                if (x2 > room.xMin && x2 < room.xMax && y2 > room.yMin && y2 < room.yMax)
                {
                    room2 = room;
                }
            }

            int xMid = Mathf.RoundToInt((x1 + x2) / 2);
            int yMid = Mathf.RoundToInt((y1 + y2) / 2);

            if (yMid <= room1.yMax - 3 && yMid <= room2.yMax - 3 && yMid >= room1.yMin + 2 && yMid >= room2.yMin + 2)
            {
                CreateHorTunnel(Mathf.Min(room1.xMax, room2.xMax), Mathf.Max(room1.xMin, room2.xMin), yMid, room1, room2);

            }
            else if (xMid <= room1.xMax - 3 && xMid <= room2.xMax - 3 && xMid >= room1.xMin + 2 && xMid >= room2.xMin + 2)
            {
                CreateVerTunnel(Mathf.Min(room1.yMax, room2.yMax), Mathf.Max(room1.yMin, room2.yMin), xMid, room1, room2);
            }
            else
            {
                tEdges.Add(edge);
            }
        }
        foreach (Edge edge in tEdges)
        {
            int x1 = Mathf.RoundToInt((float)mesh.vertices[edge.P0].x);
            int y1 = Mathf.RoundToInt((float)mesh.vertices[edge.P0].y);

            int x2 = Mathf.RoundToInt((float)mesh.vertices[edge.P1].x);
            int y2 = Mathf.RoundToInt((float)mesh.vertices[edge.P1].y);

            RectInt room1 = new RectInt(0, 0, 0, 0);
            RectInt room2 = new RectInt(0, 0, 0, 0);

            foreach (RectInt room in rooms)
            {
                if (x1 > room.xMin && x1 < room.xMax && y1 > room.yMin && y1 < room.yMax)
                {
                    room1 = room;
                }

                if (x2 > room.xMin && x2 < room.xMax && y2 > room.yMin && y2 < room.yMax)
                {
                    room2 = room;
                }
            }
            CreateTTunnel(x1, x2, y1, y2, room1, room2);

        }


    }
    // Creates a horizontal hallway between two sets of coordinates with the same y value.
    private void CreateHorTunnel(int x1, int x2, int y, RectInt room1, RectInt room2)
    {
        for (int x = Mathf.Min(x1, x2); x <= Mathf.Max(x1, x2); x++)
        {
            this.hallways[x, y - 2] = 1;
            this.hallways[x, y - 1] = 1;
            this.hallways[x, y] = 1;
            this.hallways[x, y + 1] = 1;
        }


    }

    // Creates a vertical hallway between two sets of coordinates with the same x value.
    public void CreateVerTunnel(int y1, int y2, int x, RectInt room1, RectInt room2)
    {
        for (int y = Mathf.Min(y1, y2); y <= Mathf.Max(y1, y2); y++)
        {
            this.hallways[x - 2, y] = 1;
            this.hallways[x - 1, y] = 1;
            this.hallways[x, y] = 1;
            this.hallways[x + 1, y] = 1;
        }

    }

    // Determine how to place L shaped hallways (multiple overlapping L shaped hallways makes a T shape, hence the "T" in "CreatTTunnel")
    // Needs refactoring or an algorithm change as it does not always generate appropriate hallways
    // TODO:
    // Refactor,
    // or
    // replace with A* algorithm
    private void CreateTTunnel(int x1, int x2, int y1, int y2, RectInt room1, RectInt room2)
    {
        int yMid = y2;
        int xMid = x1;
        // yMid/xMid dtermines which orientation the L hallway ("H") will go when connecting rooms A <-> B without hitting room C
        // What this methods aims to do with this spaghetti code (avoid room C):
        //  .   .   .   .   .   .   B   B   B
        //  .   H   H   H   H   H   B   B   B
        //  .   H   .   .   .   .   B   B   B
        //  .   H   .   .   .   .   .   .   .
        //  .   H   .   .   .   .   .   .   .
        //  .   H   .   .   .   .   .   .   .
        //  .   H   .   .   .   .   .   .   .
        //  A   A   A   .   .   .   C   C   C
        //  A   A   A   .   .   .   C   C   C
        //  A   A   A   .   .   .   C   C   C
        //
        // Instead of this (going through room C):
        //  .   .   .   .   .   .   B   B   B
        //  .   .   .   .   .   .   B   B   B
        //  .   .   .   .   .   .   .   H   .
        //  .   .   .   .   .   .   .   H   .
        //  .   .   .   .   .   .   .   H   .
        //  .   .   .   .   .   .   .   H   .
        //  A   A   A   .   .   .   C   H   C
        //  A   A   A   H   H   H   H   H   C
        //  A   A   A   .   .   .   C   C   C

        if ((room2.yMin < room1.yMax && room1.yMax < room2.yMax))
        {
            yMid = y1;
            xMid = x2;
        }
        else if ((room1.yMin < room2.yMax && room2.yMax < room1.yMax))
        {
            yMid = y2;
            xMid = x1;
        }
        else if ((room2.xMin < room1.xMax && room1.xMax < room2.xMax))
        {
            yMid = y2;
            xMid = x1;
        }
        else if (room1.xMin < room2.xMax && room2.xMax < room1.xMax)
        {
            yMid = y1;
            xMid = x2;
        }
        else
        {
            bool swap = false;
            for (int x = Mathf.Min(room2.xMax, room1.xMax) + 1; x < xMid; x++)
            {
                if ((this.level[x, yMid - 3] == 1 || this.level[x, yMid - 2] == 1 || this.level[x, yMid - 1] == 1 || this.level[x, yMid] == 1 || this.level[x, yMid + 1] == 1 || this.level[x, yMid + 2] == 1))
                {
                    swap = true;

                }
            }
            for (int y = Mathf.Min(room2.yMax, room1.yMax) + 1; y < yMid; y++)
            {
                if (this.level[xMid - 3, y] == 1 || this.level[xMid - 2, y] == 1 || this.level[xMid - 1, y] == 1 || this.level[xMid, y] == 1 || this.level[xMid + 1, y] == 1 || this.level[xMid + 2, y] == 1)
                {
                    swap = true;
                }

            }
            for (int x = Mathf.Max(room2.xMin, room1.xMin) - 1; x >= xMid; x--)
            {

                if ((this.level[x, yMid - 3] == 1 || this.level[x, yMid - 2] == 1 || this.level[x, yMid - 1] == 1 || this.level[x, yMid] == 1 || this.level[x, yMid + 1] == 1 || this.level[x, yMid + 2] == 1))
                {
                    swap = true;

                }
            }
            for (int y = Mathf.Max(room2.yMin, room1.yMin) - 1; y >= yMid; y--)
            {
                if (this.level[xMid - 3, y] == 1 || this.level[xMid - 2, y] == 1 || this.level[xMid - 1, y] == 1 || this.level[xMid, y] == 1 || this.level[xMid + 1, y] == 1 || this.level[xMid + 2, y] == 1)
                {
                    swap = true;
                }

            }
            if (swap)
            {
                yMid = y1;
                xMid = x2;
            }
            else
            {
                yMid = y2;
                xMid = x1;
            }
        }

        for (int x = Mathf.Min(x1, x2) - 2; x <= Mathf.Max(x1, x2) + 1; x++)
        {
            if (!(this.hallways[x, yMid - 3] == 1 || this.hallways[x, yMid - 2] == 1 || this.hallways[x, yMid - 1] == 1 || this.hallways[x, yMid] == 1 || this.hallways[x, yMid + 1] == 1 || this.hallways[x, yMid + 2] == 1))
            {
                this.hallwaysT[x, yMid - 2] = 1;
                this.hallwaysT[x, yMid - 1] = 1;
                this.hallwaysT[x, yMid] = 1;
                this.hallwaysT[x, yMid + 1] = 1;

                if (!(this.level[x, yMid - 3] == 1 && this.level[x, yMid - 2] == 1 && this.level[x, yMid - 1] == 1 && this.level[x, yMid] == 1 && this.level[x, yMid + 1] == 1 && this.level[x, yMid + 2] == 1)
                    || !(this.level[x, yMid - 3] == 0 && this.level[x, yMid - 2] == 0 && this.level[x, yMid - 1] == 0 && this.level[x, yMid] == 0 && this.level[x, yMid + 1] == 0 && this.level[x, yMid + 2] == 0))
                {
                    if ((Mathf.RoundToInt(room1.yMin) >= yMid - 2 && Mathf.RoundToInt(room1.yMin) <= yMid + 2) || (Mathf.RoundToInt(room1.yMax) >= yMid - 2 && Mathf.RoundToInt(room1.yMax) <= yMid + 2))
                    {
                        for (int i = room1.xMin; i <= room1.xMax; i++)
                        {
                            for (int j = yMid - 2; j <= yMid + 1; j++)
                            {
                                this.level[i, j] = 1;
                            }
                        }
                    }
                    if ((Mathf.RoundToInt(room2.yMin) >= yMid - 2 && Mathf.RoundToInt(room2.yMin) <= yMid + 2) || (Mathf.RoundToInt(room2.yMax) >= yMid - 2 && Mathf.RoundToInt(room2.yMax) <= yMid + 2))
                    {
                        for (int i = room2.xMin; i <= room2.xMax; i++)
                        {
                            for (int j = yMid - 2; j <= yMid + 1; j++)
                            {
                                this.level[i, j] = 1;
                            }
                        }
                    }
                }
            }
            else
            {
                // Depcrated method to remove excess hallways
                //if (x < (Mathf.Max(x1, x2) - 2) && this.hallways[x, yMid - 2] == 1)
                //{
                //    this.hallwaysT[x, yMid - 2] = 0;
                //    this.hallwaysT[x, yMid - 1] = 0;
                //    this.hallwaysT[x, yMid] = 0;
                //    this.hallwaysT[x, yMid + 1] = 0;
                //}
            }
        }

        for (int y = Mathf.Min(y1, y2) - 2; y <= Mathf.Max(y1, y2) + 1; y++)
        {
            if (!(this.hallways[xMid - 3, y] == 1 || this.hallways[xMid - 2, y] == 1 || this.hallways[xMid - 1, y] == 1 || this.hallways[xMid, y] == 1 || this.hallways[xMid + 1, y] == 1 || this.hallways[xMid + 2, y] == 1))
            {
                this.hallwaysT[xMid - 2, y] = 1;
                this.hallwaysT[xMid - 1, y] = 1;
                this.hallwaysT[xMid, y] = 1;
                this.hallwaysT[xMid + 1, y] = 1;

                if (!(this.level[xMid - 2, y] == 1 && this.level[xMid - 1, y] == 1 && this.level[xMid, y] == 1 && this.level[xMid + 1, y] == 1 && this.level[xMid - 3, y] == 1 && this.level[xMid + 2, y] == 1)
                    || !(this.level[xMid - 2, y] == 0 && this.level[xMid - 1, y] == 0 && this.level[xMid, y] == 0 && this.level[xMid + 1, y] == 0) && this.level[xMid - 3, y] == 0 && this.level[xMid + 2, y] == 0)
                {
                    if ((Mathf.RoundToInt(room1.xMin) >= xMid - 2 && Mathf.RoundToInt(room1.xMin) <= xMid + 2) || (Mathf.RoundToInt(room1.xMax) >= xMid - 2 && Mathf.RoundToInt(room1.xMax) <= xMid + 2))
                    {
                        for (int j = room1.yMin; j <= room1.yMax; j++)
                        {
                            for (int i = xMid - 2; i <= xMid + 1; i++)
                            {
                                this.level[i, j] = 1;
                            }
                        }
                    }
                    if ((Mathf.RoundToInt(room2.xMin) >= xMid - 2 && Mathf.RoundToInt(room2.xMin) <= xMid + 2) || (Mathf.RoundToInt(room2.xMax) >= xMid - 2 && Mathf.RoundToInt(room2.xMax) <= xMid + 2))
                    {
                        for (int j = room2.yMin; j <= room2.yMax; j++)
                        {
                            for (int i = xMid - 2; i <= xMid + 1; i++)
                            {
                                this.level[i, j] = 1;
                            }
                        }
                    }
                }
            }
            else
            {
                // Depcrated method to remove excess hallways
                //if (y < (Mathf.Max(y1, y2) - 2) && this.hallways[xMid - 2, y] == 1)
                //{
                //    this.hallwaysT[xMid - 2, y] = 0;
                //    this.hallwaysT[xMid - 1, y] = 0;
                //    this.hallwaysT[xMid, y] = 0;
                //    this.hallwaysT[xMid + 1, y] = 0;
                //}
            }
        }
    }

    // Displays debug graph/edges on the editor each frame if boolean variables are set to true.
    void Update()
    {
        if (drawDelaunay)
        {
            DrawDebugGraph(this.meshEdges, Color.red);
        }
        if (drawMST)
        {
            DrawDebugGraph(this.mstEdges, Color.green);
        }
        if (drawDelaunayShortcuts)
        {
            DrawDebugGraph(this.delaunayEdges, Color.blue);
        }
        if (drawBSP)
        {
            DrawDebugGraph(this.bspDebug, Color.yellow);
        }
    }

    // Method draws graph edges on the Scene view in the editor.
    void DrawDebugGraph(List<Edge> edges, Color color)
    {
        if (mesh != null)
        {
            foreach (Edge edge in edges)
            {
                Vertex v0 = mesh.vertices[edge.P0];
                Vertex v1 = mesh.vertices[edge.P1];
                Vector3 p0 = new Vector3((float)v0.x, (float)v0.y, 0.0f);
                Vector3 p1 = new Vector3((float)v1.x, (float)v1.y, 0.0f);
                Debug.DrawLine(p0, p1, color, Time.deltaTime, false);
            }
        }
    }
    // Alternate method that draws graph edges on the Scene view in the editor (with different parameters).
    void DrawDebugGraph(Dictionary<Vector3, Vector3> vectorEdges, Color color)
    {
            foreach (KeyValuePair<Vector3, Vector3> edge in vectorEdges)
            {
                Debug.DrawLine(edge.Key, edge.Value, color, Time.deltaTime, false);
            }
    }

    // Class structure that enables union/adds and finding vertices to be used with Kruskal's MST algorithm.
    // Similar in structure to a linked list.
    class DisjointSet
    {
        // Dictionary enables us to sstablish parent -> child relationships for each vertex.
        Dictionary<Vertex, Vertex> parent = new Dictionary<Vertex, Vertex>();

        // Add each vertex to its own disjoint set where none of them are connected to each other.
        public void MakeSet(HashSet<Vertex> vertices)
        {
            foreach (Vertex vertex in vertices)
            {
                parent.Add(vertex, vertex);
            }
        }

        // Find the immediate parent/neighbor of the vertex.
        public Vertex Find(Vertex k)
        {
            if (parent[k] == k)
            {
                return k;
            }
            return Find(parent[k]);
        }

        // Combine two disjoint sets
        public void Union(Vertex a, Vertex b)
        {
            Vertex x = Find(a);
            Vertex y = Find(b);
            if (parent.ContainsKey(x))
            {
                parent[x] = y;
            }
            else
            {
                parent.Add(x, y);
            }
        }
    }

    // Leaf structure class to assist with Binary Seperation Algorithm for a rectangular shape in 2D space
    public class Leaf
    {
        // Variables that affect leaf sizes
        public int x;
        public int y;
        public int width;
        public int height;
        public Leaf child1;
        public Leaf child2;
        public int min_width = 15;
        public int min_height = 12;
        public Vector3 split0;
        public Vector3 split1;
        //public Dictionary<Vector3, Vector3> bspEdges;

        private RectInt room;

        // Create leaf at position (x, y) with a set width/height
        public Leaf(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            this.room = new RectInt(0, 0, 0, 0);
        }

        // Method that either bisects a leaf vertically or horizontally as long as the leaf's size is above a minimum threshold.
        // Returns true if the bisection was succesful, returns false if the bisection was unsuccessful.
        public bool SplitLeaf()
        {
            if (child1 != null || child2 != null)
            {
                return false;
            }

            bool splitHor = true;
            if (Random.Range(0, 100) < 50)
            {
                splitHor = false;
            }

            // Deprecated constraint that creates more square-ish rooms than rectangular ones.

            //if (this.width > this.height)
            //{
            //    splitHor = false;
            //}
            //else if (this.height > this.width)
            //{
            //    splitHor = true;
            //}

            int max;

            if (splitHor)
            {
                max = this.height - this.min_height;
                if (max < this.min_height)
                {
                    return false;
                }
            }
            else
            {
                max = this.width - this.min_width;
                if (max < this.min_width)
                {
                    return false;
                }
            }

            int split;

            if (splitHor)
            {
                split = Mathf.RoundToInt(Random.Range(this.min_height, Mathf.RoundToInt(max * 0.75f)));
                this.child1 = new Leaf(this.x, this.y, this.width - 1, split - 1);
                split0 = new Vector3(x, y + split);
                split1 = new Vector3(x + this.width + 2, y + split);
                this.child2 = new Leaf(this.x, this.y + split, this.width - 1, this.height - split - 1);
            }
            else
            {
                split = Mathf.RoundToInt(Random.Range(this.min_width, max));
                this.child1 = new Leaf(this.x, this.y, split -1, this.height - 1);
                split0 = new Vector3(x + split, y);
                split1 = new Vector3(x + split, y + this.height + 2);
                this.child2 = new Leaf(this.x + split, this.y, this.width - split - 1, this.height - 1);
            }
            return true;
        }

        // Method that creates a rectangular room within the leaf.
        public void CreateRooms(ProceduralMapGenerator bspTree)
        {
            if (child1 != null || child2 != null)
            {
                if (child1 != null)
                {
                    child1.CreateRooms(bspTree);

                }
                if (child2 != null)
                {
                    child2.CreateRooms(bspTree);
                }
            }
            else
            {
                int w = Mathf.RoundToInt(Random.Range(min_width, Mathf.Min(bspTree.maxRoomSize, this.width - 2))) - 3;
                int h = Mathf.RoundToInt(Random.Range(min_height, Mathf.Min(bspTree.maxRoomSize, this.height - 2))) - 3;
                int x = Mathf.RoundToInt(Random.Range(this.x, this.x + (this.width - 2) - w)) + 2;
                int y = Mathf.RoundToInt(Random.Range(this.y, this.y + (this.height - 2) - h)) + 2;

                this.room = new RectInt(x, y, w, h);
                bspTree.CreateRoom(this.room);
            }
        }

        // Method that returns the room at each leaf by traversing the binary tree recursively.
        RectInt GetRoom()
        {
            if (!room.Equals(new RectInt(0, 0, 0, 0)))
            {
                return room;
            }
            else
            {
                RectInt room1 = new RectInt(0, 0, 0, 0);
                RectInt room2 = new RectInt(0, 0, 0, 0);
                if (this.child1 != null)
                {
                    room1 = child1.GetRoom();
                }

                if (this.child2 != null)
                {
                    room2 = child2.GetRoom();
                }

                if (!room1.Equals(new RectInt(0, 0, 0, 0)) || !room2.Equals(new RectInt(0, 0, 0, 0)))
                {
                    return new RectInt(0, 0, 0, 0);
                }
                else if (!room2.Equals(new RectInt(0, 0, 0, 0)))
                {
                    return room1;
                }
                else if (!room1.Equals(new RectInt(0, 0, 0, 0)))
                {
                    return room2;
                }
                else
                {
                    if (Random.Range(0, 100) < 50)
                    {
                        return room1;
                    }
                    else
                    {
                        return room2;
                    }
                }
            }
        }
    }
}