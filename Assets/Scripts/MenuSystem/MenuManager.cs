using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    private bool PreloadMenus = false;
    private Stack<Menu> menuStack = new Stack<Menu>();

    public static MenuManager Instance
    {
        get; set;
    }

    void Awake()
    {
        if (PreloadMenus)
        {
            LoadAllMenuResources();
        }

        Instance = this;
        MainMenu.Show(); // Open MainMenu on startup.
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    // Loads all menu prefabs from "Resources/Menus".
    private void LoadAllMenuResources()
    {
        var menus = Resources.LoadAll<Menu>("Menus");
        foreach (var m in menus)
        {
            Debug.Log("Loaded: " + m.name + " Prefab");
        }
    }

    public void CreateInstance<T>() where T : Menu
    {
        var prefab = GetMenuPrefab<T>().gameObject;

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

    private T GetMenuPrefab<T>() where T : Menu
    {
        Object[] prefabs;
        if (PreloadMenus)
        {
            prefabs = Resources.FindObjectsOfTypeAll<T>();
        }
        else
        {
            prefabs = Resources.LoadAll<T>("Menus");
        }

        if (prefabs.Length > 0)
        {
            if (prefabs.Length > 1)
                Debug.LogWarning("More then one prefab of type " + typeof(T) + " exist. Using first one found.");

            if (!PreloadMenus)
                Debug.Log("Loaded " + prefabs[0].name + " prefab");

            return (T)prefabs[0];
        }

        throw new MissingReferenceException("No prefab of type " + typeof(T) + " found in 'Resources/Menus' folder. Please add one.");
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

        // For memory optimization.
        if (!PreloadMenus) Resources.UnloadUnusedAssets();
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

