using UnityEngine;
using UnityEngine.Events;

public class Menu : MonoBehaviour
{
    [Tooltip("Destroy game object when closed.")]
    public bool DestroyWhenClosed = true;

    [Tooltip("Disable all menus under this one.")]
    public bool DisableMenusUnderneath = true;

    public UnityEvent OnOpen;
    public UnityEvent OnClose;
    public UnityEvent OnBack;

    public virtual void Open ()
	{
        gameObject.SetActive(true);
		MenuManager.Instance.OpenMenu(this);

        if (OnOpen != null)
            OnOpen.Invoke ();
    }

    public virtual void Close ()
	{
        if (OnClose != null)
            OnClose.Invoke ();

        MenuManager.Instance.CloseMenu(this);
	}

    public virtual void GoTo (Menu menu)
    {
        MenuManager.Instance.OpenMenu (menu);
    }

    /// <summary>
    /// Called on escape key or back button.
    /// </summary>
    public virtual void GoBack()
	{
        if (OnBack != null)
            OnBack.Invoke ();
	}

    public void Quit ()
    {
#if UNITY_EDITOR
        // Exit play mode if in the editor.
        UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
    }
}
