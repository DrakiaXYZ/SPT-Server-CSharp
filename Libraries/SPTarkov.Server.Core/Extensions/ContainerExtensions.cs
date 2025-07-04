﻿using SPTarkov.Server.Core.Models.Spt.Inventory;

namespace SPTarkov.Server.Core.Extensions
{
    public static class ContainerExtensions
    {
        /// <summary>
        ///     Finds a slot for an item in a given 2D container map
        /// </summary>
        /// <param name="container2D">List of container with positions filled/free</param>
        /// <param name="itemWidthX">Width of item</param>
        /// <param name="itemHeightY">Height of item</param>
        /// <returns>Location to place item in container</returns>
        public static FindSlotResult FindSlotForItem(
            this int[,] container2D,
            int? itemWidthX,
            int? itemHeightY
        )
        {
            // Assume not rotated
            var rotation = false;

            // Find the min volume the item will take up
            var minVolume = (itemWidthX < itemHeightY ? itemWidthX : itemHeightY) - 1;
            var containerY = container2D.GetLength(0); // rows
            var containerX = container2D.GetLength(1); // columns
            var limitY = containerY - minVolume;
            var limitX = containerX - minVolume;

            // Every x+y slot taken up in container, exit
            if (ContainerIsFull(container2D))
            {
                return new FindSlotResult(false);
            }

            // Down = y, iterate over rows
            for (var row = 0; row < limitY; row++)
            {
                if (RowIsFull(container2D, row))
                {
                    continue;
                }

                // Left to right across columns, look for free position
                for (var column = 0; column < limitX; column++)
                {
                    // Does item fit
                    if (
                        CanItemBePlacedInContainerAtPosition(
                            container2D,
                            column,
                            row,
                            itemWidthX.Value,
                            itemHeightY.Value
                        )
                    )
                    {
                        // Success, found a spot it fits
                        return new FindSlotResult(true, column, row, rotation);
                    }

                    if (!ItemBiggerThan1X1(itemWidthX.Value, itemHeightY.Value))
                    {
                        // Doesn't fit AND rotating won't help
                        continue;
                    }

                    // Rotate item by swapping x and y item values
                    if (
                        CanItemBePlacedInContainerAtPosition(
                            container2D,
                            column,
                            row,
                            itemHeightY.Value, // Swapped
                            itemWidthX.Value // Swapped
                        )
                    )
                    {
                        // Found a position for the item when rotated
                        rotation = true;
                        return new FindSlotResult(true, column, row, rotation);
                    }
                }
            }

            // Tried all possible positions, nothing big enough for item
            return new FindSlotResult(false);
        }

        /// <summary>
        ///     Find a free slot for an item to be placed at
        /// </summary>
        /// <param name="container2D">Container to place item in</param>
        /// <param name="columnStartPositionX">Container y size</param>
        /// <param name="rowStartPositionY">Container x size</param>
        /// <param name="itemXWidth">Items width</param>
        /// <param name="itemYHeight">Items height</param>
        /// <param name="isRotated">is item rotated</param>
        public static void FillContainerMapWithItem(
            this int[,] container2D,
            int columnStartPositionX,
            int rowStartPositionY,
            int? itemXWidth,
            int? itemYHeight,
            bool isRotated
        )
        {
            var containerY = container2D.GetLength(0); // rows
            var containerX = container2D.GetLength(1); // columns

            // Swap height/width if item needs to be rotated to fit
            var itemWidth = isRotated ? itemYHeight : itemXWidth;
            var itemHeight = isRotated ? itemXWidth : itemYHeight;

            var itemRowEndPosition = rowStartPositionY + itemHeight;
            var itemColumnEndPosition = columnStartPositionX + itemWidth;

            for (var y = rowStartPositionY; y < itemRowEndPosition; y++)
            {
                for (var x = columnStartPositionX; x < itemColumnEndPosition; x++)
                {
                    if (container2D[y, x] == 0)
                    {
                        // Flag slot as used
                        container2D[y, x] = 1;
                    }
                    else
                    {
                        throw new Exception(
                            $"Slot at({containerX}, {containerY}) is already filled. Cannot fit a {itemXWidth} by {itemYHeight} item"
                        );
                    }
                }
            }
        }

        /// <summary>
        /// Is the requested row full
        /// </summary>
        /// <param name="container2D">Container to check</param>
        /// <param name="rowIndex">Index of row to check</param>
        /// <returns>True = full</returns>
        private static bool RowIsFull(int[,] container2D, int rowIndex)
        {
            var rowFull = true;
            var containerColumnCount = container2D.GetLength(1); // Column
            for (var col = 0; col < containerColumnCount; col++)
            {
                if (container2D[rowIndex, col] == 0)
                {
                    rowFull = false;
                    break;
                }
            }

            return rowFull;
        }

        /// <summary>
        /// Is every slot in container full
        /// </summary>
        /// <param name="container2D">Container to check</param>
        /// <returns>True = full</returns>
        private static bool ContainerIsFull(int[,] container2D)
        {
            var containerY = container2D.GetLength(0); // rows
            var containerX = container2D.GetLength(1); // columns
            var containerFull = true;
            for (var y = 0; y < containerY; y++)
            {
                for (var x = 0; x < containerX; x++)
                {
                    if (container2D[y, x] == 0)
                    {
                        containerFull = false;
                        break;
                    }
                }
                if (!containerFull)
                {
                    break;
                }
            }

            return containerFull;
        }

        /// <summary>
        /// Is the item size values passed in bigger than 1x1
        /// </summary>
        /// <param name="itemWidth">Width of item</param>
        /// <param name="itemHeight">Height of item</param>
        /// <returns>True = bigger than 1x1</returns>
        private static bool ItemBiggerThan1X1(int itemWidth, int itemHeight)
        {
            return itemWidth + itemHeight > 2;
        }

        /// <summary>
        ///     Can an item of specified size be placed inside a 2d container at a specific position
        /// </summary>
        /// <param name="container">Container to find space in</param>
        /// <param name="startXPos">Starting x position for item</param>
        /// <param name="startYPos">Starting y position for item</param>
        /// <param name="itemXWidth">Items width</param>
        /// <param name="itemYHeight">Items height</param>
        /// <returns>True - slot found</returns>
        public static bool CanItemBePlacedInContainerAtPosition(
            this int[,] container,
            int startXPos,
            int startYPos,
            int itemXWidth,
            int itemYHeight
        )
        {
            var containerHeight = container.GetLength(1); // Rows
            var containerWidth = container.GetLength(0); // Columns

            // Check item isn't bigger than container when at position
            if (
                startXPos + itemXWidth > containerWidth
                || startYPos + itemYHeight > containerHeight
            )
            {
                // Item is bigger than container, will never fit
                return false;
            }

            // Single slot item, do direct check
            if (itemXWidth == 1 && itemYHeight == 1)
            {
                return container[startXPos, startYPos] == 0;
            }

            var itemEndColPosition = startXPos + itemXWidth;
            var itemEndRowPosition = startYPos + itemYHeight;
            for (var y = startYPos; y < itemEndColPosition; y++)
            {
                for (var x = startXPos; x < itemEndRowPosition; x++)
                {
                    if (container[y, x] == 1)
                    {
                        // Occupied by something
                        return false;
                    }
                }
            }

            return true; // Slot is free
        }
    }
}
