using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ChestCreator : MonoBehaviour, IObjectCreator
{
    public GameObject chestPrefab;
    public GameObject goldChestPrefab;

    public float goldChestProbability = 0.1f;

    public int minChestsPerChunk = 1;

    public int maxChestsPerChunk = 5;

    public void CreateObjects(Chunk chunk, Tilemap tilemap)
    {
        int count = Random.Range(minChestsPerChunk, maxChestsPerChunk);
        while(count-- > 0)
        {
            float p = Random.Range(0.0f, 1.0f);
            int chunkX = Random.Range(0, chunk.Width - 1);
            int chunkY = Random.Range(0, chunk.Height - 1);

            if(chunk.Map[chunkX, chunkY].Tile.Walkable)
            {
                int worldX = chunkX + chunk.Origin.x;
                int worldY = chunkY + chunk.Origin.y;

                var pos = tilemap.GetCellCenterWorld(new Vector3Int(worldX, worldY, 0));

                var prefab = (p < goldChestProbability) ? goldChestPrefab : chestPrefab;
                Instantiate(prefab, pos, Quaternion.identity);

                chunk.Map[chunkX, chunkY].ObjectsOnTile++;
            }
        }
    }
}
