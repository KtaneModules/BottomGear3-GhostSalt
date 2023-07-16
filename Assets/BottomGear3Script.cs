using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;
using System.Text.RegularExpressions;

public class BottomGear3Script : MonoBehaviour
{
    static int _moduleIdCounter = 1;
    int _moduleID = 0;

    public KMBombModule Module;
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMSelectable[] Buttons;
    public MeshRenderer Surface;
    public Texture SolvedTexture;

    private List<List<int>> Orders = new List<List<int>>()
    {
        new List<int>() { 10, 8, 9 },
        new List<int>() { 9, 1, 6 },
        new List<int>() { 10, 7, 17 },
        new List<int>() { 4, 16, 1 },
        new List<int>() { 13, 20, 7 },
        new List<int>() { 8, 6, 15 },
        new List<int>() { 18, 2, 13 },
        new List<int>() { 7, 3, 7 },
        new List<int>() { 10, 1, 12 },
        new List<int>() { 4, 9, 2 },
        new List<int>() { 20, 12, 13 },
        new List<int>() { 1, 11, 12 },
        new List<int>() { 14, 12, 15 },
        new List<int>() { 7, 16, 18 },
        new List<int>() { 16, 15, 1 },
        new List<int>() { 8, 18, 5 },
        new List<int>() { 19, 5, 16 },
        new List<int>() { 12, 19, 9 },
        new List<int>() { 17, 17, 13 },
        new List<int>() { 20, 14, 4 },
        new List<int>() { 2, 14, 8 },
        new List<int>() { 14, 15, 14 },
        new List<int>() { 11, 10, 9 },
        new List<int>() { 9, 3, 16 },
        new List<int>() { 17, 5, 4 },
        new List<int>() { 3, 1, 11 },
        new List<int>() { 6, 11, 18 },
        new List<int>() { 17, 19, 2 },
        new List<int>() { 4, 10, 10 },
        new List<int>() { 18, 10, 7 },
        new List<int>() { 14, 16, 4 },
        new List<int>() { 20, 20, 17 },
        new List<int>() { 6, 18, 4 },
        new List<int>() { 9, 8, 19 },
        new List<int>() { 11, 20, 17 },
        new List<int>() { 13, 15, 12 },
        new List<int>() { 1, 9, 20 },
        new List<int>() { 13, 11, 10 },
        new List<int>() { 19, 4, 5 },
        new List<int>() { 15, 19, 5 },
        new List<int>() { 16, 6, 16 },
        new List<int>() { 2, 12, 18 },
        new List<int>() { 8, 15, 12 },
        new List<int>() { 17, 3, 3 },
        new List<int>() { 11, 11, 13 },
        new List<int>() { 6, 15, 3 },
        new List<int>() { 14, 19, 3 },
        new List<int>() { 14, 2, 6 },
        new List<int>() { 5, 6, 5 },
        new List<int>() { 1, 7, 7 },
        new List<int>() { 8, 19, 2 },
        new List<int>() { 5, 20, 2 }
    };

    private List<int> InputOrder = new List<int>();
    private List<int> Phrases = Enumerable.Range(1, 52).ToList();
    private List<List<int>> AcceptableAnswers = new List<List<int>>();
    private Coroutine[] RunningAnims = new Coroutine[21];
    private bool[] CompletedStages = new bool[3];
    private bool Solved;
    private KMAudio.KMAudioRef[] Sounds = new KMAudio.KMAudioRef[5];

    private int FindValidity()
    {
        for (int i = 0; i < 3; i++)
        {
            if (CompletedStages.Where(x => x).Count() == 0)
            {
                for (int j = 0; j < 3; j++)
                    if (AcceptableAnswers[i][j] != InputOrder[j])
                        goto cont;
                return i;
            }
            else if (CompletedStages.Where(x => x).Count() == 1)
            {
                for (int j = 0; j < 3; j++)
                    if (AcceptableAnswers[i][(j + 1) % 3] != InputOrder[j])
                        goto cont;
                return i;
            }
            else
            {
                for (int j = 0; j < 3; j++)
                    if (AcceptableAnswers[i][(j + 2) % 3] != InputOrder[j])
                        goto cont;
                return i;
            }
            cont:;
        }
        return -1;
    }

    private List<int> FindAnAnswer()
    {
        for (int i = 0; i < 3; i++)
            if (!CompletedStages[i])
            {
                if (CompletedStages.Where(x => x).Count() == 0)
                    return AcceptableAnswers[i];
                else if (CompletedStages.Where(x => x).Count() == 1)
                    return new List<int>() { AcceptableAnswers[i][1], AcceptableAnswers[i][2], AcceptableAnswers[i][0] };
                else
                    return new List<int>() { AcceptableAnswers[i][2], AcceptableAnswers[i][0], AcceptableAnswers[i][1] };
            }
        return null;
    }

    void Awake()
    {
        _moduleID = _moduleIdCounter++;
        Phrases.Shuffle();
        Phrases = Phrases.Take(3).ToList();
        for (int i = 0; i < 3; i++)
            AcceptableAnswers.Add(Orders[Phrases[i] - 1]);
        Module.OnActivate += delegate { Audio.PlaySoundAtTransform("intro", transform); };
        Module.GetComponent<KMSelectable>().OnFocus += delegate { if (!Solved) Sounds[4] = Audio.PlaySoundAtTransformWithRef("bottom gear theme", transform); };
        Module.GetComponent<KMSelectable>().OnDefocus += delegate { if (Sounds[4] != null) Sounds[4].StopSound(); };
        Bomb.OnBombExploded += delegate { for (int i = 0; i < 4; i++) if (Sounds[i] != null) Sounds[i].StopSound(); };
        for (int i = 0; i < Buttons.Length; i++)
        {
            int x = i;
            Buttons[x].OnInteract += delegate { if (!Solved) ButtonPress(x); return false; };
        }
        Buttons[0].OnInteractEnded += delegate { if (!Solved) ButtonRelease(); };
        Debug.LogFormat("[Bottom Gear 3 #{0}] 3 cahs ned to be stahted. aceptible inputs: {1}", _moduleID, AcceptableAnswers.Select(x => "( " + x.Join(", ") + " )").Join(", "));
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void ButtonPress(int pos)
    {
        if (pos == 0)
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, Buttons[0].transform);
        else
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, Buttons[pos].transform);
        if (RunningAnims[pos] != null)
            StopCoroutine(RunningAnims[pos]);
        if (pos == 0)
            RunningAnims[0] = StartCoroutine(TopButtonPressAnim());
        else
            RunningAnims[pos] = StartCoroutine(ButtonAnim(pos));
        if (pos == 0)
        {
            for (int i = 0; i < 3; i++)
                if (Sounds[i] != null)
                    Sounds[i].StopSound();
            for (int i = 0; i < 3; i++)
                if (!CompletedStages[i])
                    Sounds[i] = Audio.PlaySoundAtTransformWithRef("sound " + Phrases[i], transform);
        }
        else
        {
            InputOrder.Add(pos);
            Debug.LogFormat("[Bottom Gear 3 #{0}] you pressé buton {1}.", _moduleID, pos);
            if (InputOrder.Count() < 3)
            {
                if (Sounds[3] != null)
                    Sounds[3].StopSound();
                Sounds[3] = Audio.HandlePlaySoundAtTransformWithRef("start fail", Buttons[pos].transform, false);
            }
            else
            {
                var validity = FindValidity();
                if (validity == -1)
                {
                    Module.HandleStrike();
                    if (Sounds[3] != null)
                        Sounds[3].StopSound();
                    Sounds[3] = Audio.HandlePlaySoundAtTransformWithRef("strike", Buttons[pos].transform, false);
                    Debug.LogFormat("[Bottom Gear 3 #{0}] BASTAD!!!!!!!!!!!! YOU SMITDED {1}!!! IDOT, YOU CAUSÉÉD A STIRKE ND MAD THE CAHS SMELL LIK WEED!!!!!!", _moduleID, "( " + InputOrder.Join(", ") + " )");
                    InputOrder = new List<int>();
                }
                else
                {
                    CompletedStages[validity] = true;
                    if (Sounds[validity] != null)
                        Sounds[validity].StopSound();
                    AcceptableAnswers[validity] = new List<int>() { -1, -1, -1 };
                    if (CompletedStages.Where(x => x).Count() == 3)
                    {
                        Debug.LogFormat("[Bottom Gear 3 #{0}] you submited {1}. good. mduole solvé!!", _moduleID, "( " + InputOrder.Join(", ") + " )");
                        if (Sounds[3] != null)
                            Sounds[3].StopSound();
                        if (Sounds[4] != null)
                            Sounds[4].StopSound();
                        Sounds[3].StopSound();
                        Module.HandlePass();
                        Audio.PlaySoundAtTransform("solve", transform);
                        Surface.material.SetTexture("_MainTex", SolvedTexture);
                        Solved = true;
                    }
                    else
                    {
                        if (Sounds[3] != null)
                            Sounds[3].StopSound();
                        Sounds[3] = Audio.HandlePlaySoundAtTransformWithRef("start", Buttons[pos].transform, false);
                        Debug.LogFormat("[Bottom Gear 3 #{0}] you submited {1}. good. {2} cah{3} stil ned to be stahted. aceptible inputs: {4}", _moduleID, "( " + InputOrder.Join(", ") + " )", CompletedStages.Where(x => !x).Count(), CompletedStages.Where(x => !x).Count() == 1 ? "" : "s", AcceptableAnswers.Where(x => !x.Contains(-1)).Select(x => "( " +
                        (CompletedStages.Where(y => y).Count() == 1 ? new List<int>() { x[1], x[2], x[0] } : new List<int>() { x[2], x[0], x[1] }).Join(", ") + " )").Join(", "));
                        InputOrder = new List<int>();
                    }
                }
            }
        }
    }

    void ButtonRelease()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonRelease, Buttons[0].transform);
        if (RunningAnims[0] != null)
            StopCoroutine(RunningAnims[0]);
        RunningAnims[0] = StartCoroutine(TopButtonReleaseAnim());
        for (int i = 0; i < 3; i++)
            if (Sounds[i] != null)
                Sounds[i].StopSound();
    }

    private IEnumerator ButtonAnim(int pos, float depression = 0.02f, float duration = 0.1f)
    {
        Vector3 initPos = Buttons[pos].transform.localPosition;
        Buttons[pos].transform.localPosition = new Vector3(initPos.x, 0.0244f, initPos.z);
        float timer = 0;
        while (timer < duration / 2)
        {
            yield return null;
            timer += Time.deltaTime;
            Buttons[pos].transform.localPosition = Vector3.Lerp(new Vector3(initPos.x, 0.0244f, initPos.z), new Vector3(initPos.x, 0.0244f - depression, initPos.z), timer / (duration / 2));
        }
        Buttons[pos].transform.localPosition = new Vector3(initPos.x, 0.0244f - depression, initPos.z);
        timer = 0;
        while (timer < duration / 2)
        {
            yield return null;
            timer += Time.deltaTime;
            Buttons[pos].transform.localPosition = Vector3.Lerp(new Vector3(initPos.x, 0.0244f - depression, initPos.z), new Vector3(initPos.x, 0.0244f, initPos.z), timer / (duration / 2));
        }
        Buttons[pos].transform.localPosition = new Vector3(initPos.x, 0.0244f, initPos.z);
    }

    private IEnumerator TopButtonPressAnim(float depression = 0.02f, float duration = 0.05f)
    {
        Vector3 initPos = Buttons[0].transform.localPosition;
        Buttons[0].transform.localPosition = new Vector3(initPos.x, 0.0246f, initPos.z);
        float timer = 0;
        while (timer < duration)
        {
            yield return null;
            timer += Time.deltaTime;
            Buttons[0].transform.localPosition = Vector3.Lerp(new Vector3(initPos.x, 0.0246f, initPos.z), new Vector3(initPos.x, 0.0246f - depression, initPos.z), timer / duration);
        }
        Buttons[0].transform.localPosition = new Vector3(initPos.x, 0.0246f - depression, initPos.z);
    }

    private IEnumerator TopButtonReleaseAnim(float depression = 0.02f, float duration = 0.05f)
    {
        Vector3 initPos = Buttons[0].transform.localPosition;
        Buttons[0].transform.localPosition = new Vector3(initPos.x, 0.0246f - depression, initPos.z);
        float timer = 0;
        while (timer < duration)
        {
            yield return null;
            timer += Time.deltaTime;
            Buttons[0].transform.localPosition = Vector3.Lerp(new Vector3(initPos.x, 0.0246f - depression, initPos.z), new Vector3(initPos.x, 0.0246f, initPos.z), timer / duration);
        }
        Buttons[0].transform.localPosition = new Vector3(initPos.x, 0.0246f, initPos.z);
    }

#pragma warning disable 414
    private string TwitchHelpMessage = "Use '!{0} hold 10' to hold the top button for 10 seconds (value can be between 1 second and 30 seocnds, inclusive). Use '!{0} 1 2 3' to press small buttons 1, 2 and 3.";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant();
        var commandArray = command.Split(' ');
        float o = 0;
        int p = 0;
        if (commandArray.Length == 2 && commandArray[0] == "hold" && float.TryParse(commandArray[1], out o))
        {
            float duration = float.Parse(command.Split(' ').Last());
            if (duration < 1 || duration > 30)
            {
                yield return "sendtochaterror Hold duration is out of range.";
                yield break;
            }
            Buttons[0].OnInteract();
            float timer = 0;
            while (timer < duration)
            {
                yield return null;
                timer += Time.deltaTime;
            }
            Buttons[0].OnInteractEnded();
        }
        else if (commandArray.Where(x => int.TryParse(x, out p)).Count() == commandArray.Count())
        {
            List<int> presses = new List<int>();
            for (int i = 0; i < commandArray.Length; i++)
            {
                var num = int.Parse(commandArray[i]);
                if (num < 1 || num > 20)
                {
                    yield return "sendtochaterror Button index is out of range.";
                    yield break;
                }
                presses.Add(num);
            }
            foreach (var press in presses)
            {
                Buttons[press].OnInteract();
                float timer = 0;
                while (timer < 0.2f)
                {
                    yield return null;
                    timer += Time.deltaTime;
                }
            }
        }
        else
        {
            yield return "sendtochaterror Invalid command.";
            yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        if (InputOrder.Count() > 0)
            InputOrder = new List<int>();
        while (!Solved)
        {
            var ans = FindAnAnswer();
            foreach (var press in ans)
            {
                Buttons[press].OnInteract();
                yield return true;
            }
        }
    }
}