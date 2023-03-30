using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public interface IObjectCreator
{
    public void CreateObjects(Chunk chunk, Tilemap tilemap);
}

public class ChestCreator : MonoBehaviour, IObjectCreator
{
    public GameObject chestPrefab;

    public void CreateObjects(Chunk chunk, Tilemap tilemap)
    {
        for(int i = 0; i < 10; i++)
        {
            int chunkX = Random.Range(0, chunk.Width - 1);
            int chunkY = Random.Range(0, chunk.Height - 1);

            if(chunk.Map[chunkX, chunkY].Tile.Walkable)
            {
                int worldX = chunkX + chunk.Origin.x;
                int worldY = chunkY + chunk.Origin.y;

                var pos = tilemap.CellToWorld(new Vector3Int(worldX, worldY, 0));

                var chest = Instantiate(chestPrefab, pos, Quaternion.identity);
            }
        }
    }
}
