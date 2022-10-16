using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ExtractJsonScene : MonoBehaviour
{
    [System.Serializable]
    public class Entity
    {
        public int index;
        public string lemma;
        public int amount;
        public string id;
    }
    [System.Serializable]
    public class Relation
    {
        public int head;
        public int tail;
        public string type;
    }
    [System.Serializable]
    public class Attribute
    {
        public int index;
        public string type;
    }
    [System.Serializable]
    public class Scene
    {
        public string[] tokens;
        public Entity[] entities;
        public Relation[] relations;
        public Attribute[] attributes;

        public void deleteAllAttributesForIndex(int index)
        {
            int amountAttributesForDeletion = attributes.Count(a => a.index == index);
            Attribute[] newAttributes = new Attribute[attributes.Length-amountAttributesForDeletion];
            int count = 0;
            for (int attribute_nr = 0; attribute_nr < attributes.Length; attribute_nr++)
            {
                if (attributes[attribute_nr].index != index)
                {
                    newAttributes[count] = attributes[attribute_nr];
                    count++;
                }
            }
            attributes = newAttributes;
        }

        public void addNewAttributesForIndex(string[] attrList, int index)
        {
            int amountAttributesForInsert = attrList.Length;
            Attribute[] newAttributes = new Attribute[attributes.Length+amountAttributesForInsert];
            int count = 0;
            for (int attribute_nr = 0; attribute_nr < newAttributes.Length; attribute_nr++)
            {
                if (attribute_nr < attributes.Length)
                {
                    newAttributes[attribute_nr] = attributes[attribute_nr];
                }
                else
                {
                    Attribute newAttribute = new Attribute();
                    newAttribute.index = index;
                    newAttribute.type = attrList[count];
                    count++;
                    newAttributes[attribute_nr] = newAttribute;
                }
            }
            attributes = newAttributes;
        }

        public List<string> getAllAttributesForIndex(int index)
        {
            return attributes.Where(a => a.index == index).Select(a => a.type).ToList();
        }
    }
    [System.Serializable]
    public class Scenes
    {
        public Scene[] result;

        public string getPrettyPrint()
        {
            string jsonPrettyText = "";
            string spaces = "";
                for (int scene_nr = 0; scene_nr < result.Length; scene_nr++)
                {
                    spaces = getInlineSpaces(0);
                    jsonPrettyText += spaces + "Scene-" + scene_nr.ToString() + "\n";
                    spaces = getInlineSpaces(1);
                    jsonPrettyText += spaces + "Tokens:" + "\n";
                    spaces = getInlineSpaces(2);
                    string[] tokens = result[scene_nr].tokens;
                    for (int token_nr = 0; token_nr < tokens.Length; token_nr++)
                    {
                        string tokenText = tokens[token_nr];
                        jsonPrettyText += spaces + token_nr.ToString() + "-" + tokenText + "\n";
                    }
                    spaces = getInlineSpaces(1);
                    jsonPrettyText += spaces + "Entities:" + "\n";
                    Entity[] entities = result[scene_nr].entities;
                    for (int entity_nr = 0; entity_nr < entities.Length; entity_nr++)
                    {
                        spaces = getInlineSpaces(2);
                        jsonPrettyText += spaces + "Entity-" + entity_nr.ToString() + "\n";
                        Entity entity = entities[entity_nr];
                        spaces = getInlineSpaces(3);
                        jsonPrettyText += spaces + "Index: " + entity.index + "\n";
                        jsonPrettyText += spaces + "Lemma: " + entity.lemma + "\n";
                        jsonPrettyText += spaces + "Amount: " + entity.amount + "\n";
                        jsonPrettyText += spaces + "Id: " + entity.id + "\n";
                    }
                    spaces = getInlineSpaces(1);
                    jsonPrettyText += spaces + "Relations:" + "\n";
                    Relation[] relations = result[scene_nr].relations;
                    for (int relation_nr = 0; relation_nr < relations.Length; relation_nr++)
                    {
                        spaces = getInlineSpaces(2);
                        jsonPrettyText += spaces + "Relation-" + relation_nr.ToString() + "\n";
                        Relation relation = relations[relation_nr];
                        spaces = getInlineSpaces(3);
                        jsonPrettyText += spaces + "Head: " + relation.head + "\n";
                        jsonPrettyText += spaces + "Tail: " + relation.tail + "\n";
                        jsonPrettyText += spaces + "Type: " + relation.type + "\n";
                    }
                    spaces = getInlineSpaces(1);
                    jsonPrettyText += spaces + "Attributes:" + "\n";
                    Attribute[] attributes = result[scene_nr].attributes;
                    for (int attribute_nr = 0; attribute_nr < attributes.Length; attribute_nr++)
                    {
                        spaces = getInlineSpaces(2);
                        jsonPrettyText += spaces + "Attribute-" + attribute_nr.ToString() + "\n";
                        Attribute attribute = attributes[attribute_nr];
                        spaces = getInlineSpaces(3);
                        jsonPrettyText += spaces + "Index: " + attribute.index + "\n";
                        jsonPrettyText += spaces + "Type: " + attribute.type + "\n";
                    }
                }
            return jsonPrettyText;
        }

        private string getInlineSpaces(int column)
        {
            return new string(' ', column*2);
        }
    }

    public Scenes scenes;

    public ExtractJsonScene(string textJson){
        scenes = getJsonScenes(textJson);
    }

    private Scenes getJsonScenes(string textJson){
        Scenes scenes = JsonUtility.FromJson<Scenes>(textJson);
        return scenes;
    }

}
