using Godot;
using SpellEditing;
using System;
using System.Threading.Tasks;

public partial class SpellEditorMetaMenu : Panel
{
	[Export] public Button saveButton;
	[Export] public Button returnButton;
	[Export] public Button exitButton;

	[Export] public TextEdit titleField;
	[Export] public TextEdit descriptionField;
	// Called when the node enters the scene tree for the first time.
	public void SetupButtons(SpellGraphEditor graphView)
	{
		saveButton.Pressed += async () => 
		{
			(string, string) texts = GetTexts();
			if(texts.Item1 == "") return;
			Texture2D portrait = await GetPhoto();
			SpellRepository.SaveSpell<SpellGraphView, VisualNode>(graphView, texts.Item1 == "" ? "newSpell" : texts.Item1 , texts.Item2, portrait);
			graphView.LaunchSpellSelector();
		};

		returnButton.Pressed += graphView.CloseMetaMenu;

		exitButton.Pressed += graphView.LaunchSpellSelector;
	}

	public (string, string) GetTexts() => (titleField.Text, descriptionField.Text);


	public async Task<Texture2D> GetPhoto() { 
		await ToSignal(RenderingServer.Singleton, RenderingServerInstance.SignalName.FramePostDraw);
		
		//STUB:
			var image = GetViewport().GetTexture().GetImage();
			image.Crop(96, 96);
		
		return ImageTexture.CreateFromImage(image);
		
	} 

}
