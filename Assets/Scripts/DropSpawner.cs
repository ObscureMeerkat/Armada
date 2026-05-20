using UnityEngine;

public class DropSpawner : MonoBehaviour
{
    [Header("Drop Prefabs")]
    public GameObject dropCannon;
    public GameObject dropSails;
    public GameObject dropArmour;

    // Weights: Cannon=5, Sails=10, Armour=20, Nothing=65
    private int weightCannon = 5;
    private int weightSails = 10;
    private int weightArmour = 20;
    private int weightNothing = 65;

    public void TrySpawnDrop(Vector2 position)
    {
        int total = weightCannon + weightSails + weightArmour + weightNothing;
        int roll = Random.Range(0, total);

        if (roll < weightCannon)
        {
            Instantiate(dropCannon, position, Quaternion.identity);
        }
        else if (roll < weightCannon + weightSails)
        {
            Instantiate(dropSails, position, Quaternion.identity);
        }
        else if (roll < weightCannon + weightSails + weightArmour)
        {
            Instantiate(dropArmour, position, Quaternion.identity);
        }
        // else nothing drops
    }
}