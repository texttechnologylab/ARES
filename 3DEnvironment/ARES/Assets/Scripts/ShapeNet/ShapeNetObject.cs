using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
/*
@source: StolperwegeVR
*/
public class ShapeNetObject
{

    public string ID { get; private set; }
    public string Name { get; private set; }
    public HashSet<string> Categories { get; private set; }
    public string PrettyCategories { get; private set; }
    public Texture2D Thumbnail;

    public ShapeNetObject(JsonData json)
    {
        ID = json["id"].ToString();
        Name = json.Keys.Contains("name") ? json["name"].ToString() : "no_name";
        Categories = new HashSet<string>();
        if (json.Keys.Contains("categories"))
        {
            PrettyCategories = "";
            for (int i = 0; i < json["categories"].Count; i++)
            {
                Categories.Add(json["categories"][i].ToString());
                PrettyCategories += json["categories"][i].ToString();
                if (i < json["categories"].Count - 1) PrettyCategories += ", ";
            }

        }
    }

    /*
    @author: Patrick Masny
    */

    public override string ToString()
    {
        return "Name: " + Name + ", ID: " + ID;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || !(obj is ShapeNetObject)) return false;
        ShapeNetObject other = (ShapeNetObject)obj;
        return other.ID.Equals(ID);
    }

    public override int GetHashCode()
    {
        return 1213502048 + EqualityComparer<string>.Default.GetHashCode(ID);
    }
}
