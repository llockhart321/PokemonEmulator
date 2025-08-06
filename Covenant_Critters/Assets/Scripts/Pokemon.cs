using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPokemon", menuName = "Pokemon/Create New Pokemon")]
public class Pokemon : ScriptableObject
{

    [Serializable]
    public class Attack
    {
        public string attackName;
        public float amount;
        public int type; // type of attack. 1. standard damage. 2. rasie your damage stat 3. raise your defense.  4 lower their attack  5. lower their defense

    }

    public string pokeName;
    public float baseHP;
    public float baseAttack;
    public float baseDefense;
    public string typeClass; // the class of pokemon. water,fire,grass,normal


    //for battle
    public Sprite frontSprite;
    public Sprite backSprite;
    //for inentory
    public Sprite inventorySprite;

    public List<Attack> attacks; // List  Attack objects


    

}

