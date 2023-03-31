using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class TreeCreator : MonoBehaviour, IObjectCreator
{
    public GameObject treePrefab;

    public int minTreesPerChunk = 10;

    public int maxTreesPerChunk = 25;

    public void CreateObjects(Chunk chunk, Tilemap tilemap)
    {
        int count = Random.Range(minTreesPerChunk, maxTreesPerChunk);

        int edgeOffset = 2; // offset from the edges of the chunk to avoid sprint clipping issues :(

        var localCoords = Enumerable.Range(0, count)
            .Select(i => new Vector2Int(Random.Range(edgeOffset, chunk.Width - edgeOffset), Random.Range(edgeOffset, chunk.Height - edgeOffset)))
            .Where(v => chunk.Map[v.x, v.y].Tile.Walkable)
            .OrderByDescending(v => v.y);

        foreach(var coord in localCoords)
        {
            int worldX = coord.x + chunk.Origin.x;
            int worldY = coord.y + chunk.Origin.y;

            var pos = tilemap.GetCellCenterWorld(new Vector3Int(worldX, worldY, 0));

            Instantiate(treePrefab, pos, Quaternion.identity);

            chunk.Map[coord.x, coord.y].ObjectsOnTile++;
        }
    }
}
