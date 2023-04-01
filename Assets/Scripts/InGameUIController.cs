using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class InGameUIController : MonoBehaviour
{
    public GameObject player;

    public CharacterStats playerStats;

    public Tilemap worldMap;

    private UIDocument doc;
    private Label worldCoordsLabel;
    private Label healthLabel;
    private Label manaLabel;
    private Label coinsLabel;

    // Start is called before the first frame update
    void Awake()
    {
        doc = GetComponent<UIDocument>();

        worldCoordsLabel = doc.rootVisualElement.Q<Label>("WorldCoordsLabel");
        healthLabel = doc.rootVisualElement.Q<Label>("HealthLabel");
        manaLabel = doc.rootVisualElement.Q<Label>("ManaLabel");
        coinsLabel = doc.rootVisualElement.Q<Label>("CoinsLabel");
    }

    void Start()
    {
        playerStats = player.GetComponent<CharacterStats>();
    }

    // Update is called once per frame
    void Update()
    {
        var pos = worldMap.WorldToCell(player.transform.position);
        worldCoordsLabel.text = $"Grid: {pos.x},{pos.y}";

        int h = Mathf.RoundToInt(playerStats.Health);
        healthLabel.text = $"Health: {h}/{playerStats.maxHealth}";
        manaLabel.text = $"Mana: {playerStats.mana}";
        coinsLabel.text = $"Coins: {playerStats.coins}";
    }
}
