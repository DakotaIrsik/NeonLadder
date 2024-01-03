using UnityEngine;

public class BossSpawn : MonoBehaviour
{
    public ParticleSystem rain;
    private GameObject boss;
    void Start()
    {
        rain.Stop();

        boss = GameObject.FindGameObjectWithTag("Boss");

        //deactivate the boss initially as it should not be visible from the start
        if (boss != null)
        {
            boss.SetActive(false);
        }
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
            boss.gameObject.SetActive(true);
        }
    }
}
