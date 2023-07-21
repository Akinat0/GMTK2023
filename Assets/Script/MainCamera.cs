using System;
using Lean.Common;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MainCamera : MonoBehaviour
{
    public Camera Camera { get; private set; }

    BoxCollider cameraBounds;

    GridController Grid => GameScene.Grid;
    
    LeanConstrainToCollider cameraConstraint;

    public Vector2 CameraSize => new Vector2(Camera.orthographicSize * 2 * Camera.aspect, Camera.orthographicSize * 2);
    public Rect CameraRect => new Rect((Vector2)transform.position - CameraSize * 0.5f, CameraSize);


    void Awake()
    {
        Camera = GetComponent<Camera>();
    }

    void Start()
    {
        cameraConstraint = gameObject.AddComponent<LeanConstrainToCollider>();
        cameraBounds = Grid.gameObject.AddComponent<BoxCollider>();
        
        cameraConstraint.Collider = cameraBounds;

        CalculateCameraConstraintBound();
    }

    void Update()
    {
        float scrollDelta = -Input.mouseScrollDelta.y;
        
        if(Mathf.Approximately(scrollDelta, 0))
            return;
        
        //here we suppose that width is higher then height so we will limit orthographic size by max (width)
        float width = Grid.Width * Grid.CellSize / 0.8f * 1.05f;
        float maxOrthographicSize = width * 0.5f / Camera.aspect;

        Camera.orthographicSize = Mathf.Clamp(Camera.orthographicSize + scrollDelta, 3, maxOrthographicSize);
        CalculateCameraConstraintBound();
    }

    void CalculateCameraConstraintBound()
    {
        Vector3 camSize = CameraSize;
        
        Vector3 gridSize = new Vector3(Grid.Width * Grid.CellSize, Grid.Height * Grid.CellSize, 0);
        cameraBounds.center = gridSize / 2 - new Vector3(Grid.CellSize / 2, Grid.CellSize / 2) + new Vector3(camSize.x * 0.2f / 2, 0, 0);
        cameraBounds.size = new Vector3(Grid.Width * Grid.CellSize, Grid.Width * Grid.CellSize, 10000) - camSize + new Vector3(camSize.x * 0.2f, 0, 0) + camSize * 0.05f;
    }
    
}
