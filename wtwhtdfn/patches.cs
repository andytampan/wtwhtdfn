using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.InlineRT;
using Quintessential;
using Quintessential.Settings;
using SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static PartDraggingInputMode;


public class patch_PartDraggingInputMode : PartDraggingInputMode
{
    public patch_PartDraggingInputMode() : base()
    {
    }

    [MonoModPublic]
    public List<PartDraggingInputMode.DraggedPart> draggedParts;
    public HexIndex selectedHex;

    [MonoModIgnore]
    public extern Maybe<IInputMode> orig_Process(SolutionEditorScreen solEditScrn);
    public Maybe<IInputMode> Process(SolutionEditorScreen solEditScrn)
    {
        if (wtwhtdfn.wtwhtdfn.MQSettings.Instance.mirrorTrack.Pressed())
        {
            if (draggedParts.Count == 1)
            {
                Part track = draggedParts[0].part;
                if (track.GetType() == PartTypes.track)
                {
                    patch_Part oldpart = draggedParts[0].part as patch_Part;
                    oldpart.trackHexes.Reverse();
                    Volume.PlaySound(Assets.sounds.click_button, 1);
                }
            }
        }
        if (wtwhtdfn.wtwhtdfn.MQSettings.Instance.regularSplit.Pressed())
        {
            if (draggedParts.Count == 1)
            {
                Part track = draggedParts[0].part;
                if (track.GetType() == PartTypes.track)
                {
                    patch_Part oldPart = track as patch_Part;

                    patch_Part newPartA = new Part(PartTypes.track, false) as patch_Part;
                    patch_Part newPartB = new Part(PartTypes.track, false) as patch_Part;

                    var coordinate = oldPart.GetHexPos();
                    newPartA.SetHexPosAndUpdate(coordinate);
                    if (oldPart.trackHexes.Count < 2) return orig_Process(solEditScrn);

                    HexIndex? handleHex = null;
                    
                    foreach (HexIndex hex in oldPart.trackHexes)
                    {

                        if (hex == selectedHex - coordinate)
                        {
                            
                            newPartB.SetHexPosAndUpdate(hex);
                            
                            handleHex = hex - coordinate;
                        };

                        if (handleHex.HasValue)
                        {
                            newPartB.AddTrackFront(hex - handleHex.Value);
                        }
                        else
                        {
                            newPartA.AddTrackFront(hex);
                            
                        }


                    }
                    if (newPartA.trackHexes.Count > 1)
                    {
                        newPartA.RemoveTrackBack();
                    }

                    if (newPartB.trackHexes.Count > 1)
                    {
                        newPartB.RemoveTrackBack();
                    }

                    newPartA.trackHexes.Reverse();
                    draggedParts.Clear();

                    DraggedPart draggedPartA = new DraggedPart();
                    draggedPartA.part = newPartA;
                    draggedParts.Add(draggedPartA);
                    DraggedPart draggedPartB = new DraggedPart();
                    draggedPartB.part = newPartB;
                    draggedParts.Add(draggedPartB);

                    Volume.PlaySound(Assets.sounds.glyph_unbonding, 1);
                }
            }
        }
        if (wtwhtdfn.wtwhtdfn.MQSettings.Instance.disjointSplit.Pressed()) {
            if (draggedParts.Count == 1)
            {
                Part track = draggedParts[0].part;
                if (track.GetType() == PartTypes.track)
                {
                    patch_Part oldPart = track as patch_Part;

                    patch_Part newPart = new Part(PartTypes.track, false) as patch_Part;

                    var coordinate = oldPart.GetHexPos();
                    newPart.SetHexPosAndUpdate(coordinate);
                    if (oldPart.trackHexes.Count < 2) return orig_Process(solEditScrn);
                    
       
                    foreach ( HexIndex hex in oldPart.trackHexes)
                    {

                        if (hex != selectedHex-coordinate) newPart.AddTrackFront(hex);

                    }
                    if (newPart.trackHexes.Count > 1)
                    {
                        newPart.RemoveTrackBack();
                    }

                    newPart.trackHexes.Reverse();
                    draggedParts.Clear();

                    DraggedPart draggedPart = new DraggedPart();
                    draggedPart.part = newPart;
                    draggedParts.Add(draggedPart);
                    
                    Volume.PlaySound(Assets.sounds.glyph_unbonding, 1);
                }
            }
        }
        return orig_Process(solEditScrn);
    }
}
public class patch_Part : Part
{
    [MonoModPublic]
    public List<HexIndex> trackHexes;

    public patch_Part(PartType type, bool isFixed) : base(type, isFixed)
    {
    }
}

public class patch_Solution
    {

        [MonoModPatch("Solution")]
        public extern bool orig_IsAllowedPlacement(Part part, HexIndex otherInputOutputIndex, HexIndex offset, HexRotation rotationOffset, out string errorMessage);

        
        public bool IsAllowedPlacement(Part part, HexIndex otherInputOutputIndex, HexIndex offset, HexRotation rotationOffset, out string errorMessage)
        {
            
            bool yea = orig_IsAllowedPlacement(part, otherInputOutputIndex, offset, rotationOffset, out errorMessage);
            if (errorMessage == Translations.Translate("There is already another part here.") && wtwhtdfn.wtwhtdfn.MQSettings.Instance.enableOverlap) return true ;
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
public class patch_PartTypeForToolbar
{
    [MonoModIgnore]
    public extern static PartTypeForToolbar orig_Make(PartType type, int indexOfType, bool isAvailable, bool hasPerms);


    public static PartTypeForToolbar Make(PartType type, int indexOfType, bool isAvailable, bool hasPerms) 
    {
        return orig_Make(type, indexOfType, wtwhtdfn.wtwhtdfn.MQSettings.Instance.disableUnique || isAvailable, hasPerms);

    }
}
