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

    private void MoveBot(Control obj) { SetPointPosition(0, obj.GetGlobalRect().GetCenter()); }
    private void MoveTip(Control obj) { SetPointPosition(1, obj.GetGlobalRect().GetCenter()); }

    public SpellLine()
    {
        Points = new Vector2[2];
    }

    public void ConnectTip(RuneSlot tip)
    {
        this.tip = tip;
        _moveTip = () => MoveTip(tip);
        tip.getMoved += _moveTip;
        _moveTip();
        GD.Print("Tip: " + Points[1] + " // Object Connected" + tip.Name + " With pos: " +  tip.GetGlobalRect().GetCenter());
        tip.spellLines.Add(this);
    }
    public void ConnectBot(RuneSlot bot)
    {
        this.bot = bot;
        _moveBot = () => MoveBot(bot);
        bot.getMoved += _moveBot;
        _moveBot();
        GD.Print("Bot: " + Points[0] + " // Object Connected" + bot.Name + " With pos: " +  bot.GetGlobalRect().GetCenter());
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
        _moveCursor();
        GD.Print("Cursor: " + Points[1] + " // Object Connected" + cursor.Name + " With pos: " +  cursor.GetGlobalRect().GetCenter());
        cursor.getMoved += _moveCursor;
    }

    public void DisconnectCursor()
    {
        if(cursor != null) { cursor.getMoved -= _moveCursor; }
        
    }
}

}