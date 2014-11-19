﻿using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using $rootnamespace$.Models;
using Assisticant;

namespace $rootnamespace$.ViewModels
{
    public sealed class MainViewModel
    {
        private readonly Document _document;
        private readonly Selection _selection;

        public MainViewModel(Document document, Selection selection)
        {
            _document = document;
            _selection = selection;
        }

        public IEnumerable<ItemHeader> Items
        {
            get
            {
                return
                    from item in _document.Items
                    select new ItemHeader(item);
            }
        }

        public ItemHeader SelectedItem
        {
            get
            {
                return _selection.SelectedItem == null
                    ? null
                    : new ItemHeader(_selection.SelectedItem);
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
                return _selection.SelectedItem == null
                    ? null
                    : new ItemViewModel(_selection.SelectedItem);
            }
        }

        public ICommand AddItem
        {
            get
            {
                return MakeCommand
                    .Do(() =>
                    {
                        _selection.SelectedItem = _document.NewItem();
                    });
            }
        }

        public ICommand DeleteItem
        {
            get
            {
                return MakeCommand
                    .When(() => _selection.SelectedItem != null)
                    .Do(() =>
                    {
                        _document.DeleteItem(_selection.SelectedItem);
                        _selection.SelectedItem = null;
                    });
            }
        }

        public ICommand MoveItemDown
        {
            get
            {
                return MakeCommand
                    .When(() =>
                        _selection.SelectedItem != null &&
                        _document.CanMoveDown(_selection.SelectedItem))
                    .Do(() =>
                    {
                        _document.MoveDown(_selection.SelectedItem);
                    });
            }
        }

        public ICommand MoveItemUp
        {
            get
            {
                return MakeCommand
                    .When(() =>
                        _selection.SelectedItem != null &&
                        _document.CanMoveUp(_selection.SelectedItem))
                    .Do(() =>
                    {
                        _document.MoveUp(_selection.SelectedItem);
                    });
            }
        }
    }
}
