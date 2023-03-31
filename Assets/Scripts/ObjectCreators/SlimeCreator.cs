using UnityEngine;
using UnityEngine.Tilemaps;

public class SlimeCreator : MonoBehaviour, IObjectCreator
{
    public GameObject slimePrefab;

    public int minGroupSize = 4;

    public int maxGroupSize = 8;

    public int groupRadiusTiles = 3;

    public float probability = 0.2f;

    public void CreateObjects(Chunk chunk, Tilemap tilemap)
    {
        float p = Random.Range(0.0f, 1.0f);

        if(p < probability)
        {
            int centreX = Random.Range(0, chunk.Width);
            int centreY = Random.Range(0, chunk.Height);
            int count = Random.Range(minGroupSize, maxGroupSize);
            int radius = groupRadiusTiles;

            while(count > 0)
            {
                int x = Random.Range(centreX - radius, centreX + radius);
                int y = Random.Range(centreY - radius, centreY + radius);
                
                x = Mathf.Min(x, chunk.Width - 1);
                x = Mathf.Max(x, 0);
                y = Mathf.Min(y, chunk.Height - 1);
                y = Mathf.Max(y, 0);

                if(chunk.Map[x, y].Tile.Walkable && chunk.Map[x, y].ObjectsOnTile == 0)
                {
                    int worldX = x + chunk.Origin.x;
                    int worldY = y + chunk.Origin.y;

                    var pos = tilemap.GetCellCenterWorld(new Vector3Int(worldX, worldY, 0));

                    Instantiate(slimePrefab, pos, Quaternion.identity);

                    count--;
                }
                else
                {
                    radius++; // Expand the radius to make the search for a free tile easier
                }
            }
        }
    }
}
