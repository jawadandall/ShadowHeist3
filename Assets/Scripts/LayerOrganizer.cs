using UnityEngine;

namespace ShadowHeist
{
    /// <summary>
    /// Utility script to help organize game objects into appropriate layers
    /// </summary>
    public class LayerOrganizer : MonoBehaviour
    {
        [Header("Layer Organization")]
        public bool organizeOnStart = false;

        // Unity Layer Constants
        private const string LAYER_DEFAULT = "Default";
        private const string LAYER_PLAYER = "Player";
        private const string LAYER_ENVIRONMENT = "Environment";
        private const string LAYER_ENEMIES = "Enemies";
        private const string LAYER_INTERACTABLES = "Interactables";
        private const string LAYER_NAVIGATION = "Navigation";
        private const string LAYER_PROPS = "Props";
        private const string LAYER_TRIGGERS = "Triggers";
        private const string LAYER_EFFECTS = "Effects";
        private const string LAYER_LIGHTING = "Lighting";

        private void Start()
        {
            if (organizeOnStart)
            {
                OrganizeLayers();
            }
        }

        /// <summary>
        /// Organizes all game objects into appropriate layers
        /// </summary>
        public void OrganizeLayers()
        {
            Debug.Log("Organizing scene objects into layers...");

            // Organize Environment objects
            GameObject environmentRoot = GameObject.Find("Environment");
            if (environmentRoot != null)
            {
                SetLayerRecursively(environmentRoot, LayerMask.NameToLayer(LAYER_ENVIRONMENT));
                
                // Find Desks and set to Props layer
                Transform desksTransform = environmentRoot.transform.Find("Desks");
                if (desksTransform != null)
                {
                    SetLayerRecursively(desksTransform.gameObject, LayerMask.NameToLayer(LAYER_PROPS));
                }
            }

            // Organize Player objects
            GameObject playerObj = GameObject.Find("Player");
            if (playerObj != null)
            {
                SetLayerRecursively(playerObj, LayerMask.NameToLayer(LAYER_PLAYER));
            }

            // Organize Navigation objects
            GameObject navigationRoot = GameObject.Find("Navigation");
            if (navigationRoot != null)
            {
                SetLayerRecursively(navigationRoot, LayerMask.NameToLayer(LAYER_NAVIGATION));
            }

            // Organize Enemies objects
            GameObject enemiesRoot = GameObject.Find("Enemies");
            if (enemiesRoot != null)
            {
                SetLayerRecursively(enemiesRoot, LayerMask.NameToLayer(LAYER_ENEMIES));
            }

            // Organize Camera objects (keeping them in Default layer)
            GameObject cameraObj = GameObject.Find("Camera");
            if (cameraObj != null)
            {
                // Cameras typically stay in the Default layer for proper rendering
                // But you can change this if needed
            }

            // Organize CinemachineCamera (keeping in Default layer)
            GameObject cinemachineObj = GameObject.Find("CinemachineCamera");
            if (cinemachineObj != null)
            {
                // Cinemachine cameras typically stay in the Default layer
                // But you can change this if needed
            }

            Debug.Log("Layer organization complete!");
        }

        /// <summary>
        /// Sets the layer of the given game object and all its children recursively
        /// </summary>
        /// <param name="obj">The root game object</param>
        /// <param name="layer">The layer to assign</param>
        private void SetLayerRecursively(GameObject obj, int layer)
        {
            if (obj == null) return;

            obj.layer = layer;

            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }

        /// <summary>
        /// Editor utility to organize layers from the Inspector
        /// </summary>
        [ContextMenu("Organize Layers")]
        private void OrganizeLayersEditor()
        {
            OrganizeLayers();
        }
    }
}
