using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnitSystem;
using UnityEngine.EventSystems;
using UnitSystem.Utility;

namespace UnitSystem.Demo
{

    /// <summary>
    /// Simple selection component used for detecting selectable layers and
    /// either reseting selection with ground layer or specifying target
    /// position for selected unit if selected unit contains component that
    /// implements <see cref="IControlUnit"/>.
    /// </summary>
    public class UnitSelector : MonoBehaviour
    {

        #region Properties

        /// <summary>
        /// Specifies layers used for detecting clicks on game objects.
        /// </summary>
        public LayerMask SelectionLayers;

        /// <summary>
        /// Specifies ground layer for resetting selection.
        /// </summary>
        public LayerMask GroundLayer;

        /// <summary>
        /// Currently selected unit.
        /// </summary>
        public Unit SelectedUnit;

        /// <summary>
        /// Action invoked when unit selection changes.
        /// </summary>
        public System.Action<Unit> OnUnitSelectionChange;

        /// <summary>
        /// Specifies the camera of the player.
        /// </summary>
        private Camera playerCamera;

        #endregion

        private void Start()
        {
            playerCamera = Camera.main;
        }

        // Update is called once per frame
        void Update()
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            // Check for unit selection (left mouse click)
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                HandlePrimaryMouseButtonDown();
            }
            // Check for selected unit action (right mouse click)
            else if (Input.GetKeyDown(KeyCode.Mouse1) && SelectedUnit)
            {
                HandleSecondaryMouseButtonDown();
            }
        }

        public void SetSelectedUnit(Unit unit)
        {
            SelectedUnit = unit;
            OnUnitSelectionChange?.Invoke(unit);
        }

        private void HandlePrimaryMouseButtonDown()
        {
            RaycastHit hit;
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, SelectionLayers))
            {
                if (hit.transform.TryGetComponent(out Unit unit))
                {
                    InfoLogger.Log("Selected unit: " + unit);
                    SetSelectedUnit(unit);
                    return;
                }
            }

            SetSelectedUnit(null);
        }

        private void HandleSecondaryMouseButtonDown()
        {
            RaycastHit hit;
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, GroundLayer))
            {
                // Get spawn point from selected unit GameObject or its children.
                var spawnPoint = SelectedUnit.GetComponent<IControlUnit>();
                if (spawnPoint != null)
                {
                    spawnPoint.SetTargetPosition(hit.point);
                }
            }
        }

    }

}