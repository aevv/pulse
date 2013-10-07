using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Un4seen.Bass;
using Un4seen.Bass.Misc;
using Un4seen.Bass.AddOn.Fx;
using System.IO;
namespace Pulse.Audio
{

    //update this class to check errors on init and such 
    class AudioManager
    {
        public static bool Init = false;
        public static Dictionary<String, AudioFX> Loaded = new Dictionary<string, AudioFX>();
        private static int frequency = 44100;

        public static int Frequency
        {
            get { return AudioManager.frequency; }
        }
        public static void setUpdate(int update)
        {
            //5?
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATEPERIOD, update);
        }
        public static void initBass()
        {
            if (!Init)
            {
                Init = true;
                Bass.BASS_Init(-1, Frequency, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
                BassFx.LoadMe();
            }
        }

        public static AudioFX loadFromFile(String path)
        {
            return loadFromFile(path, false);
        }
        //For sfx
        public static AudioFX loadFromMemory(byte[] data)
        {
            int handle = Bass.BASS_StreamCreatePush(Frequency, 2, /*BASSFlag.BASS_MUSIC_AUTOFREE deconstructor free is fine too.. i think*/ BASSFlag.BASS_DEFAULT, IntPtr.Zero);            
            Bass.BASS_StreamPutData(handle, data, data.Length);
            return new AudioFX(handle, "", true);
        }
        public static AudioFX loadFromFile(String path, bool sfx)
        {
            initBass();
            try
            {
                if (Loaded[path] != null && !sfx)
                {
                    return Loaded[path];
                }
            }
            catch (Exception)
            { //just means wasn't in dict yet
            }
            if (File.Exists(path))
            {
                //int h = Bass.BASS_StreamCreateFile(path, 0L, 0, BASSFlag.BASS_DEFAULT);
                int h = Bass.BASS_StreamCreateFile(path, 0L, 0, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_STREAM_PRESCAN);
                h = BassFx.BASS_FX_TempoCreate(h, BASSFlag.BASS_MUSIC_PRESCAN);
                //Bass.BASS_ChannelSetAttribute(h, BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, frequency);
                if (h == 0)
                {
                    Console.WriteLine(Bass.BASS_ErrorGetCode());
                }
                AudioFX fx = new AudioFX(h, path, sfx);
                fx.Volume = Config.Volume / 100.0f;
                if (!sfx)
                {
                    Loaded.Add(path, fx);
                }
                return fx;
            }
            Console.WriteLine("Could not find " + path);
            return new AudioFX(); //so game doesnt get nullpointerexceptions, no audio plays though
        }
        public static void dispose(AudioFX fx)
        {
            Bass.BASS_StreamFree(fx.handle);
            Loaded.Remove(fx.path);
        }
        public static void Free()
        {
            Bass.BASS_Free();
        }
        public static void pauseAll()
        {
            Bass.BASS_Pause();
        }
        public static void playAll()
        {
            Bass.BASS_Start();
        }
        public static float GlobalVolume
        {
            get
            {
                return Bass.BASS_GetVolume();
            }
            set
            {
                Bass.BASS_SetVolume(value);
            }
        }
    }
}
