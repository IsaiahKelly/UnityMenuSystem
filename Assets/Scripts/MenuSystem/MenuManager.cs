using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public Menu[] menus;

	public MainMenu MainMenuPrefab;
	public GameMenu GameMenuPrefab;
	public PauseMenu PauseMenuPrefab;
	public OptionsMenu OptionsMenuPrefab;
	public AwesomeMenu AwesomeMenuPrefab;

    private Stack<Menu> menuStack = new Stack<Menu>();

    public static MenuManager Instance { get; set; }

    private void Awake()
    {
        Instance = this;

		MainMenu.Show();
    }

    private void OnDestroy()
    {
        Instance = null;
    }

	public void CreateInstance<T>() where T : Menu
	{
		var prefab = GetPrefab<T>();

		Instantiate(prefab, transform);
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

        menuStack.Push(instance);
    }

    private T GetPrefab<T>() where T : Menu
    {
        // Search menus array for matching type.
        for (int i = 0; i < menus.Length; i++)
        {
            if (menus[i] is T)
                return (T)menus[i];
        }

        throw new MissingReferenceException("Prefab not found for type " + typeof(T));
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

		if (instance.DestroyWhenClosed)
        	Destroy(instance.gameObject);
		else
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
        // On Android the back button is sent as Esc
        if (Input.GetKeyDown(KeyCode.Escape) && menuStack.Count > 0)
        {
            menuStack.Peek().OnBackPressed();
        }
    }
}

