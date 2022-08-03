using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

public partial class AddressableManager
{
    private List<string> catalogForUpdate = new List<string>();
    private List<IResourceLocator> updatedLocators;
    
    private async Task CheckCatalogUpdate()
    {
        AsyncOperationHandle<List<string>> asyncCatalogCheck = Addressables.CheckForCatalogUpdates(false);
        await asyncCatalogCheck.Task;

        if (asyncCatalogCheck.Result.Count > 0)
        {
            Log($"Catalog need an update");
        }
        else
        {
            Log($"Catalog already up to date!");
        }

        catalogForUpdate = asyncCatalogCheck.Result;
        catalogForUpdate.ForEach((catalog) => Log($"Catalog needs update: {catalog}"));

        Addressables.Release(asyncCatalogCheck);
    }

    private async Task UpdateCatalog()
    {
        AsyncOperationHandle<List<IResourceLocator>> asyncUpdateCatalog = Addressables.UpdateCatalogs(catalogForUpdate, false);
        await asyncUpdateCatalog.Task;

        if (asyncUpdateCatalog.Status == AsyncOperationStatus.Succeeded)
        {
            updatedLocators = asyncUpdateCatalog.Result;
            updatedLocators.ForEach((locator) => Log($"Locator updated: {locator.LocatorId}"));
            Log($"Catalog update completed!");
        }
        else
        {
            Log($"Catalog update failed!");
        }
        Addressables.Release(asyncUpdateCatalog);
    }

}
