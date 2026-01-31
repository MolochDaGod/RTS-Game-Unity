using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace UnitSystem
{

    /// <summary>
    /// Delegate that allows implementation to prevent production of certain units
    /// when they reached the very end of production.
    /// <para>
    /// Example: This may be used if some
    /// resources like population needs to be above 0 before spawning/producing
    /// a unit is possible.
    /// </para>
    /// </summary>
    public interface IProductionQueueDelegate
    {
        /// <summary>
        /// Method invoked before producing the latest item. Implement this to
        /// return 'false' and prevent production from finishing. This will then
        /// be attempted for each <see cref="ProductionQueue.Produce(float)"/>
        /// iteration.
        /// </summary>
        /// <param name="producableQuantity">Producable and its quantity</param>
        /// <returns>
        /// Return 'true' to finish production, return 'false' to prevent it.
        /// </returns>
        public bool ShouldFinishProductionFor(ProducableQuantity producableQuantity);
    }

    /// <summary>
    /// Class used for producing items from a list, one by one.
    /// First item in queue is producing, the rest wait.
    /// It supports starting, queueing or canceling production.
    /// </summary>
    public class ProductionQueue
    {

        /// <summary>
        /// Current time of the production time for first producable in the <see cref="Queue"/>.
        /// </summary>
        private float productionTimeRemaining = -1f;

        private readonly List<ProducableQuantity> queue = new List<ProducableQuantity>();

        /// <summary>
        /// Collection of the current production items.
        /// </summary>
        public ProducableQuantity[] Queue => queue.ToArray();

        /// <summary>
        /// Action invoked once a producable finishes production.
        /// </summary>
        public Action<ProducableQuantity> OnProductionFinished;

        /// <summary>
        /// Returns range from 0 to 1 when an item is in production.
        /// When there is no production -1 is returned.
        /// </summary>
        public float CurrentProductionProgress { private set; get; } = -1f;

        /// <summary>
        /// Optional delegate that allows pausing of ongoing production.
        /// </summary>
        public IProductionQueueDelegate Delegate;

        /// <summary>
        /// Applies delta changes to current production queue. Only a single
        /// item can be produced at a time (first one in queue). If delta is
        /// larger than the remaining time of the current production, remainder
        /// is applied to the next production item (if exists).
        /// </summary>
        /// <param name="delta">Value applied to the production time. This
        /// can be either an actual time or time period value.</param>
        public void Produce(float delta)
        {
            if (queue.Count == 0)
            {
                productionTimeRemaining = -1;
                CurrentProductionProgress = -1;
                return;
            }

            var originalTimeRemaining = productionTimeRemaining;
            productionTimeRemaining -= delta;

            // In case its actually 0 still produce for one frame.
            if (productionTimeRemaining < 0)
            {
                var item = queue[0];

                if (Delegate != null && !Delegate.ShouldFinishProductionFor(item))
                {
                    productionTimeRemaining = originalTimeRemaining;
                    return;
                }

                float remaningTime = Math.Abs(productionTimeRemaining);
                queue.RemoveAt(0);
                OnProductionFinished?.Invoke(item);

                DequeueNextItem();

                Produce(remaningTime);
            }
            else
            {
                CurrentProductionProgress = 1f - (productionTimeRemaining / queue[0].Producable.ProductionDuration);
            }
        }

        /// <summary>
        /// Setup first item in queue for production.
        /// </summary>
        private void DequeueNextItem()
        {
            if (queue.Count > 0)
            {
                productionTimeRemaining = queue[0].Producable.ProductionDuration;
                CurrentProductionProgress = 0f;
            }
            else
            {
                CurrentProductionProgress = -1;
            }
        }

        /// <summary>
        /// Checks if the production queue contains this producable.
        /// Matching is done with <see cref="BasicSO.UUID"/>.
        /// </summary>
        /// <param name="producable">Producable used for matching.</param>
        /// <returns>Returns 'true' only if this item is in production.</returns>
        public bool IsProducing(ProducableSO producable)
        {
            return queue
                .Find(match => match.Producable.UUID == producable.UUID)
                .Producable != null;
        }

        /// <summary>
        /// Adds a producable and its quantity to the end of production queue.
        /// </summary>
        /// <param name="producable">Producable to be added.</param>
        /// <param name="quantity">Quantity to be produced.</param>
        /// <param name="queueMultipleOrders">
        /// If quantity should be split into multiple orders, diving by quantity 1.
        /// </param>
        public void AddProductionOrder(ProducableSO producable, float quantity, bool queueMultipleOrders = false)
        {
            // Nothing to do
            if (quantity <= 0) return;

            if (queueMultipleOrders)
            {
                while (quantity > 0)
                {
                    // Queue producable once for every 1 full production, if
                    // there are leftovers they will be queued as well.
                    // Generally full numbers are used for production, like 5 grapes
                    // but if game type desires decimals this is also supported by
                    // the leftover.
                    if (quantity > 1) queue.Add(new ProducableQuantity(producable, 1));
                    else queue.Add(new ProducableQuantity(producable, quantity));
                    quantity--;
                }
            }
            else
            {
                queue.Add(new ProducableQuantity(producable, quantity));
            }

            // If this is first item, dequeue right away
            if (queue.Count == 1)
            {
                DequeueNextItem();
            }
        }

        /// <summary>
        /// Cancels the first producable that matches the parameter.
        /// </summary>
        /// <param name="producable">
        /// Producable that will be canceled if present in queue.
        /// </param>
        /// <returns>
        /// Returns 'true' if production order was canceled.
        /// Returns 'false' only when producable with the same UUID is not present in queue.
        /// </returns>
        public bool CancelProductionOrder(ProducableSO producable)
        {
            return CancelProductionOrder(producable.UUID);
        }

        /// <summary>
        /// Cancel the first producable with UUID that matches the argument.
        /// </summary>
        /// <param name="producableUUID">
        /// UUID that will be used for finding a matching producable.
        /// </param>
        /// <returns>
        /// Returns 'true' if production order was canceled.
        /// Returns 'false' only when no producable was found with matching UUID.
        /// </returns>
        public bool CancelProductionOrder(string producableUUID)
        {
            int index = queue.FindIndex(match => match.Producable.UUID == producableUUID);
            return CancelProductionOrder(index);
        }

        /// <summary>
        /// Cancels the producable on specified index.
        /// </summary>
        /// <param name="index">
        /// Index on which the producable should be canceled.
        /// </param>
        /// <returns>
        /// Returns 'true' if item was successfully removed from the list.
        /// Returns 'false' when index is out of bounds.
        /// </returns>
        public bool CancelProductionOrder(int index)
        {
            // Check if index is too high or too low
            if (index >= queue.Count || index < 0) return false;

            queue.RemoveAt(index);

            // If the first item in list was canceled, dequeue next item
            if (index == 0)
            {
                productionTimeRemaining = -1;
                DequeueNextItem();
            }

            return true;
        }

        /// <summary>
        /// Cancels all production orders in the queue.
        /// </summary>
        public void CancelProduction()
        {
            queue.Clear();

            productionTimeRemaining = -1;
            CurrentProductionProgress = -1;
        }
    }

}