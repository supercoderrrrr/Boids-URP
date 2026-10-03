using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CinematicReef.Editor
{
    public static class ReefValidation
    {
        [MenuItem("Tools/Boids/Validate Boids")]
        public static void Validate()
        {
            var issues=new List<string>();
            int foregroundCorals=0,backgroundCorals=0,floatingCorals=0,highCorals=0;
            foreach(var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                foreach(var transform in root.GetComponentsInChildren<Transform>(true))
                {
                    if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject)>0)issues.Add("Missing script "+transform.name);
                    if(!System.Text.RegularExpressions.Regex.IsMatch(transform.name,"^[A-Z][a-zA-Z0-9]*$"))issues.Add("Non-PascalCase object "+transform.name);
                    bool foreground=transform.name.StartsWith("ForegroundCoral");
                    if(!foreground&&!transform.name.StartsWith("BackgroundCoral"))continue;
                    if(foreground)foregroundCorals++;else backgroundCorals++;
                    var points=ReefSceneBuilder.CoralBasePoints(transform.gameObject);
                    float nearest=float.PositiveInfinity;
                    foreach(var point in points)
                        if(Physics.Raycast(point+Vector3.up*.15f,Vector3.down,out var hit,5,1<<6,QueryTriggerInteraction.Ignore))nearest=Mathf.Min(nearest,Mathf.Abs(point.y-hit.point.y));
                    if(nearest>.08f){floatingCorals++;issues.Add("Unsupported coral "+transform.name+" gap="+nearest);}
                    if(points.Count>0&&points[0].y>6){highCorals++;issues.Add("Coral placed on tall rock "+transform.name);}
                }
            foreach(var renderer in UnityEngine.Object.FindObjectsOfType<Renderer>())
                foreach(var material in renderer.sharedMaterials)
                    if(material==null||material.shader==null||material.shader.name=="Hidden/InternalErrorShader")issues.Add("Missing material "+renderer.name);
            foreach(string guid in AssetDatabase.FindAssets("t:Shader",new[]{ReefSceneBuilder.Root}))
            {
                var shader=AssetDatabase.LoadAssetAtPath<Shader>(AssetDatabase.GUIDToAssetPath(guid));
                foreach(var message in ShaderUtil.GetShaderMessages(shader))
                    if(message.severity.ToString()=="Error")issues.Add(shader.name+": "+message.message);
            }
            ValidateGrid();
            ValidateLegacyFixes();
            ValidateSchoolIsolation();
            Physics.SyncTransforms();
            int blocked=0,total=0;
            foreach(var school in UnityEngine.Object.FindObjectsOfType<ReefSchool>())
            {
                int localBlocked=0;
                foreach(var p in school.SampleRouteForValidation())
                {
                    total++;if(float.IsNaN(p.x)||float.IsInfinity(p.y))issues.Add("Non-finite path "+school.name);
                    var overlap=Physics.OverlapSphere(p,.45f,1<<6,QueryTriggerInteraction.Ignore);
                    if(overlap.Length>0){blocked++;localBlocked++;if(localBlocked<=6)Debug.Log("REEF_ROUTE_BLOCKED "+school.name+" p="+p+" collider="+overlap[0].name);}
                }
                Debug.Log("REEF_ROUTE "+school.name+" blockedSamples="+localBlocked);
            }
            if(blocked>0)issues.Add("One or more swim route samples intersect an obstacle");
            if(foregroundCorals<32||foregroundCorals>48)issues.Add("Unexpected foreground colony count "+foregroundCorals);
            if(backgroundCorals!=105)issues.Add("Background colonies were not preserved");
            var report=new Report{passed=issues.Count==0,gridMatchesBruteForce=true,legacyRegressionChecks=true,schoolsIndependent=true,initialVortexReady=true,routeSamples=total,blockedRouteSamples=blocked,foregroundCorals=foregroundCorals,backgroundCorals=backgroundCorals,floatingCorals=floatingCorals,highCorals=highCorals,issues=issues.ToArray()};
            Directory.CreateDirectory("docs");
            File.WriteAllText("docs/boids-editor-validation.json",JsonUtility.ToJson(report,true));
            if(issues.Count>0)throw new InvalidOperationException(string.Join("\n",issues));
            Debug.Log("REEF_VALIDATION_PASSED grid=exact blockedRouteSamples="+blocked);
        }
        private static void ValidateGrid()
        {
            var random=new System.Random(2026);var positions=new Vector3[257];
            for(int i=0;i<positions.Length;i++)positions[i]=new Vector3((float)random.NextDouble()*30-15,(float)random.NextDouble()*20-10,(float)random.NextDouble()*30-15);
            positions[0]=new Vector3(-3.2f,0,0);positions[1]=new Vector3(-.00001f,0,0);positions[2]=new Vector3(3.2f,0,0);
            var grid=new ReefSpatialGrid(positions.Length,3.2f);grid.Rebuild(positions);
            for(int i=0;i<positions.Length;i++)
            {
                var found=new HashSet<int>();Vector3Int cell=grid.Cell(positions[i]);
                for(int z=-1;z<=1;z++)for(int y=-1;y<=1;y++)for(int x=-1;x<=1;x++)
                    for(int j=grid.First(cell+new Vector3Int(x,y,z));j>=0;j=grid.Next(j))
                        if(j!=i&&(positions[i]-positions[j]).sqrMagnitude<3.2f*3.2f)found.Add(j);
                for(int j=0;j<positions.Length;j++)
                    if(j!=i&&found.Contains(j)!=((positions[i]-positions[j]).sqrMagnitude<3.2f*3.2f))throw new InvalidOperationException("Grid query differs from brute force");
            }
        }
        private static void ValidateLegacyFixes()
        {
            var obj=new GameObject("Legacy steering regression");
            try
            {
                var agent=obj.AddComponent<BoidAgent>();
                var method=typeof(BoidAgent).GetMethod("SteerTowards",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
                Vector3 result=(Vector3)method.Invoke(agent,new object[]{Vector3.zero});
                if(result!=Vector3.zero)throw new InvalidOperationException("Zero steering must not brake an agent");
                var camera=obj.AddComponent<CameraController>();obj.transform.rotation=Quaternion.Euler(-10,25,0);
                var serialized=new SerializedObject(camera);serialized.FindProperty("lockCursorOnStart").boolValue=false;serialized.ApplyModifiedPropertiesWithoutUndo();
                var start=typeof(CameraController).GetMethod("Start",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);start.Invoke(camera,null);
                float pitch=(float)typeof(CameraController).GetField("pitch",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(camera);
                if(Mathf.Abs(pitch+10f)>.01f)throw new InvalidOperationException("Camera pitch must preserve a negative initial angle");
            }
            finally{UnityEngine.Object.DestroyImmediate(obj);}
        }

        private static void ValidateSchoolIsolation()
        {
            var schools=UnityEngine.Object.FindObjectsOfType<ReefSchool>();
            var materials=new HashSet<UnityEngine.Object>();
            ReefSchool sourceVortex=null;
            foreach(var school in schools)
            {
                var so=new SerializedObject(school);
                var material=so.FindProperty("fishMaterial").objectReferenceValue;
                if(material==null||!materials.Add(material))throw new InvalidOperationException("Schools must have separate color materials");
                if(school.IsVortex)sourceVortex=school;
            }
            if(sourceVortex==null)throw new InvalidOperationException("Vortex school is missing");
            var vortexSettings=new SerializedObject(sourceVortex);
            int expectedCount=Mathf.Clamp(vortexSettings.FindProperty("count").intValue,1,1023);
            float expectedHeight=vortexSettings.FindProperty("vortexHeight").floatValue;
            float expectedSpeed=vortexSettings.FindProperty("cruiseSpeed").floatValue;
            var flags=System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance;
            var initialize=typeof(ReefSchool).GetMethod("OnEnable",flags);
            var simulate=typeof(ReefSchool).GetMethod("Simulate",flags);
            var vortex=UnityEngine.Object.Instantiate(sourceVortex);
            vortex.enabled=false;initialize.Invoke(vortex,null);
            try
            {
                float low=float.PositiveInfinity,high=float.NegativeInfinity;
                var occupied=new HashSet<int>();
                foreach(var position in vortex.Positions)
                {
                    low=Mathf.Min(low,position.y);high=Mathf.Max(high,position.y);
                    var p=position-vortex.VortexCenter;
                    int angle=Mathf.FloorToInt(Mathf.Repeat(Mathf.Atan2(p.z,p.x)/(Mathf.PI*2),1)*12);
                    int height=Mathf.Clamp(Mathf.FloorToInt((p.y/Mathf.Max(expectedHeight,.001f)+.5f)*8),0,7);
                    occupied.Add(height*12+angle);
                }
                int requiredCells=Mathf.Min(96,Mathf.Max(1,expectedCount/4));
                if(vortex.AgentCount!=expectedCount||(expectedCount>=16&&high-low<expectedHeight*.95f)||occupied.Count<requiredCells||vortex.AverageSpeed<expectedSpeed*.9f)
                    throw new InvalidOperationException("Vortex must start populated and already circulating");
                foreach(var source in schools)
                {
                    if(source.IsVortex)continue;
                    var alone=UnityEngine.Object.Instantiate(source);
                    var together=UnityEngine.Object.Instantiate(source);
                    alone.enabled=false;together.enabled=false;
                    initialize.Invoke(alone,null);initialize.Invoke(together,null);
                    try
                    {
                        for(int step=0;step<60;step++)
                        {
                            simulate.Invoke(alone,null);simulate.Invoke(vortex,null);simulate.Invoke(together,null);
                            for(int i=0;i<alone.AgentCount;i++)
                                if((alone.Positions[i]-together.Positions[i]).sqrMagnitude>1e-12f)
                                    throw new InvalidOperationException("Vortex changed the flow trajectory of "+source.name);
                        }
                    }
                    finally{UnityEngine.Object.DestroyImmediate(alone.gameObject);UnityEngine.Object.DestroyImmediate(together.gameObject);}
                }
            }
            finally{UnityEngine.Object.DestroyImmediate(vortex.gameObject);}
            Debug.Log("REEF_SCHOOL_ISOLATION_PASSED startupAgents="+expectedCount+" comparedSteps=60");
        }
        [Serializable] private sealed class Report
        {
            public bool passed,gridMatchesBruteForce,legacyRegressionChecks;
            public bool schoolsIndependent,initialVortexReady;
            public int routeSamples,blockedRouteSamples;
            public int foregroundCorals,backgroundCorals,floatingCorals,highCorals;
            public string[] issues;
        }
    }
}
