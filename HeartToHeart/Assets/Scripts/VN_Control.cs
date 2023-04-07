using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.IO;
using UnityEngine.SceneManagement;

public class VN_Control : MonoBehaviour
{
    // information for the textbox & visuals
    public TextMeshProUGUI txt;
    public float txtSpd;
    public GameObject textbox;
    bool crawling = false;

    // can players interact with the visual novel?
    bool interactable = false;

    // AUTO MODE
    bool autoMode = false;

    // information for the data being read from the file
    private List<string> VNScript;
    private int index;

    // objects to fade between scenes
    public Image bg_fader;
    public Image scene_fader;

    // ---- BACKGROUND LIST ---- //
    // 0 - APARTMENT
    // 1 - SUBWAY
    // 2 - OFFICE
    // 3 - BLACK SCREEN (for extended fades)
    // 4 - WHITE SCREEN
    public List<Texture> backgrounds;
    public RawImage currBg;

    // reference to the character manager
    public CharacterManager cManager;

    // sounds for text crawling and VN music
    public AudioSource txtBeep;
    public AudioSource VN_music;
    public List<AudioClip> VN_songs;

    // prefab and gameobject to store the current rhythm game
    public GameObject gamePrefab;
    GameObject rhythmComponent;

    // Start is called before the first frame update
    void Start()
    {
        // txtSpd = 1; // based on speed

        ReadScript(1);

        StartCoroutine(FadeScene(true, 1f));

        index = -1;

        Invoke("ReadNextLine", 1f);
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            SceneManager.LoadScene(0);

        if (Input.GetKeyDown("l"))
            CompleteSong();

        // check to see if button was pressed
        if(interactable && 
            (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
        {
            // if true set text to be finished immediately
            if (crawling)
                crawling = false;
            else
                ReadNextLine();
        }
    }

    // --------------------- SCRIPT READING CONTROL --------------------- //
    void ReadScript(int sceneNumber)
    {
        // read lines using the stupid resources operation becaue of course this shit is packaged away in a place where I listerally can't get to it
        string rawScript = Resources.Load<TextAsset>("VN_Files/scene" + sceneNumber).text;

        // split it by newline delimeter because fuck it i guess why not ahaahaha
        // THANK YOU TO THE KIND FOLKS WHO PROGRAMMED STRING ARRAY TO GIVE FUNCTONALITY TO CONVERT TO LISTS
        VNScript = rawScript.Split("\n").ToList();
    }
    void ReadNextLine()
    {
        index++;

        // at end of script, deactivate textbox and characters
        if (index > VNScript.Count - 1)
        {
            SceneManager.LoadScene(0);

            textbox.SetActive(false);
            //cManager.FadeCharacterInOut(false);
            //cManager.FadeCharacterInOut(true);
            return;
        }

        // -------- COMMANDS THAT SHOULD NOT REQUIRE A NOT INPUT CLICK GO IN HERE -------- //
        while (VNScript[index].Contains("[Character]") || VNScript[index].Contains("[Toggle]") 
                || VNScript[index].Contains("[Volume]") || VNScript[index].Contains("[Dim]") 
                || VNScript[index].Contains("[Song]") || VNScript[index].Contains("[Auto]")
                || VNScript[index].Length < 3)
        {
            if (VNScript[index].Contains("[Character]"))
                cManager.ParseCharacter(VNScript[index].Substring(12));
            else if (VNScript[index].Contains("[Song]"))
                ChangeVNSong(VNScript[index].Substring(7));
            else if (VNScript[index].Contains("[Toggle]"))
            {
                string toToggle = VNScript[index].Substring(9);

                if (toToggle.Contains("npc"))
                {
                    cManager.FadeCharacterInOut(false);
                }
                else if (toToggle.Contains("Aria"))
                {
                    cManager.FadeCharacterInOut(true);
                }
            }
            else if (VNScript[index].Contains("[Volume]"))
            {
                if (VNScript[index].Contains("[F]"))
                {
                    string rawLine = VNScript[index].Substring(9);
                    string[] vals = rawLine.Split(" ");
                    float vol = float.Parse(vals[0]);
                    float duration = float.Parse(vals[2]);

                    ChangeVNVolume(vol, true, duration);
                }
                else
                {
                    float vol = float.Parse(VNScript[index].Substring(9));
                    ChangeVNVolume(vol);
                }

            }
            else if (VNScript[index].Contains("[Dim]"))
            {
                if (VNScript[index].Contains("[F]"))
                {
                    string rawLine = VNScript[index].Substring(6);
                    string[] vals = rawLine.Split(" ");
                    float alpha = float.Parse(vals[0]);
                    float duration = float.Parse(vals[2]);

                    DimBG(alpha, true, duration);
                }
                else
                {
                    float alpha = float.Parse(VNScript[index].Substring(6));
                    DimBG(alpha);
                }
            }
            else if (VNScript[index].Contains("[Auto]"))
                autoMode = !autoMode;
            index++;
        }

        // if [Wait] is called, set interactable to false for the duration of time
        if (VNScript[index].Contains("[Wait]"))
        {
            string line = VNScript[index].Substring(7);
            StartCoroutine(Wait(float.Parse(line)));
        }

        // read the next line!
        if (VNScript[index].Contains("[Line]"))
        {
            parseTypedLine();
        }
        else if (VNScript[index].Contains("[Background]"))
        {
            // do we need to fade?
            if (VNScript[index].Contains("[F]"))
            {
                // change bg!
                string bgName = VNScript[index].Substring(17);
                ChangeBG(bgName, true);
            }
            else
            {
                // change bg!
                string bgName = VNScript[index].Substring(13);
                ChangeBG(bgName, false);
            }
        }
        else if (VNScript[index].Contains("[Load Game]"))
        {
            string song = VNScript[index].Substring(12);

            // load in prefab
            rhythmComponent = Instantiate(gamePrefab);
            
            // feed it the song it needs
            if (song.Contains("tutorial"))
                rhythmComponent.GetComponentInChildren<RhythmManager>().setMusicTrack(0, gameObject);
            // placeholder for now
            else if (song.Contains("Wesley"))
                rhythmComponent.GetComponentInChildren<RhythmManager>().setMusicTrack(1, gameObject);

            // disable VN interactability
            txt.gameObject.GetComponentInParent<Image>().gameObject.SetActive(false);
            interactable = false;

            // dimbg, clear text
            StartCoroutine(DimBG(0.35f, 1f));
            txt.text = "";

            // start song (done automagically in rhythm manager
        }
    }
    void parseTypedLine()
    {
        // clear current text
        txt.text = "";

        // enable nametag
        cManager.SetNameTag(true);

        string line = VNScript[index].Substring(7);
        StartCoroutine(Type(line, cManager.activeCharacter.txtCol(), cManager.activeCharacter.txtSpeed(), cManager.activeCharacter.getTxtPitch()));

        // shake char
        cManager.ShakeCharacter(2.0f);
    }
    IEnumerator Type(string line, Color col, float speed, float pitch)
    {
        // set text color
        txt.color = col;

        // set the current text pitch
        txtBeep.pitch = pitch;

        crawling = true;

        foreach (char c in line.ToCharArray())
        {
            // at any point, if 'crawling' is turned off, stop early
            if (!crawling)
            {
                txt.text = line;
                break;
            }

            // if not, remain crawlin!
            txt.text += c;

            // play a sound
            txtBeep.Play();

            yield return new WaitForSeconds(speed / 2.0f);
        }

        crawling = false;

        // if in auto mode, type the next line
        if (autoMode)
            ReadNextLine();
    }

    // --------------------- RHYTHM COMPONENT & MUSIC CONTROL --------------------- //
    public void CompleteSong()
    {
        Destroy(rhythmComponent);

        // let the VN be usable again
        txt.gameObject.GetComponentInParent<Image>(true).gameObject.SetActive(true);
        interactable = true;

        // undim bg
        StartCoroutine(DimBG(1f, 1f));

        ReadNextLine();
    }
    void ChangeVNSong(string songName)
    {
        // get current time
        float currTime = VN_music.time;
        // pause audio

        VN_music.Pause();

        if (songName.Contains("calm"))
            VN_music.clip = VN_songs[0];
        else if (songName.Contains("upbeat"))
            VN_music.clip = VN_songs[1];
        else
            print("no song of name:" + songName);

        // play
        VN_music.Play();

        // set time
        VN_music.time = currTime;
    }
    void ChangeVNVolume(float val, bool fade = false, float duration = 1f)
    {
        // if not fading, change value
        if (!fade)
            VN_music.volume = val;
        // if fading, call coroutine
        else
            StartCoroutine(ChangeVNVolFade(val, duration));
    }
    IEnumerator ChangeVNVolFade(float val, float duration)
    {
        float currTime = 0;
        float currVol = VN_music.volume;
        float step = (val - currVol) / duration;

        while (currTime < duration)
        {
            currTime += Time.deltaTime;
            VN_music.volume += step * Time.deltaTime;

            yield return null;
        }

        VN_music.volume = val;
    }

    // --------------------- BACKGROUND CONTROL --------------------- //
    void ChangeBG(string bg, bool fade, float wait = 1.0f)
    {

        float length = 0f;

        if (fade)
            length = 0.5f;

        if (bg.Contains("Apartment"))
            StartCoroutine(ChangeBG(backgrounds[0], length));
        else if (bg.Contains("Subway"))
            StartCoroutine(ChangeBG(backgrounds[1], length));
        else if (bg.Contains("Office"))
            StartCoroutine(ChangeBG(backgrounds[2], length));
        else if (bg.Contains("Black"))
            StartCoroutine(ChangeBG(backgrounds[3], length));
        else if (bg.Contains("White"))
            StartCoroutine(ChangeBG(backgrounds[4], length));
        else
            StartCoroutine(ChangeBG(backgrounds[3], length));
    }
    IEnumerator ChangeBG(Texture bgChange, float duration, float extrWait = 1.0f)
    {
        interactable = false;
        float time = 0;

        // fade to black
        while (time < duration)
        {
            bg_fader.color = new Color(0, 0, 0, (time / duration));
            time += Time.deltaTime;

            yield return null;
        }


        bg_fader.color = new Color(0, 0, 0, 1);

        // clear current text
        txt.text = "";
        // change bg texture
        currBg.texture = bgChange;
        // disable nametag
        cManager.SetNameTag(false);

        // wait for the amount of time, if needed
        if (extrWait > 0.05f)
            yield return new WaitForSeconds(extrWait);

        // fade to transparent
        time = 0;
        while (time < duration)
        {
            bg_fader.color = new Color(0, 0, 0, 1f - (time / duration));
            time += Time.deltaTime;

            yield return null;
        }
        bg_fader.color = new Color(0, 0, 0, 0);
        interactable = true;

        // read next line
        ReadNextLine();
    }
    void DimBG(float val, bool fade = false, float duration = 1f)
    {
        // if not fading, change val
        if (!fade)
            currBg.color = new Color(1, 1, 1, val);
        else
            StartCoroutine(DimBG(val, duration));
    }
    IEnumerator DimBG(float val, float duration)
    {
        float curLevel = currBg.color.a;
        float step = (val - curLevel) / duration;

        float curTime = 0f;

        while (curTime < duration)
        {
            curTime += Time.deltaTime;
            currBg.color = new Color(1, 1, 1, curLevel + (step * curTime));

            yield return null;
        }

        // then set to min/max to make sure we have no floating point issues
        currBg.color = new Color(1, 1, 1, val);

    }

    // --------------------- SCENE MANGEMENT AND TIMING CONTROL --------------------- //
    IEnumerator FadeScene(bool dir, float duration)
    {
        // wait for a brief moment
        yield return new WaitForSeconds(duration);

        // dir = true, fades in, dir = false fades out
        float alpha;
        float curTime = 0f;

        while (curTime < duration)
        {
            curTime += Time.deltaTime;

            // begin fading
            alpha = (dir) ? 1 - (curTime / duration) : (curTime / duration);
            scene_fader.color = new Color(0, 0, 0, alpha);

            yield return null;
        }

        // then set to min/max to make sure we have no floating point issues
        alpha = (dir) ? 0 : 1;
        scene_fader.color = new Color(0, 0, 0, alpha);

        interactable = true;
    }
    IEnumerator Wait(float duration)
    {
        interactable = false;

        yield return new WaitForSeconds(duration);

        interactable = true;
        ReadNextLine();
    }
}