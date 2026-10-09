using System.Collections.Generic;
using UnityEngine;

public class Simulate : MonoBehaviour
{
    [Header("Simulation Settings")]
    public float secondsPerIteration = 1f;

    [Header("Simulation Entities")]
    public List<Trainer> trainers =
        new List<Trainer>();

    public List<WildPokemon> wildPokemons =
        new List<WildPokemon>();

    public List<TallGrass> tallGrassZones =
        new List<TallGrass>();

    private float time = 0f;

    private void Start()
    {
        RefreshEntities();
    }

    private void Update()
    {
        time += Time.deltaTime;

        if (time >= secondsPerIteration)
        {
            time = 0f;
            RunSimulation();
        }
    }

    void RunSimulation()
    {
        //Actualiza las entidades existentes
        RefreshEntities();

        //Simula todas las zonas de hierba alta
        foreach (TallGrass grass in tallGrassZones)
        {
            if (grass != null)
            {
                grass.Simulate(
                    secondsPerIteration
                );
            }
        }

        //Vuelve a buscar Pokémones porque la hierba
        //puede haber creado uno nuevo
        RefreshWildPokemon();

        //Simula todos los Pokémones salvajes
        foreach (WildPokemon pokemon in wildPokemons)
        {
            if (pokemon != null &&
                pokemon.isActive)
            {
                pokemon.Simulate(
                    secondsPerIteration
                );
            }
        }

        //Simula todos los entrenadores
        foreach (Trainer trainer in trainers)
        {
            if (trainer != null)
            {
                trainer.Simulate(
                    secondsPerIteration
                );
            }
        }
    }

    void RefreshEntities()
    {
        Trainer[] foundTrainers =
            FindObjectsByType<Trainer>(
                FindObjectsSortMode.InstanceID
            );

        trainers =
            new List<Trainer>(foundTrainers);

        TallGrass[] foundGrass =
            FindObjectsByType<TallGrass>(
                FindObjectsSortMode.InstanceID
            );

        tallGrassZones =
            new List<TallGrass>(foundGrass);

        RefreshWildPokemon();
    }

    void RefreshWildPokemon()
    {
        WildPokemon[] foundPokemon =
            FindObjectsByType<WildPokemon>(
                FindObjectsSortMode.InstanceID
            );

        wildPokemons =
            new List<WildPokemon>(foundPokemon);
    }
}