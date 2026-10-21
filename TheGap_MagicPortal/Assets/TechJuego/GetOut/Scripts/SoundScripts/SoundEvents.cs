namespace TechJuego.GetOut.Sound
{
    // Class to manage sound-related events using delegates
    public class SoundEvents
    {
        // Delegate for performing generic sound-related actions
        public delegate void PerformAction();

        // Event triggered to stop a looping sound
        public static PerformAction OnStopLoopSound;

        // Delegate for playing a sound by specifying a clip name
        public delegate void PlaySound(string _ClipName);

        // Event triggered to play a single-shot (one-time) sound
        public static PlaySound OnPlaySingleShotSound;

        // Event triggered to play a looping sound
        public static PlaySound OnPlayLoopSound;
    }
}
