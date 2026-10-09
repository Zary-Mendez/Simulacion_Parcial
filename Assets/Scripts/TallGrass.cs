using UnityEngine;

public enum TallGrassState
{
    Available,
    EncounterActive,
    Cooldown
}

public class TallGrass : MonoBehaviour
{
    [Header("Tall Grass Settings")]
    public GameObject wildPokemonPrefab;
    public float encounterRadius = 1f;
    [Range(0f, 1f)]
    public float encounterChance = 0.7f;
    public float cooldownDuration = 5f;

    [Header("Tall Grass State")]
    public TallGrassState currentState = TallGrassState.Available;

    private float cooldownTimer = 0f;
    private WildPokemon activePokemon;

    public void Simulate(float h)
    {
        switch (currentState)
        {
            case TallGrassState.Available:
                SearchTrainer();
                break;

            case TallGrassState.EncounterActive:
                // Si el Pokémon ya no existe, comienza el tiempo de espera.
                if (activePokemon == null)
                {
                    StartCooldown();
                }
                break;

            case TallGrassState.Cooldown:
                cooldownTimer -= h;

                if (cooldownTimer <= 0f)
                {
                    currentState = TallGrassState.Available;
                }
                break;
        }
    }

    void SearchTrainer()
    {
        Trainer[] trainers = FindObjectsByType<Trainer>(
            FindObjectsSortMode.InstanceID
        );

        foreach (Trainer trainer in trainers)
        {
            if (trainer == null)
            {
                continue;
            }

            // No busca entrenadores que ya terminaron o tienen un encuentro.
            if (trainer.currentState == TrainerState.Defeated ||
                trainer.currentState == TrainerState.Winner ||
                trainer.currentState == TrainerState.Fighting ||
                trainer.currentState == TrainerState.Capturing)
            {
                continue;
            }

            float distance = Vector2.Distance(
                transform.position,
                trainer.transform.position
            );

            if (distance <= encounterRadius)
            {
                TryCreateEncounter(trainer);
                return;
            }
        }
    }

    void TryCreateEncounter(Trainer trainer)
    {
        // Determina aleatoriamente si aparece un Pokémon.
        if (Random.value > encounterChance)
        {
            StartCooldown();
            return;
        }

        if (wildPokemonPrefab == null)
        {
            Debug.LogWarning("No se ha asignado el prefab del Pokémon.");
            StartCooldown();
            return;
        }

        // Hace que el Pokémon aparezca al lado de la hierba
        // para que no quede debajo del entrenador.
        Vector3 spawnPosition = transform.position +
                                new Vector3(1.2f, -0.2f, 0f);

        GameObject newPokemon = Instantiate(
            wildPokemonPrefab,
            spawnPosition,
            Quaternion.identity
        );

        // Coloca el Pokémon delante de la hierba y del entrenador.
        SpriteRenderer pokemonRenderer =
            newPokemon.GetComponent<SpriteRenderer>();

        if (pokemonRenderer != null)
        {
            pokemonRenderer.sortingOrder = 2;
        }

        activePokemon = newPokemon.GetComponent<WildPokemon>();

        if (activePokemon != null)
        {
            currentState = TallGrassState.EncounterActive;
            activePokemon.isActive = true;
            activePokemon.Initialize(this, trainer);
            trainer.BeginEncounter(activePokemon);
        }
        else
        {
            Debug.LogWarning(
                "El prefab no contiene el componente WildPokemon."
            );

            Destroy(newPokemon);
            StartCooldown();
        }
    }

    public void FinishEncounter()
    {
        activePokemon = null;
        StartCooldown();
    }

    void StartCooldown()
    {
        currentState = TallGrassState.Cooldown;
        cooldownTimer = cooldownDuration;
    }

    public bool IsAvailable()
    {
        return currentState == TallGrassState.Available;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, encounterRadius);
    }
}