using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TechJuego.GetOut
{
    public class Socialmedia
    {
        public static void SubscribeOnYoutube()
        {
            Application.OpenURL("https://www.youtube.com/channel/UCmySbBuGZ4WczmSZRJhyPew?sub_confirmation=1");
        }
        public static void FollowOnInstagram()
        {
            Application.OpenURL("https://www.instagram.com/techjuego3/?hl=en");
        }
        public static void FollowOnTweeter()
        {
            Application.OpenURL("https://twitter.com/techjuego");
        }
        public static void FollowOnFacebook()
        {
            Application.OpenURL("https://www.facebook.com/techjuego/");
        }
        public static void ConnectOnDiscord()
        {
            Application.OpenURL("https://discord.gg/SUE6bZnZ");
        }
    }
}