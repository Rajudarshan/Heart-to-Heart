using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// DELETE ME
public enum HOLD_NOTE_TYPE {HU, HD, HR, HL};

public class HoldNote : MonoBehaviour
{
    public float time;
    public float secondNoteTime;
    public float firstNoteTime;
    public HOLD_NOTE_TYPE type;
    public Note firstNote;
    public Note secondNote;
    GameObject newNote1;
    GameObject newNote2;
    public bool held = false;
    bool created = false;
    public int index = -1;
    public bool toBeDeleted = false;
    float offset;
    static float noteOffset = 0.768f;

    public float trackSpeed;

    public RhythmManager parent;

    // Start is called before the first frame update
    void Start()
    {
        // get reference to RhythmManager
        parent = GameObject.Find("Field").GetComponent<RhythmManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (created == true)
        {
            Vector3 centerPos = new Vector3(newNote1.transform.position.x + newNote2.transform.position.x, newNote1.transform.position.y + newNote2.transform.position.y) / 2;
            if (secondNoteTime >= 0.02225)
            {
                if (type == HOLD_NOTE_TYPE.HU || type == HOLD_NOTE_TYPE.HD)
                {
                    float scaleY = Mathf.Abs(newNote1.transform.position.y - newNote2.transform.position.y);
                    transform.localScale = new Vector3(1, scaleY * 10f, 1);
                }
                else
                {
                    float scaleX = Mathf.Abs(newNote1.transform.position.x - newNote2.transform.position.x);
                    transform.localScale = new Vector3(scaleX * 10f, 1, 1);
                }
            }
            else
            {
                centerPos = new Vector3(100, 100);
            }
            transform.position = centerPos;
        }
        if (held == true)
        {
            // toggle ring flash ON
            switch (firstNote.type)
            {
                case NOTE_TYPE.HL:
                    parent.ringL.toggleRing(true);
                    break;
                case NOTE_TYPE.HR:
                    parent.ringR.toggleRing(true);
                    break;
                case NOTE_TYPE.HU:
                    parent.ringU.toggleRing(true);
                    break;
                case NOTE_TYPE.HD:
                    parent.ringD.toggleRing(true);
                    break;
            }

            if (Input.GetKey(KeyCode.A) && firstNote.type == NOTE_TYPE.HL)
            {
                firstNoteTime = 0;
                checkTime();
            }
            else if (Input.GetKeyUp(KeyCode.A) && firstNote.type == NOTE_TYPE.HL)
            {
                checkHitSecond();
                checkTime();
                held = false;
                parent.ringL.toggleRing(false);
            }

            if (Input.GetKey(KeyCode.D) && firstNote.type == NOTE_TYPE.HR)
            {
                firstNoteTime = 0;
                checkTime();
            }
            else if (Input.GetKeyUp(KeyCode.D) && firstNote.type == NOTE_TYPE.HR)
            {
                checkHitSecond();
                checkTime();
                held = false;
                parent.ringR.toggleRing(false);
            }

            if (Input.GetKey(KeyCode.W) && firstNote.type == NOTE_TYPE.HU)
            {
                firstNoteTime = 0;
                checkTime();
            }
            else if (Input.GetKeyUp(KeyCode.W) && firstNote.type == NOTE_TYPE.HU)
            {
                checkHitSecond();
                checkTime();
                held = false;
                parent.ringU.toggleRing(false);
            }

            if (Input.GetKey(KeyCode.S) && firstNote.type == NOTE_TYPE.HD)
            {
                firstNoteTime = 0;
                checkTime();
            }

            else if (Input.GetKeyUp(KeyCode.S) && firstNote.type == NOTE_TYPE.HD)
            {
                checkHitSecond();
                checkTime();
                held = false;
                parent.ringD.toggleRing(false);
            }
        }
    }
    public void checkTime()
    {
        if (secondNoteTime <= -0.25f)
        {
            // combo
            takeDamage();
        }
    }
    public void checkHitSecond()
    {
        int value = secondNote.checkHit();
        if (value != 4)
        {
            if (value == 1)
            {
                parent.combo += 1;
                comboCaller(1);
            }
            if (value == 2)
            {
                parent.combo += 1;
                comboCaller(2);
            }
            if (value == 3)
            {
                parent.combo = 0;
                comboCaller(3);
            }
        }
    }
    public void comboCaller(int grade)
    {
        parent.printCombo();
        toBeDeleted = true;
        parent.destroyGameObject(index);
        parent.flashGrade(grade, firstNote.type);
        Destroy(newNote1);
        Destroy(newNote2);
        Destroy(gameObject);
    }
    public void incrementPosition()
    {
        // reduce note time
        if (held == true)
        {
            secondNoteTime -= Time.deltaTime;
        }
        else
        {
            
            firstNoteTime -= Time.deltaTime;
            secondNoteTime -= Time.deltaTime;
        }
        // draw note
        drawNote();

        // if position hits 0, take damage, delete note
        if (firstNoteTime <= -0.2864)
        {
            takeDamage();

        }
    }

    // creates general note given the type and time associated with it
    public void initializeNote(HOLD_NOTE_TYPE _type, NOTE_TYPE _type2, float _time, Sprite sprite, Sprite noteSprite, GameObject noteInfo, GameObject noteInfo2, float offset)
    {
        newNote1 = noteInfo;
        newNote2 = noteInfo2;
        this.offset = offset;
        time = _time + offset;
        firstNoteTime = _time;
        type = _type;
        // Change sprite
        GetComponent<SpriteRenderer>().sprite = sprite;
        newNote1.GetComponent<Note>().initializeNote(_type2, _time, noteSprite);
        if (_type2 == NOTE_TYPE.HD || _type2 == NOTE_TYPE.HL)
        {
            secondNoteTime = _time + (offset) ;
        }
        else
        {
            secondNoteTime = _time + (offset);
        }
        newNote2.GetComponent<Note>().initializeNote(_type2, _time + offset, noteSprite);
        firstNote = newNote1.GetComponent<Note>();
        secondNote = newNote2.GetComponent<Note>();
        created = true;

        // draw note for initial position
        drawNote();
    }

    public void drawNote()
    {
        switch (type)
        {
            case HOLD_NOTE_TYPE.HL:
                firstNote.transform.position = new Vector3(-(noteOffset + (firstNoteTime * trackSpeed)), 0, 0);
                secondNote.transform.position = new Vector3(-(noteOffset + (secondNoteTime * trackSpeed)), 0, 0);
                firstNote.setTime(firstNoteTime);
                secondNote.setTime(secondNoteTime);
                break;
            case HOLD_NOTE_TYPE.HR:
                firstNote.transform.position = new Vector3(noteOffset + (firstNoteTime * trackSpeed), 0, 0);
                secondNote.transform.position = new Vector3(noteOffset + (secondNoteTime * trackSpeed), 0, 0);
                firstNote.setTime(firstNoteTime);
                secondNote.setTime(secondNoteTime);
                break;
            case HOLD_NOTE_TYPE.HU:
                firstNote.transform.position = new Vector3(0, noteOffset + (firstNoteTime * trackSpeed), 0);
                secondNote.transform.position = new Vector3(0, noteOffset + (secondNoteTime * trackSpeed), 0);
                firstNote.setTime(firstNoteTime);
                secondNote.setTime(secondNoteTime);
                break;
            case HOLD_NOTE_TYPE.HD:
                firstNote.transform.position = new Vector3(0, -(noteOffset + (firstNoteTime * trackSpeed)), 0);
                secondNote.transform.position = new Vector3(0, -(noteOffset + (secondNoteTime * trackSpeed)), 0);
                firstNote.setTime(firstNoteTime);
                secondNote.setTime(secondNoteTime);
                break;
        }
    }
    void takeDamage()
    {
        // talk to parent class about damage
        parent.takeDamage();

        // destroy object
        Destroy(newNote1);
        Destroy(newNote2);
        Destroy(gameObject);
    }

    public float getTime()
    {
        return time;
    }
}
