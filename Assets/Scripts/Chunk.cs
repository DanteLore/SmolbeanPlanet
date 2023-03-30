using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class Chunk
{
    private MapSquare[] map;
    private Vector2Int origin;
    private int height;
    private int width;
    Dictionary<int, NeighbourSpec> neighbourSpecs;
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

    public Vector2Int Origin
    {
        get { return origin; }
    }

    public int Width
    {
        get { return width; }
    }

    public int Height
    {
        get { return height; }
    }

    public Chunk(Vector2Int origin, int width, int height, NeighbourSpec[] neighbourSpecs, TileSpec[] tileSpecs)
    {
        this.origin = origin;
        this.width = width;
        this.height = height;
        this.neighbourSpecs = neighbourSpecs.ToDictionary(n => n.Index);
        this.tileSpecs = tileSpecs;
        this.map = new MapSquare[this.width * this.height];
        this.mapIndex = new MapIndexer(map, this.width);

        // Initialise the map with all possible combinations of tile
        for (int x = 0; x < this.width; x++)
        {
            for (int y = 0; y < this.height; y++)
            {
                map[(y * this.width) + x] = new MapSquare(x, y, neighbourSpecs.Select(n => n.Index), tileSpecs);
            }
        }
    }

    public void BuildTilemap()
    {
        int loopLimit = height * width;

        while (map.Any(s => s.Possibilities.Count() > 1) && loopLimit-- > 0)
        {
            // Select the square on the map with the least possible tile options
            var square = map
                .Where(s => s.Possibilities.Count() > 1)
                .OrderBy(s => s.Possibilities.Count())
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
        var choices = probabilities.Zip(square.Possibilities, (a, b) => (P: a, I: b)).OrderByDescending(c => c.P).ToList();
        
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
            var allowed = square.Possibilities.Select(p => neighbourSpecs[p]).SelectMany(x => x.AllowedRight).Distinct().ToList();
            if(other.Restrict(allowed))
                recurse.Add(other);
        }

        // Bottom
        if (square.y > 0)
        {
            var other = map[(square.y - 1) * width + square.x];
            var allowed = square.Possibilities.Select(p => neighbourSpecs[p]).SelectMany(x => x.AllowedBelow).Distinct().ToList();
            if(other.Restrict(allowed))
                recurse.Add(other);
        }

        // Left
        if (square.x > 0)
        {
            var other = map[square.y * width + square.x - 1];
            var allowed = square.Possibilities.Select(p => neighbourSpecs[p]).SelectMany(x => x.AllowedLeft).Distinct().ToList();
            if(other.Restrict(allowed))
                recurse.Add(other);
        }

        // Top
        if (square.y < height - 1)
        {
            var other = map[(square.y + 1) * width + square.x];
            var allowed = square.Possibilities.Select(p => neighbourSpecs[p]).SelectMany(x => x.AllowedAbove).Distinct().ToList();
            if(other.Restrict(allowed))
                recurse.Add(other);
        }

        recurse.ForEach(s => Collapse(map, s));
    }

    internal void ProcessNeighbourChunkOnRight(Chunk rightNeighbour)
    {
        List<MapSquare> recurse = new List<MapSquare>();

        for(int i = 0; i < height; i++)
        {
            int tile = rightNeighbour.Map[0, i].TileIndex;

            var target = Map[width - 1, i];
            var allowed = neighbourSpecs[tile].AllowedLeft;
            if(target.Restrict(allowed))
                recurse.Add(target);
        }

        recurse.ForEach(s => Collapse(map, s));
    }

    internal void ProcessNeighbourChunkOnLeft(Chunk leftNeighbour)
    {
        List<MapSquare> recurse = new List<MapSquare>();

        for(int i = 0; i < height; i++)
        {
            int tile = leftNeighbour.Map[width - 1, i].TileIndex;

            var target = Map[0, i];
            var allowed = neighbourSpecs[tile].AllowedRight;
            if(target.Restrict(allowed))
                recurse.Add(target);
        }

        recurse.ForEach(s => Collapse(map, s));
    }

    internal void ProcessNeighbourChunkOnTop(Chunk topNeighbour)
    {
        List<MapSquare> recurse = new List<MapSquare>();

        for(int i = 0; i < width; i++)
        {
            int tile = topNeighbour.Map[i, 0].TileIndex;

            var target = Map[i, height - 1];
            var allowed = neighbourSpecs[tile].AllowedBelow;
            if(target.Restrict(allowed))
                recurse.Add(target);
        }

        recurse.ForEach(s => Collapse(map, s));
    }

    internal void ProcessNeighbourChunkOnBottom(Chunk bottomNeighbour)
    {
        List<MapSquare> recurse = new List<MapSquare>();

        for(int i = 0; i < width; i++)
        {
            int tile = bottomNeighbour.Map[i, height - 1].TileIndex;

            var target = Map[i, 0];
            var allowed = neighbourSpecs[tile].AllowedAbove;
            if(target.Restrict(allowed))
                recurse.Add(target);
        }

        recurse.ForEach(s => Collapse(map, s));
    }
}