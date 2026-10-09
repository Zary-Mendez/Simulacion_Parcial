using UnityEngine;

//Estados que puede tener el entrenador
public enum TrainerState
{
    Exploring,
    SearchingGrass,
    Fighting,
    Capturing,
    Defeated,
    Winner
}

public class Trainer : MonoBehaviour
{
    [Header("Trainer Settings")]
    public float health = 20f;
    public float strength = 5f;
    public float speed = 2f;
    public float visionRange = 8f;
    public int pokeballs = 5;
    public int capturedPokemon = 0;
    public int requiredCaptures = 3;

    [Header("Movement Area")]
    public Vector2 areaSize = new Vector2(16f, 8f);

    [Header("Trainer State")]
    public TrainerState currentState = TrainerState.Exploring;

    private Vector3 destination;
    private Vector3 areaCenter;
    private TallGrass targetGrass;
    private WildPokemon targetPokemon;

    private void Start()
    {
        areaCenter = transform.position;
        SelectRandomDestination();
    }

    //Este método será llamado desde Simulate.cs
    public void Simulate(float h)
    {
        if (currentState == TrainerState.Defeated ||
            currentState == TrainerState.Winner)
        {
            return;
        }

        CheckState();

        switch (currentState)
        {
            case TrainerState.Exploring:
                Explore(h);
                break;

            case TrainerState.SearchingGrass:
                SearchGrass(h);
                break;

            case TrainerState.Fighting:
                Fight();
                break;

            case TrainerState.Capturing:
                CapturePokemon();
                break;
        }

        CheckState();
    }

    void Explore(float h)
    {
        //Busca la hierba alta disponible más cercana
        targetGrass = FindNearestGrass();

        if (targetGrass != null)
        {
            currentState = TrainerState.SearchingGrass;
            destination = targetGrass.transform.position;
            return;
        }

        //Si no hay hierba disponible, continúa explorando
        if (Vector3.Distance(transform.position, destination) < 0.1f)
        {
            SelectRandomDestination();
        }

        Move(h);
    }

    void SearchGrass(float h)
    {
        //Si la hierba ya no está disponible, busca otra
        if (targetGrass == null || !targetGrass.IsAvailable())
        {
            targetGrass = null;
            currentState = TrainerState.Exploring;
            return;
        }

        destination = targetGrass.transform.position;
        Move(h);
    }

    //La hierba alta llama este método cuando aparece un Pokémon
    public void BeginEncounter(WildPokemon wildPokemon)
    {
        if (wildPokemon == null)
        {
            return;
        }

        targetPokemon = wildPokemon;
        currentState = TrainerState.Fighting;

        Debug.Log(name + " encontró un Pokémon salvaje.");
    }

    void Fight()
    {
        //Si el Pokémon desapareció o huyó, vuelve a explorar
        if (targetPokemon == null || !targetPokemon.isActive)
        {
            targetPokemon = null;
            targetGrass = null;
            currentState = TrainerState.Exploring;
            return;
        }

        //Espera la decisión inicial del Pokémon
        if (targetPokemon.currentState == WildPokemonState.Waiting)
        {
            return;
        }

        //Si el Pokémon decide huir, termina el encuentro
        if (targetPokemon.currentState == WildPokemonState.Fleeing)
        {
            targetPokemon = null;
            targetGrass = null;
            currentState = TrainerState.Exploring;
            return;
        }

        float trainerResult = strength + Random.Range(0f, 6f);
        float pokemonResult = targetPokemon.strength + Random.Range(0f, 6f);

        if (trainerResult >= pokemonResult)
        {
            //Si gana la batalla, intenta capturarlo
            currentState = TrainerState.Capturing;
        }
        else
        {
            //Si pierde, recibe daño y el Pokémon escapa
            health -= targetPokemon.attackDamage;

            Debug.Log(name + " perdió la batalla. Vida restante: " + health);

            targetPokemon.Escape();
            targetPokemon = null;
            targetGrass = null;
            currentState = TrainerState.Exploring;
        }
    }

    void CapturePokemon()
    {
        if (targetPokemon == null || !targetPokemon.isActive)
        {
            targetPokemon = null;
            targetGrass = null;
            currentState = TrainerState.Exploring;
            return;
        }

        if (pokeballs <= 0)
        {
            currentState = TrainerState.Defeated;
            return;
        }

        pokeballs--;

        //La posibilidad de captura depende de la fuerza de ambos
        float captureChance = 0.65f +
            ((strength - targetPokemon.strength) * 0.05f);

        captureChance = Mathf.Clamp(captureChance, 0.2f, 0.95f);

        if (Random.value <= captureChance)
        {
            targetPokemon.Capture();
            capturedPokemon++;

            Debug.Log(
                name + " capturó un Pokémon. Total: " + capturedPokemon
            );
        }
        else
        {
            Debug.Log(name + " no pudo capturar al Pokémon.");

            targetPokemon.Escape();
        }

        targetPokemon = null;
        targetGrass = null;

        if (capturedPokemon >= requiredCaptures)
        {
            currentState = TrainerState.Winner;
            Debug.Log(name + " ganó la simulación.");
        }
        else
        {
            currentState = TrainerState.Exploring;
            SelectRandomDestination();
        }
    }

    void Move(float h)
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            destination,
            speed * h
        );
    }

    void SelectRandomDestination()
    {
        destination = new Vector3(
            Random.Range(
                areaCenter.x - areaSize.x / 2f,
                areaCenter.x + areaSize.x / 2f
            ),
            Random.Range(
                areaCenter.y - areaSize.y / 2f,
                areaCenter.y + areaSize.y / 2f
            ),
            transform.position.z
        );
    }

    TallGrass FindNearestGrass()
    {
        TallGrass[] allGrass = FindObjectsByType<TallGrass>(
            FindObjectsSortMode.InstanceID
        );

        TallGrass nearestGrass = null;
        float minimumDistance = Mathf.Infinity;

        foreach (TallGrass grass in allGrass)
        {
            if (!grass.IsAvailable())
            {
                continue;
            }

            float distance = Vector2.Distance(
                transform.position,
                grass.transform.position
            );

            if (distance < minimumDistance &&
                distance <= visionRange)
            {
                minimumDistance = distance;
                nearestGrass = grass;
            }
        }

        return nearestGrass;
    }

    void CheckState()
    {
        if (health <= 0f)
        {
            currentState = TrainerState.Defeated;
            Debug.Log(name + " fue derrotado.");
        }

        if (pokeballs <= 0 &&
            capturedPokemon < requiredCaptures &&
            targetPokemon == null)
        {
            currentState = TrainerState.Defeated;
            Debug.Log(name + " se quedó sin Pokébolas.");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(
            Application.isPlaying ? areaCenter : transform.position,
            new Vector3(areaSize.x, areaSize.y, 0f)
        );
    }
}