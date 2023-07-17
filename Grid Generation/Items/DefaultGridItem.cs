using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Sahan.Generation.Grid
{
    public class DefaultGridItem : MonoBehaviour, ISpawnable
    {
        public void SpawnContent()
        {
            //No content spawns for default grid item
        }
        
        public void SetUpdateEvents(UnityAction<GridState> updateAction, GridState state)
        {
            //No actions for this event
        }
    }
}
