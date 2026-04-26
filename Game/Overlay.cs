using Godot;

namespace Mahjong;

public partial class Overlay : CanvasLayer
{
	[Export] public Label Label;
	[Export] public Button Button;

	public override void _Ready()
	{
		Button.Pressed += OnMenuPressed;
		Visible = false;
	}

	public void ShowWin(int id)
	{
		if (id >= 0)
			Label.Text = $"Player {id} won!";
		else
			Label.Text = "Deck empty!";
		Visible = true;
	}

	private void OnMenuPressed()
	{
		GetTree().ChangeSceneToFile("res://GameUI/TitleScreen.tscn");
	}
}
