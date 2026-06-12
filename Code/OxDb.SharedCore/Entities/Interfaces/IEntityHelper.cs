using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.PlayerFiltering.Interfaces;
using System;
using System.Collections.Generic;

namespace OxDb.SharedCore.Entities.Interfaces
{
    public interface IEntityHelper : ISetupDictionaryItem<long>
    {
        List<IIdName> GetChildList(IFilteredObject obj);

        // Find an object of the given type.
        IIdName Find(IFilteredObject obj, long id);

        string GetIconAtlasName(IFilteredObject obj, long entityId, EEntityIconCategories category);

        string GetEditorPropertyName();

        bool IsMapEntity();

        bool IsEntityHelperFor(string name);

        Type GetParentType();
        Type GetChildType();

        string GetIconSuffix();
    }
}


