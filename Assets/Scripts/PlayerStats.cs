using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int cannonCount = 1;
    public float sailSpeedBonus = 0f;
    public float armourBonus = 0f;

    private PlayerMovement playerMovement;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    public float GetMoveSpeed()
    {
        return playerMovement != null ?
               playerMovement.maxSpeed + sailSpeedBonus :
               5f + sailSpeedBonus;
    }

    public float GetRotationSpeed()
    {
        return playerMovement != null ?
               playerMovement.rotationSpeed + sailSpeedBonus * 10f :
               150f;
    }

    public void AddCannon()
    {
        cannonCount++;
    }

    public void AddSails(float amount)
    {
        sailSpeedBonus += amount;
    }

    public void AddArmour(float amount)
    {
        armourBonus += amount;
        Health health = GetComponent<Health>();
        if (health != null)
        {
            health.maxHealth += amount;
            health.currentHealth += amount;
        }
    }
}