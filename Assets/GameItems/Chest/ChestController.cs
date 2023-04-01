using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractableObject
{
    public void EnteredRange();

    public void ExitedRange();

    public void Interact();
}

public class ChestController : MonoBehaviour, IInteractableObject
{
    public Sprite closedChestSprite;
    public Sprite openChestSprite;

    public Sprite closedEmptyChestSprite;
    public Sprite openEmptyChestSprite;

    private SpriteRenderer spriteRenderer;

    private DropController drops;

    private bool isOpen = false;
    public void ExitedRange()
    {
        isOpen = false;
        spriteRenderer.sprite = drops.IsFull ? closedChestSprite : closedEmptyChestSprite;;
    }

    public void EnteredRange()
    {
    }

    public void Interact()
    {
        isOpen = !isOpen;

        if(drops.IsFull)
            spriteRenderer.sprite = isOpen ? openChestSprite : closedChestSprite;
        else
            spriteRenderer.sprite = isOpen ? openEmptyChestSprite : closedEmptyChestSprite;

        if(isOpen)
            drops.Drop();
    }

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        drops = GetComponent<DropController>();
    }

    // Update is called once per frame
    void Update()
    {
    }
}
