using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextGeneration : MonoBehaviour
{
    public Font font;

    public RawImage rawImage;

    public Text text;

    private TextGenerator generator;
    // Start is called before the first frame update
    void Start()
    {
        TextGenerationSettings settings = new TextGenerationSettings();
        settings.textAnchor = TextAnchor.MiddleCenter;
        settings.color = Color.red;
        settings.generationExtents = new Vector2(500.0F, 200.0F);
        settings.pivot = Vector2.zero;
        settings.richText = true;
        settings.font = font;
        settings.fontSize = 32;
        settings.fontStyle = FontStyle.Normal;
        settings.verticalOverflow = VerticalWrapMode.Overflow;
        generator = new TextGenerator();
        if (generator.Populate("I am a string", settings))
        {
            Debug.Log("I generated: " + generator.vertexCount + " verts!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (rawImage != null && text != null)
        {
            rawImage.texture = text.mainTexture;
        }
    }
}
