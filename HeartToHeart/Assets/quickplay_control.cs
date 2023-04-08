using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class quickplay_control : MonoBehaviour
{
    public GameObject gamePrefab;
    GameObject rhythmComponent;

    public int testSong;

    // Start is called before the first frame update
    void Start()
    {
        // load in prefab
        rhythmComponent = Instantiate(gamePrefab);

        // feed it the song it needs
        rhythmComponent.GetComponentInChildren<RhythmManager>().setMusicTrack(testSong, gameObject);

        // start song (done automagically in rhythm manager)
    }

    public void CompleteSong()
    {
        // destroy object
        Destroy(rhythmComponent);

        // return to home screen
        SceneManager.LoadScene(0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
