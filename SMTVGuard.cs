using MelonLoader;
using Il2Cpp;
using smtv_guard;
using HarmonyLib;
using Il2Cppnewbattle_H;

[assembly: MelonInfo(typeof(SMTVGuard), "SMT V Guard", "1.0.0", "Matthiew Purple")]
[assembly: MelonGame("アトラス", "smt3hd")]

namespace smtv_guard;
public class SMTVGuard : MelonMod
{
    // After getting the effectiveness of an attack on 1 target
    [HarmonyPatch(typeof(nbCalc), nameof(nbCalc.nbGetAisyo))]
    private class Patch
    {
        public static void Postfix(ref uint __result, ref int formindex)
        {
            bool isGuarding = nbMainProcess.nbGetPartyFromFormindex(formindex).count[19] == 1;

            // If it's not resisted/blocked/drained/repelled
            if (isGuarding && !(__result < 100 || (__result >= 65536 && __result < 2147483648)))
            {
                __result = 80;
            }
        }
    }

    // After getting the type of a hit (0 = normal, 1 = critical, 2 = weakness)
    [HarmonyPatch(typeof(nbCalc), nameof(nbCalc.nbGetHitType))]
    private class Patch2
    {
        public static void Postfix(ref int __result, ref int dformindex)
        {
            bool isGuarding = nbMainProcess.nbGetPartyFromFormindex(dformindex).count[19] == 1;

            if (isGuarding) __result = 0;
        }
    }

    // Before displaying the list of skill in the battle command panel
    [HarmonyPatch(typeof(nbCommSelProcess), nameof(nbCommSelProcess.DispCommandList2))]
    private class Patch3
    {
        public static void Prefix(ref nbCommSelProcessData_t s)
        {
            s.act.party.count[19] = 0; // Stop guarding

            var skillIndices = new List<ushort> { };
            var skills = s.commlist[0].ToList().Where(x => x != 0);

            if (skills.Any(x => x != 0))
                skillIndices.AddRange(skills);

            if (!skillIndices.Contains(80))
            {
                skillIndices[skillIndices.FindIndex(s => s == 32770)] = 80;
                skillIndices.Add(32770);
            }
            
            skillIndices = skillIndices.Distinct().ToList();

            var skillCommands = new ushort[288];
            for (ushort i = 0; i < skillIndices.Count; i++)
                skillCommands[i] = skillIndices[i];

            s.commlist[0] = skillCommands;
            s.commcnt[0] = skillIndices.Count;
        }
    }

    // After getting the description of a skill
    [HarmonyPatch(typeof(datSkillHelp_msg), nameof(datSkillHelp_msg.Get))]
    private class Patch4
    {
        public static void Postfix(ref int id, ref string __result)
        {
            if (id == 80)
            {
                __result = "Decreases damage and \nchance of being inflincted \nwith an ailment.";
            }
        }
    }

    // After getting the name of a skill
    [HarmonyPatch(typeof(datSkillName), nameof(datSkillName.Get))]
    private class Patch5
    {
        public static void Postfix(ref int id, ref string __result)
        {
            if (id == 80)
            {
                __result = "Guard";
            }
        }
    }

    // After initiating a phase
    [HarmonyPatch(typeof(nbMainProcess), nameof(nbMainProcess.nbSetPressMaePhase))]
    private class Patch6
    {
        public static void Postfix(ref nbMainProcessData_t data)
        {
            short activeunit = nbMainProcess.nbGetMainProcessData().activeunit; // Get the formindex of the first active demon

            // If that demon is an ally
            if (activeunit < 4)
            {
                for (int i = 0; i < data.party.Length; i++)
                {
                    data.party[i].count[19] = 0;
                }
            }
        }
    }

    // When launching the game
    public override void OnInitializeMelon()
    {
        datSkill.tbl[80].skillattr = datSkill.tbl[224].skillattr;
        datSkill.tbl[80].type = datSkill.tbl[224].type;

        datNormalSkill.tbl[80].costtype = 0;
        datNormalSkill.tbl[80].hojotype = 16384;
        datNormalSkill.tbl[80].hptype = datNormalSkill.tbl[224].hptype;
        datNormalSkill.tbl[80].targetrule = datNormalSkill.tbl[224].targetrule;
        datNormalSkill.tbl[80].use = datNormalSkill.tbl[224].use;
    }
}
