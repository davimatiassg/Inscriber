using Godot;
using Godot.NativeInterop;
using System;
using System.Linq;

namespace SpellEditing
{
public partial class SpellLine : Line2D
{
    private SpellSlot tip;
    private SpellSlot bot;

    public SpellSlot Tip
    {
        get => tip;
        set {
            if(tip != null && value == null) tip.spellLines.Remove(this); 
            else if(value != null) { tip = value; tip.spellLines.Add(this); }
            else tip = value;
        }
    }
    public SpellSlot Bot
    {
        get => bot;
        set {
            if(bot != null && value == null) bot.spellLines.Remove(this); 
            else if(value != null) { bot = value; bot.spellLines.Add(this); }
            else bot = value;
            
        }
    }
    
    public void MoveBot(Vector2 vect) => SetPointPosition(0, vect); 
    public void MoveBot(SpellSlot obj) => MoveBot(obj.GetGlobalRect().GetCenter());
    public void MoveBot() => MoveBot(Bot);
    public void MoveTip(Vector2 vect) => SetPointPosition(1, vect); 
    public void MoveTip(SpellSlot obj) => MoveTip(obj.GetGlobalRect().GetCenter());
    public void MoveTip() => MoveTip(Tip);

    public void UpdatePosition() { MoveTip(); MoveBot(); }

    public SpellLine()
    {
        Points = new Vector2[2];
        this.WidthCurve = new Curve();
        this.WidthCurve.AddPoint(Vector2.Down);
        this.WidthCurve.AddPoint(Vector2.Right);
    }

    public SpellLine(SpellSlot bot)
    {
        Points = new Vector2[2];
        this.WidthCurve = new Curve();
        this.WidthCurve.AddPoint(Vector2.Down);
        this.WidthCurve.AddPoint(Vector2.Right);
        MoveBot(bot);
    }
    public SpellLine(SpellSlot bot, SpellSlot tip)
    {
        Points = new Vector2[2];
        this.WidthCurve = new Curve();
        this.WidthCurve.AddPoint(Vector2.Down);
        this.WidthCurve.AddPoint(Vector2.Right);
        MoveBot(bot);
        MoveBot(tip);
    }

    public void DisconnectAll()
    {
        Tip = null;
        Bot = null;
    }
}
}