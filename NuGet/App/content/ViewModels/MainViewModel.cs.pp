using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using $rootnamespace$.Models;
using Assisticant;

namespace $rootnamespace$.ViewModels
{
    public sealed class MainViewModel : IEquatable<MainViewModel>
    {
        private readonly Document _document;
        private readonly Selection _selection;

        public MainViewModel(Document document, Selection selection)
        {
            _document = document;
            _selection = selection;
        }

        #region Properties
        public IEnumerable<ItemHeader> Items
        {
            get
            {
                return _document.Items.Select(item => new ItemHeader(item));
            }
        }

        public ItemHeader SelectedItem
        {
            get
            {
                return _selection.IsItemSelected
                    ? new ItemHeader(_selection.SelectedItem)
                    : null;
            }
            set
            {
                if (value != null)
                    _selection.SelectedItem = value.Item;
            }
        }

        public ItemViewModel ItemDetail
        {
            get
            {
                return _selection.IsItemSelected
                    ? new ItemViewModel(_selection.SelectedItem)
                    : null;
            }
        }
        #endregion

        #region Commands
        public ICommand AddItem
        {
            get
            {
                return MakeCommand
                    .Do(CreateAndSelectNewItem);
            }
        }

        public ICommand DeleteItem
        {
            get
            {
                return MakeCommand
                    .When(() => _selection.IsItemSelected)
                    .Do(DeleteSelectedItem);
            }
        }

        public ICommand MoveItemDown
        {
            get
            {
                return MakeCommand
                    .When(AnItemIsSelectedThatIsNotOnBottom)
                    .Do(MoveTheSelectedItemDown);
            }
        }

        public ICommand MoveItemUp
        {
            get
            {
                return MakeCommand
                    .When(AnItemIsSelectedThatIsNotOnTop)
                    .Do(MoveTheSelectedItemUp);
            }
        }
        #endregion

        #region Helper Methods
        private void CreateAndSelectNewItem()
        {
            _selection.SelectedItem = _document.NewItem();
        }

        private void DeleteSelectedItem()
        {
            _document.DeleteItem(_selection.SelectedItem);
            _selection.SelectedItem = null;
        }

        private bool AnItemIsSelectedThatIsNotOnBottom()
        {
            return _selection.IsItemSelected 
                && _document.CanMoveDown(_selection.SelectedItem);
        }

        private void MoveTheSelectedItemDown()
        {
            _document.MoveDown(_selection.SelectedItem);
        }

        private bool AnItemIsSelectedThatIsNotOnTop()
        {
            return _selection.IsItemSelected 
                && _document.CanMoveUp(_selection.SelectedItem);
        }

        private void MoveTheSelectedItemUp()
        {
            _document.MoveUp(_selection.SelectedItem);
        }
        #endregion

        #region Equality
        public bool Equals(MainViewModel other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return IsEqualTo(other);
        }

        public override bool Equals(object other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return IsEqualTo(other as MainViewModel);
        }

        private bool IsEqualTo(MainViewModel other)
        {
            return SelectedItem.Equals(other.SelectedItem)
                && Items.SequenceEqual(other.Items);
        }

        public static bool operator ==(MainViewModel @this, MainViewModel other)
        {
            if (ReferenceEquals(@this, other)) return true;
            if (ReferenceEquals(null, @this)) return false;

            return @this.Equals(other);
        }

        public static bool operator !=(MainViewModel @this, MainViewModel other)
        {
            return !(@this == other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((SelectedItem != null ? SelectedItem.GetHashCode() : 0) * 397
                    ^ (Items != null ? Items.GetHashCode() : 0));
            }
        }
        #endregion
    }
}
