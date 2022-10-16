using LitJson;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
/*
@source: StolperwegeVR
*/
public class ShapeNetModel : ShapeNetObject
{

    public string SynSet { get; private set; }

    public HashSet<string> Lemmas { get; private set; }
    public HashSet<string> Tags { get; private set; }
    public float SolidVolume { get; private set; }
    public bool IsContainer { get; private set; }
    public float SurfaceVolume { get; private set; }
    public float Unit { get; private set; }
    public float SupportSurfaceArea { get; private set; }
    public Vector3 AlignedDimensions { get; private set; } 
    public Vector3 Up { get; private set; }
    public Vector3 Front { get; private set; }
    public float staticFrictionForce { get; private set; }


    public ShapeNetModel(JsonData json) : base(json)
    {
        SynSet = json.Keys.Contains("wnsynset") ? json["wnsynset"].ToString() : "";
        SolidVolume = json.Keys.Contains("solidVolume") ? float.Parse(json["solidVolume"].ToString().Replace(".", ",")) : 0;
        IsContainer = json.Keys.Contains("isContainer") ? bool.Parse(json["isContainer"].ToString()) : false;
        SurfaceVolume = json.Keys.Contains("surfaceVolume") ? float.Parse(json["surfaceVolume"].ToString().Replace(".", ",")) : 0;
        Unit = json.Keys.Contains("unit") ? float.Parse(json["unit"].ToString().Replace(".", ",")) : 0;
        SupportSurfaceArea = json.Keys.Contains("supportSurfaceArea") ? float.Parse(json["supportSurfaceArea"].ToString().Replace(".", ",")) : 0;
        AlignedDimensions = json.Keys.Contains("alignedDims") ? ParseVector(json["alignedDims"].ToString()) : Vector3.one;        
        Up = json.Keys.Contains("up") ? ParseVector(json["up"].ToString()) : Vector3.up;
        Front = json.Keys.Contains("front") ? ParseVector(json["front"].ToString()) : Vector3.forward;
        staticFrictionForce = json.Keys.Contains("staticFrictionForce") ? float.Parse(json["staticFrictionForce"].ToString().Replace(".", ",")) : 1;

        Tags = new HashSet<string>();
        if (json.Keys.Contains("tags"))
        {
            for (int i = 0; i < json["tags"].Count; i++)
                Tags.Add(json["tags"][i].ToString().ToLower());
                
        }

        Lemmas = new HashSet<string>();
        if (json.Keys.Contains("wnlemmas"))
        {
            for (int i = 0; i < json["wnlemmas"].Count; i++)
                Lemmas.Add(json["wnlemmas"][i].ToString().ToLower());
        }
    }

    private static Vector3 ParseVector(string input)
    {
        string[] vectorSplit = input.Replace("[", "").Replace("]", "").Replace(" ", "").Split(',');
        return new Vector3(float.Parse(vectorSplit[0].Replace(".", ",")), float.Parse(vectorSplit[1].Replace(".", ",")), float.Parse(vectorSplit[2].Replace(".", ",")));
    }

    public bool ContainsTag(string searchElement)
    {
        bool hasTag = false;
        searchElement = searchElement.ToLower();
        foreach (string tag in Tags)
        {
            hasTag = hasTag || tag.ToLower().Contains(searchElement.ToLower());
        }
        return hasTag;
    }

}
