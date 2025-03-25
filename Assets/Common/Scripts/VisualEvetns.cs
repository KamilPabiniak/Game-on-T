using System;
using UnityEngine;

namespace Common.Scripts
{
    /// <summary>
    /// Contains events used to transfer block data from the Game scene to the UI scene.
    /// </summary>
    public static class VisualEvetns
    {
        /// <summary>
        /// Event data for block-related events.
        /// </summary>
        public class BlockInfoEventArgs : EventArgs
        {
            public int BlockId { get; }
            public Vector3 WorldPosition { get; }
            public Color BlockColor { get; }

            public BlockInfoEventArgs(int blockId, Vector3 worldPosition, Color blockColor)
            {
                BlockId = blockId;
                WorldPosition = worldPosition;
                BlockColor = blockColor;
            }
        }

        /// <summary>
        /// Raised when a new block is created.
        /// </summary>
        public static event EventHandler<BlockInfoEventArgs> OnBlockCreated;

        /// <summary>
        /// Raised when an existing block’s position is updated.
        /// </summary>
        public static event EventHandler<BlockInfoEventArgs> OnBlockPositionUpdated;

        /// <summary>
        /// Raised when a block is removed (for example, after row clearing).
        /// </summary>
        public static event EventHandler<BlockInfoEventArgs> OnBlockRemoved;

        /// <summary>
        /// Call this method to notify that a block has been created.
        /// </summary>
        public static void RaiseBlockCreated(int blockId, Vector3 worldPosition, Color blockColor)
        {
            OnBlockCreated?.Invoke(null, new BlockInfoEventArgs(blockId, worldPosition, blockColor));
        }

        /// <summary>
        /// Call this method to notify that a block’s position has been updated.
        /// </summary>
        public static void RaiseBlockPositionUpdated(int blockId, Vector3 worldPosition, Color blockColor)
        {
            OnBlockPositionUpdated?.Invoke(null, new BlockInfoEventArgs(blockId, worldPosition, blockColor));
        }

        /// <summary>
        /// Call this method to notify that a block has been removed.
        /// </summary>
        public static void RaiseBlockRemoved(int blockId)
        {
            OnBlockRemoved?.Invoke(null, new BlockInfoEventArgs(blockId, Vector3.zero, Color.white));
        }
    }
}
