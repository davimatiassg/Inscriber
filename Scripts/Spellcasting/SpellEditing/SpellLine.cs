using Godot;
using Godot.NativeInterop;
using System;
using System.Linq;

namespace SpellEditing
{
public partial class SpellLine : Line2D
{
    
    public Action _moveTip;
    public RuneSlot tip;
    public Action _moveBot;
    public RuneSlot bot;
    public Action _moveCursor;
    private SpellCursor cursor;

    private void MoveBot(Control obj) { Points.SetValue(obj.GetGlobalRect().GetCenter(), 0); GD.Print("Pos0:" + Points[0]);}
    private void MoveTip(Control obj) { Points.SetValue(obj.GetGlobalRect().GetCenter(), 1); GD.Print("Pos1:" + Points[1] + " // " + obj.Name + " With pos: " +  obj.GetGlobalRect().GetCenter());}

    public SpellLine()
    {
    }

    public void ConnectTip(RuneSlot tip)
    {
        this.tip = tip;
        _moveTip = () => MoveTip(tip);
        tip.getMoved += _moveTip;
        tip.spellLines.Add(this);
    }
    public void ConnectBot(RuneSlot bot)
    {
        this.bot = bot;
        _moveBot = () => MoveBot(bot);
        bot.getMoved += _moveBot;
        bot.spellLines.Add(this);
    }

    public void Connect(RuneSlot tip, RuneSlot bot) {ConnectTip(tip); ConnectBot(bot); } 

    public void Disconnect()
    {
        if(bot != null)
        {
            bot.getMoved -= _moveBot;
            bot.spellLines.Remove(this);
            bot = null;
        }
        if(tip != null)
        {
            tip.getMoved -= _moveTip;
            tip.spellLines.Remove(this);
            tip = null;
        }
        DisconnectCursor();
        QueueFree();
    }

    public void ConnectCursor(SpellCursor cursor)
    {
        this.cursor = cursor;
        _moveCursor = () => {MoveTip(cursor);};
        cursor.getMoved += _moveCursor;
    }

    public void DisconnectCursor()
    {
        if(cursor != null) { cursor.getMoved -= _moveCursor; }
        
    }
}

}