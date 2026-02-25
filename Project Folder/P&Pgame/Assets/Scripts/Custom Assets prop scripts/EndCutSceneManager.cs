using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;

public class EndCutSceneManager : MonoBehaviour
{
    [Header("Ref")]
    [SerializeField] PlayableDirector director;
    [SerializeField] CinemachineBrain cineBrain;
    [SerializeField] GameObject cutSceneCameraRoot;

    [SerializeField] GameObject player;
    [SerializeField] MonoBehaviour[] scriptsToDisableDuringCutScene;
    [SerializeField] GameObject missionCompleteUI; //UI to show at the end
    [SerializeField] CanvasGroup missionCompleteGroup;
    [SerializeField] float returnToMenuDelay = 7f;
    [SerializeField] int mainMenuBuildIndex = 1;
    [SerializeField] float uiFadeTime = 1f;

    [SerializeField] GameObject handCam;
    [SerializeField] GameObject playerHud;


    [Header("CutScene Events")]
    [SerializeField] ShipDoorController shipDoors;
    [SerializeField] ShipGlowController shipGlow;
    [SerializeField] ParticleSystem[] underShipSmoke;
    [SerializeField] Rigidbody shipRigidbody;
    [SerializeField] Transform shipTransform;


    int originalCullingMask;
    bool started;


    private void Awake()
    {
        //Important: prevent takeover at game start
        if (director) director.playOnAwake = false;

        if (cutSceneCameraRoot) cutSceneCameraRoot.SetActive(false);
        if (cineBrain) cineBrain.enabled = false;

        if (missionCompleteUI) missionCompleteUI.SetActive(false);

        if (director) director.stopped += OnDirectorStopped;
    }

    private void OnDestroy()
    {
        if(director)
            director.stopped -= OnDirectorStopped;
    }

    void OnDirectorStopped(PlayableDirector director)
    {
        if (cutSceneCameraRoot) cutSceneCameraRoot.SetActive(false);
        if (cineBrain) cineBrain.enabled = false;

        if (missionCompleteUI && !missionCompleteUI.activeSelf) missionCompleteUI.SetActive(true);

        //If we want the controls back after cutscene

        //foreach (var script in scriptsToDisableDuringCutScene)
        //    if (script) script.enabled = true;
    }

    public void StartEndCutscene()
    {
        if (started) return;
        started = true;

        //diable player control
        foreach(var script in scriptsToDisableDuringCutScene)
            if (script) script.enabled = false;

        if (cutSceneCameraRoot) cutSceneCameraRoot.SetActive(true);
        if (cineBrain) cineBrain.enabled = true;

        if (handCam)
            handCam.SetActive(false);

        if (playerHud) playerHud.SetActive(false);

        if (director)
        {
            director.time = 0;
            director.Evaluate();
            director.Play();
        }
        else
        {
            ShowMissionComplete();
        }
    }

    // --- These methods get called by TimeLine Signals ---

    public void GlowOn() => shipGlow?.SetGlow(true);
    public void GlowOff() => shipGlow?.SetGlow(false);

    public void CloseDoors()
    {
        if (!shipDoors) return;

        if (shipDoors.isOpen)
            shipDoors.Close();
    }

    public void StartSmoke()
    {
        foreach(var ps in underShipSmoke)
            if (ps) ps.Play();
    }

    public void StopSmoke()
    {
        foreach(var ps in underShipSmoke)
            if (ps) ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    public void ShowMissionComplete()
    {
        if (missionCompleteUI)
            missionCompleteUI.SetActive(true);

        if (missionCompleteGroup)
        {
            missionCompleteGroup.alpha = 0f;
            StartCoroutine(FadeCanvas(missionCompleteGroup, 1f, uiFadeTime));
        }

        StartCoroutine(ReturnToMenuAfterDelay());
    }
    IEnumerator ReturnToMenuAfterDelay()
    {
        yield return new WaitForSeconds(returnToMenuDelay);
        UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuBuildIndex);
    }
    IEnumerator FadeCanvas(CanvasGroup cg, float target, float time)
    {
        float start = cg.alpha;
        float t = 0f;
        while (t < time)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, target, t / time);
            yield return null;
        }
        cg.alpha = target;
    }
}
