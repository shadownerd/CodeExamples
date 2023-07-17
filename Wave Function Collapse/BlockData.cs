using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sahan.Archery.Generation
{
    [CreateAssetMenu(fileName = "New BlockData", menuName = "Block/Default BlockData")]
    public class BlockData : ScriptableObject
    {
        public GameObject blockModel;
        public int spawnWeight;
        
        public SocketData leftSocket;
        public SocketData rightSocket;
        public SocketData topSocket;
        public SocketData bottomSocket;
    }
    
    [System.Serializable]
    public class SocketData
    {
        public SocketType socketType;
    }
    
    public enum SocketType
    {
        Open,
        Wall,
        Sea
    }
}
