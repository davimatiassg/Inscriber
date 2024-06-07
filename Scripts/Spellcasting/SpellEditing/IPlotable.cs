using System.Threading.Tasks;
using Godot;

namespace SpellEditing
{
public interface IPlotable
{
    public Texture2D Portrait { get; }
    public string Name { get; protected set; }
}

}

