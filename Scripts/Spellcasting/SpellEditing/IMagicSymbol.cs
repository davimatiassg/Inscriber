using System.Threading.Tasks;
using Godot;

namespace SpellEditing
{

public interface IMagicSymbol
{
    public Texture2D Portrait { get; }
    public string Name { get; protected set; }
    public string Category { get; }
    public string Description { get; }
    public Color Color { get; }
}

}

