using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Sahan.Generation.Grid
{
    public class GridManager : MonoBehaviour
    {
        public int gridWidth;
        public int gridLength;

        public float gridOffsetX;
        public float gridOffsetZ;

        public float gridSpawnHeight;
        
        public float gridScaleX;
        public float gridScaleZ;
        
        [HideInInspector] public GridObject[,] gridObjects;
        [SerializeField] private GameObject gridBasePrefab;
        [SerializeField] private Transform gridParent;

        [SerializeField] private Vector2Int initialStartPoint;

        private void Start()
        {
            SetupGrid();
            SetGridEvents();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SetStartPoint();
            }
        }

        private void SetupGrid()
        {
            gridObjects = new GridObject[gridWidth, gridLength];

            for (int i = 0; i < gridLength; i++)
            {
                for (int j = 0; j < gridWidth; j++)
                {
                    Vector3 spawnCoordinates = GetSpawnCoordinates(j, i);
                    
                    GridObject gridObject = Instantiate(gridBasePrefab, spawnCoordinates, Quaternion.identity, gridParent)
                        .GetComponent<GridObject>();
                    gridObject.UpdateGridObject(GridState.Default);
                    gridObjects[j, i] = gridObject;
                }
            }
        }

        private void SetGridEvents()
        {
            for (int i = 0; i < gridLength; i++)
            {
                for (int j = 0; j < gridWidth; j++)
                {
                    if (j - 1 >= 0)
                    {
                        gridObjects[j, i].onGridItemUpdated += gridObjects[j - 1, i].OnConnectedGridItemUpdated;
                    }
                    if (j + 1 < gridWidth)
                    {
                        gridObjects[j, i].onGridItemUpdated += gridObjects[j + 1, i].OnConnectedGridItemUpdated;
                    }
                    if (i - 1 >= 0)
                    {
                        gridObjects[j, i].onGridItemUpdated += gridObjects[j, i - 1].OnConnectedGridItemUpdated;
                    }
                    if (i + 1 < gridLength)
                    {
                        gridObjects[j, i].onGridItemUpdated += gridObjects[j, i + 1].OnConnectedGridItemUpdated;
                    }
                }
            }
        }

        private Vector3 GetSpawnCoordinates(int xIndex, int zIndex)
        {
            return new Vector3(xIndex * gridScaleX - gridOffsetX, gridSpawnHeight, zIndex * gridScaleZ - gridOffsetZ);
        }
        
        private void SetStartPoint()
        {
            gridObjects[initialStartPoint.x, initialStartPoint.y].UpdateGridObject(GridState.Expanded);
        }
    }
}
