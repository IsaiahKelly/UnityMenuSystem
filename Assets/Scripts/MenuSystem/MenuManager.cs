using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    private Menu startMenu;

    private Stack<Menu> menuStack = new Stack<Menu>();

    public static MenuManager Instance { get; set; }

    private void Awake()
    {
        Instance = this;

        if (startMenu)
            startMenu.Open ();
    }

    private void OnDestroy()
    {
        Instance = null;
    }

	public void OpenMenu(Menu instance)
    {
        // De-activate top menu
        if (menuStack.Count > 0)
        {
			if (instance.DisableMenusUnderneath)
			{
				foreach (var menu in menuStack)
				{
					menu.gameObject.SetActive(false);

					if (menu.DisableMenusUnderneath)
						break;
				}
			}

            var topCanvas = instance.GetComponent<Canvas>();
            var previousCanvas = menuStack.Peek().GetComponent<Canvas>();
			topCanvas.sortingOrder = previousCanvas.sortingOrder + 1;
        }

        instance.gameObject.SetActive (true);
        menuStack.Push(instance);
    }

	public void CloseMenu(Menu menu)
	{
		if (menuStack.Count == 0)
		{
			Debug.LogErrorFormat(menu, "{0} cannot be closed because menu stack is empty", menu.GetType());
			return;
		}

		if (menuStack.Peek() != menu)
		{
			Debug.LogErrorFormat(menu, "{0} cannot be closed because it is not on top of stack", menu.GetType());
			return;
		}

		CloseTopMenu();
	}

	public void CloseTopMenu()
    {
        var instance = menuStack.Pop();

		//if (instance.DestroyWhenClosed)
  //      	Destroy(instance.gameObject);
		//else
			instance.gameObject.SetActive(false);

        // Re-activate top menu
		// If a re-activated menu is an overlay we need to activate the menu under it
		foreach (var menu in menuStack)
		{
            menu.gameObject.SetActive(true);

			if (menu.DisableMenusUnderneath)
				break;
		}
    }

    private void Update()
    {
        // Menu escape key.
        if (Input.GetKeyDown(KeyCode.Escape) && menuStack.Count > 0)
        {
            menuStack.Peek ().GoBack ();
        }
    }
}

