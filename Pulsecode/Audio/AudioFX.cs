using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;
using Pulse.Mechanics;

namespace Pulse.Audio
{
    /// <summary>
    /// Class encapsulating music/sound files, providing objects that can be played, paused, and so forth.
    /// </summary>
    public class AudioFX
    {
        public int handle;
        public String path;
        public bool SFX;
        ~AudioFX()
        {
            if (SFX)
            {
                Bass.BASS_StreamFree(handle);
            }
        }
        private Boolean loop;
        /// <summary>
        /// Determines whether the music loops after finishing
        /// </summary>
        public Boolean Loop
        {
            get
            {
                return loop;
            }
            set
            {
                loop = value;
                if (value)
                {
                    Bass.BASS_ChannelFlags(handle, BASSFlag.BASS_MUSIC_LOOP, BASSFlag.BASS_MUSIC_LOOP);
                }
                else
                {
                    Bass.BASS_ChannelFlags(handle, BASSFlag.BASS_DEFAULT, BASSFlag.BASS_MUSIC_LOOP);
                }
            }
        }
        public AudioFX()
        {
        }
        public AudioFX(int handle_, String p)
            : this(handle_, p, false)
        {
        }
        public AudioFX(int handle_, String p, bool sfx)
        {
            SFX = sfx;
            handle = handle_;
            path = p;
            
            //Delegate throws GC related exceptions, hard to debug, finish is now only set upon stop or manual setting
            /*        Bass.BASS_ChannelSetSync(handle, /*BASSSync.BASS_SYNC_ONETIME | needed? BASSSync.BASS_SYNC_END, 0, delegate(int i, int j, int k, IntPtr ptr) {
                        Finished = true;
                        Console.WriteLine("song finished " + path);
                        if(loop) {
                          //  Bass.BASS_ChannelPlay(handle, true);
                            }

                    }, IntPtr.Zero);*/
        }
        float speed = 0;

        public float Speed
        {
            get
            {
                return speed;
            }
            set
            {
                speed = value;
                Bass.BASS_ChannelSetAttribute(handle, BASSAttribute.BASS_ATTRIB_TEMPO, speed);
            }
        }
        float frequency = 1.0f;

        public float Frequency
        {
            get
            {
                return frequency;
            }
            set
            {
                frequency = value;
                float temp = AudioManager.Frequency;
                temp = temp * (frequency);
                Bass.BASS_ChannelSetAttribute(handle, BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, temp);
            }
        }
        bool finished = false;
        public Boolean Finished
        {
            get
            {
                return finished;
            }
            set
            {
                finished = value;
            }
        }
        public bool Paused
        {
            get;
            set;
        }
        /// <summary>
        /// Returns length of the file in milliseconds.
        /// </summary>
        public double Length
        {
            get
            {
                double temp = Bass.BASS_ChannelGetLength(handle, BASSMode.BASS_POS_BYTES);
                return Bass.BASS_ChannelBytes2Seconds(handle, (long)temp) * 1000;
            }
        }
        public void play(bool restart)
        {
            play(restart, false);
        }
        /// <summary>
        /// Starts the audio file.
        /// </summary>
        /// <param name="restart">Start the file from the beginning?</param>
        /// <param name="loopp">Loop the music?</param>
        public void play(bool restart, bool loopp)
        {
            Bass.BASS_ChannelPlay(handle, restart);
            Paused = false;
            finished = false;
            Loop = loopp;
        }
        public void pause()
        {
            Bass.BASS_ChannelPause(handle);
            Paused = true;
        }
        /// <summary>
        /// Sets the position of the music, in seconds
        /// </summary>
        public double Position
        {
            get
            {
                return Bass.BASS_ChannelBytes2Seconds(handle, Bass.BASS_ChannelGetPosition(handle)) * 1000;
            }
            set
            {
                Bass.BASS_ChannelSetPosition(handle, Bass.BASS_ChannelSeconds2Bytes(handle, value / 1000d));
            }
        }
        public void stop()
        {
            Bass.BASS_ChannelStop(handle);
            Finished = true;
        }
        /// <summary>
        /// Sets the volume of the music as a float from 0.0 to 1.0
        /// </summary>
        public float Volume
        {
            get
            {
                float toreturn = 1f;
                Bass.BASS_ChannelGetAttribute(handle, BASSAttribute.BASS_ATTRIB_VOL, ref toreturn);
                return toreturn;
            }
            set
            {
                Bass.BASS_ChannelSetAttribute(handle, BASSAttribute.BASS_ATTRIB_VOL, value);
            }
        }
    }
}
