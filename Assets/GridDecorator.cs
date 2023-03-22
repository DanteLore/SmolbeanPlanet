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

public class GridDecorator : MonoBehaviour
{
    public int mapWidth = 100;
    public int mapHeight = 100;

    public TileSpec[] tileSpecs;

    public NeighbourSpec[] neighbourSpecs;

    private System.Random r = new System.Random();

    public Tile plainGrass;
    public Tilemap baseTilemap;
    public Tilemap embelishmentTilemap;

    // Start is called before the first frame update
    void Start()
    {
        MapSquare[] map = InitialiseMap();

        var t = Time.realtimeSinceStartup;
        BuildTilemap(map);
        print($"Wave function collapse completed in {(Time.realtimeSinceStartup - t).ToString("f6")}");

        DrawMap(map);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private class MapSquare 
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

    private void BuildTilemap(MapSquare[] map)
    {
        int loopLimit = mapHeight * mapWidth;

        while (map.Any(s => s.Possibilities.Count > 1) && loopLimit-- > 0)
        {
            // Select a square on the map
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

    private void DrawMap(MapSquare[] map)
    {
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                int drawX = (-mapWidth / 2) + x;
                int drawY = (mapHeight / 2) - y;
                int tileIndex = map[(y * mapWidth) + x].TileIndex;
                var spec = tileSpecs[tileIndex];
                baseTilemap.SetTile(new Vector3Int(drawX, drawY, 0), spec.Tile);

                if(spec.Embelishments.Any() && r.NextDouble() < spec.embelishmentRate)
                {
                    int i = r.Next(spec.Embelishments.Count());
                    embelishmentTilemap.SetTile(new Vector3Int(drawX, drawY, 0), spec.Embelishments[i]);
                }
            }
        }
    }

    private MapSquare[] InitialiseMap()
    {
        var map = new MapSquare[mapWidth * mapHeight];

        // Initialise the map with all possible combinations of tile
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                map[(y * mapWidth) + x] = new MapSquare(x, y, neighbourSpecs.Select(n => n.Index));
            }
        }

        return map;
    }

    private void Collapse(MapSquare[] map, MapSquare square)
    {
        List<MapSquare> recurse = new List<MapSquare>();

        // Right
        if (square.x < mapWidth - 1)
        {
            var other = map[square.y * mapWidth + square.x + 1];
            var allowed = neighbourSpecs.Where(s => square.Possibilities.Contains(s.Index)).SelectMany(x => x.AllowedRight).Distinct();
            if(other.Restrict(allowed))
                recurse.Add(other);
        }

        // Bottom
        if (square.y < mapHeight - 1)
        {
            var other = map[(square.y + 1) * mapWidth + square.x];
            var allowed = neighbourSpecs.Where(s => square.Possibilities.Contains(s.Index)).SelectMany(x => x.AllowedBelow).Distinct();
            if(other.Restrict(allowed))
                recurse.Add(other);
        }

        // Left
        if (square.x > 0)
        {
            var other = map[square.y * mapWidth + square.x - 1];
            var allowed = neighbourSpecs.Where(s => square.Possibilities.Contains(s.Index)).SelectMany(x => x.AllowedLeft).Distinct();
            if(other.Restrict(allowed))
                recurse.Add(other);
        }

        // Top
        if (square.y > 0)
        {
            var other = map[(square.y - 1) * mapWidth + square.x];
            var allowed = neighbourSpecs.Where(s => square.Possibilities.Contains(s.Index)).SelectMany(x => x.AllowedAbove).Distinct();
            if(other.Restrict(allowed))
                recurse.Add(other);
        }

        recurse.ForEach(s => Collapse(map, s));
    }
}
