using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using System.Linq;

[System.Serializable]
public struct TileSpec 
{
    public Tile Tile;

    public float Weight;

    public Tile[] Embelishments;

    public float embelishmentRate;
}

[System.Serializable]
public struct NeighbourSpec
{
    public int Index;

    public int[] AllowedRight;

    public int[] AllowedBelow;

    public int[] AllowedLeft;

    public int[] AllowedAbove;
}


public class MapSquare 
{
    private List<int> possibilities;
    public int x;
    public int y;

    public List<int> Possibilities
    {
        get { return possibilities; }
    }
    public int TileIndex
    {
        get { return possibilities.FirstOrDefault(); }
    }

    public MapSquare(int x, int y, IEnumerable<int> possibilities)
    {
        this.x = x;
        this.y = y;
        this.possibilities = possibilities.ToList();
    }

    internal void SelectTile(int selected)
    {
        possibilities = new List<int>() { selected };
    }

    internal bool Restrict(IEnumerable<int> allowed)
    {
        if(possibilities.Count > 1){
            int numRemoved = possibilities.RemoveAll(p => !allowed.Contains(p));
            return numRemoved > 0;
        }

        return false;
    }
}

public class Chunk
{
    private MapSquare[] map;
    private int height;
    private int width;
    NeighbourSpec[] neighbourSpecs;
    TileSpec[] tileSpecs;
    System.Random r = new System.Random();

    public class MapIndexer
    {
        private MapSquare[] map;
        private int width;

        public MapSquare this[int x, int y]
        {
            get { return map[((y) * width) + x]; }
        }

        public MapIndexer(MapSquare[] map, int width)
        {
            this.map = map;
            this.width = width;
        }
    }

    private MapIndexer mapIndex;
    public MapIndexer Map
    {
        get { return mapIndex; }
    }

    public Chunk(int width, int height, NeighbourSpec[] neighbourSpecs, TileSpec[] tileSpecs)
    {
        this.width = width;
        this.height = height;
        this.neighbourSpecs = neighbourSpecs;
        this.tileSpecs = tileSpecs;
        this.map = new MapSquare[this.width * this.height];
        this.mapIndex = new MapIndexer(map, this.width);

        // Initialise the map with all possible combinations of tile
        for (int x = 0; x < this.width; x++)
        {
            for (int y = 0; y < this.height; y++)
            {
                map[(y * this.width) + x] = new MapSquare(x, y, neighbourSpecs.Select(n => n.Index));
            }
        }
    }

    public void BuildTilemap()
    {
        int loopLimit = height * width;

        while (map.Any(s => s.Possibilities.Count > 1) && loopLimit-- > 0)
        {
            // Select the square on the map with the least possible tile options
            var square = map
                .Where(s => s.Possibilities.Count > 1)
                .OrderBy(s => s.Possibilities.Count)
                .First();

            // Assign a random, legal tile to that square
            int selected = SelectWeightedRandomTile(square);

            square.SelectTile(selected);

            // Collapse probabilities on neighbour squares
            Collapse(map, square);

            // Repeat!
        }
    }

    private int SelectWeightedRandomTile(MapSquare square)
    {
        // Add some noise to the values so we get a random sort order and break ties between items with the same weighting in a random way
        float noiseWeight = 0.01f;
        var probabilities = square.Possibilities.Select(p => tileSpecs[p].Weight + (r.NextDouble() * noiseWeight));

        // Order by priority, biggest first
        var choices = probabilities.Zip(square.Possibilities, (a, b) => (P: a, I: b)).OrderByDescending(c => c.P);
        
        double max = choices.Sum(c => c.P);
        double cutoff = r.NextDouble() * max;
        double sum = 0;

        // Walk down the list until we pass the randomly selected cutoff.  Return that index.
        foreach (var c in choices)
        {
            sum += c.P;
            if (sum > cutoff)
                return c.I;
        }

        // Must be the last item...
        return choices.Last().I;
    }

    private void Collapse(MapSquare[] map, MapSquare square)
    {
        List<MapSquare> recurse = new List<MapSquare>();

        // Right
        if (square.x < width - 1)
        {
            var other = map[square.y * width + square.x + 1];
            var allowed = neighbourSpecs.Where(s => square.Possibilities.Contains(s.Index)).SelectMany(x => x.AllowedRight).Distinct();
            if(other.Restrict(allowed))
                recurse.Add(other);
        }

        // Bottom
        if (square.y > 0)
        {
            var other = map[(square.y - 1) * width + square.x];
            var allowed = neighbourSpecs.Where(s => square.Possibilities.Contains(s.Index)).SelectMany(x => x.AllowedBelow).Distinct();
            if(other.Restrict(allowed))
                recurse.Add(other);
        }

        // Left
        if (square.x > 0)
        {
            var other = map[square.y * width + square.x - 1];
            var allowed = neighbourSpecs.Where(s => square.Possibilities.Contains(s.Index)).SelectMany(x => x.AllowedLeft).Distinct();
            if(other.Restrict(allowed))
                recurse.Add(other);
        }

        // Top
        if (square.y < height - 1)
        {
            var other = map[(square.y + 1) * width + square.x];
            var allowed = neighbourSpecs.Where(s => square.Possibilities.Contains(s.Index)).SelectMany(x => x.AllowedAbove).Distinct();
            if(other.Restrict(allowed))
                recurse.Add(other);
        }

        recurse.ForEach(s => Collapse(map, s));
    }

    internal void ProcessNeighbourChunkOnRight(Chunk rightNeighbour)
    {
        for(int i = 0; i < height; i++)
        {
            int tile = rightNeighbour.Map[0, i].TileIndex;

            var target = Map[width - 1, i];
            var allowed = neighbourSpecs.First(s => s.Index == tile).AllowedLeft;
            target.Restrict(allowed);
        }
    }

    internal void ProcessNeighbourChunkOnLeft(Chunk leftNeighbour)
    {
        for(int i = 0; i < height; i++)
        {
            int tile = leftNeighbour.Map[width - 1, i].TileIndex;

            var target = Map[0, i];
            var allowed = neighbourSpecs.First(s => s.Index == tile).AllowedRight;
            target.Restrict(allowed);
        }
    }

    internal void ProcessNeighbourChunkOnTop(Chunk topNeighbour)
    {
        for(int i = 0; i < width; i++)
        {
            int tile = topNeighbour.Map[i, 0].TileIndex;

            var target = Map[i, height - 1];
            var allowed = neighbourSpecs.First(s => s.Index == tile).AllowedBelow;
            target.Restrict(allowed);
        }
    }

    internal void ProcessNeighbourChunkOnBottom(Chunk bottomNeighbour)
    {
        for(int i = 0; i < width; i++)
        {
            int tile = bottomNeighbour.Map[i, height - 1].TileIndex;

            var target = Map[i, 0];
            var allowed = neighbourSpecs.First(s => s.Index == tile).AllowedAbove;
            target.Restrict(allowed);
        }
    }
}

public class GridDecorator : MonoBehaviour
{
    public int chunkWidth = 50;
    public int chunkHeight = 50;

    public GameObject player;

    public TileSpec[] tileSpecs;

    public NeighbourSpec[] neighbourSpecs;

    private System.Random r = new System.Random();

    public Tile plainGrass;
    public Tilemap baseTilemap;
    public Tilemap embelishmentTilemap;

    private Dictionary<Vector2Int, Chunk> chunks = new Dictionary<Vector2Int, Chunk>();

    void Start()
    {
    }

    private static int DivTowardsNegInfinity(int a, int b)
    {
        return a < 0 ? (a - b) / b : a / b;
    }

    void Update()
    {
        var pos = baseTilemap.WorldToCell(player.transform.position);

        var currentTile = new Vector2Int(DivTowardsNegInfinity((int)pos.x, chunkWidth) * chunkWidth, DivTowardsNegInfinity((int)pos.y, chunkHeight) * chunkHeight);
        Vector2Int[] vectors = GetChunksToDraw(currentTile);

        foreach (Vector2Int vector in vectors)
        {
            if (!chunks.ContainsKey(vector))
            {
                Chunk chunk = new Chunk(chunkWidth, chunkHeight, neighbourSpecs, tileSpecs);
                chunks.Add(vector, chunk);

                // Copy edges from existing neighbours
                var rightLocation = new Vector2Int(vector.x + chunkWidth,  vector.y);
                if(chunks.ContainsKey(rightLocation))
                    chunk.ProcessNeighbourChunkOnRight(chunks[rightLocation]);

                var leftLocation = new Vector2Int(vector.x - chunkWidth,  vector.y);
                if(chunks.ContainsKey(leftLocation))
                    chunk.ProcessNeighbourChunkOnLeft(chunks[leftLocation]);

                var topLocation = new Vector2Int(vector.x,  vector.y + chunkHeight);
                if(chunks.ContainsKey(topLocation))
                    chunk.ProcessNeighbourChunkOnTop(chunks[topLocation]);

                var bottomLocation = new Vector2Int(vector.x,  vector.y - chunkHeight);
                if(chunks.ContainsKey(bottomLocation))
                    chunk.ProcessNeighbourChunkOnBottom(chunks[bottomLocation]);

                var t = Time.realtimeSinceStartup;
                chunk.BuildTilemap();
                print($"Wave function collapse completed in {(Time.realtimeSinceStartup - t).ToString("f6")}");

                DrawChunk(chunk, vector);
            }
        }
    }

    private Vector2Int[] GetChunksToDraw(Vector2Int currentTile)
    {
        return new Vector2Int[]
            {
                currentTile, // This tile
                new Vector2Int(currentTile.x - chunkWidth, currentTile.y), // Left
                new Vector2Int(currentTile.x - chunkWidth, currentTile.y - chunkHeight), // Bottom Left
                new Vector2Int(currentTile.x, currentTile.y - chunkHeight), // Bottom
                new Vector2Int(currentTile.x + chunkWidth, currentTile.y - chunkHeight), // Bottom Right
                new Vector2Int(currentTile.x + chunkWidth, currentTile.y), // Right
                new Vector2Int(currentTile.x + chunkWidth, currentTile.y + chunkHeight), // Top Right
                new Vector2Int(currentTile.x, currentTile.y + chunkHeight), // Top
                new Vector2Int(currentTile.x - chunkWidth, currentTile.y + chunkHeight), // Top Left
            };
    }

    private void DrawChunk(Chunk chunk, Vector2Int origin)
    {
        for (int y = 0; y < chunkHeight; y++)
        {
            for (int x = 0; x < chunkWidth; x++)
            {
                int tileIndex = chunk.Map[x, y].TileIndex;
                var spec = tileSpecs[tileIndex];

                int drawX = origin.x + x;
                int drawY = origin.y + y;

                baseTilemap.SetTile(new Vector3Int(drawX, drawY, 0), spec.Tile);

                if(spec.Embelishments.Any() && r.NextDouble() < spec.embelishmentRate)
                {
                    int i = r.Next(spec.Embelishments.Count());
                    embelishmentTilemap.SetTile(new Vector3Int(drawX, drawY, 0), spec.Embelishments[i]);
                }
            }
        }
    }
}
