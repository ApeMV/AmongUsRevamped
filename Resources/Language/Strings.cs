using System.Reflection;

namespace AmongUsRevamped;

// Used post-1.4.0 incase AUR ever gets translations. Not fully updated.

/* TEMPLATE

    public static string x{get{
    return $"y";}}
*/
public static class String
{
    public static string noKcdMode{get{
    return $"0 Kill Cooldown:\n\nImpostors have no kill cooldown, Crewmates have low tasks\nThink fast and pay attention!";}}

    public static string SnSModeOne{get{
    return $"Shift and Seek:\n\nImpostors can only kill someone while shapeshifted as them\nSabotages & Meetings = Off";}}

    public static string SnSModeTwo{get{
    return $"Crew wins by tasks/surviving {Options.CrewAutoWinsGameAfter.GetInt()}s\nImp wins by killing\nOne wrong kill = Can't kill for {Options.CantKillTime.GetInt()}s\n{Utils.BasicIntToWord(Options.MisfiresToSuicide.GetInt())} wrong kills = suicide";}}

    public static string speedrunMode{get{
    return $"Speedrun:\n\nEveryone is a crewmate. The 1st player to finish tasks wins the game. Game auto ends after {Options.GameAutoEndsAfter.GetInt()}s";}}

    public static string allCommandsFull{get{
    return $"Commands:\n/r - Current mode description\n/0kc, /sns, /sp - Specific mode description\n/l - Shows last winner info\n/kick, /ban - Bans or kicks a player by name\n/ckick, /cban - Bans or kicks a player by color";}}

    public static string allCommandsOne{get{
    return $"Commands:\n/r - Current mode description\n/0kc, /sns, /sp - Specific mode description\n/l - Shows last winner info";}}

    public static string allCommandsTwo{get{
    return $"/kick, /ban - Bans or kicks a player by name\n/ckick, /cban - Bans or kicks a player by color";}}

    public static string SocialsAll{get{
    return $"AUR socials:\n\ng i t h u b . c o m /\nApeMV/AmongUsRevamped\n\nd i s c o r d . g g /\n83Zhzhyhya";}}

    public static string Mayor{get{
    return $"PRIVATE:\n\nYour role is Mayor!\n\nYou are a crewmate with i total votes. Use your voting power to eject Impostors.";}}

    public static string Jester{get{
    return $"PRIVATE:\n\nYour role is Jester!\nYou are NOT a crewmate!\n\nYou win by getting voted. Act as suspicious as possible.";}}
}
