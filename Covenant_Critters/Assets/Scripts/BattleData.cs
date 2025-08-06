using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class to store battle information between scenes
[System.Serializable]
public class BattleData
{
    public List<PokemonInstance> enemyPokemon;
    public bool isTrainerBattle;
    public string trainerName;
    public Sprite trainerSprite;

    public BattleData(List<PokemonInstance> enemyPokemon, bool isTrainerBattle = false, string trainerName = "", Sprite trainerSprite = null)
    {
        this.enemyPokemon = enemyPokemon;
        this.isTrainerBattle = isTrainerBattle;
        this.trainerName = trainerName;
        this.trainerSprite = trainerSprite;
    }

    public void Reset()
    {
        // i used to have these commeneted out so check back here if there are issues.
        enemyPokemon = null;
        isTrainerBattle = false;
        trainerName = "";
        trainerSprite = null;
    }
}