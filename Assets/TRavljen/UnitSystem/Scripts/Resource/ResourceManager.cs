using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnitSystem.Utility;
using System;

namespace UnitSystem
{

    /// <summary>
    /// Resource managment component, responsible for keeping track of current
    /// and maximal resources available for a player. Provides a set of methods
    /// that allow simple modification of resources.
    /// </summary>
    public class ResourceManager : MonoBehaviour
    {

        #region Properties
        [Header("Max Resources"), SerializeField,
            Tooltip("Specifies default resource capacity. For all resources specified in maxResources list, this value is ignored.")]
        private float commonMaxResourceQuantity = 20_000;

        /// <summary>
        /// Specifies default resource capacity. For all resources specified in
        /// <see cref="maxResources"/> list, this value is ignored.
        /// </summary>
        public float CommonMaxResourceQuantity => commonMaxResourceQuantity;

        /// <summary>
        /// Specifies if the max resources are ignored, therefore resources can
        /// store to "unlimited" amount. Then the resources amount is limited
        /// by <see cref="float.MaxValue"/>.
        /// </summary>
        [Tooltip("Specifies if the max resources are ignored.")]
        public bool UnlimitedResourcesQuantity = false;

        [SerializeField, Tooltip("Producables UUIDs defined in this array will be " +
            "ignored for maximal resource")]
        private List<string> unlimitedMaxProducables = new List<string>();

        /// <summary>
        /// This flag enables configuration of max resources per resource.
        /// If this is set to FALSE, resources will share a single storage
        /// and all resources combined should <c>NOT</c> exceed the common
        /// max storage (<see cref="CommonMaxResourceQuantity"/>).
        /// In such case to modify max common storage of all resources you need
        /// to adjust the <see cref="CommonMaxResourceQuantity"/> property by
        /// invoking <see cref="ModifyCommonMaxResourceQuantity(float)"/>.
        /// </summary>
        [SerializeField]
        public bool UseUniqueMaxResources = true;

        [SerializeField]
        protected List<ProducableQuantity> maxResources = new List<ProducableQuantity>();

        [Header("Resources")]
        [SerializeField, Tooltip("List of resources. Add through inspector for " +
            "default values.")]
        protected List<ProducableQuantity> resources = new List<ProducableQuantity>();

        [Header("Events")]
        /// <summary>
        /// Action invoked when resource was updated and its maximal capacity
        /// was reached.
        /// </summary>
        public Action<ProducableSO> OnResourceFull;

        /// <summary>
        /// Action invoked when resource quantity has changed.
        /// </summary>
        public Action<ProducableSO> OnResourceUpdate;

        /// <summary>
        /// Action invoked when maximal resource quantity has changed.
        /// </summary>
        public Action<ProducableSO> OnMaxResourceUpdate;

        #endregion

        #region Modify Resources

        /// <summary>
        /// Modifies existing resource quantity or adds a new one if none
        /// exists yet. Internally calls <see cref="AddResource(ProducableQuantity)"/>
        /// for each resource quantity.
        /// </summary>
        /// <param name="resources">Resources to be added.</param>
        /// <returns>
        /// Returns list of remaining resources. If this is empty, then
        /// all resources were added to the manager.
        /// </returns>
        public ProducableQuantity[] AddResources(ProducableQuantity[] resources)
        {
            List<ProducableQuantity> remainingResources =
                new List<ProducableQuantity>(resources);

            for (int index = resources.Length-1; index >= 0; index--)
            {
                var resource = resources[index];
                var remainder = AddResource(resource);

                if (remainder > 0)
                {
                    // Update resource quantity
                    resource.Quantity = remainder;
                    remainingResources[index] = resource;
                }
                else
                {
                    // Remove resource quantity from the list
                    remainingResources.RemoveAt(index);
                }
            }

            return remainingResources.ToArray();
        }

        /// <summary>
        /// Modifies existing resource quantity or adds a new one if none
        /// exists yet. Also invokes <see cref="OnResourceUpdate"/> after
        /// resource quantity is modified and invokes <see cref="OnResourceFull"/>
        /// when resource has reached its max capacity.
        /// </summary>
        /// <param name="resourceQuantity">Resource to be added.</param>
        /// <returns>
        /// Returns a value of remaining resource quantity. If this value is 0,
        /// then full quantity was applied to the manager.
        /// </returns>
        public float AddResource(ProducableQuantity resourceQuantity)
        {
            if (resourceQuantity.Quantity == 0) return 0;

            string resourceUUID = resourceQuantity.Producable.UUID;
            float maxQuantity = GetMaxResourceQuantity(resourceUUID);
            int index = resources.FindIndex(match => match.Producable.UUID == resourceQuantity.Producable.UUID);

            if (index == -1)
            {
                resources.Add(new ProducableQuantity(resourceQuantity.Producable, 0f));
                index = resources.Count - 1;
            }

            // Check if the quantity is unlimited
            if (maxQuantity == -1)
            {
                var resource = resources[index];
                resources[index] = new ProducableQuantity(resource.Producable, resource.Quantity + resourceQuantity.Quantity);
                return 0f;
            }

            float currentResourceQuantity = resources[index].Quantity;

            if (!UseUniqueMaxResources)
                maxQuantity -= GetResourcesQuantityForLimitedResources() - currentResourceQuantity;

            var newQuantity = currentResourceQuantity + resourceQuantity.Quantity;
            var newClampedQuantity = UnlimitedResourcesQuantity ? newQuantity : Mathf.Min(newQuantity, maxQuantity);

            resources[index] = new ProducableQuantity(
                resourceQuantity.Producable, newClampedQuantity);

            // Invoke resource update action when resource is updated
            OnResourceUpdate?.Invoke(resourceQuantity.Producable);

            float remainingResource = newQuantity - newClampedQuantity;

            // Check if resource is now FULL
            if (newClampedQuantity >= maxQuantity)
            {
                OnResourceFull?.Invoke(resourceQuantity.Producable);
                InfoLogger.Log("Resource reached maximum capacity/quantity: " + resourceQuantity.Producable.Name);
            }

            return remainingResource;
        }

        /// <summary>
        /// Removes the provided resource quantity from existing
        /// resource if there is enough of it.
        /// Also invokes <see cref="OnResourceUpdate"/> if resource
        /// was modified.
        /// </summary>
        /// <param name="resourceQuantity">Resource to remove from manager.</param>
        /// <returns>
        /// Returns 'false' if there is not enough resource available.
        /// </returns>
        public bool ConsumeResource(ProducableQuantity resourceQuantity)
        {
            for (int index = 0; index < resources.Count; index++)
            {
                var currentResourceQuantity = resources[index];

                if (currentResourceQuantity.Producable.UUID == resourceQuantity.Producable.UUID)
                {
                    var newQuantity = currentResourceQuantity.Quantity - resourceQuantity.Quantity;

                    // Check if there are enough resources present
                    if (newQuantity < 0)
                    {
                        InfoLogger.Log("Attempted to take more resource than available! " + resourceQuantity.Producable.Name);
                        break;
                    }

                    resources[index] = new ProducableQuantity(
                        currentResourceQuantity.Producable, newQuantity);

                    OnResourceUpdate?.Invoke(resourceQuantity.Producable);

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Removes the provided resources quantity from existing
        /// resources if there is enough resources available.
        /// Also invokes <see cref="OnResourceUpdate"/> for each resource
        /// that was modified.
        /// </summary>
        /// <param name="resourceQuantity">Resource to remove from manager.</param>
        /// <returns>
        /// Returns 'false' if there is not enough resources available.
        /// </returns>
        public bool ConsumeResources(List<ProducableQuantity> resourceQuantities)
        {
            if (HasEnoughResources(resourceQuantities))
            {
                resourceQuantities.ForEach(resource => ConsumeResource(resource));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Applies changes to current maximal resource quantity
        /// available in <see cref="maxResources"/>. If there
        /// is no existing maximal resource, then this quantity
        /// is simply set as initial value.
        /// </summary>
        /// <param name="resourceQuantity">Resource quantity used matching and modifying.</param>
        public void ModifyMaxResourceQuantity(ProducableQuantity resourceQuantity)
        {
            var resourceUUID = resourceQuantity.Producable.UUID;
            var index = maxResources.FindIndex(
                resource => resource.Producable.UUID == resourceUUID);

            if (index == -1)
            {
                maxResources.Add(resourceQuantity);
            }
            else
            {
                var maxResource = maxResources[index];
                maxResource.Quantity = Mathf.Max(0, resourceQuantity.Quantity + maxResource.Quantity);
                maxResources[index] = maxResource;
            }

            OnMaxResourceUpdate?.Invoke(resourceQuantity.Producable);
        }

        /// <summary>
        /// Modify <see cref="CommonMaxResourceQuantity"/>.
        /// </summary>
        /// <param name="quantity">Quantity to add</param>
        public void ModifyCommonMaxResourceQuantity(float quantity)
        {
            commonMaxResourceQuantity += quantity;
            OnMaxResourceUpdate?.Invoke(null);
        }

        /// <summary>
        /// Set the <see cref="CommonMaxResourceQuantity"/>.
        /// </summary>
        /// <param name="newQuantity">New quantity</param>
        public void SetCommonMaxResourceQuantity(float newQuantity)
        {
            commonMaxResourceQuantity = newQuantity;
            OnMaxResourceUpdate?.Invoke(null);
        }

        /// <summary>
        /// Setting maximal resource quantity by overriding existing one,
        /// or adding a new one if one does not exist.
        /// </summary>
        /// <param name="resourceQuantity">Overriding maximal resource quantity.</param>
        public void SetMaxResourceQuantity(ProducableQuantity resourceQuantity)
        {
            string resourceUUID = resourceQuantity.Producable.UUID;
            int index = GetIndexFromList(resourceQuantity.Producable.UUID, maxResources);

            if (index != -1)
            {
                maxResources[index] = resourceQuantity;
            }
            else
            {
                maxResources.Add(resourceQuantity);
            }

            OnMaxResourceUpdate?.Invoke(resourceQuantity.Producable);
        }

        /// <summary>
        /// Setting current resource quantity by overriding existing one,
        /// or adding a new one if one does not exist.
        /// </summary>
        /// <param name="resourceQuantity">Overriding resource quantity.</param>
        public void SetResourceQuantity(ProducableQuantity resourceQuantity)
        {
            var resourceUUID = resourceQuantity.Producable.UUID;
            var index = resources.FindIndex(
                resource => resource.Producable.UUID == resourceUUID);

            if (index != -1)
            {
                resources[index] = resourceQuantity;
            }
            else
            {
                resources.Add(resourceQuantity);
            }

            OnResourceUpdate?.Invoke(resourceQuantity.Producable);
        }

        #endregion

        #region Calculate Resources

        /// <summary>
        /// Compares the quantity of the currently available resource.
        /// </summary>
        /// <param name="resource">Resource used for matching</param>
        /// <param name="quantity">Quantity used for comparison</param>
        /// <returns>Returns 'false' if there is not enough resource available.</returns>
        public bool HasEnoughStorage(ProducableSO resource, float quantity)
        {
            float maxResource = GetMaxResourceQuantity(resource.UUID);

            for (int index = 0; index < resources.Count; index++)
            {
                var currentResourceQuantity = resources[index];

                if (currentResourceQuantity.Producable.UUID == resource.UUID)
                {
                    return currentResourceQuantity.Quantity + quantity <= maxResource;
                }
            }

            // Not entry of the resource yet
            return quantity <= maxResource;
        }

        /// <summary>
        /// Compares the quantity of the currently available resource.
        /// </summary>
        /// <param name="resourceQuantity">Resource and its quantity used for matching.</param>
        /// <returns>Returns 'false' if there is not enough resource available.</returns>
        public bool HasEnoughResource(ProducableQuantity resourceQuantity)
        {
            for (int index = 0; index < resources.Count; index++)
            {
                var currentResourceQuantity = resources[index];

                if (currentResourceQuantity.Producable.UUID == resourceQuantity.Producable.UUID)
                {
                    return currentResourceQuantity.Quantity >= resourceQuantity.Quantity;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks for quantity of the resources available.
        /// </summary>
        /// <param name="resources">Resources and its quantities used for matching.</param>
        /// <returns>Returns 'false' if there is not enough resources available.</returns>
        public bool HasEnoughResources(List<ProducableQuantity> resources)
        {
            var unfulfilledResourceCost = resources
                .FindAll(resource => !HasEnoughResource(resource));
            return unfulfilledResourceCost.Count == 0;
        }

        /// <summary>
        /// Retrieves the current maximal resource quantity for the provided
        /// resource UUID, or the default value (<see cref="CommonMaxResourceQuantity"/>)
        /// when <see cref="UseUniqueMaxResources"/> is set to FALSE or the
        /// custom max resource is not set for this resource.
        /// </summary>
        /// <param name="resourceUUID">UUID used for matching with <see cref="maxResources"/></param>
        /// <returns>Returns the max resource quantity allowed for this resource manager.</returns>
        public float GetMaxResourceQuantity(string resourceUUID)
        {
            // Check if all resources are unlimited or perhaps this one is specified.
            if (UnlimitedResourcesQuantity) return -1;
            if (unlimitedMaxProducables.Contains(resourceUUID)) return -1;

            // If unique resources are not used, take common quantity.
            if (!UseUniqueMaxResources) return commonMaxResourceQuantity;

            foreach (var resource in maxResources)
            {
                if (resource.Producable.UUID == resourceUUID)
                {
                    return resource.Quantity;
                }
            }

            return commonMaxResourceQuantity;
        }

        /// <summary>
        /// Returns the current resource quantity for the provided
        /// resource UUID.
        /// </summary>
        /// <param name="resourceUUID">UUID used for matching the resource</param>
        /// <returns>Returns the current resource quantity for the given UUID.</returns>
        public float GetResourceQuantity(string resourceUUID)
        {
            foreach (var resource in resources)
            {
                var producable = resource.Producable;
                if (producable != null && producable.UUID == resourceUUID)
                    return resource.Quantity;
            }

            return 0;
        }

        /// <summary>
        /// Checks if the resource is maxed out based on its maximal capacity.
        /// </summary>
        public bool IsResourceMaxedOut(string resourceUUID)
        {
            if (UnlimitedResourcesQuantity) return false;
            if (unlimitedMaxProducables.Contains(resourceUUID)) return false;

            if (UseUniqueMaxResources)
            {
                // If this gets below zero, then we still have some space.
                return GetResourceQuantity(resourceUUID) - GetMaxResourceQuantity(resourceUUID) >= 0;
            }

            return GetResourcesQuantityForLimitedResources() >= commonMaxResourceQuantity;
        }

        /// <summary>
        /// Calculates the sum of all resources that are not unlimited in capacity.
        /// This is generally useful when these resources share a common storage.
        /// </summary>
        /// <returns>Returns sum of all limited resources.</returns>
        public float GetResourcesQuantityForLimitedResources()
        {
            float groupQuantity = 0;

            foreach (var resource in resources)
            {
                // Ignore unlimited producables
                if (unlimitedMaxProducables.Contains(resource.Producable.UUID))
                    continue;

                groupQuantity += resource.Quantity;
            }

            return groupQuantity;
        }

        #endregion

        #region Convenience

        /// <summary>
        /// Current resources.
        /// <para>
        /// This quantity can be set below 0 and will generally behave
        /// as it would with 0. This cannot happen with taking resources, but
        /// only with <see cref="SetResourceQuantity(ProducableQuantity)"/>
        /// method if this is ever desired.
        /// </para>
        /// </summary>
        public ProducableQuantity[] GetResources() => resources.ToArray();

        /// <summary>
        /// Current maximal resources. If resource is not specified here,
        /// default value is used (<see cref="CommonMaxResourceQuantity"/>) for
        /// maximal storage of the resource.
        /// </summary>
        public ProducableQuantity[] GetMaxResources() => maxResources.ToArray();

        /// <summary>
        /// Finds the index of the specified producableUUID.
        /// </summary>
        /// <param name="producableUUID">UUID of the producable</param>
        /// <param name="producables">List of producables on which the index
        /// will be searched</param>
        /// <returns>Returns index of the producable. If index is negative (-1)
        /// then the producable was not found the in the list.</returns>
        private int GetIndexFromList(string producableUUID, List<ProducableQuantity> producables)
        {
            for(int index = 0; index < producables.Count; index++)
            {
                if (producables[index].Producable.UUID == producableUUID)
                    return index;
            }

            return -1;
        }

        #endregion

    }

}