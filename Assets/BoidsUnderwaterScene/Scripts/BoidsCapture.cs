#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BoidsUnderwaterScene
{
    public sealed class BoidsCapture : MonoBehaviour
    {
        private string output;
        private readonly List<string> errors=new List<string>();
        private Camera view;
        private RenderTexture target;
        private Texture2D readback;
        private BoidsSchool[] schools;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            string[] args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-boidsCapture");
            if(index<0||index+1>=args.Length)return;
            var component=new GameObject("BoidsCaptureValidation").AddComponent<BoidsCapture>();component.output=args[index+1];
        }
        private void OnEnable()=>Application.logMessageReceived+=Log;
        private void OnDisable()=>Application.logMessageReceived-=Log;
        private void Log(string message,string trace,LogType type){if(type==LogType.Error||type==LogType.Exception)errors.Add(message);}
        private IEnumerator Start()
        {
            Directory.CreateDirectory(output);view=Camera.main;schools=FindObjectsOfType<BoidsSchool>();
            view.GetComponent<UnderwaterCameraController>().enabled=false;
            Application.runInBackground=true;QualitySettings.vSyncCount=0;Application.targetFrameRate=-1;
            Time.captureFramerate=30;
            target=new RenderTexture(1600,900,24,RenderTextureFormat.ARGB32);target.Create();
            readback=new Texture2D(1600,900,TextureFormat.RGB24,false);
            double sum=0;long candidates=0;float maxError=0;int steps=900,penetrations=0;
            var schoolErrors=new float[schools.Length];
            var vortexStart=new Dictionary<BoidsSchool,Vector3[]>();
            float vortexTurn=0;int vortexSamples=0;
            int initialCount=0;
            float initialLow=float.PositiveInfinity,initialHigh=float.NegativeInfinity,initialSpeed=0;
            foreach(var school in schools)
                if(school.IsVortex)
                {
                    initialCount+=school.AgentCount;initialSpeed=school.AverageSpeed;
                    foreach(var p in school.Positions){initialLow=Mathf.Min(initialLow,p.y);initialHigh=Mathf.Max(initialHigh,p.y);}
                }
            for(int frame=0;frame<steps;frame++)
            {
                yield return null;
                if(frame==0)Capture("Startup.png");
                if(frame<180&&frame%2==0)Capture("startupFrame"+(frame/2).ToString("0000")+".png");
                foreach(var school in schools){sum+=school.LastSimulationMilliseconds;candidates+=school.CandidateTests;maxError=Mathf.Max(maxError,school.MaximumCorridorError);}
                for(int s=0;s<schools.Length;s++)schoolErrors[s]=Mathf.Max(schoolErrors[s],schools[s].MaximumCorridorError);
                if(frame==300)foreach(var school in schools)if(school.IsVortex)vortexStart[school]=(Vector3[])school.Positions.Clone();
                if(frame==330)
                    foreach(var pair in vortexStart)
                        for(int i=0;i<pair.Value.Length;i++)
                        {
                            Vector3 from=pair.Value[i]-pair.Key.VortexCenter,to=pair.Key.Positions[i]-pair.Key.VortexCenter;
                            from.y=0;to.y=0;vortexTurn+=Vector3.SignedAngle(from,to,Vector3.up);vortexSamples++;
                        }
                if(frame==60||frame==300||frame==600||frame==899)Capture("overview-"+frame+".png");
                if(frame==600)
                {
                    foreach(var school in schools)foreach(var p in school.Positions)
                        if(Physics.CheckSphere(p,.12f,1<<6,QueryTriggerInteraction.Ignore))penetrations++;
                }
            }
            Vector3 start=view.transform.position;Quaternion rotation=view.transform.rotation;
            for(int frame=0;frame<120;frame++)
            {
                view.transform.position=start+new Vector3(Mathf.Sin(frame/119f*Mathf.PI)*1.4f,0,frame/119f*2f);
                yield return null;yield return null;
                Capture("frame-"+frame.ToString("0000")+".png");
            }
            view.transform.position=new Vector3(0,12,16);view.transform.LookAt(new Vector3(0,28,35));yield return null;Capture("surface.png");
            view.transform.position=new Vector3(0,6,12);view.transform.LookAt(new Vector3(3,7,34));yield return null;Capture("arch.png");
            var vortexSchool=Array.Find(schools,school=>school.IsVortex);
            Vector3 vortexCenter=vortexSchool!=null?vortexSchool.VortexCenter:new Vector3(0,14.5f,62);
            view.transform.position=vortexCenter+new Vector3(0,2,-32);view.transform.LookAt(vortexCenter);yield return null;Capture("vortex.png");
            for(int frame=0;frame<90;frame++){yield return null;yield return null;Capture("vortexFrame"+frame.ToString("0000")+".png");}
            view.transform.position=new Vector3(2,6,-3);view.transform.LookAt(new Vector3(7,10,2));yield return null;Capture("bubbles.png");
            view.transform.position=new Vector3(0,13,-17);view.transform.LookAt(new Vector3(-9,1,0));yield return null;Capture("corals.png");
            view.transform.position=start;view.transform.rotation=rotation;
            Time.captureFramerate=0;
            for(int warmup=0;warmup<60;warmup++)yield return null;
            var frameTimes=new float[120];
            var renderWatch=new System.Diagnostics.Stopwatch();
            for(int sample=0;sample<frameTimes.Length;sample++)
            {
                yield return null;renderWatch.Restart();
                var oldTarget=view.targetTexture;var oldActive=RenderTexture.active;
                view.targetTexture=target;view.Render();RenderTexture.active=target;
                // A one-pixel readback forces GPU completion without PNG encoding
                readback.ReadPixels(new Rect(0,0,1,1),0,0,false);
                view.targetTexture=oldTarget;RenderTexture.active=oldActive;renderWatch.Stop();
                frameTimes[sample]=(float)renderWatch.Elapsed.TotalMilliseconds;
            }
            Array.Sort(frameTimes);
            var report=new Report{agents=0,frames=steps,averageSchoolCpuMs=sum/steps,averageCandidateTests=candidates/steps,maximumCorridorError=maxError,intersectionsAt20Seconds=penetrations,errors=errors.ToArray()};
            report.synchronousRenderMedianMs=frameTimes[60];report.synchronousRender95thMs=frameTimes[114];report.graphicsDevice=SystemInfo.graphicsDeviceName;
            report.startupVortexAgents=initialCount;report.startupVortexMinHeight=initialLow;report.startupVortexMaxHeight=initialHigh;report.startupVortexAverageSpeed=initialSpeed;
            report.vortexRotationDegreesPerSecond=vortexSamples>0?vortexTurn/vortexSamples:0;
            report.schools=new SchoolReport[schools.Length];
            for(int i=0;i<schools.Length;i++)
            {
                float distance=0;foreach(var position in schools[i].Positions)distance=Mathf.Max(distance,schools[i].DistanceToGuidance(position));
                report.schools[i]=new SchoolReport{name=schools[i].name,averageSpeed=schools[i].AverageSpeed,maximumGuidanceError=schoolErrors[i],geometricGuidanceDistanceAtEnd=distance};
            }
            foreach(var school in schools){report.agents+=school.AgentCount;report.archPassages+=school.ArchPassages;}
            foreach(var school in schools)
            {
                if(!school.IsVortex)continue;
                report.vortexAgents=school.AgentCount;
                report.vortexMinHeight=float.PositiveInfinity;report.vortexMaxHeight=float.NegativeInfinity;
                foreach(var p in school.Positions){report.vortexMinHeight=Mathf.Min(report.vortexMinHeight,p.y);report.vortexMaxHeight=Mathf.Max(report.vortexMaxHeight,p.y);}
            }
            File.WriteAllText(Path.Combine(output,"runtime-validation.json"),JsonUtility.ToJson(report,true));
            Application.Quit(errors.Count==0?0:2);
        }
        private void Capture(string name)
        {
            var old=RenderTexture.active;var oldTarget=view.targetTexture;
            view.targetTexture=target;view.Render();RenderTexture.active=target;
            readback.ReadPixels(new Rect(0,0,1600,900),0,0);readback.Apply();File.WriteAllBytes(Path.Combine(output,name),readback.EncodeToPNG());
            view.targetTexture=oldTarget;RenderTexture.active=old;
        }
        private void OnDestroy(){Time.captureFramerate=0;if(target!=null){target.Release();Destroy(target);}if(readback!=null)Destroy(readback);}
        [Serializable] private sealed class Report
        {
            public int agents,frames,intersectionsAt20Seconds,archPassages;
            public double averageSchoolCpuMs;
            public long averageCandidateTests;
            public float maximumCorridorError;
            public int vortexAgents;
            public int startupVortexAgents;
            public float startupVortexMinHeight,startupVortexMaxHeight,startupVortexAverageSpeed;
            public float vortexMinHeight,vortexMaxHeight;
            public float vortexRotationDegreesPerSecond;
            public SchoolReport[] schools;
            public float synchronousRenderMedianMs,synchronousRender95thMs;
            public string graphicsDevice;
            public string[] errors;
        }
        [Serializable] private sealed class SchoolReport
        {
            public string name;
            public float averageSpeed,maximumGuidanceError;
            public float geometricGuidanceDistanceAtEnd;
        }
    }
}
#endif
