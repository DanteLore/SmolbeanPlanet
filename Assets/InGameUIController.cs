using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class InGameUIController : MonoBehaviour
{
    public GameObject player;
    public Tilemap worldMap;

    private UIDocument doc;
    private Label worldCoordsLabel;

    // Start is called before the first frame update
    void Awake()
    {
        doc = GetComponent<UIDocument>();
        worldCoordsLabel = doc.rootVisualElement.Q<Label>("WorldCoordsLabel");
    }

    // Update is called once per frame
    void Update()
    {
        var pos = worldMap.WorldToCell(player.transform.position);
        worldCoordsLabel.text = $"{pos.x},{pos.y}";
    }
}
