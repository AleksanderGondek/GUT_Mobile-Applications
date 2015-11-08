﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.WindowsAzure.MobileServices;


namespace greengrocer_gut
{
    sealed partial class MainPage
    {
        private MobileServiceCollection<Groceries, Groceries> _items;
        private MobileServiceUser _user;
        private readonly IMobileServiceTable<Groceries> _groceriesTable = App.MobileService.GetTable<Groceries>();

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
        private async void MainPageLoaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //TODO: Refresh
            //await RefreshTodoItems();
        }

        public async void AuthenticateAsync()
        {
            while (_user == null)
            {
                string message;
                try
                {
                    _user = await App.MobileService.LoginAsync(MobileServiceAuthenticationProvider.Twitter);
                    message = $"You are now signed in - {_user.UserId}";
                }
                catch (InvalidOperationException)
                {
                    message = "You must log in. Login Required";
                }

                var dialog = new MessageDialog(message);
                dialog.Commands.Add(new UICommand("OK"));
                await dialog.ShowAsync();
            }
        }

        private async Task InsertTodoItem(Groceries todoItem)
        {
            await _groceriesTable.InsertAsync(todoItem);
            _items.Add(todoItem);
        }

        private async Task<Groceries> FindGroceryByName(string itemName)
        {
            var list = await _groceriesTable.Where(x => x.Name.Equals(itemName) && x.OwnerUserId.Equals(_user.UserId)).ToListAsync();
            return list.Count > 0 ? list.FirstOrDefault() : null;
        }

        private async Task RefreshTodoItems()
        {
            MobileServiceInvalidOperationException exception = null;
            try
            {
                _items = await _groceriesTable
                    .Where(x => string.Equals(x.OwnerUserId, _user.UserId))
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
                    var quantity = Convert.ToInt32(QuantityInput.Text);
                    existingItem.Quantity = quantity;
                }
                catch (Exception)
                {
                    QuantityInput.Text = existingItem.Quantity.ToString();
                    return;
                }

                if (existingItem.Quantity > 0)
                {
                    await UpdateCheckedTodoItem(existingItem);
                }
                else
                {
                    _items.Remove(existingItem);
                    await _groceriesTable.DeleteAsync(existingItem);
                }

                await _groceriesTable.UpdateAsync(existingItem);
            }

            try
            {
                var quantity = Convert.ToInt32(QuantityInput.Text);
                if (quantity <= 0) return;

                var todoItem = new Groceries {Id = Guid.NewGuid().ToString("N"), Name = TextInput.Text, Quantity = quantity, OwnerUserId = _user.UserId};
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
                groceryItem.Quantity = groceryItem.Quantity - 1;
            }
            catch (Exception)
            {
                return;
            }

            if (groceryItem.Quantity > 0)
            {
                await UpdateCheckedTodoItem(groceryItem);
            }
            else
            {
                _items.Remove(groceryItem);
                await _groceriesTable.DeleteAsync(groceryItem);
            }
        }

        private async void GroceryItemQuantityChange(object sender, RoutedEventArgs routedEventArgs)
        {
            var textBox = sender as TextBox;
            var groceryItem = textBox?.DataContext as Groceries;
            if (groceryItem == null) return;

            try
            {
                var quantity = Convert.ToInt32(textBox.Text);
                groceryItem.Quantity = quantity;
            }
            catch (Exception)
            {
                textBox.Text = groceryItem.Quantity.ToString();
                return;
            }

            if (groceryItem.Quantity > 0)
            {
                await UpdateCheckedTodoItem(groceryItem);
            }
            else
            {
                _items.Remove(groceryItem);
                await _groceriesTable.DeleteAsync(groceryItem);
            }
        }
    }
}
