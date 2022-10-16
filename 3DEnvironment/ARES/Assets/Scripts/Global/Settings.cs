using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings : MonoBehaviour
{
    //text analysis
    public static string apiEndpoint = "http://127.0.0.1:5000/";
    public static string model = "SpaCy";
    //scene algorithm
    public static float errorScoreGroundMovement = 1;
    public static float errorScoreRotationMovement = 5;
    public static float errorScorePositiveHeightMovement = 10;
    public static float errorScoreNegativeHeightMovement = 100;
    public static bool debugMode = true;
    public static bool customAttributes = false;
    //scene builder
    public static int maxEntitySearchResults = 10;
}
