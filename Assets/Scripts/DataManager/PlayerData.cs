using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerData
{
    // Movement
    public float moveSpeed;
    public float acceleration;
    public float deceleration;
    public float slideFactor;

    // Dash
    public float dashSpeed;
    public int dashIframes;
    public float dashSlideDuration;
    public float dashDuration;
    public float dashCooldown;

    // Attack
    public int baseDamage;


    // Health
    public float maxHealth;
    public float currentHealth;
    public float regenerationRate;
    public float regenDelay;
    public float invulnerableTime;

    // Transform
    public Vector2 position;

    // PowerUps (referencias directas)
    public List<PowerUp> initialPowerUps = new();
}