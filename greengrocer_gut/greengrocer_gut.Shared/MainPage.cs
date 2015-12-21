using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;  // offline sync
using Microsoft.WindowsAzure.MobileServices.Sync;         // offline sync


namespace greengrocer_gut
{
    sealed partial class MainPage
    {
        private MobileServiceCollection<Groceries, Groceries> _items;
        private MobileServiceUser _user;
        //private readonly IMobileServiceTable<Groceries> _groceriesTable = App.MobileService.GetTable<Groceries>();
        private IMobileServiceSyncTable<Groceries> _groceriesTable = App.MobileService.GetSyncTable<Groceries>(); // offline sync

        public MainPage()
        {
            InitializeComponent();

            // Schedule Autheticate Async as the next thing to be handled by UI thread
            // This is a very ugly workaround for changed approach to authetication
            // In WP 8.0 this was not necessary
            Loaded += async (sender, args) =>
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, AuthenticateAsync);
            };
        }

        public async void AuthenticateAsync()
        {
            while (_user == null)
            {
                string message;
                try
                {
                    //_user = await App.MobileService.LoginAsync(MobileServiceAuthenticationProvider.Twitter);
                    // Temporary just used fixed user
                    _user = new MobileServiceUser("SonsOfAnarchy");
                    message = $"You are now signed in - {_user.UserId}";
                }
                catch (InvalidOperationException)
                {
                    message = "You must log in. Login Required";
                }

                var dialog = new MessageDialog(message);
                dialog.Commands.Add(new UICommand("OK"));
                await dialog.ShowAsync();

                await InitLocalStoreAsync(); // offline sync
                await RefreshTodoItems();
            }
        }

        private async Task InsertTodoItem(Groceries todoItem)
        {
            await _groceriesTable.InsertAsync(todoItem);
            _items.Add(todoItem);
        }

        private async Task<Groceries> FindGroceryByName(string itemName)
        {
            var list = await _groceriesTable.Where(x => x.Name == itemName && x.OwnerUserId ==_user.UserId).ToListAsync();
            return list.Count > 0 ? list.FirstOrDefault() : null;
        }

        private async Task RefreshTodoItems()
        {
            MobileServiceInvalidOperationException exception = null;
            try
            {
                _items = await _groceriesTable
                    .Where(x => x.OwnerUserId == _user.UserId)
                    .ToCollectionAsync();
            }
            catch (MobileServiceInvalidOperationException e)
            {
                exception = e;
            }

            if (exception != null)
            {
                await new MessageDialog(exception.Message, "Error loading items").ShowAsync();
            }
            else
            {
                ListItems.ItemsSource = _items;
                ButtonSave.IsEnabled = true;
            }
        }

        private async Task UpdateCheckedTodoItem(Groceries item)
        {
            await _groceriesTable.UpdateAsync(item);

            _items.Remove(item);
            ListItems.Focus(FocusState.Unfocused);

            ButtonRefresh_Click(null, null);
        }

        private async void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            ButtonRefresh.IsEnabled = false;
            if(sender != null && e != null)
            {
                await SyncAsync(); // offline sync
            }
            await RefreshTodoItems();
            ButtonRefresh.IsEnabled = true;
        }

        private async void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextInput.Text) && string.IsNullOrEmpty(QuantityInput.Text)) return;

            // If item exists, just change quantity
            var existingItem = await FindGroceryByName(TextInput.Text);
            if (existingItem != null)
            {
                try
                {
                    if(existingItem.Before == 0)
                    {
                        existingItem.Before = existingItem.Quantity;
                    }

                    var quantity = Convert.ToInt32(QuantityInput.Text);
                    existingItem.Quantity = quantity;
                }
                catch (Exception)
                {
                    QuantityInput.Text = existingItem.Quantity.ToString();
                    return;
                }

                await UpdateCheckedTodoItem(existingItem);
                return;
            }

            try
            {
                var quantity = Convert.ToInt32(QuantityInput.Text);
                if (quantity <= 0) return;

                var todoItem = new Groceries {Id = Guid.NewGuid().ToString("N"), Name = TextInput.Text, Quantity = quantity, OwnerUserId = _user.UserId, Before = quantity};
                await InsertTodoItem(todoItem);
            }
            catch (Exception) {}
            finally
            {
                TextInput.Text = string.Empty;
                QuantityInput.Text = "0";
            }
        }

        private async void GroceryItemNameChange(object sender, RoutedEventArgs routedEventArgs)
        {
            var textBox = sender as TextBox;
            var groceryItem = textBox?.DataContext as Groceries;
            if (groceryItem == null) return;

            try
            {
                groceryItem.Name = textBox.Text;
            }
            catch (Exception)
            {
                textBox.Text = groceryItem.Name;
                return;
            }

            await UpdateCheckedTodoItem(groceryItem);
        }

        private async void GroceryItemAdd(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var groceryItem = button?.DataContext as Groceries;
            if (groceryItem == null) return;

            try
            {
                if (groceryItem.Before == 0)
                {
                    groceryItem.Before = groceryItem.Quantity;
                }
                groceryItem.Quantity = groceryItem.Quantity + 1;
            }
            catch (Exception)
            {
                return;
            }
                
            await UpdateCheckedTodoItem(groceryItem);
        }

        private async void GroceryItemRemove(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            var groceryItem = button?.DataContext as Groceries;
            if (groceryItem == null) return;

            try
            {
                if (groceryItem.Before == 0)
                {
                    groceryItem.Before = groceryItem.Quantity;
                }
                groceryItem.Quantity = groceryItem.Quantity - 1;
            }
            catch (Exception)
            {
                return;
            }

            await UpdateCheckedTodoItem(groceryItem);
        }

        private async void GroceryItemQuantityChange(object sender, RoutedEventArgs routedEventArgs)
        {
            var textBox = sender as TextBox;
            var groceryItem = textBox?.DataContext as Groceries;
            if (groceryItem == null) return;

            try
            {
                if (groceryItem.Before == 0)
                {
                    groceryItem.Before = groceryItem.Quantity;
                }
                var quantity = Convert.ToInt32(textBox.Text);
                groceryItem.Quantity = quantity;
            }
            catch (Exception)
            {
                textBox.Text = groceryItem.Quantity.ToString();
                return;
            }

            await UpdateCheckedTodoItem(groceryItem);
        }

        private async Task InitLocalStoreAsync()
        {
            if (!App.MobileService.SyncContext.IsInitialized)
            {
                var store = new MobileServiceSQLiteStore("greengrocer-gut_db.db");
                store.DefineTable<Groceries>();
                //await App.MobileService.SyncContext.InitializeAsync(store, new SyncHandler(App.MobileService));
                await App.MobileService.SyncContext.InitializeAsync(store);
            }

            //await SyncAsync();
        }

        private async Task SyncAsync()
        {

            String errorString = null;

            try
            {
                await _groceriesTable.PullAsync("greengrocer_gut.Groceries", _groceriesTable.CreateQuery());
                await App.MobileService.SyncContext.PushAsync();
                await _groceriesTable.PullAsync("greengrocer_gut.Groceries", _groceriesTable.CreateQuery()); // first param is query ID, used for incremental sync
            }

            catch (MobileServicePushFailedException ex)
            {
                errorString = "Push failed because of sync errors: " +
                  ex.PushResult.Errors.Count + " errors, message: " + ex.Message;
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
        }

        private async Task ResolveConflict(Groceries localVersion, Groceries azuresVersion)
        {
            localVersion.Version = azuresVersion.Version;
            
            // this way we have a number to add to , sign will be good
            int delta = localVersion.Quantity - localVersion.Before;
            localVersion.Quantity = azuresVersion.Quantity + delta;
            localVersion.Before = 0;

            await _groceriesTable.UpdateAsync(localVersion);
        }
    }
}
