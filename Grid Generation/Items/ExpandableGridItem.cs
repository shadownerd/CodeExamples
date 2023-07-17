using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Sahan.Generation.Grid
{
    public class ExpandableGridItem : MonoBehaviour, ISpawnable
    {
        public Button expandButton;

        [SerializeField] private float objectAppearTime;
        [SerializeField] private Ease objectAppearMode;
        
        [SerializeField] private Vector3 objectScale;
        
        public void SpawnContent()
        {
            expandButton.transform.DOScale(objectScale, objectAppearTime).SetEase(objectAppearMode);
        }

        public void SetUpdateEvents(UnityAction<GridState> updateAction, GridState state)
        {
            expandButton.onClick.AddListener(() =>
            {
                updateAction(state);
            });
        }

        private void OnDestroy()
        {
            expandButton.onClick.RemoveAllListeners();
        }
    }
}
