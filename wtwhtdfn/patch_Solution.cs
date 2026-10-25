using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.InlineRT;
using Quintessential;
using System;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil;
using System.Linq;

using static System.Net.Mime.MediaTypeNames;
public class patch_Solution
    {

        [MonoModPatch("Solution")]
        public extern bool orig_IsAllowedPlacement(Part part, HexIndex otherInputOutputIndex, HexIndex offset, HexRotation rotationOffset, out string errorMessage);

        
        public bool IsAllowedPlacement(Part part, HexIndex otherInputOutputIndex, HexIndex offset, HexRotation rotationOffset, out string errorMessage)
        {
            
            bool yea = orig_IsAllowedPlacement(part, otherInputOutputIndex, offset, rotationOffset, out errorMessage);
            if (errorMessage == Translations.Translate("There is already another part here.") && wtwhtdfn.wtwhtdfn.enabled) return true ;
            return yea;
        }
}





public class patch_Sim : Sim
{

    [MonoModPublic]


    public extern bool orig_AllOutputsComplete();


    public static bool cm_OutputsPlaced;
    public bool AllOutputsComplete()
    {
        return orig_AllOutputsComplete() && cm_OutputsPlaced;
    }

    public static void ProductCheck(SolutionEditorBase solutionEditor)
    {
        Solution solution = solutionEditor.GetSolution();
        int outputCount = 0;
        foreach (Part part in solution.parts)
        {
            if (part.GetType() == PartTypes.normalOutput || part.GetType() == PartTypes.repeatingOutput) outputCount++;
        }
        ;

        cm_OutputsPlaced = outputCount >= solution.GetPuzzle().outputs.Length;



    }

    [MonoModPublic]
    public static extern Maybe<Sim> orig_ConstructSimulation(SolutionEditorBase solutionEditor);
    public static Maybe<Sim> ConstructSimulation(SolutionEditorBase solutionEditor)
    {
        var a = orig_ConstructSimulation(solutionEditor);
        ProductCheck(solutionEditor);
        return a;
    }

}

public class patch_SolutionEditorProgramPanel
{
    [MonoModILInject("RenderFrame")]
    public static void Sim(MethodDefinition method, CustomAttribute attribute)
    {

        ILCursor cursor = new(new ILContext(method));

        cursor.TryGotoNext(MoveType.Before, instr => instr.MatchLdstr("All products must be placed."));

        cursor.Index--;
        cursor.Index--;
        cursor.Next.OpCode = OpCodes.Br;
        Instruction skipTo = cursor.Next;


        cursor.Goto(0);
        cursor.TryGotoNext(MoveType.After, instr => instr.MatchStloc(118));
        cursor.TryGotoNext(MoveType.After, instr => instr.MatchStloc(118));

        cursor.Emit(OpCodes.Br, skipTo);
        Instruction replacement = cursor.Prev;
        Instruction existing = cursor.Next;


        cursor.TryGotoPrev(MoveType.Before, instr => instr.MatchStloc(118));
        cursor.TryGotoPrev(MoveType.Before, instr => instr.MatchStloc(118));
        cursor.Index++;

        cursor.Next.Operand = replacement;

    }
}

public class patch_SolutionEditorScreen : SolutionEditorScreen
{
    public patch_SolutionEditorScreen(Solution solution, MusicTrack musicTrack, Sound fanfare) : base(solution, musicTrack, fanfare)
    {

    }

    public static float cm_AverageSpeed = 4f;

    [MonoModILInject("GetSimulationSpeed")]

    public static void GetSimulationSpeed(MethodDefinition method, CustomAttribute attribute)
    {


        //want to make sure your code isn't secretly empty? do this
        /* MonoModRule.Modder.Log("patching hell");
        ILCursor cursorA = new(new ILContext(method));     
            MonoModRule.Modder.Log(cursorA.Context);
        */

        ILCursor cursor = new(new ILContext(method));

        TypeDefinition sim = MonoModRule.Modder.FindType("Sim").Resolve();
        FieldDefinition outputsPlaced = sim.Fields.First(f => f.Name.Equals("cm_OutputsPlaced"));
        FieldDefinition simStartSpeed = method.DeclaringType.Fields.First(f => f.Name.Equals("cm_AverageSpeed"));


        cursor.Index = cursor.Index + 8;
        var instructionEnd = cursor.Next.Operand;
        Instruction instructionDefault;

        cursor.TryGotoNext(MoveType.Before, instr => instr.MatchLdcR4(1));

        cursor.Emit(OpCodes.Ldsfld, outputsPlaced);

        Instruction lastPos = cursor.Next;
        Instruction currentPos = cursor.Previous;

        cursor.TryGotoPrev(MoveType.Before, instr => instr.MatchStloc0());
        instructionDefault = (Instruction)cursor.Previous.Previous.Operand;
        cursor.Previous.Previous.Operand = currentPos;

        cursor.TryGotoNext(MoveType.Before, instr => instr == lastPos);
        cursor.Emit(OpCodes.Brfalse_S, instructionDefault);
        cursor.Emit(OpCodes.Ldsfld, simStartSpeed);
        cursor.Emit(OpCodes.Stloc_0);
        cursor.Emit(OpCodes.Br, instructionEnd);

    }

}


