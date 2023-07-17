using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Sahan.Generation.Grid
{
    public class ExpandedGridItem : MonoBehaviour, ISpawnable
    {
        [SerializeField] private List<Transform> spawnPoints;
        [SerializeField] private List<SpawnableItem> spawnableItems;

        public void SpawnContent()
        {
            foreach (Transform spawnPoint in spawnPoints)
            {
                GameObject objectToSpawn = GetWeightedRandomItem();
                if (objectToSpawn != null)
                {
                    Instantiate(objectToSpawn, spawnPoint);
                }
            }
        }
        
        public void SetUpdateEvents(UnityAction<GridState> updateAction, GridState state)
        {
            //No actions for this event
        }
        
        private GameObject GetWeightedRandomItem()
        {
            int[] weights = GetWeightsArray();
            int randomWeight = UnityEngine.Random.Range(0, weights.Sum());
            for (int i = 0;i < weights.Length; ++i)
            {
                randomWeight -= weights[i];
                if (randomWeight < 0)
                {
                    return spawnableItems[i].itemPrefab;
                }
            }

            return null;
        }

        private int[] GetWeightsArray()
        {
            int[] weights = new int[spawnableItems.Count];
            for (int i = 0; i < spawnableItems.Count; i++)
            {
                weights[i] = spawnableItems[i].spawnWeight;
            }

            return weights;
        }
    }

    [System.Serializable]
    public class SpawnableItem
    {
        public GameObject itemPrefab;
        public int spawnWeight;
    }
}
