using System;
using UIKit;
using System.Collections.Generic;
using System.Linq;
using Foundation;

namespace Assisticant.Binding
{
    public static class TableViewBindingExtensions
    {
        class ItemContainer<T>
        {
            private readonly T _item;
            private BindingManager _bindings = new BindingManager();

            public ItemContainer(T item)
            {
                _item = item;
            }

            public T Item
            {
                get { return _item; }
            }

            public BindingManager Bindings
            {
                get { return _bindings; }
            }
        }

        class CollectionBinding<T> : UITableViewSource, IInputSubscription
        {
            private readonly UITableView _tableView;
            private readonly Action<UITableViewCell, T, BindingManager> _bind;
            private readonly Action<T> _selected;
            private List<ItemContainer<T>> _itemContainers = new List<ItemContainer<T>>();
   
            public CollectionBinding(
                UITableView tableView,
                Action<UITableViewCell, T, BindingManager> bind,
                Action<T> selected)
            {
                _tableView = tableView;
                _bind = bind;
                _selected = selected;
            }

            public void UpdateItems (IEnumerable<T> items)
            {
                foreach (var itemContainer in _itemContainers)
                    itemContainer.Bindings.Unbind();
                _itemContainers.Clear();
                _itemContainers.AddRange(items.Select(item =>
                    new ItemContainer<T>(item)));

                _tableView.ReloadData();
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                return _itemContainers.Count;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var itemContainer = _itemContainers[indexPath.Row];
                itemContainer.Bindings.Unbind();

                UITableViewCell cell = _tableView.DequeueReusableCell ("TableCell");
                // if there are no cells to reuse, create a new one
                if (cell == null)
                    cell = new UITableViewCell (UITableViewCellStyle.Default, "TableCell");

                var scheduler = UpdateScheduler.Begin();
                _bind(cell, itemContainer.Item, itemContainer.Bindings);
                foreach (var update in scheduler.End())
                    update();

                return cell;
            }

            public override void CellDisplayingEnded(UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
            {
				if (_itemContainers.Count > indexPath.Row)
				{
					var itemContainer = _itemContainers[indexPath.Row];
					itemContainer.Bindings.Unbind();
				}
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
				if (_itemContainers.Count > indexPath.Row)
				{
					var itemContainer = _itemContainers[indexPath.Row];
					_selected(itemContainer.Item);
				}
            }

            public void Subscribe ()
            {
            }

            public void Unsubscribe ()
            {
                foreach (var itemContainer in _itemContainers)
                {
                    itemContainer.Bindings.Unbind();
                }
            }
        }

        /// <summary>
        /// Bind the items of a UICollectionView to a collection.
        /// </summary>
        /// <typeparam name="T">The type of item in the collection.</typeparam>
        /// <param name="bindings">The binding manager.</param>
        /// <param name="control">The ListView to bind.</param>
        /// <param name="output">The collection to which to bind.</param>
        /// <param name="bind">The delegate that binds each item of the collection.</param>
        public static void BindItems<T>(
            this BindingManager bindings,
            UITableView control,
            Func<IEnumerable<T>> output,
            Action<UITableViewCell, T, BindingManager> bind)
        {
            BindItems<T>(bindings, control, output, () => default(T), _ => { }, bind);
        }

        /// <summary>
        /// Bind the items of a UICollectionView to a collection.
        /// </summary>
        /// <typeparam name="T">The type of item in the collection.</typeparam>
        /// <param name="bindings">The binding manager.</param>
        /// <param name="control">The ListView to bind.</param>
        /// <param name="output">The collection to which to bind.</param>
        /// <param name="selection">Get current selection.</param>
        /// <param name="selected">Set the current selection.</param>
        /// <param name="bind">The delegate that binds each item of the collection.</param>
        public static void BindItems<T>(
            this BindingManager bindings,
            UITableView control,
            Func<IEnumerable<T>> output,
            Func<T> selection,
            Action<T> selected,
            Action<UITableViewCell, T, BindingManager> bind)
        {
            var binding = new CollectionBinding<T>(control, bind, selected);
            control.Source = binding;
            bindings.Bind(() => output().ToList(), items => binding.UpdateItems(items), binding);
        }
    }
}

