using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyAttributes : MonoBehaviour
{
    public static void applyAll(GameObject model, List<string> attributes)
    {
        foreach (string attribute in attributes)
        {
            applyOne(model, attribute);
        }
    }
    public static void applyOne(GameObject model, string attribute)
    {
        AttributeJson.getAttributesJson();
        AttributeJson.Attribute foundAttribute = GlobalVariable.attributes.getAttribute(attribute);
        if (foundAttribute != null)
        {
            setColor(model, foundAttribute.attributeColor.getColor());
        }
    }
    public static void setColor(GameObject model, Color destinationColor)
    {
        Renderer rend = model.GetComponent<Renderer>();
         foreach (Material material in rend.materials)
         {
            material.shader = Shader.Find("HDRP/Lit");
            Color originColor = rend.material.GetColor("_BaseColor");
            float r = originColor.r + (destinationColor.r - originColor.r) * (destinationColor.a);
            float g = originColor.g + (destinationColor.g - originColor.g) * (destinationColor.a);
            float b = originColor.b + (destinationColor.b - originColor.b) * (destinationColor.a);
            Color finalColor = new Color(r,g,b,originColor.a);
            material.SetColor("_BaseColor", finalColor);
         }
    }
}
