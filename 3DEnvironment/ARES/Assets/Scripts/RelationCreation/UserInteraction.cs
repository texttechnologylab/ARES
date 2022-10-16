using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInteraction : MonoBehaviour
{
    public Transform headCube;
    public Transform tailCube;


    public float cubePositionSpeed = 0.1f;
    public float cubeScaleSpeed = 0.1f;
    public float cubeRotationSpeed = 1f;
    private float currKeyImpact;
    private Transform currCube;

    enum State {Position, LocalScale, Rotation};
    private State currState = State.Position;

    private void addVector3OntoCube(Vector3 vec)
    {
        switch (currState)
        {
            case State.Position:
                currCube.position += vec;
                break;
            case State.LocalScale:
                currCube.localScale += vec;
                break;
            case State.Rotation:
                currCube.eulerAngles += vec;
                break;
        }
    }

    private void setCurrKeyImpact()
    {
        switch (currState)
        {
            case State.Position:
                currKeyImpact = cubePositionSpeed;
                break;
            case State.LocalScale:
                currKeyImpact = cubeScaleSpeed;
                break;
            case State.Rotation:
                currKeyImpact = cubeRotationSpeed;
                break;
        }
    }

    void Start()
    {
        currCube = headCube;
        setCurrKeyImpact();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            addVector3OntoCube(new Vector3(-currKeyImpact, 0, 0));
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            addVector3OntoCube(new Vector3(currKeyImpact, 0, 0));
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            addVector3OntoCube(new Vector3(0, 0, currKeyImpact));
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            addVector3OntoCube(new Vector3(0, 0, -currKeyImpact));
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            addVector3OntoCube(new Vector3(0, currKeyImpact, 0));
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            addVector3OntoCube(new Vector3(0, -currKeyImpact, 0));
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            switch (currState)
            {
            case State.Position:
                currState = State.LocalScale;
                break;
            case State.LocalScale:
                currState = State.Rotation;
                break;
            case State.Rotation:
                currState = State.Position;
                break;
            }
            setCurrKeyImpact();
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currCube = headCube;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currCube = tailCube;
        }
    }
}
