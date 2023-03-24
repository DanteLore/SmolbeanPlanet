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
    public MapSquare[] map;

    public Chunk(MapSquare[] map)
    {
        this.map = map;
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

    // Start is called before the first frame update
    void Start()
    {
    }

    private static int DivTowardsNegInfinity(int a, int b)
    {
        return a < 0 ? (a - b) / b : a / b;
    }

    // Update is called once per frame
    void Update()
    {
        var pos = baseTilemap.WorldToCell(player.transform.position);

        var currentTile = new Vector2Int(DivTowardsNegInfinity((int)pos.x, chunkWidth) * chunkWidth, DivTowardsNegInfinity((int)pos.y, chunkHeight) * chunkHeight);

        var vectors = new Vector2Int[] 
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

        foreach(Vector2Int vector in vectors)
        {
            if(!chunks.ContainsKey(vector))
            {
                Chunk chunk = InitialiseChunk();
                chunks.Add(vector, chunk);

                // Copy edges from existing neighbour

                var t = Time.realtimeSinceStartup;
                BuildTilemap(chunk);
                print($"Wave function collapse completed in {(Time.realtimeSinceStartup - t).ToString("f6")}");

                DrawChunk(chunk, vector);
            }
        }
    }
 
    private void BuildTilemap(Chunk chunk)
    {
        int loopLimit = chunkHeight * chunkWidth;

        while (chunk.map.Any(s => s.Possibilities.Count > 1) && loopLimit-- > 0)
        {
            // Select the square on the map with the least possible tile options
            var square = chunk.map
                .Where(s => s.Possibilities.Count > 1)
                .OrderBy(s => s.Possibilities.Count)
                .First();

            // Assign a random, legal tile to that square
            int selected = SelectWeightedRandomTile(square);

            square.SelectTile(selected);

            // Collapse probabilities on neighbour squares
            Collapse(chunk.map, square);

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

    private void DrawChunk(Chunk chunk, Vector2Int origin)
    {
        for (int y = 0; y < chunkHeight; y++)
        {
            for (int x = 0; x < chunkWidth; x++)
            {
                int tileIndex = chunk.map[(y * chunkWidth) + x].TileIndex;
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

    private Chunk InitialiseChunk()
    {
        var map = new MapSquare[chunkWidth * chunkHeight];

        // Initialise the map with all possible combinations of tile
        for (int x = 0; x < chunkWidth; x++)
        {
            for (int y = 0; y < chunkHeight; y++)
            {
                map[(y * chunkWidth) + x] = new MapSquare(x, y, neighbourSpecs.Select(n => n.Index));
            }
        }

        return new Chunk(map);
    }

    private void Collapse(MapSquare[] map, MapSquare square)
    {
        List<MapSquare> recurse = new List<MapSquare>();

        // Right
        if (square.x < chunkWidth - 1)
        {
            var other = map[square.y * chunkWidth + square.x + 1];
            var allowed = neighbourSpecs.Where(s => square.Possibilities.Contains(s.Index)).SelectMany(x => x.AllowedRight).Distinct();
            if(other.Restrict(allowed))
                recurse.Add(other);
        }

        // Bottom
        if (square.y > 0)
        {
            var other = map[(square.y - 1) * chunkHeight + square.x];
            var allowed = neighbourSpecs.Where(s => square.Possibilities.Contains(s.Index)).SelectMany(x => x.AllowedBelow).Distinct();
            if(other.Restrict(allowed))
                recurse.Add(other);
        }

        // Left
        if (square.x > 0)
        {
            var other = map[square.y * chunkWidth + square.x - 1];
            var allowed = neighbourSpecs.Where(s => square.Possibilities.Contains(s.Index)).SelectMany(x => x.AllowedLeft).Distinct();
            if(other.Restrict(allowed))
                recurse.Add(other);
        }

        // Top
        if (square.y < chunkHeight - 1)
        {
            var other = map[(square.y + 1) * chunkWidth + square.x];
            var allowed = neighbourSpecs.Where(s => square.Possibilities.Contains(s.Index)).SelectMany(x => x.AllowedAbove).Distinct();
            if(other.Restrict(allowed))
                recurse.Add(other);
        }

        recurse.ForEach(s => Collapse(map, s));
    }
}
