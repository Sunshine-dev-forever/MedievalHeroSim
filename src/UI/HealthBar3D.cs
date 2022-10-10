using Godot;

public class HealthBar3D : Sprite3D {
	private ProgressBar? bar;
	public override void _Ready(){
		bar = GetNode<ProgressBar>("Viewport/HealthBar2D");
		this.Texture = GetNode<Viewport>("Viewport").GetTexture();
		SetHealthPercent(1);
	}

	public void SetHealthPercent(float percent){
		if(bar != null){
			bar.Value = percent * 100;
		}
	}
}