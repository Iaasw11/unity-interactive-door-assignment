using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>可重复运行的场景验收。实际进入 Play 模式，验证按键入口和真实触发器行为。</summary>
[InitializeOnLoad]
public static class DoorLabValidation
{
    private static int stage;
    private static double next;
    private static DoorController door;
    private static PlayerController player;
    private static readonly List<string> results=new List<string>();
    private static readonly string Evidence=Path.GetFullPath(Path.Combine(Application.dataPath,"../Docs/Verification"));

    static DoorLabValidation() { EditorApplication.playModeStateChanged+=OnPlay; }

    [MenuItem("Door Lab/5. Run acceptance checks %#&t")]
    public static void Run()
    {
        if(EditorApplication.isPlaying)return;
        Directory.CreateDirectory(Evidence);results.Clear();stage=0;
        // SessionState 跨越进入 Play 模式时的程序集重载，避免验收回调丢失。
        SessionState.SetBool("DoorLab.AcceptanceRequested",true);
        EditorApplication.isPlaying=true;
    }
    private static void OnPlay(PlayModeStateChange change)
    {
        if(change==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("DoorLab.AcceptanceRequested",false))
        {
            SessionState.SetBool("DoorLab.AcceptanceRequested",false);
            stage=0;results.Clear();Directory.CreateDirectory(Evidence);
            door=UnityEngine.Object.FindObjectOfType<DoorController>();
            player=UnityEngine.Object.FindObjectOfType<PlayerController>();
            next=Time.time+1;
            EditorApplication.update+=Step;
        }
        if(change==PlayModeStateChange.ExitingPlayMode)
        {
            EditorApplication.update-=Step;
        }
    }
    private static void Check(bool success,string name)
    {
        string line=(success?"PASS":"FAIL")+" | "+name;
        results.Add(line);if(success)Debug.Log(line);else Debug.LogError(line);
    }
    private static float Angle { get { return Mathf.Abs(Mathf.DeltaAngle(0,door.hinge.localEulerAngles.y)); } }
    private static void Shot(string filename) { ScreenCapture.CaptureScreenshot(Path.Combine(Evidence,filename+".png")); }
    private static void Wait(double seconds) { next=Time.time+seconds;stage++; }
    private static void Step()
    {
        if(!EditorApplication.isPlaying || Time.time<next)return;
        try
        {
            switch(stage)
            {
                case 0:
                    player.ResetPosition();player.LockCursor(false);
                    Check(door!=null && player!=null,"Scene references and player exist");
                    Check(!door.IsOpen && !door.IsMoving && Angle<1,"Initial closed state");
                    Check(door.openSound!=null && door.closeSound!=null,"Both audio clips assigned");
                    Check(door.sensor.GetComponent<Collider>().isTrigger && door.sensor.GetComponent<Rigidbody>().isKinematic,"Trigger and kinematic Rigidbody configured");
                    Check(!door.TryInteract(),"Far E interaction is rejected");
                    Shot("01-overview");player.Teleport(new Vector3(0,.08f,-2.15f));Wait(.5);break;
                case 1:
                    Check(door.PlayerNearby,"Near interaction range recognized");
                    Check(door.TryInteract(),"Near E interaction opens door");
                    Check(door.GetComponent<AudioSource>().isPlaying,"Opening audio starts with motion");
                    Check(!door.TryInteract(),"Repeated input during motion is ignored");
                    Wait(1.6);break;
                case 2:
                    Check(door.IsOpen && !door.IsMoving && Angle>100 && Angle<104,"Manual opening reaches 102 degrees");
                    Check(door.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Opened"),"Animator enters Opened state");
                    Check(door.transform.Find("StatusLight").GetComponent<Light>().intensity>2,"Opening animation raises light intensity");
                    Shot("02-manual-open");Wait(.3);break;
                case 3:
                    Check(door.TryInteract(),"Near E interaction closes door");
                    Check(door.GetComponent<AudioSource>().isPlaying,"Closing audio starts with motion");Wait(1.6);break;
                case 4:
                    Check(!door.IsOpen && !door.IsMoving && Angle<1,"Manual closing reaches zero degrees");
                    Shot("03-manual-closed");player.Teleport(new Vector3(0,.08f,-5.8f));door.SetAutomaticMode(true);Wait(.7);break;
                case 5:
                    Check(door.sensor.OccupantCount==0,"Sensor empty after player leaves");
                    player.Teleport(new Vector3(0,.08f,-2.0f));Wait(2);break;
                case 6:
                    Check(door.sensor.OccupantCount>0,"Physics trigger detects entering player");
                    Check(door.IsOpen && Angle>100,"Automatic entry opens door");Shot("04-auto-open");
                    player.Teleport(new Vector3(0,.08f,-5.8f));Wait(2);break;
                case 7:
                    Check(door.sensor.OccupantCount==0,"Physics trigger detects exiting player");
                    Check(!door.IsOpen && Angle<1,"Automatic exit closes door");Shot("05-auto-closed");
                    door.SetAutomaticMode(false);player.Teleport(new Vector3(0,.08f,-2.15f));Wait(.5);break;
                case 8:
                    door.RequestOpen(true);door.RequestOpen(false);Wait(2.9);break;
                case 9:
                    Check(!door.IsMoving && !door.IsOpen && Angle<1,"Queued reverse request finishes without snapping");
                    player.ResetPosition();door.SetAutomaticMode(false);Wait(.5);break;
                case 10:
                    var open=AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Animations/Door_Open.anim");
                    Check(AnimationUtility.GetCurveBindings(open).Length>=2,"Opening clip contains hinge and light keyframes");
                    var light=door.transform.Find("StatusLight").GetComponent<Light>();
                    Check(Mathf.Abs(light.intensity-.25f)<.1f,"Status light returns to closed intensity");
                    File.WriteAllLines(Path.Combine(Evidence,"acceptance-results.txt"),results);
                    Debug.Log("DOOR_LAB_VALIDATION_COMPLETE: "+results.Count+" checks");
                    EditorApplication.update-=Step;player.LockCursor(false);Shot("06-final-overview");break;
            }
        }
        catch(Exception ex)
        {
            results.Add("FAIL | "+ex);File.WriteAllLines(Path.Combine(Evidence,"acceptance-results.txt"),results);
            Debug.LogException(ex);EditorApplication.update-=Step;
        }
    }
}
