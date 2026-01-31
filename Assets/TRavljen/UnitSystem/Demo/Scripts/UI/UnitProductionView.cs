using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnitSystem;

namespace UnitSystem.Demo
{

    /// <summary>
    /// Basic DEMO view component for controlling visual elements
    /// of the production unit action (<see cref="ProductionAction"/>).
    /// </summary>
    public class UnitProductionView : MonoBehaviour
    {

        public enum ProductionActionButtonState
        {
            /// <summary>
            /// Production is enabled.
            /// </summary>
            Default,
            /// <summary>
            /// Production is completed.
            /// </summary>
            Completed,
            /// <summary>
            /// Production is in progress and may be canceled. For
            /// demo this will only happen on ResearchSO producable types.
            /// </summary>
            Cancellable,
            /// <summary>
            /// Production is disabled, does not meet requirements (Cost excluded).
            /// </summary>
            Disabled
        }

        #region Properties

        [SerializeField]
        private Text titleText = null;

        [SerializeField]
        private Text descriptionText = null;

        [SerializeField]
        private Button actionButton = null;

        private ProductionAction action;

        public System.Action<ProductionAction> OnActionButtonClicked;

        #endregion

        #region Lifecycle

        /// <summary>
        /// Configure UI with action and its state.
        /// </summary>
        /// <param name="action">Production action to be displayed.</param>
        /// <param name="buttonState">Button state of the action.</param>
        public void Configure(ProductionAction action, ProductionActionButtonState buttonState)
        {
            this.action = action;

            ProducableSO producable = action.Producable;

            if (action.Quantity > 1)
            {
                titleText.text = (int)action.Quantity + " " + producable.name + "s";
            }
            else
            {
                titleText.text = producable.name;
            }

            var description = "Requirements: \n";

            foreach (ProducableQuantity requirement in producable.Requirements)
            {
                description += "- " + requirement.Producable.name + "\n";
            }

            foreach (ProducableQuantity price in producable.Cost)
            {
                description += "- " + price.Quantity * action.Quantity + " " + price.Producable.name + "\n";
            }

            descriptionText.text = description;

            actionButton
                .GetComponentInChildren<Text>()
                .text = GetActionButtonText(producable, buttonState);

            actionButton.interactable = buttonState == ProductionActionButtonState.Default || buttonState == ProductionActionButtonState.Cancellable;
        }

        #endregion

        #region Actions

        private string GetActionButtonText(ProducableSO producable, ProductionActionButtonState buttonState)
        {
            switch (buttonState)
            {
                case ProductionActionButtonState.Cancellable: return "Cancel";
                case ProductionActionButtonState.Disabled: return "Doesn't meet requirements";
                case ProductionActionButtonState.Completed: return "Researched";
                case ProductionActionButtonState.Default:
                    switch (producable)
                    {
                        case AUnitSO _: return "Train";
                        case ResearchSO _: return "Research";
                        default: return "Not defined";
                    }
                default: return null;
            }
        }

        public void ActionButtonClicked()
        {
            OnActionButtonClicked?.Invoke(action);
        }

        #endregion

    }

}