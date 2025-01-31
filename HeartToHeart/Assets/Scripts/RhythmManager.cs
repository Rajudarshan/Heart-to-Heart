using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RhythmManager : MonoBehaviour
{
    // reference to visual novel control scene
    public VN_Control parent = null;

    // values to track health, score, and combo
    public int health;     // VARIABLE
    public int combo;
    public GameObject gradePrefab;
    public GameObject gradeLocationRight, gradeLocationLeft, gradeLocationUp, gradeLocationDown;
    public Sprite perfect, good, bad;
    GameObject badObject;
    GameObject goodObject;
    GameObject perfectObject;

    // heart related variables
    public HeartControl heart;
    public bool inv; // true if invincible. We can have this on true for the tutorial stage!
    public Text comboText;

    // invincibility frames
    static float invTime = 2.0f;

    // bool for advancing notes
    bool notePause = true;

    // song source + songs
    public AudioSource audSource, sfxAudSource;
    public AudioClip perfectSFX, goodSFX, badSFX;
    public List<AudioClip> musicTracks;

    // track for notes
    List<Note> notes;
    List<HoldNote> holdNotes;

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
        health = 4; // change this based on difficulty
        combo = 0;
        inv = false;
        sfxAudSource.volume = 0.5f;

        // get camera for canvas
        GameObject.Find("GameScreenCanvas").GetComponent<Canvas>().worldCamera = GameObject.Find("Main Camera").GetComponent<Camera>();

        // timing loading
        gradePrefab.GetComponent<SpriteRenderer>().sprite = perfect;
        perfectObject = Instantiate(gradePrefab, (gradeLocationRight.transform));
        gradePrefab.GetComponent<SpriteRenderer>().sprite = good;
        goodObject = Instantiate(gradePrefab, (gradeLocationRight.transform));
        gradePrefab.GetComponent<SpriteRenderer>().sprite = bad;
        badObject = Instantiate(gradePrefab, (gradeLocationRight.transform));
        setZero();

        // set song
        notePause = true;
    }

    void loadMapNotes(string folder)
    {
        // note lists
        holdNotes = noteGen.getHoldNotes(folder + "holdMap");
        notes = noteGen.getNotes(folder + "map");
    }

    public void setMusicTrack(int track, GameObject vnReference)
    {
        parent = vnReference.GetComponent<VN_Control>();
        audSource.clip = musicTracks[track];

        // load note maps depending on the game
        string folder;
        switch(track)
        {
            default:
            case 0:
                folder = "Map/tutorial/";
                break;
            case 1:
                folder = "Map/wesley/";
                break;
        }

        loadMapNotes(folder);

        Invoke("startSong", 2f);
    }

    public void startSong()
    {
        audSource.Play();
        notePause = false;
    }

    // Update is called once per frame
    void Update()
    { 
        // if the song is over, tell someone
        if (!(audSource.isPlaying || notePause))
        {
            print("song over!");

            // if not null tell the vn to delete this prefab and continue the VN
            if (parent != null)
                parent.CompleteSong();
        }

        if (!notePause)
            advanceNotes();

        tapTracking();

        if (Input.GetKeyDown("p") || Input.GetKeyDown(KeyCode.Escape))
        {
            notePause = !notePause;
            if (audSource.isPlaying)
                audSource.Pause();
            else
                audSource.Play();
        }
        else if (Input.GetKeyDown("h"))
            SceneManager.LoadScene(0);
    }

    void advanceNotes()
    {
       for (int i = 0; i < notes.Count; i++)
           if (notes[i] == null)
               notes.Remove(notes[i]);
           else
               notes[i].incrementPosition();

        for (int i = 0; i < holdNotes.Count; i++)
            if (holdNotes[i] == null)
                holdNotes.Remove(holdNotes[i]);
            else
                holdNotes[i].incrementPosition();
    }

    // function that determines what note is tapped
    void tapTracking()
    {
        // flash rings on tap
        if (Input.GetKeyDown("w"))
        {
            ringU.flashRing();
            if (!checkHoldNoteTap(NOTE_TYPE.HU))
            {
                
            }
            // check if note was hit
            // if not, make diagonal check
            if (!checkNoteTap(NOTE_TYPE.U) && !checkNoteTap(NOTE_TYPE.HU))
                StartCoroutine(diagonalTracking(0.1f, NOTE_TYPE.U));
        }
        if (Input.GetKeyDown("a"))
        {
            ringL.flashRing();
            if (!checkHoldNoteTap(NOTE_TYPE.HL))
            {
               
            }
            // check if note was hit
            // if not, make diagonal check
            if (!checkNoteTap(NOTE_TYPE.L) && !checkNoteTap(NOTE_TYPE.HL))
                StartCoroutine(diagonalTracking(0.1f, NOTE_TYPE.L));
        }
        if (Input.GetKeyDown("s"))
        {
            ringD.flashRing();
            if (!checkHoldNoteTap(NOTE_TYPE.HD))
            {
              
            }
            // check if note was hit
            // if not, make diagonal check
            if (!checkNoteTap(NOTE_TYPE.D) && !checkNoteTap(NOTE_TYPE.HD))
                StartCoroutine(diagonalTracking(0.1f, NOTE_TYPE.D));
        }
        if (Input.GetKeyDown("d"))
        {
            ringR.flashRing();
            if (!checkHoldNoteTap(NOTE_TYPE.HR))
            {
              
            }
            // check if note was hit
            // if not, make diagonal check
            if (!checkNoteTap(NOTE_TYPE.R) && !checkNoteTap(NOTE_TYPE.HR))
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
                else if (Input.GetKeyDown("d"))
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
                else if (Input.GetKeyDown("d"))
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
                else if (Input.GetKeyDown("s"))
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
                else if (Input.GetKeyDown("s"))
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
            if (notes[i].checkHit()!=4)
                if (notes[i].type == direction)
                {
                    // note was in hiting range AND is the correct type, we can remove it from the list and delete the game ovject
                    // flash the track, increment combo, add to score
                    track.flashTrack(direction);
                    GameObject toDelete = notes[i].gameObject;
                    if (notes[i].checkHit() == 1)
                    {
                        combo++;
                        printCombo();
                        flashGrade(1, notes[i].type);
                    }
                    else if (notes[i].checkHit() == 2)
                    {
                        combo++;
                        printCombo();
                        flashGrade(2, notes[i].type);
                    }
                    else if (notes[i].checkHit() == 3){
                        combo = 0;
                        printCombo();
                        flashGrade(3, notes[i].type);
                    }


                    notes.Remove(notes[i]);
                    Destroy(toDelete);

                    //INCREMENT COMBO, ADD TO SCORE
                    //Adjust poor great and perfect here as well using notes[i].checkHit's vaule

                    return true;
                }
        }

        return false;
    }

    bool checkHoldNoteTap(NOTE_TYPE direction)
    {
        // check the most recent entries in the 'notes' list to determine if a hit was secured in the range
        // use helper function in note class
        for (int i = 0; i < holdNotes.Count; i++)
        {
            if (holdNotes[i].firstNote.checkHit() != 4)
            {
                if (holdNotes[i].firstNote.type == direction)
                {
                    // note was in hiting range AND is the correct type, we can remove it from the list and delete the game ovject
                    // flash the track, increment combo, add to score
                    // Slowly remove fill until done
                    track.flashTrack(direction);
                    Debug.Log(direction);
                    holdNotes[i].index = i;
                    holdNotes[i].held = true;
                    if (holdNotes[i].firstNote.checkHit() == 1)
                    {
                        combo++;
                        printCombo();
                        flashGrade(1, holdNotes[i].firstNote.type);
                    }
                    else if (holdNotes[i].firstNote.checkHit() == 2)
                    {
                        combo++;
                        printCombo();
                        flashGrade(2, holdNotes[i].firstNote.type);
                    }
                    else if (holdNotes[i].firstNote.checkHit() == 3)
                    {
                        combo = 0;
                        printCombo();
                        flashGrade(3, holdNotes[i].firstNote.type);
                    }
                    //INCREMENT COMBO, ADD TO SCORE
                    //Adjust poor great and perfect here as well using notes[i].checkHit's vaule
                    return true;
                }
            }
        }
        return false;
    }
    public void destroyGameObject(int indexGiven)
    {
        if (indexGiven != -1)
        {
            for (int i = 0; i < holdNotes.Count; i++)
            {
                if (holdNotes[i].toBeDeleted == true)
                {
                    track.flashTrack(holdNotes[i].secondNote.type);
                    holdNotes.Remove(holdNotes[i]);
                }
            }

        }
    }
    // ------------ DAMAGE ------------ //
    public void takeDamage()
    {
        if (!inv)
        {
            health -= 1;
            heart.dmgAnimation();
            StartCoroutine(invincibility(invTime));
            heart.changeHeart(health);
            print("Health remaining: "+health);
            heart.dmgFlash();
            print(health);
            combo = 0;
            printCombo();
        }
        if(health <= 0){
            SceneManager.LoadScene("GameOver");
        }
    }

    IEnumerator invincibility(float duration)
    {
        inv = true;
        yield return new WaitForSeconds(duration);
        inv = false;
    }
    // ------------ COMBO ------------ //

    public void setCombo(int num)
    {
        combo = num;
    }
    public void printCombo()
    {
        comboText.text = "Combo: "+ combo;
    }
    public void sfxGrade(int grade)
    {
        switch (grade)
        {
            case 1:
                // perfect
                sfxAudSource.PlayOneShot(perfectSFX,1f);
                break;
            case 2:
                // good
                sfxAudSource.PlayOneShot(goodSFX, 1f);
                break;
            case 3:
                // bad
                sfxAudSource.PlayOneShot(badSFX, 1f);
                break;
        }
    }
    public void flashGrade(int grade, NOTE_TYPE type)
    {
        float duration = 0.6f;
        // end previous coroutine
        // call coroutine
        sfxGrade(grade);
        StopCoroutine("printGrade");
        StartCoroutine(printGrade(duration, grade, type));
    }
    IEnumerator printGrade(float duration, int grade, NOTE_TYPE type)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            // change alpha channel to decrease back to half opacity
            float alpha = Mathf.Lerp(1f, 0.0f, elapsed / duration) + .2f;
            switch(grade)
            {
                case 1:
                    perfectObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, alpha);
                    break;
                case 2:
                    goodObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, alpha);
                    break;
                case 3:
                    badObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, alpha);
                    break;
            }
            elapsed += Time.deltaTime;

            yield return null;
        }
        setZero();
    }
    public void setZero()
    {
        perfectObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
        goodObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
        badObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
    }
}
