using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PokemonInstance
{
    public Pokemon basePokemon;
    public int level;
    [HideInInspector] public float experience;
    public float currentHP;
    
    // Additional properties for uniqueness
    [HideInInspector] public int uniqueID;
    public string nickname;

    

    // things that dont change from instantiantion
    
    [HideInInspector] public float maxHP;
    //for battle
    [HideInInspector] public Sprite frontSprite;
    [HideInInspector] public Sprite backSprite;
    //for inentory
    [HideInInspector] public Sprite inventorySprite;
    [HideInInspector] public List<Pokemon.Attack> attacks;
    [HideInInspector] public string typeClass;
    [HideInInspector] public float attack;
    [HideInInspector] public float defense;
    public float attackMod =0.0f;
    public float defenseMod =0.0f;
    private float XPtoLevel = 100;

    // Constructor for creating a new Pokemon instance
    public PokemonInstance(Pokemon basePokemon, int level)
    {
        this.basePokemon = basePokemon;
        this.level = Mathf.Clamp(level, 1, 100); // Ensure level is between 1-100
        
        // Calculate base stats
        this.maxHP = (basePokemon.baseHP);//* (float)(level/10.0f));
        this.attack = (basePokemon.baseAttack);// * (float)(level/10.0f));
        this.defense = (basePokemon.baseDefense);//* (float)(level/10.0f));
        
        // Generate a unique ID
        this.uniqueID = System.Guid.NewGuid().GetHashCode();
        this.nickname = basePokemon.pokeName;

        this.frontSprite = basePokemon.frontSprite;
        this.backSprite = basePokemon.backSprite;
        this.inventorySprite = basePokemon.inventorySprite;
        this.typeClass = basePokemon.typeClass;
        this.attacks = basePokemon.attacks;
        
        this.currentHP = this.maxHP;
        //Debug.Log("why: "+this.currentHP);
        

    }
    
    public float getCurrentHP(){
        return this.currentHP;
    }

    
    // Method to level up
    public void LevelUp()
    {
        //reset level xp
        this.experience = this.experience - this.XPtoLevel;
        this.XPtoLevel += (this.level * 12f);

        //increase attack
        this.attack = this.attack * 1.1f;

        //increse defense
        this.defense = this.defense * 1.1f;

        this.level ++;

    }
    
    // Set nickname
    public void SetNickname(string newName)
    {
        if (!string.IsNullOrEmpty(newName))
        {
            nickname = newName;
        }
    }

    public void AwardExperiencePoints(){
        // after completing battle get xp


    }

    public void CheckLevelUp(){
        //idfk 
        while(this.experience >= XPtoLevel){
            LevelUp();
        }
    }


}