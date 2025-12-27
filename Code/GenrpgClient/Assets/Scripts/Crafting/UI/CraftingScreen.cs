using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Crafting.PlayerData.Recipes;
using Genrpg.Shared.Crafting.Settings.Recipes;
using Genrpg.Shared.Inventory.Constants;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class CraftingScreen : ItemIconScreen
{
    public const string RecipeRow = "RecipeRow";
    public const string ReagentRow = "ReagentRow";
    public const string CraftSlotIcon = "CraftSlotIcon";
    public const string CraftInventoryIcon = "CraftInventoryIcon";

    public GButton CraftButton;
    public GButton ClearButton;
    public InventoryPanel _inventoryPanel;
    public GameObject _recipeListParent;
    public ReagentRow _baseReagents;
    public ReagentRow _coreReagents;
    public ReagentRow _optionalReagents;

    private List<RecipeRow> _recipes { get; set; }

    private RecipeRow _currentRecipe = null;

    protected override async Task OnStartOpen(object data, CancellationToken token)
    {
        Init();
        await base.OnStartOpen(data, token);
    }

    public override void OnRightClickIcon(ItemIcon icon)
    {
    }

    public override void OnLeftClickIcon(ItemIcon icon)
    {
    }

    public void ClickCraft()
    {

    }

    public void ClickClear()
    {
        ClearScreen();
    }

    private void ClearScreen()
    {

    }

    public override void Init()
    {
        base.Init();
        _uiService.SetButton(ClearButton, GetName(), ClickClear);
        _uiService.SetButton(CraftButton, GetName(), ClickCraft);

        _recipes = new List<RecipeRow>();

        RecipeData recipeData = _gs.ch.Get<RecipeData>();
        foreach (RecipeType recipe in _gameData.Get<RecipeSettings>(_gs.ch).GetData())
        {

            RecipeStatus recipeStatus = recipeData.Get(recipe.IdKey);

            ShowOneRecipe(recipeStatus);
        }

        if (_inventoryPanel != null)
        {
            _inventoryPanel.Init(InventoryGroup.Reagents, this, _gs.ch, CraftInventoryIcon, GetToken());
        }

    }


    protected void ShowOneRecipe(RecipeStatus status)
    {
        if (status == null)
        {
            return;
        }

        _assetService.LoadAsset(AssetCategoryNames.UI, RecipeRow, OnLoadRecipeRow,
             _recipeListParent, GetToken(), status, Subdirectory);
    }

    private void OnLoadRecipeRow(GameObject go, RecipeStatus status, CancellationToken token)
    {

        RecipeRow row = go.GetComponent<RecipeRow>();
        if (status == null || row == null)
        {
            _clientEntityService.Destroy(go);
            return;
        }

        row.Init(status, this);
        _recipes.Add(row);
    }

    public void SetActiveRecipe(RecipeRow row)
    {
        if (row == _currentRecipe)
        {
            return;
        }

        if (_recipes == null)
        {
            return;
        }

        foreach (RecipeRow rec in _recipes)
        {
            rec.SetIsActive(rec == row);
        }
        _currentRecipe = row;
    }
}



