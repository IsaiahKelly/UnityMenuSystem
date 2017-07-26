using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    private bool loaded = false; // Check to see if resources are already loaded.
    private Stack<Menu> menuStack = new Stack<Menu>();

    public static MenuManager Instance
    {
        get; set;
    }

    private void Awake()
    {
        Instance = this;
        MainMenu.Show(); // Open MainMenu on startup.
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    private void LoadMenuResources()
    {
        if (loaded) return;

        // Probably a better way to do this, but right now I need to load everything
        // to prevent null references when using Resources.FindObjectsOfTypeAll later.
        var menus = Resources.LoadAll("Menus", typeof(Menu));

        foreach (var m in menus)
        {
            Debug.Log("Loaded resource: " + m.name);
        }

        loaded = true;
    }

    public void CreateInstance<T>() where T : Menu
    {
        var prefab = GetMenuPrefab<T>();

        Instantiate(prefab, transform);
    }

    public void OpenMenu(Menu instance)
    {
        if (menuStack.Count > 0)
        {
            // Disable all other menus.
            if (instance.DisableMenusUnderneath)
            {
                foreach (var menu in menuStack)
                {
                    menu.gameObject.SetActive(false);

                    if (menu.DisableMenusUnderneath)
                        break;
                }
            }

            // Place new menu above all others.
            instance.transform.SetAsLastSibling();
        }

        menuStack.Push(instance);
    }

    private GameObject GetMenuPrefab<T>() where T : Menu
    {
        LoadMenuResources();
        T[] menuPrefabs = Resources.FindObjectsOfTypeAll<T>();
        if (menuPrefabs.Length > 0)
        {
            if (menuPrefabs.Length > 1)
                Debug.LogWarning("There is more then one prefab for type " + typeof(T) + ". Using first one found.");

            return menuPrefabs[0].gameObject;
        }

        throw new MissingReferenceException("No prefab for type " + typeof(T) + " found. Please create one.");
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

        // Re-enable top menu
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
        // On Android the back button is sent as escape key.
        if (Input.GetKeyDown(KeyCode.Escape) && menuStack.Count > 0)
        {
            menuStack.Peek().OnBackPressed();
        }
    }
}

