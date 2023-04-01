using UnityEngine;

[System.Serializable]
public struct DropConfig
{
    public GameObject Prefab;

    public int Count;
}

public class DropController : MonoBehaviour
{
    public DropConfig[] Drops;
    
    public bool IsFull { get; private set; } = true;

    public void Drop()
    {
        if(IsFull)
        {
            IsFull = false;
            foreach(var drop in Drops)
            {
                for(int i = 0; i < drop.Count; i++)
                    Instantiate(drop.Prefab, transform.position, Quaternion.identity);
            }
        }
    }
}
