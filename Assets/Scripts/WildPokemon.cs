using UnityEngine;

//Estados que puede tener un Pokémon salvaje
public enum WildPokemonState
{
    Waiting,
    Fighting,
    Fleeing,
    Captured
}

public class WildPokemon : MonoBehaviour
{
    [Header("Pokemon Appearance")]
    public Sprite[] pokemonSprites;

    [Header("Pokemon Settings")]
    public float health = 10f;
    public float strength = 5f;
    public float attackDamage = 3f;
    public float speed = 2f;
    public float fleeChance = 0.3f;
    public bool isActive = true;

    [Header("Random Statistics")]
    public float minimumStrength = 2f;
    public float maximumStrength = 8f;

    [Header("Pokemon State")]
    public WildPokemonState currentState =
        WildPokemonState.Waiting;

    private TallGrass originGrass;
    private Trainer opponent;
    private Vector3 fleeDirection;
    private float fleeTimer = 0f;
    private bool hasDecided = false;

    //Este método se ejecuta cuando aparece en la hierba
    public void Initialize(
        TallGrass grass,
        Trainer trainer
    )
    {
        originGrass = grass;
        opponent = trainer;

        //Genera características aleatorias
        strength = Random.Range(
            minimumStrength,
            maximumStrength
        );

        attackDamage =
            Mathf.Max(1f, strength * 0.5f);

        fleeChance =
            Random.Range(0.2f, 0.5f);

        currentState =
            WildPokemonState.Waiting;

        isActive = true;
        hasDecided = false;

        SelectRandomPokemonSprite();
    }

    void SelectRandomPokemonSprite()
    {
        SpriteRenderer spriteRenderer =
            GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogWarning(
                "El Pokémon no tiene SpriteRenderer."
            );

            return;
        }

        if (pokemonSprites == null ||
            pokemonSprites.Length == 0)
        {
            Debug.LogWarning(
                "No se asignaron imágenes de Pokémon."
            );

            return;
        }

        int randomIndex = Random.Range(
            0,
            pokemonSprites.Length
        );

        Sprite selectedSprite =
            pokemonSprites[randomIndex];

        spriteRenderer.sprite =
            selectedSprite;

        //Conserva los colores originales
        spriteRenderer.color =
            Color.white;

        //Cambia el nombre del objeto por el Pokémon elegido
        gameObject.name =
            selectedSprite.name + "_Wild";
    }

    //Este método será llamado desde Simulate.cs
    public void Simulate(float h)
    {
        if (!isActive)
        {
            return;
        }

        switch (currentState)
        {
            case WildPokemonState.Waiting:
                DecideAction();
                break;

            case WildPokemonState.Fighting:
                //Espera a que el entrenador
                //resuelva la batalla
                break;

            case WildPokemonState.Fleeing:
                Flee(h);
                break;

            case WildPokemonState.Captured:
                break;
        }
    }

    void DecideAction()
    {
        if (hasDecided)
        {
            return;
        }

        hasDecided = true;

        //El Pokémon decide aleatoriamente
        //si huye o pelea
        if (Random.value <= fleeChance)
        {
            Escape();
        }
        else
        {
            currentState =
                WildPokemonState.Fighting;

            Debug.Log(
                name +
                " decidió pelear contra el entrenador."
            );
        }
    }

    public void Escape()
    {
        if (!isActive ||
            currentState ==
                WildPokemonState.Captured)
        {
            return;
        }

        currentState =
            WildPokemonState.Fleeing;

        fleeTimer = 2f;

        if (opponent != null)
        {
            fleeDirection = (
                transform.position -
                opponent.transform.position
            ).normalized;
        }
        else
        {
            fleeDirection =
                Random.insideUnitCircle.normalized;
        }

        if (fleeDirection == Vector3.zero)
        {
            fleeDirection = Vector3.right;
        }

        Debug.Log(name + " está huyendo.");
    }

    void Flee(float h)
    {
        transform.position +=
            fleeDirection * speed * h;

        fleeTimer -= h;

        if (fleeTimer <= 0f)
        {
            FinishEscape();
        }
    }

    void FinishEscape()
    {
        isActive = false;

        if (originGrass != null)
        {
            originGrass.FinishEncounter();
        }

        Debug.Log(name + " escapó.");

        Destroy(gameObject);
    }

    public void Capture()
    {
        if (!isActive)
        {
            return;
        }

        currentState =
            WildPokemonState.Captured;

        isActive = false;

        if (originGrass != null)
        {
            originGrass.FinishEncounter();
        }

        Debug.Log(name + " fue capturado.");

        Destroy(gameObject);
    }
}