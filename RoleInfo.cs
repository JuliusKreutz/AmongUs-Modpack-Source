using System.Linq;
using System.Collections.Generic;
using static Modpack.Modpack;
using UnityEngine;

namespace Modpack
{
    internal class RoleInfo
    {
        public Color color;
        public readonly string name;
        public readonly string introDescription;
        public readonly string shortDescription;
        public readonly RoleId roleId;

        private RoleInfo(string name, Color color, string introDescription, string shortDescription, RoleId roleId)
        {
            this.color = color;
            this.name = name;
            this.introDescription = introDescription;
            this.shortDescription = shortDescription;
            this.roleId = roleId;
        }

        public static readonly RoleInfo jester =
            new RoleInfo("Jester", Jester.color, "Get voted out", "Get voted out", RoleId.Jester);

        public static readonly RoleInfo mayor = new RoleInfo("Mayor", Mayor.color, "Your vote counts twice",
            "Your vote counts twice", RoleId.Mayor);

        public static readonly RoleInfo engineer = new RoleInfo("Engineer", Engineer.color,
            "Maintain important systems on the ship", "Repair the ship", RoleId.Engineer);

        public static readonly RoleInfo sheriff = new RoleInfo("Sheriff", Sheriff.color,
            "Shoot the <color=#FF1919FF>Impostors</color>", "Shoot the Impostors", RoleId.Sheriff);

        public static readonly RoleInfo lighter = new RoleInfo("Lighter", Lighter.color, "Your light never goes out",
            "Your light never goes out", RoleId.Lighter);

        public static readonly RoleInfo godfather = new RoleInfo("Godfather", Godfather.color, "Kill all Crewmates",
            "Kill all Crewmates", RoleId.Godfather);

        public static readonly RoleInfo mafioso = new RoleInfo("Mafioso", Mafioso.color,
            "Work with the <color=#FF1919FF>Mafia</color> to kill the Crewmates", "Kill all Crewmates", RoleId.Mafioso);

        public static readonly RoleInfo janitor = new RoleInfo("Janitor", Janitor.color,
            "Work with the <color=#FF1919FF>Mafia</color> by hiding dead bodies", "Hide dead bodies", RoleId.Janitor);

        public static readonly RoleInfo morphling = new RoleInfo("Morphling", Morphling.color,
            "Change your look to not get caught", "Change your look", RoleId.Morphling);

        public static readonly RoleInfo camouflager = new RoleInfo("Camouflager", Camouflager.color,
            "Camouflage and kill the Crewmates", "Hide among others", RoleId.Camouflager);

        public static readonly RoleInfo vampire = new RoleInfo("Vampire", Vampire.color,
            "Kill the Crewmates with your bites", "Bite your enemies", RoleId.Vampire);

        public static readonly RoleInfo eraser = new RoleInfo("Eraser", Eraser.color,
            "Kill the Crewmates and erase their roles", "Erase the roles of your enemies", RoleId.Eraser);

        public static readonly RoleInfo trickster = new RoleInfo("Trickster", Trickster.color,
            "Use your jack-in-the-boxes to surprise others", "Surprise your enemies", RoleId.Trickster);

        public static readonly RoleInfo cleaner = new RoleInfo("Cleaner", Cleaner.color,
            "Kill everyone and leave no traces", "Clean up dead bodies", RoleId.Cleaner);

        public static readonly RoleInfo warlock = new RoleInfo("Warlock", Warlock.color,
            "Curse other players and kill everyone", "Curse and kill everyone", RoleId.Warlock);

        public static readonly RoleInfo detective = new RoleInfo("Detective", Detective.color,
            "Find the <color=#FF1919FF>Impostors</color> by examining footprints", "Examine footprints",
            RoleId.Detective);

        public static readonly RoleInfo timeMaster = new RoleInfo("Time Master", TimeMaster.color,
            "Save yourself with your time shield", "Use your time shield", RoleId.TimeMaster);

        public static readonly RoleInfo medic = new RoleInfo("Medic", Medic.color, "Protect someone with your shield",
            "Protect other players", RoleId.Medic);

        public static readonly RoleInfo shifter = new RoleInfo("Shifter", Shifter.color, "Shift your role",
            "Shift your role", RoleId.Shifter);

        public static readonly RoleInfo swapper = new RoleInfo("Swapper", Swapper.color,
            "Swap votes to exile the <color=#FF1919FF>Impostors</color>", "Swap votes", RoleId.Swapper);

        public static readonly RoleInfo seer = new RoleInfo("Seer", Seer.color, "You will see players die",
            "You will see players die", RoleId.Seer);

        public static readonly RoleInfo hacker = new RoleInfo("Hacker", Hacker.color,
            "Hack systems to find the <color=#FF1919FF>Impostors</color>", "Hack to find the Impostors", RoleId.Hacker);

        public static readonly RoleInfo niceMini = new RoleInfo("Nice Mini", Mini.color,
            "No one will harm you until you grow up", "No one will harm you", RoleId.Mini);

        public static readonly RoleInfo evilMini = new RoleInfo("Evil Mini", Palette.ImpostorRed,
            "No one will harm you until you grow up", "No one will harm you", RoleId.Mini);

        public static readonly RoleInfo tracker = new RoleInfo("Tracker", Tracker.color,
            "Track the <color=#FF1919FF>Impostors</color> down", "Track the Impostors down", RoleId.Tracker);

        public static readonly RoleInfo snitch = new RoleInfo("Snitch", Snitch.color,
            "Finish your tasks to find the <color=#FF1919FF>Impostors</color>", "Finish your tasks", RoleId.Snitch);

        public static readonly RoleInfo jackal = new RoleInfo("Jackal", Jackal.color,
            "Kill all Crewmates and <color=#FF1919FF>Impostors</color> to win", "Kill everyone", RoleId.Jackal);

        public static readonly RoleInfo sidekick = new RoleInfo("Sidekick", Sidekick.color,
            "Help your Jackal to kill everyone", "Help your Jackal to kill everyone", RoleId.Sidekick);

        public static readonly RoleInfo spy = new RoleInfo("Spy", Spy.color,
            "Confuse the <color=#FF1919FF>Impostors</color>", "Confuse the Impostors", RoleId.Spy);

        public static readonly RoleInfo securityGuard = new RoleInfo("Security Guard", SecurityGuard.color,
            "Seal vents and place cameras", "Seal vents and place cameras", RoleId.SecurityGuard);

        public static readonly RoleInfo arsonist =
            new RoleInfo("Arsonist", Arsonist.color, "Let them burn", "Let them burn", RoleId.Arsonist);

        public static readonly RoleInfo goodGuesser = new RoleInfo("Nice Guesser", Guesser.color, "Guess and shoot",
            "Guess and shoot", RoleId.Guesser);

        public static readonly RoleInfo badGuesser = new RoleInfo("Evil Guesser", Palette.ImpostorRed,
            "Guess and shoot", "Guess and shoot", RoleId.Guesser);

        public static readonly RoleInfo impostor = new RoleInfo("Impostor", Palette.ImpostorRed,
            Helpers.cs(Palette.ImpostorRed, "Sabotage and kill everyone"), "Sabotage and kill everyone",
            RoleId.Impostor);

        public static readonly RoleInfo crewmate = new RoleInfo("Crewmate", Color.white, "Find the Impostors",
            "Find the Impostors", RoleId.Crewmate);

        public static readonly RoleInfo lover = new RoleInfo("Lover", Lovers.color, "You are in love",
            "You are in love", RoleId.Lover);

        public static readonly List<RoleInfo> allRoleInfos = new List<RoleInfo>
        {
            impostor,
            godfather,
            mafioso,
            janitor,
            morphling,
            camouflager,
            vampire,
            eraser,
            trickster,
            cleaner,
            warlock,
            niceMini,
            evilMini,
            goodGuesser,
            badGuesser,
            lover,
            jester,
            arsonist,
            jackal,
            sidekick,
            crewmate,
            shifter,
            mayor,
            engineer,
            sheriff,
            lighter,
            detective,
            timeMaster,
            medic,
            swapper,
            seer,
            hacker,
            tracker,
            snitch,
            spy,
            securityGuard
        };

        public static List<RoleInfo> getRoleInfoForPlayer(PlayerControl p)
        {
            var infos = new List<RoleInfo>();
            if (p == null) return infos;

            // Special roles
            if (p == Jester.jester) infos.Add(jester);
            if (p == Mayor.mayor) infos.Add(mayor);
            if (p == Engineer.engineer) infos.Add(engineer);
            if (p == Sheriff.sheriff) infos.Add(sheriff);
            if (p == Lighter.lighter) infos.Add(lighter);
            if (p == Godfather.godfather) infos.Add(godfather);
            if (p == Mafioso.mafioso) infos.Add(mafioso);
            if (p == Janitor.janitor) infos.Add(janitor);
            if (p == Morphling.morphling) infos.Add(morphling);
            if (p == Camouflager.camouflager) infos.Add(camouflager);
            if (p == Vampire.vampire) infos.Add(vampire);
            if (p == Eraser.eraser) infos.Add(eraser);
            if (p == Trickster.trickster) infos.Add(trickster);
            if (p == Cleaner.cleaner) infos.Add(cleaner);
            if (p == Warlock.warlock) infos.Add(warlock);
            if (p == Detective.detective) infos.Add(detective);
            if (p == TimeMaster.timeMaster) infos.Add(timeMaster);
            if (p == Medic.medic) infos.Add(medic);
            if (p == Shifter.shifter) infos.Add(shifter);
            if (p == Swapper.swapper) infos.Add(swapper);
            if (p == Seer.seer) infos.Add(seer);
            if (p == Hacker.hacker) infos.Add(hacker);
            if (p == Mini.mini) infos.Add(p.Data.IsImpostor ? evilMini : niceMini);
            if (p == Tracker.tracker) infos.Add(tracker);
            if (p == Snitch.snitch) infos.Add(snitch);
            if (p == Jackal.jackal ||
                Jackal.formerJackals != null && Jackal.formerJackals.Any(x => x.PlayerId == p.PlayerId))
                infos.Add(jackal);
            if (p == Sidekick.sidekick) infos.Add(sidekick);
            if (p == Spy.spy) infos.Add(spy);
            if (p == SecurityGuard.securityGuard) infos.Add(securityGuard);
            if (p == Arsonist.arsonist) infos.Add(arsonist);
            if (p == Guesser.guesser) infos.Add(p.Data.IsImpostor ? badGuesser : goodGuesser);

            // Default roles
            if (infos.Count == 0 && p.Data.IsImpostor) infos.Add(impostor); // Just Impostor
            if (infos.Count == 0 && !p.Data.IsImpostor) infos.Add(crewmate); // Just Crewmate

            // Modifier
            if (p == Lovers.lover1 || p == Lovers.lover2) infos.Add(lover);

            return infos;
        }
    }
}