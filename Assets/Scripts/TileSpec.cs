using UnityEngine.Tilemaps;

[System.Serializable]
public struct TileSpec 
{
    public Tile Tile;

    public float Weight;

    public Tile[] Embelishments;

    public float embelishmentRate; 

    public bool Walkable;
}
