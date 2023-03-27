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

    private SpriteRenderer spriteRenderer;

    private bool isOpen = false;

    public void ExitedRange()
    {
        isOpen = false;
        spriteRenderer.sprite = closedChestSprite;
    }

    public void EnteredRange()
    {
    }

    public void Interact()
    {
        isOpen = !isOpen;
        spriteRenderer.sprite = isOpen ? openChestSprite : closedChestSprite;
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
