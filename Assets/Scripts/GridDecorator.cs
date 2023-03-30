using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class GridDecorator : MonoBehaviour
{
    public int chunkWidth = 50;
    public int chunkHeight = 50;

    public GameObject player;

    public TileSpec[] tileSpecs;

    public NeighbourSpec[] neighbourSpecs;

    private System.Random r = new System.Random();

    public Tilemap baseTilemap;
    public Tilemap embelishmentTilemap;
    public Tilemap collisionTilemap;

    private Dictionary<Vector2Int, Chunk> chunks = new Dictionary<Vector2Int, Chunk>();

    private bool running = false;

    private Vector3Int playerPos;
    public Vector3Int PlayerPos
    {
        get { return playerPos; }
        set 
        {
            if(value != playerPos)
            {
                playerPos = value;
                PlayerMoved();
            }
        }
    }

    public Vector2Int CurrentChunkCoords
    {
        get { return new Vector2Int(DivTowardsNegInfinity((int)PlayerPos.x, chunkWidth) * chunkWidth, DivTowardsNegInfinity((int)PlayerPos.y, chunkHeight) * chunkHeight); }
    }

    public Chunk CurrentChunk
    {
        get { return chunks[CurrentChunkCoords]; }
    }

    public MapSquare CurrentSquare
    {
        get { return CurrentChunk.Map[ PlayerPos.x - CurrentChunkCoords.x, PlayerPos.y - CurrentChunkCoords.y ]; }
    }


    void Start()
    {
        var chunks = AddNewChunks(GetNeighbourCoords(new Vector2Int(0, 0)));
        foreach(Chunk chunk in chunks)
        {
            DrawChunk(chunk);
        }

        float range = 1.0f;

        player.transform.position += baseTilemap.GetCellCenterWorld(new Vector3Int(0, 0, 0));

        while(!CurrentSquare.Tile.Walkable)
        {
            print($"Moving player to find a walkable tile.  Range = {range}");
            float dX = Random.Range(-range, range);
            float dY = Random.Range(-range, range);
            player.transform.position += new Vector3(dX, dY, 0.0f);
            PlayerPos = baseTilemap.WorldToCell(player.transform.position);
            range += 1;
        }
    }

    private static int DivTowardsNegInfinity(int a, int b)
    {
        return a < 0 ? (a - b) / b : a / b;
    }

    void Update()
    {
        PlayerPos = baseTilemap.WorldToCell(player.transform.position);
    }

    void PlayerMoved()
    {
        var missingChunkCoords = GetNeighbourCoords(CurrentChunkCoords).Where(v => !chunks.ContainsKey(v)).ToArray();

        if(missingChunkCoords.Any() && !running)
        {
            running = true;
            ThreadedDataRequester.RequestData(() => {
                return AddNewChunks(missingChunkCoords);
            }, OnChunksDecorated);
        }
    }

    private List<Chunk> AddNewChunks(Vector2Int[] missingChunkCoords) 
    {
        var addedChunks = new List<Chunk>();

        foreach (Vector2Int vector in missingChunkCoords)
        {
            bool done = false;
            int loops = 0;
            Chunk chunk = null;
            while(!done && loops++ < 10)
            {
                try
                {
                    var watch = new System.Diagnostics.Stopwatch();
                    watch.Start();

                    chunk = new Chunk(vector, chunkWidth, chunkHeight, neighbourSpecs, tileSpecs);

                    // Copy edges from existing neighbours
                    var rightLocation = new Vector2Int(vector.x + chunkWidth, vector.y);
                    if (chunks.ContainsKey(rightLocation))
                        chunk.ProcessNeighbourChunkOnRight(chunks[rightLocation]);

                    var leftLocation = new Vector2Int(vector.x - chunkWidth, vector.y);
                    if (chunks.ContainsKey(leftLocation))
                        chunk.ProcessNeighbourChunkOnLeft(chunks[leftLocation]);

                    var topLocation = new Vector2Int(vector.x, vector.y + chunkHeight);
                    if (chunks.ContainsKey(topLocation))
                        chunk.ProcessNeighbourChunkOnTop(chunks[topLocation]);

                    var bottomLocation = new Vector2Int(vector.x, vector.y - chunkHeight);
                    if (chunks.ContainsKey(bottomLocation))
                        chunk.ProcessNeighbourChunkOnBottom(chunks[bottomLocation]);

                    chunk.BuildTilemap();
                    watch.Stop();
                    print($"Wave function collapse completed in {watch.ElapsedMilliseconds}");
                    done = true;
                }
                catch(ImpossibleCombinationException)
                {
                    print($"Wave function collapse FAILED!");
                }
            }

            chunks.Add(vector, chunk);
            addedChunks.Add(chunk);
        }

        return addedChunks;
    }

    private void OnChunksDecorated(object obj)
    {
        running = false;
        var chunks = obj as List<Chunk>;
        foreach(Chunk chunk in chunks)
            DrawChunk(chunk);
    }

    private Vector2Int[] GetNeighbourCoords(Vector2Int currentTile)
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

    private void DrawChunk(Chunk chunk)
    {
        for (int y = 0; y < chunkHeight; y++)
        {
            for (int x = 0; x < chunkWidth; x++)
            {
                int tileIndex = chunk.Map[x, y].TileIndex;
                var spec = tileSpecs[tileIndex];

                int drawX = chunk.Origin.x + x;
                int drawY = chunk.Origin.y + y;

                if(spec.Walkable)
                    baseTilemap.SetTile(new Vector3Int(drawX, drawY, 0), spec.Tile);
                else
                    collisionTilemap.SetTile(new Vector3Int(drawX, drawY, 0), spec.Tile);

                if(spec.Embelishments.Any() && r.NextDouble() < spec.embelishmentRate)
                {
                    int i = r.Next(spec.Embelishments.Count());
                    embelishmentTilemap.SetTile(new Vector3Int(drawX, drawY, 0), spec.Embelishments[i]);
                }
            }
        }

        foreach(var creator in GetComponents<IObjectCreator>())
            creator.CreateObjects(chunk, baseTilemap);
    }
}
