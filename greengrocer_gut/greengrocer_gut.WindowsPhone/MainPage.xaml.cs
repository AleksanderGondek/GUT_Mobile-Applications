using Windows.UI.Xaml.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;  // offline sync
using Microsoft.WindowsAzure.MobileServices.Sync;

namespace greengrocer_gut
{
    public sealed partial class MainPage : Page
    {
        private async void ButtonPull_Click(object sender, RoutedEventArgs e)
        {
            string errorString = null;

            // Prevent extra clicking while Pull is in progress
            ButtonPull.Focus(FocusState.Programmatic);
            ButtonPull.IsEnabled = false;

            try
            {
                // All items should be synced since other clients might mark an item as complete
                // The first parameter is a query ID that uniquely identifies the query.
                // This is used in incremental sync to get only newer items the next time PullAsync is called
                await _groceriesTable.PullAsync("greengrocer_gut.Groceries", _groceriesTable.CreateQuery());

                await RefreshTodoItems();
            }
            catch (MobileServicePushFailedException ex)
            {
                errorString = "Internal Push operation during pull request failed because of sync errors: " +
                  ex.PushResult.Errors.Count + " errors, message: " + ex.Message;

                foreach (var conflictItem in ex.PushResult.Errors)
                {
                    var serverItem = conflictItem.Item.ToObject<Groceries>();
                    serverItem.Version = conflictItem.Item.GetValue("__version").ToObject<string>();
                    var localItem = await FindGroceryById(serverItem.Id);

                    await ResolveConflict(localItem, serverItem);
                }
            }
            catch (Exception ex)
            {
                errorString = "Pull failed: " + ex.Message +
                  "\n\nIf you are still in an offline scenario, " +
                  "you can try your Pull again when connected with your Mobile Serice.";
            }

            if (errorString != null)
            {
                MessageDialog d = new MessageDialog(errorString);
                await d.ShowAsync();
            }

            ButtonPull.IsEnabled = true;
        }

        private async void ButtonPush_Click(object sender, RoutedEventArgs e)
        {
            string errorString = null;

            // Prevent extra clicking while Push is in progress
            ButtonPush.Focus(FocusState.Programmatic);
            ButtonPush.IsEnabled = false;

            try
            {
                await App.MobileService.SyncContext.PushAsync();
            }
            catch (MobileServicePushFailedException ex)
            {
                errorString = "Push failed because of sync errors: " +
                  ex.PushResult.Errors.Count + " errors, message: " + ex.Message;

                foreach(var conflictItem in ex.PushResult.Errors)
                {
                    var serverItem = conflictItem.Result.ToObject<Groceries>();
                    serverItem.Version = conflictItem.Result.GetValue("__version").ToObject<string>();
                    var localItem = await FindGroceryById(serverItem.Id);

                    await ResolveConflict(localItem, serverItem);
                }
            }
            catch (Exception ex)
            {
                errorString = "Push failed: " + ex.Message +
                  "\n\nIf you are still in an offline scenario, " +
                  "you can try your Push again when connected with your Mobile Serice.";
            }

            if (errorString != null)
            {
                MessageDialog d = new MessageDialog(errorString);
                await d.ShowAsync();
            }

            ButtonPush.IsEnabled = true;
        }

        private async Task<Groceries> FindGroceryById(string itemId)
        {
            var list = await _groceriesTable.Where(x => x.Id == itemId).ToListAsync();
            return list.Count > 0 ? list.FirstOrDefault() : null;
        }
    }
}
