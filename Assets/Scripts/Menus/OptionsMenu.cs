using UnityEngine.UI;

public class OptionsMenu : Menu
{
	public Slider Slider;

	public void OnMagicButtonPressed()
	{
		AwesomeMenu.Show(Slider.value);
	}
}
