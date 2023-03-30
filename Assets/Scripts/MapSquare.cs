using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MapSquare 
{
    private List<int> possibilities;
    public int x;
    public int y;
    TileSpec[] tileSpecs;

    public IEnumerable<int> Possibilities
    {
        get { return possibilities; }
    }
    public int TileIndex
    {
        get { return possibilities.FirstOrDefault(); }
    }
    
    public TileSpec Tile
    {
        get{ return tileSpecs[TileIndex]; }
    }

    public MapSquare(int x, int y, IEnumerable<int> possibilities, TileSpec[] tileSpecs)
    {
        this.x = x;
        this.y = y;
        this.possibilities = possibilities.ToList();
        this.tileSpecs = tileSpecs;
    }

    public void SelectTile(int selected)
    {
        if(!possibilities.Contains(selected))
            throw new ImpossibleCombinationException("You tried to select a tile that's not in the list of possibilities for this square");
            
        possibilities = new List<int>() { selected };
    }

    public bool Restrict(IEnumerable<int> allowed)
    {
        if(possibilities.Count > 1)
        {
            int numRemoved = possibilities.RemoveAll(p => !allowed.Contains(p));

            if(possibilities.Count == 0)
                throw new ImpossibleCombinationException("Restrict removed all possible options");

            return numRemoved > 0;
        }

        return false;
    }
}
