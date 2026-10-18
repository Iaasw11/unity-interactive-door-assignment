using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

/// <summary>场景搭建辅助工具，只在编辑器中运行。正式交互全部由 Scripts 中的脚本完成。</summary>
public static class DoorLabBuilder
{
    private const string ScenePath = "Assets/Scenes/DoorDemo.unity";
    private static Material wall, floor, seam, trim, dark, metal, cyan, white, orange, glass;

    [MenuItem("Door Lab/0. Open demonstration scene _F5")]
    public static void OpenDemo()
    {
        if (!EditorApplication.isPlaying && EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            EditorSceneManager.OpenScene(ScenePath);
    }

    [MenuItem("Door Lab/1. Build demonstration scene")]
    public static void Build()
    {
        Directory.CreateDirectory("Assets/Scenes");
        Directory.CreateDirectory("Assets/Materials");
        Directory.CreateDirectory("Assets/Animations");
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        wall = Mat("Wall", new Color(.16f,.21f,.26f), .15f,.3f);
        floor = Mat("Floor", new Color(.12f,.16f,.2f), .35f,.48f);
        seam = Mat("Seam",new Color(.055f,.08f,.105f),.1f,.3f);
        trim = Mat("Trim",new Color(.28f,.36f,.42f),.7f,.5f);
        dark = Mat("Door",new Color(.06f,.14f,.19f),.65f,.55f);
        metal = Mat("Hardware",new Color(.58f,.67f,.71f),.8f,.7f);
        cyan = Mat("CyanEmission",new Color(.08f,.8f,.95f),.15f,.5f,true);
        white = Mat("WhiteEmission",new Color(.65f,.9f,1),.1f,.5f,true);
        orange = Mat("AmberEmission",new Color(1,.56f,.14f),.1f,.3f,true);
        glass = Mat("Window",new Color(.035f,.34f,.46f),.65f,.85f,true);

        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(.34f,.44f,.55f);
        RenderSettings.ambientEquatorColor = new Color(.23f,.29f,.35f);
        RenderSettings.ambientGroundColor = new Color(.12f,.16f,.2f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(.045f,.075f,.105f);
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = .018f;
        RenderSettings.skybox = null;
        QualitySettings.shadowDistance = 45;

        var architecture = new GameObject("Environment").transform;
        Cube("Floor", new Vector3(0,-.14f,1),new Vector3(18,.25f,22),floor,architecture);
        for(int x=-8;x<=8;x+=2) Cube("FloorSeam_X",new Vector3(x,-.007f,1),new Vector3(.025f,.015f,22),seam,architecture,false);
        for(int z=-9;z<=11;z+=2) Cube("FloorSeam_Z",new Vector3(0,-.006f,z),new Vector3(18,.015f,.025f),seam,architecture,false);
        Cube("EntranceWall_L",new Vector3(-5.2f,2.3f,0),new Vector3(7.5f,4.6f,.48f),wall,architecture);
        Cube("EntranceWall_R",new Vector3(5.2f,2.3f,0),new Vector3(7.5f,4.6f,.48f),wall,architecture);
        Cube("EntranceWall_Top",new Vector3(0,4.02f,0),new Vector3(2.9f,1.16f,.48f),wall,architecture);
        Cube("SideWall_L",new Vector3(-8.8f,2.3f,2),new Vector3(.35f,4.6f,18),wall,architecture);
        Cube("SideWall_R",new Vector3(8.8f,2.3f,2),new Vector3(.35f,4.6f,18),wall,architecture);
        Cube("BackWall",new Vector3(0,2.3f,8),new Vector3(18,4.6f,.4f),wall,architecture);
        Cube("Ceiling",new Vector3(0,4.7f,1),new Vector3(18,.2f,20),seam,architecture);
        for(int i=-1;i<=1;i++)
        {
            Cube("CeilingRail",new Vector3(i*4,4.48f,1),new Vector3(.22f,.12f,18),trim,architecture,false);
            Cube("CeilingLight",new Vector3(i*4,4.39f,1),new Vector3(.08f,.03f,17),white,architecture,false);
        }
        for(int sign=-1;sign<=1;sign+=2)
        {
            Cube("DoorFrame",new Vector3(sign*1.34f,1.7f,-.06f),new Vector3(.25f,3.4f,.72f),trim,architecture);
            Cube("FrameLight",new Vector3(sign*1.18f,1.68f,-.44f),new Vector3(.045f,3.2f,.035f),cyan,architecture,false);
            Cube("WallBelt",new Vector3(sign*5.1f,.34f,-.26f),new Vector3(7.25f,.08f,.04f),trim,architecture,false);
            Cube("FloorGuide",new Vector3(sign*1.5f,.008f,-3.9f),new Vector3(.055f,.012f,6.8f),cyan,architecture,false);
            for(int i=0;i<3;i++) Cube("WallPanel",new Vector3(sign*(3.0f+i*2.05f),2.1f,-.27f),new Vector3(1.75f,2.3f,.07f),seam,architecture,false);
        }
        Cube("Lintel",new Vector3(0,3.48f,-.06f),new Vector3(2.92f,.25f,.72f),trim,architecture);
        Cube("Threshold",new Vector3(0,.025f,0),new Vector3(2.5f,.05f,.85f),metal,architecture,false);
        Cube("HeaderSign",new Vector3(0,3.98f,-.3f),new Vector3(5.8f,.66f,.08f),seam,architecture,false);
        WorldText("MOTION  /  LAB",new Vector3(-2.57f,4.16f,-.36f),.18f,Color.white,architecture);
        WorldText("01   -   INTERACTIVE ACCESS",new Vector3(-2.55f,3.84f,-.36f),.073f,new Color(.25f,.85f,1),architecture);

        // 动画的根对象保持固定，Hinge 在门板左边缘，门板向右偏移半个宽度。
        var rig = new GameObject("DoorRig");
        var hinge = new GameObject("Hinge").transform;
        hinge.SetParent(rig.transform,false);
        hinge.localPosition = new Vector3(-1.12f,0,0);
        CubeLocal("DoorPanel",new Vector3(1.12f,1.62f,0),new Vector3(2.24f,3.24f,.19f),dark,hinge);
        CubeLocal("UpperWindowFrame",new Vector3(1.12f,2.37f,-.115f),new Vector3(1.67f,.52f,.05f),trim,hinge,false);
        CubeLocal("UpperWindow",new Vector3(1.12f,2.37f,-.15f),new Vector3(1.48f,.35f,.035f),glass,hinge,false);
        CubeLocal("LowerInset",new Vector3(1.12f,.57f,-.111f),new Vector3(1.68f,.52f,.03f),seam,hinge,false);
        for(int i=0;i<4;i++) CubeLocal("Vent",new Vector3(1.12f,.4f+i*.11f,-.14f),new Vector3(1.38f,.025f,.015f),trim,hinge,false);
        CubeLocal("DoorStripe",new Vector3(.19f,1.62f,-.112f),new Vector3(.055f,2.86f,.025f),cyan,hinge,false);
        CubeLocal("HandleMount",new Vector3(1.9f,1.45f,-.13f),new Vector3(.22f,.5f,.07f),trim,hinge,false);
        CubeLocal("Handle",new Vector3(1.92f,1.45f,-.23f),new Vector3(.07f,.37f,.12f),metal,hinge,false);
        WorldText("ACCESS 01",new Vector3(-.73f,1.88f,-.131f),.085f,new Color(.62f,.79f,.86f),hinge);
        var indicator = new GameObject("StatusLight");
        indicator.transform.SetParent(rig.transform,false);
        indicator.transform.localPosition = new Vector3(-1.95f,1.55f,-.6f);
        var point = indicator.AddComponent<Light>();point.type=LightType.Point;point.color=new Color(.08f,.85f,1);point.range=3;point.intensity=.25f;
        Cube("AccessTerminal",new Vector3(-2.05f,1.55f,-.38f),new Vector3(.62f,.94f,.2f),dark,architecture,false);
        Cube("TerminalDisplay",new Vector3(-2.05f,1.69f,-.49f),new Vector3(.4f,.34f,.025f),glass,architecture,false);
        Cube("TerminalLight",new Vector3(-2.05f,1.23f,-.49f),new Vector3(.3f,.035f,.025f),cyan,architecture,false);
        WorldText("LAB 01",new Vector3(-2.23f,1.72f,-.51f),.046f,Color.white,architecture);
        WorldText("AUTHORIZED ENTRY",new Vector3(1.85f,1.82f,-.33f),.07f,new Color(.5f,.7f,.8f),architecture);
        WorldText("E  /  INTERACT",new Vector3(1.85f,1.54f,-.33f),.09f,Color.white,architecture);
        WorldText("M  /  AUTO MODE",new Vector3(1.85f,1.27f,-.33f),.07f,new Color(.5f,.7f,.8f),architecture);

        for(int side=-1;side<=1;side+=2)
        {
            Cube("WorkBench",new Vector3(side*4.2f,1.02f,4.8f),new Vector3(3.0f,.17f,1.6f),trim,architecture);
            Cube("BenchBase",new Vector3(side*4.2f,.47f,4.8f),new Vector3(2.6f,.9f,1.35f),dark,architecture);
            Cube("Monitor",new Vector3(side*4.2f,1.67f,5.18f),new Vector3(1.7f,.9f,.12f),seam,architecture,false);
            Cube("MonitorDisplay",new Vector3(side*4.2f,1.67f,5.1f),new Vector3(1.48f,.68f,.035f),glass,architecture,false);
            Cube("MonitorStand",new Vector3(side*4.2f,1.18f,5.18f),new Vector3(.18f,.4f,.18f),metal,architecture,false);
        }
        Cube("ExhibitPlinth",new Vector3(0,.23f,5.2f),new Vector3(2.3f,.45f,2.3f),dark,architecture);
        Cube("ExhibitRim",new Vector3(0,.48f,5.2f),new Vector3(2.25f,.035f,2.25f),cyan,architecture,false);
        var exhibit=Cube("Sculpture",new Vector3(0,1.55f,5.2f),new Vector3(1.1f,1.1f,1.1f),metal,architecture,false);
        exhibit.transform.rotation=Quaternion.Euler(25,35,25);
        WorldText("VIRTUAL REALITY",new Vector3(-3.2f,3.35f,7.74f),.24f,new Color(.65f,.9f,1),architecture);
        WorldText("ANIMATION   -   INTERACTION   -   EXPERIENCE",new Vector3(-3.2f,2.83f,7.73f),.075f,Color.white,architecture);

        var sun=new GameObject("KeyLight");var light=sun.AddComponent<Light>();light.type=LightType.Directional;light.color=new Color(.85f,.93f,1);light.intensity=1.05f;light.shadows=LightShadows.Soft;sun.transform.rotation=Quaternion.Euler(46,-32,0);
        var fill=new GameObject("InteriorLight");var inside=fill.AddComponent<Light>();inside.type=LightType.Point;inside.color=new Color(.23f,.7f,1);inside.intensity=2.8f;inside.range=12;fill.transform.position=new Vector3(0,3.8f,4);

        var player=new GameObject("Player");player.tag="Player";player.transform.position=new Vector3(0,.08f,-5.8f);
        var cc=player.AddComponent<CharacterController>();cc.height=1.8f;cc.radius=.28f;cc.center=new Vector3(0,.9f,0);cc.stepOffset=.25f;
        var fps=player.AddComponent<PlayerController>();
        var cameraObject=new GameObject("Main Camera");cameraObject.tag="MainCamera";cameraObject.transform.SetParent(player.transform,false);cameraObject.transform.localPosition=new Vector3(0,1.65f,0);
        var cam=cameraObject.AddComponent<Camera>();cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.035f,.06f,.08f);cam.fieldOfView=64;cam.nearClipPlane=.05f;cam.farClipPlane=100;cameraObject.AddComponent<AudioListener>();fps.view=cameraObject.transform;

        var animator=rig.AddComponent<Animator>();animator.runtimeAnimatorController=CreateAnimations();animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
        rig.AddComponent<AudioSource>();var control=rig.AddComponent<DoorController>();control.player=player.transform;control.hinge=hinge;control.openSound=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/DoorOpen.wav");control.closeSound=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/DoorClose.wav");
        var trigger=new GameObject("DoorSensor");trigger.transform.position=new Vector3(0,1.35f,0);var box=trigger.AddComponent<BoxCollider>();box.isTrigger=true;box.size=new Vector3(3.6f,2.7f,4.7f);var rb=trigger.AddComponent<Rigidbody>();rb.isKinematic=true;rb.useGravity=false;var sensor=trigger.AddComponent<DoorTrigger>();sensor.door=control;control.sensor=sensor;
        CreateHud(control,fps);
        RefreshPresentation();

        PlayerSettings.companyName="Coursework";PlayerSettings.productName="Motion Lab - Interactive Door";
        PlayerSettings.defaultScreenWidth=1600;PlayerSettings.defaultScreenHeight=900;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
        PlayerSettings.runInBackground=true;
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),ScenePath);
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
        AssetDatabase.SaveAssets();Selection.activeGameObject=rig;
        SceneView.lastActiveSceneView?.LookAt(new Vector3(0,1.6f,0),Quaternion.Euler(5,0,0),7);
        Debug.Log("DOOR_LAB_BUILD_OK");
    }

    private static Material Mat(string name,Color color,float metallic,float smooth,bool emission=false)
    {
        string path="Assets/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(m==null){m=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(m,path);}
        m.color=color;m.SetFloat("_Metallic",metallic);m.SetFloat("_Glossiness",smooth);
        if(emission){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",color*1.25f);}
        EditorUtility.SetDirty(m);return m;
    }

    private static GameObject Cube(string name,Vector3 pos,Vector3 scale,Material mat,Transform parent,bool collision=true)
    {
        var obj=GameObject.CreatePrimitive(PrimitiveType.Cube);obj.name=name;obj.transform.SetParent(parent,false);obj.transform.position=pos;obj.transform.localScale=scale;obj.GetComponent<Renderer>().sharedMaterial=mat;if(!collision)Object.DestroyImmediate(obj.GetComponent<Collider>());return obj;
    }
    private static GameObject CubeLocal(string name,Vector3 pos,Vector3 scale,Material mat,Transform parent,bool collision=true)
    {
        var obj=Cube(name,Vector3.zero,scale,mat,parent,collision);obj.transform.localPosition=pos;return obj;
    }
    private static void WorldText(string text,Vector3 pos,float size,Color color,Transform parent)
    {
        var obj=new GameObject(text);obj.transform.SetParent(parent,true);obj.transform.position=pos;
        var tm=obj.AddComponent<TextMesh>();tm.text=text;tm.fontSize=64;tm.characterSize=size;tm.color=color;tm.anchor=TextAnchor.UpperLeft;tm.font=Resources.GetBuiltinResource<Font>("Arial.ttf");obj.GetComponent<Renderer>().sharedMaterial=tm.font.material;
    }

    [MenuItem("Door Lab/7. Refresh configuration %#&p")]
    public static void RefreshPresentation()
    {
        // TextMesh 的字形尺寸与 fontSize 相关；缩放换算成所需的场景米制尺寸。
        string path="Assets/Materials/WorldText.mat";
        var textMaterial=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(textMaterial==null)
        {
            textMaterial=new Material(Shader.Find("DoorLab/WorldText"));
            AssetDatabase.CreateAsset(textMaterial,path);
        }
        var font=Resources.GetBuiltinResource<Font>("Arial.ttf");
        textMaterial.mainTexture=font.material.mainTexture;
        foreach(var text in Object.FindObjectsOfType<TextMesh>())
        {
            text.transform.localScale=Vector3.one*(10f/64f);
            text.GetComponent<Renderer>().sharedMaterial=textMaterial;
        }
        RenderSettings.ambientSkyColor=new Color(.62f,.72f,.82f);
        RenderSettings.ambientEquatorColor=new Color(.45f,.54f,.62f);
        RenderSettings.ambientGroundColor=new Color(.26f,.31f,.36f);
        var front=GameObject.Find("EntranceFill");if(front==null)front=new GameObject("EntranceFill");
        front.transform.position=new Vector3(0,3.5f,-4.3f);
        var lamp=front.GetComponent<Light>();if(lamp==null)lamp=front.AddComponent<Light>();
        lamp.type=LightType.Point;lamp.color=new Color(.68f,.84f,1);lamp.range=14;lamp.intensity=3.5f;
        EditorUtility.SetDirty(textMaterial);
        // 运动片段播放结束后无条件进入静止态，再由 Open 参数决定下一个动作。
        var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Animations/DoorAnimator.controller");
        if(controller!=null)
            foreach(var state in controller.layers[0].stateMachine.states)
                if(state.state.name=="Opening" || state.state.name=="Closing")
                    foreach(var transition in state.state.transitions)
                    { transition.conditions=new AnimatorCondition[0];EditorUtility.SetDirty(transition); }
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        SaveAll();
        SetGameResolution();
    }

    private static void SetGameResolution()
    {
        // 仅影响编辑器预览尺寸，项目玩家设置仍为 1600×900 窗口。
        var assembly=typeof(Editor).Assembly;
        var sizesType=assembly.GetType("UnityEditor.GameViewSizes");
        var singleton=typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
        var sizes=singleton.GetProperty("instance",BindingFlags.Public|BindingFlags.Static|BindingFlags.FlattenHierarchy).GetValue(null,null);
        var groupType=assembly.GetType("UnityEditor.GameViewSizeGroupType");
        var group=sizesType.GetMethod("GetGroup").Invoke(sizes,new[]{System.Enum.Parse(groupType,"Standalone")});
        var groupClass=group.GetType();
        int built=(int)groupClass.GetMethod("GetBuiltinCount").Invoke(group,null);
        int custom=(int)groupClass.GetMethod("GetCustomCount").Invoke(group,null);
        int index=-1;
        for(int i=0;i<built+custom;i++)
        {
            var item=groupClass.GetMethod("GetGameViewSize").Invoke(group,new object[]{i});
            int width=(int)item.GetType().GetProperty("width").GetValue(item,null);
            int height=(int)item.GetType().GetProperty("height").GetValue(item,null);
            if(width==1600 && height==900){index=i;break;}
        }
        if(index<0)
        {
            var sizeType=assembly.GetType("UnityEditor.GameViewSize");
            var enumType=assembly.GetType("UnityEditor.GameViewSizeType");
            var size=System.Activator.CreateInstance(sizeType,BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance,null,new object[]{System.Enum.Parse(enumType,"FixedResolution"),1600,900,"Door Lab 1600x900"},null);
            groupClass.GetMethod("AddCustomSize").Invoke(group,new[]{size});index=built+custom;
        }
        var viewType=assembly.GetType("UnityEditor.GameView");var view=EditorWindow.GetWindow(viewType);
        viewType.GetProperty("selectedSizeIndex",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(view,index,null);
        view.Repaint();
    }

    private static RuntimeAnimatorController CreateAnimations()
    {
        var closed=Clip("Door_Closed",0,0,.25f,.25f,.1f);
        var opening=Clip("Door_Open",0,-102,.25f,2.2f,1.2f);
        var opened=Clip("Door_Opened",-102,-102,2.2f,2.2f,.1f);
        var closing=Clip("Door_Close",-102,0,2.2f,.25f,1.2f);
        string path="Assets/Animations/DoorAnimator.controller";
        var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if(controller!=null)AssetDatabase.DeleteAsset(path);
        controller=AnimatorController.CreateAnimatorControllerAtPath(path);controller.AddParameter("Open",AnimatorControllerParameterType.Bool);
        var sm=controller.layers[0].stateMachine;
        var s0=sm.AddState("Closed",new Vector3(160,80));s0.motion=closed;sm.defaultState=s0;
        var s1=sm.AddState("Opening",new Vector3(390,80));s1.motion=opening;
        var s2=sm.AddState("Opened",new Vector3(390,240));s2.motion=opened;
        var s3=sm.AddState("Closing",new Vector3(160,240));s3.motion=closing;
        Transition(s0,s1,false,true);Transition(s1,s2,true,true);Transition(s2,s3,false,false);Transition(s3,s0,true,false);
        return controller;
    }
    private static void Transition(AnimatorState a,AnimatorState b,bool exit,bool value)
    {
        var t=a.AddTransition(b);t.hasExitTime=exit;t.exitTime=1;t.duration=0;t.hasFixedDuration=true;
        if(!exit)t.AddCondition(value?AnimatorConditionMode.If:AnimatorConditionMode.IfNot,0,"Open");
    }
    private static AnimationClip Clip(string name,float from,float to,float lightFrom,float lightTo,float length)
    {
        string path="Assets/Animations/"+name+".anim";var c=AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if(c==null){c=new AnimationClip();c.name=name;AssetDatabase.CreateAsset(c,path);}
        c.frameRate=60;
        AnimationUtility.SetEditorCurve(c,EditorCurveBinding.FloatCurve("Hinge",typeof(Transform),"localEulerAnglesRaw.y"),AnimationCurve.EaseInOut(0,from,length,to));
        AnimationUtility.SetEditorCurve(c,EditorCurveBinding.FloatCurve("StatusLight",typeof(Light),"m_Intensity"),AnimationCurve.EaseInOut(0,lightFrom,length,lightTo));
        var settings=AnimationUtility.GetAnimationClipSettings(c);settings.loopTime=false;AnimationUtility.SetAnimationClipSettings(c,settings);EditorUtility.SetDirty(c);return c;
    }

    private static RectTransform Rect(string name,Transform parent,Vector2 min,Vector2 max,Vector2 offsetMin,Vector2 offsetMax)
    {
        var obj=new GameObject(name,typeof(RectTransform));var rt=obj.GetComponent<RectTransform>();rt.SetParent(parent,false);rt.anchorMin=min;rt.anchorMax=max;rt.offsetMin=offsetMin;rt.offsetMax=offsetMax;return rt;
    }
    private static Text Label(string name,string value,Transform parent,int size,Color color,TextAnchor align,Vector2 min,Vector2 max,Vector2 a,Vector2 b)
    {
        var rt=Rect(name,parent,min,max,a,b);var t=rt.gameObject.AddComponent<Text>();t.text=value;t.font=Resources.GetBuiltinResource<Font>("Arial.ttf");t.fontSize=size;t.color=color;t.alignment=align;t.raycastTarget=false;t.horizontalOverflow=HorizontalWrapMode.Overflow;return t;
    }
    private static Image Panel(string name,Transform parent,Color color,Vector2 min,Vector2 max,Vector2 a,Vector2 b)
    {
        var rt=Rect(name,parent,min,max,a,b);var i=rt.gameObject.AddComponent<Image>();i.color=color;i.raycastTarget=false;return i;
    }
    private static void CreateHud(DoorController door,PlayerController player)
    {
        var obj=new GameObject("HUD",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));var canvas=obj.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        var scaler=obj.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.matchWidthOrHeight=.5f;
        var hud=obj.AddComponent<LabHud>();hud.door=door;hud.player=player;var root=obj.transform;
        Panel("HeaderShade",root,new Color(.025f,.045f,.065f,.86f),new Vector2(0,1),new Vector2(1,1),new Vector2(0,-112),Vector2.zero);
        Panel("Accent",root,new Color(.14f,.82f,.94f),new Vector2(0,1),new Vector2(0,1),new Vector2(40,-82),new Vector2(45,-30));
        Label("Brand","MOTION / LAB",root,29,Color.white,TextAnchor.MiddleLeft,new Vector2(0,1),new Vector2(0,1),new Vector2(62,-65),new Vector2(480,-24));
        Label("Course","交互式开关门动画系统  /  UNITY ANIMATION",root,14,new Color(.6f,.76f,.83f),TextAnchor.MiddleLeft,new Vector2(0,1),new Vector2(0,1),new Vector2(63,-91),new Vector2(680,-63));
        hud.modeText=Label("Mode","MANUAL",root,20,new Color(.3f,.89f,1),TextAnchor.MiddleRight,new Vector2(1,1),new Vector2(1,1),new Vector2(-400,-65),new Vector2(-50,-26));
        hud.stateText=Label("State","CLOSED",root,14,new Color(.73f,.84f,.87f),TextAnchor.MiddleRight,new Vector2(1,1),new Vector2(1,1),new Vector2(-300,-91),new Vector2(-50,-63));
        hud.statusDot=Panel("StatusIndicator",root,new Color(.25f,.85f,1),new Vector2(1,1),new Vector2(1,1),new Vector2(-38,-82),new Vector2(-29,-73));
        Panel("PromptShade",root,new Color(.025f,.045f,.065f,.87f),new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(-365,90),new Vector2(365,200));
        Panel("PromptLine",root,new Color(.12f,.78f,.91f),new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(-365,197),new Vector2(365,200));
        hud.promptText=Label("Prompt","APPROACH TO INTERACT",root,26,Color.white,TextAnchor.MiddleCenter,new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(-350,139),new Vector2(350,185));
        hud.subtitleText=Label("PromptDetail","WASD / MOUSE / E",root,15,new Color(.61f,.77f,.83f),TextAnchor.MiddleCenter,new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(-350,104),new Vector2(350,139));
        Label("Controls","WASD 移动   /   鼠标观察   /   E 开关门   /   M 切换模式   /   R 返回起点   /   Esc 释放鼠标",root,14,new Color(.75f,.83f,.86f),TextAnchor.MiddleCenter,new Vector2(0,0),new Vector2(1,0),new Vector2(30,24),new Vector2(-30,58));
        Label("Crosshair","+",root,22,new Color(.65f,.95f,1,.8f),TextAnchor.MiddleCenter,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(-15,-15),new Vector2(15,15));
        new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
    }

    [MenuItem("Door Lab/2. Record opening clip _F6")]
    public static void RecordOpening() { PrepareRecording("Door_Open"); }
    [MenuItem("Door Lab/3. Record closing clip _F7")]
    public static void RecordClosing() { PrepareRecording("Door_Close"); }
    private static void PrepareRecording(string clip)
    {
        if(EditorApplication.isPlaying)return;
        var door=Object.FindObjectOfType<DoorController>();if(door==null)return;
        Selection.activeGameObject=door.gameObject;
        var window=EditorWindow.GetWindow<AnimationWindow>();window.position=new Rect(350,210,960,480);window.Show();
        EditorApplication.delayCall+=()=>
        {
            typeof(AnimationWindow).GetProperty("animationClip")?.SetValue(window,AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Animations/"+clip+".anim"),null);
            typeof(AnimationWindow).GetProperty("time")?.SetValue(window,1.2f,null);
            Selection.activeGameObject=door.hinge.gameObject;
            window.Repaint();
        };
    }

    [MenuItem("Door Lab/4. Save all %#&s")]
    public static void SaveAll() { EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();Debug.Log("DOOR_LAB_SAVED"); }

    [MenuItem("Door Lab/6. Build Windows player")]
    public static void BuildPlayer()
    {
        string output=Path.GetFullPath("../DoorLab-Windows/DoorLab.exe");Directory.CreateDirectory(Path.GetDirectoryName(output));
        var result=BuildPipeline.BuildPlayer(new[]{ScenePath},output,BuildTarget.StandaloneWindows64,BuildOptions.None);
        Debug.Log("DOOR_LAB_PLAYER_BUILD: "+result.summary.result+" / "+result.summary.totalSize);
    }
}
