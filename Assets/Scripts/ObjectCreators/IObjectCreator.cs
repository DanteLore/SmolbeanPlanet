using UnityEngine.Tilemaps;

public interface IObjectCreator
{
    public void CreateObjects(Chunk chunk, Tilemap tilemap);
}
