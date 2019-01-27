using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFootstepController : MonoBehaviour
{
    public AudioClip walkSound;

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("PlaySound", 0.0f, 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void PlaySound()
    {
        if (GetComponent<Rigidbody2D>().velocity.magnitude > 0)
        {
            GetComponent<AudioSource>().PlayOneShot(walkSound);
        }
    }
}
