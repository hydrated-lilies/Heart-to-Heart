using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RhythmManager : MonoBehaviour
{
    // values to track health, score, and combo
    int health;
    int combo;
    int score;

    // invincibility frames
    static float invTime = 2.0f;
    float curInv;


    // song to play
    AudioSource song;

    // track for notes
    List<Note> notes;

    // buffer for diag taps
    float buff;

    // ring segments
    public RingControl ringL;
    public RingControl ringR;
    public RingControl ringU;
    public RingControl ringD;

    // track hilighter
    public highlighTrack track;

    // note generator
    public NoteGen noteGen;

    // Start is called before the first frame update
    void Start()
    {
        // init ints
        health = 6; 
        combo = 0;
        score = 0;
        curInv = 0;

        song = GetComponent<AudioSource>();

        print("generating notes...");
        // create random test notes
        notes = noteGen.genRandNotes(40f);
    }

    // Update is called once per frame
    void Update()
    {
        tapTracking();
    }

    // function that determines what note is tapped
    void tapTracking()
    {
        // flash rings on tap
        if (Input.GetKeyDown("w"))
        {
            ringU.flashRing();

            // check if note was hit
            // if not, make diagonal check
            if (!checkNoteTap(NOTE_TYPE.U))
                StartCoroutine(diagonalTracking(0.1f, NOTE_TYPE.U));
        }

        if (Input.GetKeyDown("a"))
        {
            ringL.flashRing();

            // check if note was hit
            // if not, make diagonal check
            if (!checkNoteTap(NOTE_TYPE.L))
                StartCoroutine(diagonalTracking(0.1f, NOTE_TYPE.L));
        }

        if (Input.GetKeyDown("s"))
        {
            ringD.flashRing();

            // check if note was hit
            // if not, make diagonal check
            if (!checkNoteTap(NOTE_TYPE.D))
                StartCoroutine(diagonalTracking(0.1f, NOTE_TYPE.D));
        }

        if (Input.GetKeyDown("d"))
        {
            ringR.flashRing();

            // check if note was hit
            // if not, make diagonal check
            if (!checkNoteTap(NOTE_TYPE.R))
                StartCoroutine(diagonalTracking(0.1f, NOTE_TYPE.R));
        }
    }

    // helper IEnumerator that determines in a note was a diagonal tap
    IEnumerator diagonalTracking(float duration, NOTE_TYPE type)
    {
        // duration = buffer between note taps
        // NOTE_TYPE to determine the first input note and notes to check

        while (duration > 0)
        {
            // each input note has its own possible diagonals
            // check if note was hit, then flash track
            if (type == NOTE_TYPE.U)
            { 
                // check L or R
                // LEFT
                if (Input.GetKeyDown("a"))
                {
                    if (checkNoteTap(NOTE_TYPE.UL))
                        track.flashTrack(NOTE_TYPE.UL);
                    break;
                }
                // RIGHT
                if (Input.GetKeyDown("d"))
                {
                    if (checkNoteTap(NOTE_TYPE.UR))
                        track.flashTrack(NOTE_TYPE.UR);
                    break;
                }
            }
            else if (type == NOTE_TYPE.D)
            {
                // check L or R
                // LEFT
                if (Input.GetKeyDown("a"))
                {
                    if (checkNoteTap(NOTE_TYPE.DL))
                        track.flashTrack(NOTE_TYPE.DL);
                    break;
                }
                // RIGHT
                if (Input.GetKeyDown("d"))
                {
                    if (checkNoteTap(NOTE_TYPE.DR))
                        track.flashTrack(NOTE_TYPE.DR);
                    break;
                }
            }
            else if (type == NOTE_TYPE.L)
            {
                // check U or D
                // UP
                if (Input.GetKeyDown("w"))
                {
                    if (checkNoteTap(NOTE_TYPE.UL))
                        track.flashTrack(NOTE_TYPE.UL);
                    break;
                }
                // DOWN
                if (Input.GetKeyDown("s"))
                {
                    if (checkNoteTap(NOTE_TYPE.DL))
                        track.flashTrack(NOTE_TYPE.DL);
                    break;
                }
            }
            else if (type == NOTE_TYPE.R)
            {
                // check U or D
                // UP
                if (Input.GetKeyDown("w"))
                {
                    if (checkNoteTap(NOTE_TYPE.UR))
                        track.flashTrack(NOTE_TYPE.UR);
                    break;
                }
                // DOWN
                if (Input.GetKeyDown("s"))
                {
                    if (checkNoteTap(NOTE_TYPE.DR))
                        track.flashTrack(NOTE_TYPE.DR);
                    break;
                }
            }

            duration -= Time.deltaTime;

            yield return null;
        }
    }

    // function that detemines in a note in the list has been tapped
    bool checkNoteTap(NOTE_TYPE direction)
    {
        // check the most recent entries in the 'notes' list to determine if a hit was secured in the range
        // use helper function in note class

        for (int i = 0; i < notes.Count; i++)
        {
            if (notes[i].checkHit())
                if (notes[i].type == direction)
                {
                    // note was in hiting range AND is the correct type, we can remove it from the list and delete the game ovject
                    // flash the track, increment combo, add to score
                    track.flashTrack(direction);
                    GameObject toDelete = notes[i].gameObject;

                    notes.Remove(notes[i]);
                    Destroy(toDelete);

                    //INCREMENT COMBO, ADD TO SCORE

                    return true;
                }
        }

        return false;
    }
}