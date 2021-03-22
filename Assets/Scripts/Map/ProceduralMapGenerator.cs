using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Tilemaps;
using TriangleNet.Geometry;
using TriangleNet.Topology;
public class ProceduralMapGenerator : MonoBehaviour
{
    [SerializeField]
    public int maxLeafSize = 30;

    [SerializeField]
    public int maxRoomSize = 20;
    
    [SerializeField]
    public int mapHeight = 60;
    [SerializeField]
    public int mapWidth = 80;

    [SerializeField]
    public int offSetBorder = 20;

    [SerializeField]
    public bool drawDeluanay = true;

    public int offSet = 10;

    //[SerializeField]
    //public int maxLeafSize = 500;

    //[SerializeField]
    //public int maxRoomSize = 340;
    //[SerializeField]
    //public int minRoomSize = 140;
    //[SerializeField]
    //public int mapHeight = 900;
    //[SerializeField]
    //public int mapWidth = 1600;


    [SerializeField]
    private TileBase dungeon;
    [SerializeField]
    private Tilemap grid;

    //private List<Vector2Int> level;
    private int[,] level;
    private int[,] hallways;
    private List<Leaf> leafs;
    private List<RectInt> rooms;
    private TriangleNet.Mesh mesh;
    private List<Edge> mstEdges;
    //private List<Vector2Int> vertices;
    //private List<Vector2Int, Vector2Int> edges;
    //public ProceduralMapGenerator map;
    void Awake()
    {
        this.rooms = new List<RectInt>();
        this.level = generateBSP(this.mapWidth, this.mapHeight);
        this.mesh = deluanayTriangulation();
        this.mstEdges = findMST();

        //createHallways();

        //this.edges = bowyerWatsonDeluanyTriangle();
        this.grid = GetComponentInParent<Tilemap>();
        for (int x = 0; x < this.mapWidth + offSetBorder * 3; x++)
        {
            for (int y = 0; y < this.mapHeight + offSetBorder * 3; y++)
            {
                if (level[x, y] == 1)
                {
                    grid.SetTile(new Vector3Int(x, y, 0), this.dungeon);
                    Debug.Log(x);
                }

            }
        }

        this.hallways = createHallways(this.mapWidth, this.mapHeight);
        for (int x = 0; x < this.mapWidth + offSetBorder * 3; x++)
        {
            for (int y = 0; y < this.mapHeight + offSetBorder * 3; y++)
            {
                if (hallways[x, y] == 1)
                {
                    grid.SetTile(new Vector3Int(x, y, 0), this.dungeon);
                    Debug.Log(x);
                }

            }
        }


        //gameObject.AddComponent<MeshFilter>();
        //gameObject.AddComponent<MeshRenderer>();
        //Mesh mesh = GetComponent<MeshFilter>().mesh;

        //mesh.Clear();

        //mesh.vertices = new Vector3[] { new Vector3(0, 0, 0), new Vector3(300, 0, 0), new Vector3(0, 300, 0) };
        ////mesh.uv = new Vector2[] { new Vector2(0, 0), new Vector2(300, 0), new Vector2(0, 300) };
        //mesh.triangles = new int[] { 0, 1, 2 };
        Debug.DrawLine(new Vector3(0, 0, 0), new Vector3(220, 220, 0), Color.green, 0f, false);
        //level = new List<Vector2Int>();


    }
   List<Edge> findMST()
    {
        if (mesh != null)
        {
            List<Edge> edges = new List<Edge>();
            List<float> weights = new List<float>();
            foreach (Edge edge in mesh.Edges)
            {
                Vertex v0 = mesh.vertices[edge.P0];
                Vertex v1 = mesh.vertices[edge.P1];
                Vector3 p0 = new Vector3((float)v0.x, (float)v0.y, 0.0f);
                Vector3 p1 = new Vector3((float)v1.x, (float)v1.y, 0.0f);
                edges.Add(edge);
                weights.Add(Mathf.Sqrt((((float)v0.x - (float)v1.x) * ((float)v0.x - (float)v1.x)) + (((float)v0.y - (float)v1.y) * ((float)v0.y - (float)v1.y))));
            }
            float[] weightsArr = weights.ToArray();
            Edge[] edgesArr = edges.ToArray();
            System.Array.Sort(weightsArr, edgesArr);
            HashSet<Vertex> usedVertices = new HashSet<Vertex>();
            for (int i = 0; i < weightsArr.Length; i++)
            {
                Vertex v0 = mesh.vertices[edgesArr[i].P0];
                Vertex v1 = mesh.vertices[edgesArr[i].P1];
                Debug.Log(weightsArr[i] + "- x0: " + (float)v0.x + " y0: " + (float)v0.y + " x1: " + (float)v1.x + " y1: " +(float)v1.y);
                usedVertices.Add(v0);
                usedVertices.Add(v1);
            }
            
            //Set<Vertex> touched = new List<Vertex>();
            List<Edge> mst = new List<Edge>();

            DisjointSet ds = new DisjointSet();
            ds.makeSet(usedVertices);
            int index = 0;
            while (mst.Count != rooms.Count - 1)
            {
                Edge nextEdge = edgesArr[index];
                
                Vertex x = ds.find(mesh.vertices[nextEdge.P0]);
                Vertex y = ds.find(mesh.vertices[nextEdge.P1]);
                Debug.Log(weightsArr[index] + " - MST - x0: " + (float)x.x + " y0: " + (float)x.y + " x1: " + (float)y.x + " y1: " + (float)y.y);
                index++;
                if (x != y)
                {
                    mst.Add(nextEdge);
                    ds.union(x, y);
                }
            }
            return mst;
            //for (int i = 0; i < weightsArr.Length; i++)
            //{
            //    Vertex v0 = mesh.vertices[edgesArr[i].P0];
            //    Vertex v1 = mesh.vertices[edgesArr[i].P1];
            //    if (!usedVertices.Contains(v0) || !usedVertices.Contains(v1))
            //    {
            //        keep.Add(edgesArr[i]);
            //        usedVertices.Add(v0);
            //        usedVertices.Add(v1);
            //    }
                
            //    if (keep.Count >= rooms.Count - 1)
            //    {
            //        break;
            //    }
            //}
            //for (int i = 0; i < weightsArr.Length; i++)
            //{
            //    Vertex v0 = mesh.vertices[edgesArr[i].P0];
            //    Vertex v1 = mesh.vertices[edgesArr[i].P1];
            //    if (!usedVertices.Contains(v0) || !usedVertices.Contains(v1))
            //    {
            //        keep.Add(edgesArr[i]);
            //        usedVertices.Add(v0);
            //        usedVertices.Add(v1);
            //    }
                
            //    if (keep.Count >= rooms.Count - 1)
            //    {
            //        break;
            //    }
            //}
        }
        return null;
    }
    TriangleNet.Mesh deluanayTriangulation()
    {
        Polygon polygon = new Polygon();
        //polygon.Add(new Vertex())

        for (int i = 0; i < rooms.Count; i++)
        {
            polygon.Add(new Vertex(rooms[i].center.x, rooms[i].center.y));
        }
        Debug.Log("Room count:" + rooms.Count);

        TriangleNet.Meshing.ConstraintOptions options = new TriangleNet.Meshing.ConstraintOptions()
        {
            ConformingDelaunay = false
        };

        return (TriangleNet.Mesh)polygon.Triangulate(options);
    }

    // List<Vector2Int> bowyerWatsonDeluanyTriangle()
    //  {
    //edges = new List<Vector2Int>();
    //edges.Add(new Vector2Int(0, ))
    //return edges;
    // }
    int[,] generateBSP(int mapWidth, int mapHeight)
    {

        //level.Add(new Vector2Int(0, 0));
        level = new int[mapWidth + offSetBorder * 3, mapHeight + offSetBorder * 3];
        //level = new int[220, 220];
        leafs = new List<Leaf>();

        Leaf root = new Leaf(offSetBorder, offSetBorder, mapWidth, mapHeight);
        leafs.Add(root);

        bool splitSuccess = true;

        while (splitSuccess)
        {
            splitSuccess = false;
            for (int i = 0; i < this.leafs.Count; i++)
            {
                if (this.leafs[i].child1 == null && this.leafs[i].child2 == null)
                {
                    if (this.leafs[i].width > this.maxLeafSize || this.leafs[i].height > Mathf.RoundToInt(this.maxLeafSize*0.75f) || Random.Range(0,100)< 0) {
                        if (this.leafs[i].splitLeaf())
                        {
                            this.leafs.Add(this.leafs[i].child1);
                            this.leafs.Add(this.leafs[i].child2);
                            splitSuccess = true;
                        }
                    }
                }
            }
        }
        root.createRooms(this);

        return this.level;


    }
    public void createRoom(RectInt room)
    {
        this.rooms.Add(room);
        Debug.Log("room size: " + room.allPositionsWithin.ToString());
        Debug.Log("room size: " + room.min.x + " " + room.max.x);
        for (int x = room.min.x; x < room.max.x; x++)
        {
            for (int y = room.min.y; y < room.max.y; y++)
            {
                Debug.Log("x: " + x + " y: " + y);

                this.level[x, y] = 1;
            }
        }
    }

    int[,] createHallways(int mapWidth, int mapHeight)
    {

        //level.Add(new Vector2Int(0, 0));
        hallways = new int[mapWidth + offSetBorder * 3, mapHeight + offSetBorder * 3];


        foreach (Edge edge in mstEdges)
        {
            int x1 = Mathf.RoundToInt((float) mesh.vertices[edge.P0].x);
            int y1 = Mathf.RoundToInt((float)mesh.vertices[edge.P0].y);

            int x2 = Mathf.RoundToInt((float)mesh.vertices[edge.P1].x);
            int y2 = Mathf.RoundToInt((float)mesh.vertices[edge.P1].y);

            if (Random.Range(0, 100) < 50)
            {
                this.createHorTunnel(x1, x2, y1);
                this.createVirTunnel(y1, y2, x2);
            }
            else
            {
                this.createVirTunnel(y1, y2, x1);
                this.createHorTunnel(x1, x2, y2);

            }
        }

        return this.hallways;
        //int x1 = Mathf.RoundToInt(room1.center.x);
        //int y1 = Mathf.RoundToInt(room1.center.y);

        //int x2 = Mathf.RoundToInt(room2.center.x);
        //int y2 = Mathf.RoundToInt(room2.center.y);

        //Debug.DrawLine(new Vector3(x1, y1, 0), new Vector3(x2, y2, 0));
        
    }
    private void createHorTunnel(int x1, int x2, int y)
    {
        for (int x = Mathf.Min(x1, x2) - 2; x < Mathf.Max(x1, x2) + 2; x++) {
            this.hallways[x, y -2] = 1;
            this.hallways[x, y - 1] = 1;
            this.hallways[x, y] = 1;
            this.hallways[x, y + 1] = 1;
        }

            
    }
    public void createVirTunnel(int y1, int y2, int x)
    {
        for (int y = Mathf.Min(y1, y2) - 2; y < Mathf.Max(y1, y2) + 2; y++)
        {
            this.hallways[x - 2, y] = 1;
            this.hallways[x - 1, y] = 1;
            this.hallways[x, y] = 1;
            this.hallways[x + 1, y] = 1;
            
        }

    }
    private Renderer myRenderer;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.DrawLine(new Vector3(0, 0, 0), new Vector3(100, 100, 0), Color.green, Time.deltaTime, false);
        //for (int i = 0; i < this.rooms.Count; i++)
        //{
        //    Debug.DrawLine(new Vector3(rooms[i].center.x, rooms[i].center.y, 0), new Vector3(100, 100, 0), Color.green, Time.deltaTime, false);
        //}
        Debug.DrawLine(new Vector3(0, 0, 0), new Vector3(220, 0, 0), Color.blue, Time.deltaTime, false);
        Debug.DrawLine(new Vector3(220, 0, 0), new Vector3(0, 220, 0), Color.blue, Time.deltaTime, false);
        Debug.DrawLine(new Vector3(0, 220, 0), new Vector3(0, 0, 0), Color.blue, Time.deltaTime, false);
        if (drawDeluanay)
        {
            drawDebugMeshDeluanay();
            drawDebugMST();
        }
        
        

        

    }

    void drawDebugMST()
    {
        if (mesh != null)
        {
            // We're probably in the editor
            foreach (Edge edge in mstEdges)
            {
                Vertex v0 = mesh.vertices[edge.P0];
                Vertex v1 = mesh.vertices[edge.P1];
                Vector3 p0 = new Vector3((float)v0.x, (float)v0.y, 0.0f);
                Vector3 p1 = new Vector3((float)v1.x, (float)v1.y, 0.0f);
                Debug.DrawLine(p0, p1, Color.green, Time.deltaTime, false);
            }
        }
    }
    void drawDebugMeshDeluanay()
    {
        if (mesh != null)
        {
            // We're probably in the editor
            foreach (Edge edge in mesh.Edges)
            {
                Vertex v0 = mesh.vertices[edge.P0];
                Vertex v1 = mesh.vertices[edge.P1];
                Vector3 p0 = new Vector3((float)v0.x, (float)v0.y, 0.0f);
                Vector3 p1 = new Vector3((float)v1.x, (float)v1.y, 0.0f);
                Debug.DrawLine(p0, p1, Color.red, Time.deltaTime, false);
            }
        }
    }
    
}

class DisjointSet
{
    Dictionary<Vertex, Vertex> parent = new Dictionary<Vertex, Vertex>();

    public void makeSet(HashSet<Vertex> vertices)
    {
        foreach(Vertex vertex in vertices)
        {
            parent.Add(vertex, vertex);
        }
        Debug.Log("Count -" + parent.Count);
    }

    public Vertex find(Vertex k)
    {
        if (parent[k]==k)
        {
            return k;
        }

        return find(parent[k]);
    }

    public void union(Vertex a, Vertex b)
    {
        Vertex x = find(a);
        Vertex y = find(b);
        if (parent.ContainsKey(x))
        {
            parent[x] = y;
        } else
        {
            parent.Add(x, y);
        }
        //try
        //{
        //    parent.Add(x, y);
        //} catch(Exception)
        //{

        //}
        
    }


}


public class Leaf
{
    public int x;
    public int y;
    public int width;
    public int height;
    public Leaf child1;
    public Leaf child2;
    public int min_width = 12;
    public int min_height = 8;

    
    private RectInt room;
    public Leaf(int x, int y, int width, int height)
    {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
        this.room = new RectInt(0, 0, 0, 0);
        

    }
    public bool splitLeaf()
    {
        if (child1!=null||child2!=null)
        {
            return false;
        }

        bool splitHor = true;
        if (Random.Range(0, 100) < 50)
        {
            splitHor = false;
        }
    

        if (this.width > this.height)
        {
            splitHor = false;
        } else if (this.height > this.width)
        {
            splitHor = true;
        }

        int max = 0;

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
            split = Mathf.RoundToInt(Random.Range(this.min_height, Mathf.RoundToInt(max*0.75f)));
            this.child1 = new Leaf(this.x, this.y, this.width, split);

            this.child2 = new Leaf(this.x, this.y + split, this.width, this.height - split);
        } else
        {
            split = Mathf.RoundToInt(Random.Range(this.min_width, max));
            this.child1 = new Leaf(this.x, this.y, split, this.height);

            this.child2 = new Leaf(this.x + split, this.y, this.width - split, this.height);
        }
        return true;
    }

    public void createRooms(ProceduralMapGenerator bspTree)
    {
        if (this.child1 != null || this.child2 != null)
        {
            if (this.child1 != null)
            {
                this.child1.createRooms(bspTree);

            }
            if (this.child2 != null)
            {
                this.child2.createRooms(bspTree);
            }
            //if (this.child1 != null && this.child2 != null)
            //{
            //    if (!child1.getRoom().Equals(new RectInt(0, 0, 0, 0)) && !child2.getRoom().Equals(new RectInt(0, 0, 0, 0)))
            //    {
            //        bspTree.createHallways(this.child1.getRoom(), this.child2.getRoom());
            //    }
                    
            //}
            // connect with hallway
           
        }
        else
        {

            //int w = Random.Range(Mathf.RoundToInt(bspTree.minRoomSize/10), Mathf.RoundToInt(Mathf.Min(bspTree.maxRoomSize, this.width - 10)/10)) * 10;

            //int h = Random.Range(Mathf.RoundToInt(bspTree.minRoomSize/10), Mathf.RoundToInt(Mathf.Min(bspTree.maxRoomSize, this.height - 10)/10)) * 10;

            //int x = Random.Range(Mathf.RoundToInt(this.x/10), Mathf.RoundToInt(this.x + (this.width - 10) - w)/10) * 10;

            //int y = Random.Range(Mathf.RoundToInt(this.y/10), Mathf.RoundToInt(this.y + (this.height - 10) - h)/10) * 10;

            int w = Mathf.RoundToInt(Random.Range(min_width, Mathf.Min(bspTree.maxRoomSize, this.width - 2)));

            int h = Mathf.RoundToInt(Random.Range(min_height, Mathf.Min(bspTree.maxRoomSize, this.height - 2)));

            int x = Mathf.RoundToInt(Random.Range(this.x, this.x + (this.width - 1) - w));

            int y = Mathf.RoundToInt(Random.Range(this.y, this.y + (this.height - 1) - h));
            Debug.Log("x: " + x + " y: " + y + " w: " + w + " h: " + h);
            this.room = new RectInt(x, y, w, h);

            bspTree.createRoom(this.room);
        }
		
			
    }
    RectInt getRoom()
    {
        if (!this.room.Equals(new RectInt(0, 0, 0, 0)))
        {
            return this.room;
        }
        else
        {
            RectInt room1 = new RectInt(0, 0, 0, 0);
            RectInt room2 = new RectInt(0, 0, 0, 0);
            if (this.child1 != null)
            {
                room1 = this.child1.getRoom();
            }

            if (this.child2 != null)
            {
                room2 = this.child2.getRoom();
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
