using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LuaInterface;
namespace Pulse.Scripting
{
    public class LuaScript
    {
        public static List<LuaScript> scripts = new List<LuaScript>();
        Lua lua;
        string scriptdir;
        public  LuaScript(String script)
        {
            lua = new Lua();
            scriptdir = script;
        }
        public void execute()
        {
            try
            {
                lua.DoFile(scriptdir);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        public LuaFunction FindFunc(string name)
        {
            try
            {
                LuaFunction retfunc = this.lua.GetFunction(name);
                return retfunc;
            }
            catch (Exception e)
            {
                return null;
            }

        }
        /// <summary>
        /// possible to have variable number of args at runtime?
        /// </summary>
        /// <param name="name"></param>
        public void CallFunction(string name, params object[] args)
        {
            if (FindFunc(name) == null)
            {
                return;
            }
            try
            {
                LuaFunction retfunc = this.lua.GetFunction(name);
                retfunc.Call(args); 
            }
            catch (Exception e)
            {
            }

        }
        //won't passing a params object be equivalent to just passing array as first param? Can't have arbitrary amount of arguments when calling a method (in code) during runtime?
        public static void processScript(String script)
        {
            Lua lua = new Lua();
            lua[""] = "";
            lua.GetFunction("orz").Call();

        }
    }
}
