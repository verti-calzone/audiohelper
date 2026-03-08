
using Celeste.Mod;
using System.Linq;
using System;
using System.Reflection;
using Celeste.Mod.audiohelper;
using System.Collections.Generic;
using Monocle;
using MonoMod.ModInterop;

namespace Celeste.Mod.audiohelper;

internal static class SpeedrunToolIop{
  internal static List<object> toDeregister = new List<object>();

  [AttributeUsage(AttributeTargets.Field)]
  public class Static : Attribute { }
  static void SetupStaticAttr(){
    foreach(var t in typeof(audiohelperModule).Assembly.GetTypesSafe()){
      List<string> st = new();
      foreach (var f in t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)){
        if(f.IsDefined(typeof(Static))){
          if(!f.IsStatic) throw new Exception("SrtIOP.Static attribute applied to non-static class");
          else if(!st.Contains(f.Name)) st.Add(f.Name);
        }
      }
      if(st.Count==0) continue;
      //Logger.Verbose("auspicioushelper",$"(SRT) Type {t.FullName}: adding static fields [{string.Join(", ",st)}]");
      toDeregister.Add(SpeedrunToolImport.RegisterStaticTypes(t, st.ToArray()));
    }
  }

  #pragma warning disable CS0649
  [ModImportName("SpeedrunTool.SaveLoad")]
  internal static class SpeedrunToolImport {
    public static Func<
      Action<Dictionary<Type, Dictionary<string, object>>, Level>, //onSave
      Action<Dictionary<Type, Dictionary<string, object>>, Level>, //onLoad
      Action, //onClear
      Action<Level>, //beforeSave
      Action<Level>, //beforeLoad
      Action, //preClone
    object> RegisterSaveLoadAction;
    public static Func<Type, string[], object> RegisterStaticTypes;
    public static Action<object> Unregister;
    public static Func<object, object> DeepClone;
  }
  #pragma warning restore CS0649
  internal static void srtloaduseapi(){
    Logger.Log("audiohelper","Doing srt setup");
    typeof(SpeedrunToolImport).ModInterop();
    if(SpeedrunToolImport.RegisterStaticTypes!=null){
      SetupStaticAttr();
    }
  }
  internal static void Unload()
  {
    foreach(object o in toDeregister){
      SpeedrunToolImport.Unregister(o);
    }
    toDeregister.Clear();
  }
}