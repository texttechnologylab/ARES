using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class AttributeJson : MonoBehaviour
{
    [System.Serializable]
    public class AttributeColor
    {
        public float r;
        public float g;
        public float b;
        public float a;
        public bool isActive;
        
        public Color getColor()
        {
            return new Color(r,g,b,a);
        }
        public AttributeColor(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }
    }
    [System.Serializable]
    public class Attribute
    {
        public string name;
        public AttributeColor attributeColor;
    }
    [System.Serializable]
    public class Attributes
    {
        public Attribute[] result;

        public void removeAttributeAtIndex(int index)
        {
            Attribute[] newResult = new Attribute[result.Length-1];
            int count = 0;
            for (int attribute_nr = 0; attribute_nr < result.Length; attribute_nr++)
            {
                if (attribute_nr != index)
                {
                    newResult[count] = result[attribute_nr];
                    count++;
                }
            }
            result = newResult;
        }

        public Attribute getAttribute(string name)
        {
            foreach (Attribute attribute in result)
            {
                if (attribute.name == name)
                {
                    return attribute;
                }
            }
            return null;
        }
    }

    public static void getAttributesJson()
    {
        if (GlobalVariable.attributes.result == null)
        {
            string attributePath = "Data/AttributeDefinitions/currAttributeList";
            TextAsset textJSON = Resources.Load<TextAsset>(attributePath);
            GlobalVariable.attributes = JsonUtility.FromJson<AttributeJson.Attributes>(textJSON.text);
        }
    }

    public static void saveJsonFile()
    {
        string outputString = JsonUtility.ToJson(GlobalVariable.attributes);
        string attributesPath = "/Resources/Data/AttributeDefinitions/currAttributeList.json";
        File.WriteAllText(Application.dataPath + attributesPath, outputString);
    }
}
