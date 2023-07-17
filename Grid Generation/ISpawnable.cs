using UnityEngine.Events;

namespace Sahan.Generation.Grid
{
    public interface ISpawnable
    {
        void SpawnContent();

        void SetUpdateEvents(UnityAction<GridState> updateAction, GridState state);
    }
}
