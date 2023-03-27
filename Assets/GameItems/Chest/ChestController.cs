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

    public GameObject coinPrefab;

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

        if(isOpen)
        {
            for(int i = 0; i < 10; i++)
                Instantiate(coinPrefab, transform.position, Quaternion.identity);
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
