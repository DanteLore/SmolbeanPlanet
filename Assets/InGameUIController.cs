using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class InGameUIController : MonoBehaviour
{
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
        worldCoordsLabel.text = Time.timeSinceLevelLoad.ToString();
    }
}
