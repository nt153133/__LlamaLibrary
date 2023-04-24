using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.NeoProfiles;
using ff14bot.RemoteWindows;
using LlamaLibrary.Helpers;
using LlamaLibrary.Helpers.NPC;
using LlamaLibrary.Helpers.Ping;
using LlamaLibrary.JsonObjects;
using LlamaLibrary.Logging;
using LlamaLibrary.RemoteWindows;

namespace LlamaLibrary.Utilities;

public class GilShopping
{
    private static readonly LLogger Log = new("GilShopper", Colors.Gold);

    public static Dictionary<uint, uint> ItemVendors = new()
    {
        { 38829, 1044548 },
        { 38825, 1044548 },
        { 38826, 1044548 },
        { 38827, 1044548 },
        { 38828, 1044548 },
    };

    public static Npc AndenVendor = new Npc(1044548, 816, new Vector3(-246.9673f, 51.059f, 617.0291f));

    public static List<Npc> Vendors = new List<Npc>() { AndenVendor };

    public static async Task<bool> GetRequiredItems(StoredRecipe recipe, int amount = 1)
    {
        var ingredients = recipe.GetRequiredIngredients(amount);

       // Log.Information($"\n\t{string.Join("\n\t", ingredients.Select(i => i.ToString()))}");

        var gotAll = true;
        foreach (var recipeIngredient in ingredients.Where(i => i.Type == IngredientType.GilShop && ItemVendors.ContainsKey(i.Item)))
        {
            if (!ConditionParser.HasAtLeast(recipeIngredient.Item, (int)recipeIngredient.Amount))
            {
                Log.Information($"Need {(recipeIngredient.Amount - ConditionParser.ItemCount(recipeIngredient.Item))} more");
                if (!await PurchaseIngredients(recipeIngredient.Item, (uint)(recipeIngredient.Amount - ConditionParser.ItemCount(recipeIngredient.Item))))
                {
                    gotAll = false;
                }
            }
        }

        return gotAll;
    }

    public static async Task<bool> PurchaseIngredients(uint itemId, uint amount)
    {
        if (!ItemVendors.ContainsKey(itemId))
        {
            Log.Information($"No vendor for {itemId}");
            return false;
        }

        var vendor = Vendors.FirstOrDefault(i => i.NpcId == ItemVendors[itemId]);

        if (!await Navigation.GetToInteractNpc(vendor, ShopProxy.Instance))
        {
            Log.Information("Something went wrong");
            TreeRoot.Stop("Stop Requested");
            return true;
        }

        Log.Information($"Purchase {amount}");
        Shop.Purchase(itemId, amount);

        if (!await Coroutine.Wait(5000, () => SelectYesno.IsOpen))
        {
            Log.Error("Yes/No not open");
            return false;
        }

        SelectYesno.Yes();
        await Coroutine.Wait(5000, () => !SelectYesno.IsOpen);

        await Coroutine.Sleep(PingChecker.CurrentPing);
        await Coroutine.Sleep(500);
        Shop.Close();

        return ConditionParser.HasAtLeast(itemId, (int)amount);
    }
}