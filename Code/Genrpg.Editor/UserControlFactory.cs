using Genrpg.Editor.Entities.Core;
using Genrpg.Shared.Utils.Data;

namespace Genrpg.Editor
{
    public class UserControlFactory
    {
        public UserControlBase Create(EditorGameState gs,DataWindow win, object obj, object parent, object grandparent, DataView parentView)
        {
            UserControlBase uc = GetOverrideControl(gs, win, obj, parent, grandparent, parentView);
            if (uc != null)
            {
                return uc;
            }

            uc = new DataView(gs, win, obj, parent, grandparent, parentView);
            return uc;
        }
        // Use this to create custom controls for certain types or whatever you want.
        protected UserControlBase GetOverrideControl(EditorGameState gs,DataWindow win, object obj, object parent, object grandparent, DataView parentView)
        {

            MyColorF mc = obj as MyColorF;
            if (mc != null)
            {
                //return new ColorDataViewOld(gs, win, obj, parent, grandparent, parentView);
            }


            return null;

        }


    }
}
