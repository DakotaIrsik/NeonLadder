using UnityEngine;

public class BossSpawn : MonoBehaviour
{
    public ParticleSystem rain;
    // Start is called before the first frame update
    void Start()
    {
        rain.Stop();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name == "Player")
        {
            rain.Play();
        }
    }
}
