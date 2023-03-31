using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractableObject
{
    public void EnteredRange();

    public void ExitedRange();

    public void Interact();
}

[System.Serializable]
public struct DropConfig
{
    public GameObject Prefab;

    public int Count;
}

public class ChestController : MonoBehaviour, IInteractableObject
{
    public Sprite closedChestSprite;
    public Sprite openChestSprite;

    public Sprite closedEmptyChestSprite;
    public Sprite openEmptyChestSprite;

    public DropConfig[] Drops;

    private SpriteRenderer spriteRenderer;

    private bool isOpen = false;

    private bool isFull = true;

    public void ExitedRange()
    {
        isOpen = false;
        spriteRenderer.sprite = isFull ? closedChestSprite : closedEmptyChestSprite;;
    }

    public void EnteredRange()
    {
    }

    public void Interact()
    {
        isOpen = !isOpen;

        if(isFull)
            spriteRenderer.sprite = isOpen ? openChestSprite : closedChestSprite;
        else
            spriteRenderer.sprite = isOpen ? openEmptyChestSprite : closedEmptyChestSprite;

        if(isOpen && isFull)
        {
            isFull = false;
            foreach(var drop in Drops)
            {
                for(int i = 0; i < drop.Count; i++)
                    Instantiate(drop.Prefab, transform.position, Quaternion.identity);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
    }
}
