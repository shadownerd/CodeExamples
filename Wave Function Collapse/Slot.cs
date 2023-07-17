using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sahan.Archery.Generation
{
    [System.Serializable]
    public class Slot
    {
        public List<BlockData> availableBlocks;
        public bool isStartingBlock = false;
        public bool isEndingBlock = false;
        public int collapsePriority;
        public int xIndex;
        public int zIndex;
        public bool isCollapsed = false;
        
        public Slot(int xIndex, int zIndex)
        {
            this.xIndex = xIndex;
            this.zIndex = zIndex;
        }

        public bool UpdateAvailableBlocks(List<BlockData> newBlocksList)
        {
            bool isWFCBroken = false;
            if (!isCollapsed)
            {
                if (newBlocksList.Count == 0)
                {
                    isWFCBroken = true;
                }
                else
                {
                    availableBlocks.Clear();
                    availableBlocks = new List<BlockData>(newBlocksList);
                    if (availableBlocks.Count == 1)
                    {
                        isCollapsed = true;
                    }
                }
            }
            else
            {
                isWFCBroken = true;
            }

            return isWFCBroken;
        }
    }
    
    public enum SlotDirection
    {
        Left,
        Right,
        Top,
        Bottom
    }
}
