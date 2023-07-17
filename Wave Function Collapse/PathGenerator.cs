using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
using DG.Tweening;
using Sequence = DG.Tweening.Sequence;

namespace Sahan.Archery.Generation
{
    public class PathGenerator : MonoBehaviour
    {
        [SerializeField] private List<BlockData> blocksList;
        [SerializeField] private int gridWidth;
        [SerializeField] private int gridLength;
        [SerializeField] private Transform gridParent;
        [SerializeField] private float gridScale;
        [SerializeField] private Vector3 gridOffset;
        
        [SerializeField] private int boarderPriority;
        
        [SerializeField] private float spawnDelay;
        [SerializeField] private float resetDelay;
        
        private Slot[,] slotMap;
        private List<Slot> spawnOrderList = new List<Slot>();
        private bool isWaveBroken = false;
        
        private void Start()
        {
            InitGenerator();
        }

        private void InitGenerator()
        {
            ResetGrid();
            GenerateGrid();
            CollapseGrid();
        }
        
        private void GenerateGrid()
        {
            slotMap = new Slot[gridWidth, gridLength];
            for (int i = 0; i < gridLength; i++)
            {
                for (int j = 0; j < gridWidth; j++)
                {
                    Slot slot = new Slot(j, i);
                    slot.availableBlocks = new List<BlockData>(blocksList);
                    slotMap[j,i] = slot;
                }
            }
            SetPriorities();
        }

        private void ResetGrid()
        {
            spawnOrderList.Clear();
            isWaveBroken = false;
            ClearSpawnedGrid();
            Randomizer.Instance.SetRandomizer();
        }

        private void ClearSpawnedGrid()
        {
            for (int i = 0; i < gridParent.childCount; i++)
            {
                Destroy(gridParent.GetChild(i).gameObject);
            }
        }

        private void SetPriorities()
        {
            for (int i = 0; i < gridLength; i++)
            {
                for (int j = 0; j < gridWidth; j++)
                {
                    if (i == 0 || i == gridLength - 1 || j == 0 || j == gridWidth - 1)
                    {
                        slotMap[j, i].collapsePriority = boarderPriority;
                    }
                }
            }
        }

        private void CollapseGrid()
        {
            while (!IsWaveFunctionCollapsed())
            {
                RunCollapse();
                if (isWaveBroken)
                {
                    break;
                }
            }

            if (isWaveBroken)
            {
                InitGenerator();
            }
            else
            {
                SpawnGrid();
            }
        }

        private bool IsWaveFunctionCollapsed()
        {
            bool isCollapsed = true;
            for (int i = 0; i < gridLength; i++)
            {
                for (int j = 0; j < gridWidth; j++)
                {
                    if (slotMap[j, i].availableBlocks.Count != 1)
                    {
                        isCollapsed = false;
                    }
                }
            }
            return isCollapsed;
        }
        
        private void RunCollapse()
        {
            Slot slotToCollapse = GetMinEntropyItem();
            
            if (slotToCollapse.availableBlocks.Count > 1)
            {
                CollapseItem(slotToCollapse);
                PropagateGrid(slotToCollapse);
            }
        }

        private Slot GetMinEntropyItem()
        {
            List<Slot> slotsWithMinEntropy = GetMinEntropyList(FindMinEntropy());
            slotsWithMinEntropy = slotsWithMinEntropy.OrderByDescending(o=>o.collapsePriority).ToList();
            return slotsWithMinEntropy[0];
        }

        private int FindMinEntropy()
        {
            int minEntropy = blocksList.Count;
            for (int i = 0; i < gridLength; i++)
            {
                for (int j = 0; j < gridWidth; j++)
                {
                    if (slotMap[j, i].availableBlocks.Count < minEntropy  && slotMap[j, i].availableBlocks.Count > 1)
                    {
                        minEntropy = slotMap[j, i].availableBlocks.Count;
                    }
                }
            }
            return minEntropy;
        }

        private List<Slot> GetMinEntropyList(int minEntropy)
        {
            List<Slot> slotsWithMinEntropy = new List<Slot>();
            for (int i = 0; i < gridLength; i++)
            {
                for (int j = 0; j < gridWidth; j++)
                {
                    if (slotMap[j, i].availableBlocks.Count == minEntropy)
                    {
                        slotsWithMinEntropy.Add(slotMap[j, i]);
                    }
                }
            }
            return slotsWithMinEntropy;
        }
        
        private void CollapseItem(Slot slot)
        {
            HandleBoarderBlocks(slot);
            BlockData randomBlock = RandomWithWeights(slot);

            slot.availableBlocks.Clear();
            slot.availableBlocks.Add(randomBlock);
            spawnOrderList.Add(slot);
        }

        private void HandleBoarderBlocks(Slot slot)
        {
            List<BlockData> remainingBlocks = new List<BlockData>();
            foreach (BlockData blockData in slot.availableBlocks)
            {
                if (blockData.leftSocket.socketType == SocketType.Open && slot.xIndex == 0)
                {
                }
                else if (blockData.rightSocket.socketType == SocketType.Open && slot.xIndex == gridWidth - 1)
                {
                }
                else if (blockData.bottomSocket.socketType == SocketType.Open && slot.zIndex == 0)
                {
                }
                else if (blockData.topSocket.socketType == SocketType.Open && slot.zIndex == gridLength - 1)
                {
                }
                else
                {
                    remainingBlocks.Add(blockData);
                }
            }
            isWaveBroken = slot.UpdateAvailableBlocks(remainingBlocks);
        }

        private BlockData RandomWithWeights(Slot slot)
        {
            BlockData randomizedBlock = null;

            int totalWeight = 0;
            foreach (BlockData block in slot.availableBlocks)
            {
                totalWeight += block.spawnWeight;
            }

            int randomWeight = Randomizer.Instance.GetRandomValue(0, totalWeight + 1);

            int processedWeight = 0;
            foreach (BlockData block in slot.availableBlocks)
            {
                processedWeight += block.spawnWeight;
                if (randomWeight <= processedWeight)
                {
                    randomizedBlock = block;
                    break;
                }
            }
            return randomizedBlock;
        }
        
        //Use this function to debug the grid when needed
        private void DisplayGrid()
        {
            string gridString = "";
            for (int i = 0; i < gridWidth; i++)
            {
                for (int j = 0; j < gridLength; j++)
                {
                    gridString += slotMap[i, j].availableBlocks.Count + " ";
                }
                gridString += "\n";
            }
            Debug.Log(gridString);
        }
        
        private void PropagateGrid(Slot slot)
        {
            List<Slot> modifiedItems = new List<Slot>();
            modifiedItems.Add(slot);
            
            while (modifiedItems.Count > 0)
            {
                Slot modifiedItem = modifiedItems[0];
                modifiedItems.Remove(modifiedItem);
                
                if (modifiedItem.xIndex - 1 >= 0)
                {
                    //check left
                    if (slotMap[modifiedItem.xIndex - 1, modifiedItem.zIndex].availableBlocks.Count > 1)
                    {
                        if (GetCompatibleList(modifiedItem, modifiedItem.xIndex - 1, modifiedItem.zIndex, SlotDirection.Left))
                        {
                            modifiedItems.Add(slotMap[modifiedItem.xIndex - 1, modifiedItem.zIndex]);
                        }
                    }
                }
                if (modifiedItem.xIndex + 1 < gridWidth)
                {
                    //check right
                    if (slotMap[modifiedItem.xIndex + 1, modifiedItem.zIndex].availableBlocks.Count > 1)
                    {
                        if (GetCompatibleList(modifiedItem, modifiedItem.xIndex + 1, modifiedItem.zIndex,
                                SlotDirection.Right))
                        {
                            modifiedItems.Add(slotMap[modifiedItem.xIndex + 1, modifiedItem.zIndex]);
                        }
                    }
                }
                if (modifiedItem.zIndex - 1 >= 0)
                {
                    //check top
                    if (slotMap[modifiedItem.xIndex, modifiedItem.zIndex - 1].availableBlocks.Count > 1)
                    {
                        if (GetCompatibleList(modifiedItem, modifiedItem.xIndex, modifiedItem.zIndex - 1,
                                SlotDirection.Bottom))
                        {
                            modifiedItems.Add(slotMap[modifiedItem.xIndex, modifiedItem.zIndex - 1]);
                        }
                    }
                }
                if (modifiedItem.zIndex + 1 < gridLength)
                {
                    //check bottom
                    if (slotMap[modifiedItem.xIndex, modifiedItem.zIndex + 1].availableBlocks.Count > 1)
                    {
                        if (GetCompatibleList(modifiedItem, modifiedItem.xIndex, modifiedItem.zIndex + 1,
                                SlotDirection.Top))
                        {
                            modifiedItems.Add(slotMap[modifiedItem.xIndex, modifiedItem.zIndex + 1]);
                        }
                    }
                }
            }
        }
        
        private bool GetCompatibleList(Slot slot, int xIndex, int zIndex, SlotDirection direction)
        {
            bool isUpdated = false;
            if (!slotMap[xIndex, zIndex].isCollapsed)
            {
                List<BlockData> remainingBlocks = GetRemainingBlocks(slot, xIndex, zIndex, direction);

                if (slotMap[xIndex, zIndex].availableBlocks.Count > remainingBlocks.Count)
                {
                    isUpdated = true;
                    if (remainingBlocks.Count > 0)
                    {
                        isWaveBroken = slotMap[xIndex, zIndex].UpdateAvailableBlocks(remainingBlocks);
                        if (slotMap[xIndex, zIndex].isCollapsed)
                        {
                            spawnOrderList.Add(slotMap[xIndex, zIndex]);
                        }
                    }
                    else
                    {
                        isWaveBroken = true;
                    }
                }
            }
            return isUpdated;
        }
        
        private List<BlockData> GetRemainingBlocks(Slot slot, int xIndex, int zIndex, SlotDirection direction)
        {
            List<BlockData> remainingBlocks = new List<BlockData>();
            foreach (BlockData block in slot.availableBlocks)
            {
                SocketData socketOne = GetPrimarySocket(block, direction);
                foreach (BlockData blockSec in slotMap[xIndex, zIndex].availableBlocks)
                {
                    if (AreSocketsCompatible(socketOne, blockSec, direction, IsSeaBlock(slot)))
                    {
                        if (!remainingBlocks.Contains(blockSec))
                        {
                            remainingBlocks.Add(blockSec);
                        }
                    }
                }
            }
            return remainingBlocks;
        }

        private bool IsSeaBlock(Slot slot)
        {
            bool isSeaBlock = false;
            if (slot.isCollapsed)
            {
                isSeaBlock = slot.availableBlocks[0].leftSocket.socketType == SocketType.Sea ||
                             slot.availableBlocks[0].rightSocket.socketType == SocketType.Sea
                             || slot.availableBlocks[0].topSocket.socketType == SocketType.Sea ||
                             slot.availableBlocks[0].bottomSocket.socketType == SocketType.Sea;
            }

            return isSeaBlock;
        }

        private SocketData GetPrimarySocket(BlockData block, SlotDirection direction)
        {
            SocketData socket = new SocketData();
            switch (direction)
            {
                case SlotDirection.Left:
                    socket = block.leftSocket;
                    break;
                case SlotDirection.Right:
                    socket = block.rightSocket;
                    break;
                case SlotDirection.Top:
                    socket = block.topSocket;
                    break;
                case SlotDirection.Bottom:
                    socket = block.bottomSocket;
                    break;
            }
            return socket;
        }
        
        private SocketData GetSecondarySocket(BlockData block, SlotDirection direction)
        {
            SocketData socket = new SocketData();
            switch (direction)
            {
                case SlotDirection.Left:
                    socket = block.rightSocket;
                    break;
                case SlotDirection.Right:
                    socket = block.leftSocket;
                    break;
                case SlotDirection.Top:
                    socket = block.bottomSocket;
                    break;
                case SlotDirection.Bottom:
                    socket = block.topSocket;
                    break;
            }
            return socket;
        }
        
        private bool AreSocketsCompatible(SocketData socketOne, BlockData block, SlotDirection direction, bool isSeaBlockPossible)
        {
            bool isCompatible = false;
            SocketData socketTwo = GetSecondarySocket(block, direction);
            if (socketOne.socketType == SocketType.Open && socketTwo.socketType == SocketType.Wall)
            {
                //Not compatible
            }
            else if (socketOne.socketType == SocketType.Wall && socketTwo.socketType == SocketType.Open)
            {
                //Not compatible
            }
            else if (socketOne.socketType == SocketType.Sea && socketTwo.socketType == SocketType.Open)
            {
                //Not compatible
            }
            else if (socketOne.socketType == SocketType.Open && socketTwo.socketType == SocketType.Sea)
            {
                //Not compatible
            }
            else if (socketOne.socketType == SocketType.Wall && socketTwo.socketType == SocketType.Wall)
            {
                //Not compatible
            }
            else
            {
                isCompatible = true;
            }

            return isCompatible;
        }
        
        private void SpawnGrid()
        {
            for (int i = 0; i < spawnOrderList.Count; i++)
            {
                Vector3 coordinates = new Vector3(spawnOrderList[i].xIndex, 0 , spawnOrderList[i].zIndex) * gridScale + gridOffset;
                GameObject go = Instantiate(spawnOrderList[i].availableBlocks[0].blockModel, coordinates, Quaternion.identity, gridParent);
                go.transform.localScale = Vector3.zero;
                go.GetComponent<BlockHandler>().slot = spawnOrderList[i];
                go.transform.DOScale(Vector3.one, 0.1f).SetEase(Ease.OutBack).SetDelay(i * spawnDelay);
            }

            StartCoroutine(ResetAfterDelay());
        }

        private IEnumerator ResetAfterDelay()
        {
            yield return new WaitForSecondsRealtime(spawnDelay * gridLength * gridWidth + resetDelay);
            InitGenerator();
        }

    }
}
