using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace Sahan.Generation.Grid
{
    public class GridObject : MonoBehaviour
    {
        public UnityAction<GridState> onGridItemUpdated;

        [SerializeField] private GameObject defaultPrefab;
        [SerializeField] private GameObject expandablePrefab;
        [SerializeField] private GameObject expandedPrefab;

        [SerializeField] private float objectDisappearTime;
        [SerializeField] private Ease objectDisappearMode;
        
        [SerializeField] private float objectAppearTime;
        [SerializeField] private Ease objectAppearMode;

        [SerializeField] private Vector3 objectScale;
        
        private GameObject currentDisplayedObject;
        private GridState currentState;
        
        public void UpdateGridObject(GridState gridState)
        {
            currentState = gridState;
            onGridItemUpdated?.Invoke(gridState);
            RemoveCurrentObject(SpawnObject);
        }

        private void RemoveCurrentObject(UnityAction callBackAction = null)
        {
            if (currentDisplayedObject != null)
            {
                currentDisplayedObject.transform.DOScale(Vector3.zero, objectDisappearTime).SetEase(objectDisappearMode)
                    .OnComplete(() =>
                    {
                        Destroy(currentDisplayedObject);
                        currentDisplayedObject = null;
                        callBackAction?.Invoke();
                    });
            }
            else
            {
                callBackAction?.Invoke();
            }
        }

        private void SpawnObject()
        {
            currentDisplayedObject = Instantiate(GetObjectToSpawn(), transform);

            ISpawnable spawnable = currentDisplayedObject.GetComponent<ISpawnable>();
            spawnable.SetUpdateEvents(UpdateGridObject, GetUpdatedState());

            currentDisplayedObject.transform.localScale = Vector3.zero;
            currentDisplayedObject.transform.DOScale(objectScale, objectAppearTime).SetEase(objectAppearMode)
                .OnComplete(() =>
                {
                    spawnable.SpawnContent();
                });
        }

        private GameObject GetObjectToSpawn()
        {
            GameObject objectToSpawn;
            
            switch (currentState)
            {
                case GridState.Default :
                    objectToSpawn = defaultPrefab;
                    break;
                case GridState.Expandable :
                    objectToSpawn = expandablePrefab;
                    break;
                case GridState.Expanded :
                    objectToSpawn = expandedPrefab;
                    break;
                default:
                    objectToSpawn = defaultPrefab;
                    break;
            }
            return objectToSpawn;
        }

        public void OnConnectedGridItemUpdated(GridState gridState)
        {
            if (gridState == GridState.Expanded && currentState == GridState.Default)
            {
                UpdateGridObject(GridState.Expandable);
            }
        }

        private GridState GetUpdatedState()
        {
            if (currentState == GridState.Default)
            {
                return GridState.Expandable;
            }
            else if (currentState == GridState.Expandable)
            {
                return GridState.Expanded;
            }
            else
            {
                return GridState.Expanded;
            }
        }
    }
}
