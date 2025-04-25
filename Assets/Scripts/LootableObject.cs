using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LootableObject : MonoBehaviour
{
    // Loot properties
    [System.Serializable]
    public class LootItem
    {
        public string itemName;
        public Sprite itemSprite;
        [Range(0, 100)]
        public int dropChance = 100; // Percentage chance to drop
    }
    
    // Inspector configurable properties
    public List<LootItem> possibleLoot = new List<LootItem>();
    public float searchTime = 2f;
    public bool destroyAfterLooting = false;
    public Sprite openedSprite; // For containers that change appearance after being looted
    
    // Visual feedback
    public GameObject searchingEffect; // Optional particle effect or animation
    public AudioClip searchSound; // Optional sound effect

    // Internal state
    private bool isBeingSearched = false;
    private bool hasBeenLooted = false;
    private SpriteRenderer spriteRenderer;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    public void StartLoot()
    {
        // Only allow looting if not already being searched and hasn't been looted
        if (!isBeingSearched && !hasBeenLooted)
        {
            isBeingSearched = true;
            StartCoroutine(LootCoroutine());
        }
        else if (hasBeenLooted)
        {
            // Show feedback that object has already been looted
            Debug.Log(gameObject.name + " has already been searched.");
            // You could display UI text here
        }
    }
    
    IEnumerator LootCoroutine()
    {
        // Start searching animation or effect
        if (searchingEffect != null)
        {
            GameObject effect = Instantiate(searchingEffect, transform.position, Quaternion.identity);
            Destroy(effect, searchTime + 0.5f);
        }
        
        // Play sound effect
        if (searchSound != null)
        {
            AudioSource.PlayClipAtPoint(searchSound, transform.position);
        }
        
        // Wait for search time
        yield return new WaitForSeconds(searchTime);
        
        // Give loot to player
        GiveLoot();
        
        // Mark as looted
        hasBeenLooted = true;
        isBeingSearched = false;
        
        // Change appearance if needed
        if (openedSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = openedSprite;
        }
        
        // Destroy if set to do so
        if (destroyAfterLooting)
        {
            Destroy(gameObject, 0.5f); // Small delay so player can see it being destroyed
        }
    }
    
    void GiveLoot()
    {
        // Determine which items to give based on drop chance
        List<LootItem> itemsToGive = new List<LootItem>();
        
        foreach (LootItem item in possibleLoot)
        {
            // Roll for drop chance
            if (Random.Range(0, 100) < item.dropChance)
            {
                itemsToGive.Add(item);
            }
        }
        
        // If nothing was selected, don't give any loot
        if (itemsToGive.Count == 0)
        {
            Debug.Log("Player found nothing in " + gameObject.name);
            // You could display UI text here
            return;
        }
        
        // Log the items found (replace with actual inventory system later)
        string itemsFound = "Player found: ";
        foreach (LootItem item in itemsToGive)
        {
            itemsFound += item.itemName + ", ";
            
            // Here you would add the item to the player's inventory
            // For example:
            // PlayerInventory.AddItem(item.itemName, item.itemSprite);
        }
        
        // Remove trailing comma and space
        itemsFound = itemsFound.TrimEnd(' ', ',');
        Debug.Log(itemsFound);
        
        // You could display UI text here
    }
    
    // For visualization in the Editor
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}