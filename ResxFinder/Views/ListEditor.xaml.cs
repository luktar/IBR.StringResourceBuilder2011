using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Windows.Markup;
using System.Collections.Specialized;

namespace ResxFinder.Views
{

  [DefaultProperty("Header")]
  public partial class ListEditor : UserControl
  {
    public ListEditor()
    {
      InitializeComponent();

      this.btnUndo.IsEnabled   = false;
      this.btnRedo.IsEnabled   = false;
      this.btnAdd.IsEnabled    = false;
      this.btnRemove.IsEnabled = false;
    }

    static ListEditor()
    {
      HeaderProperty = DependencyProperty.Register("Header", typeof(string), typeof(ListEditor),
                                                   new FrameworkPropertyMetadata(">>Header<<",
                                                                                 new PropertyChangedCallback(OnHeaderChanged)));
    }

    private enum eAction
    {
      Add,
      Delete
    }

    private struct tAction
    {
      public tAction(eAction action,
                     string text)
      {
        Action = action;
        Text   = text;
      }

      public eAction Action;
      public string Text;
    }

    public static DependencyProperty HeaderProperty;

    private List<tAction> m_UndoBuffer = new List<tAction>();
    private List<tAction> m_RedoBuffer = new List<tAction>();

    public string Header
    {
      get { return ((string)GetValue(HeaderProperty)); }
      set { SetValue(HeaderProperty, value); }
    }

    public IEnumerable<string> Items
    {
      get { return (this.lstList.Items.OfType<string>()); }
      set
      {
        this.lstList.Items.Clear();

        foreach (string item in value)
          this.lstList.Items.Add(item);
      }
    }

    private void lstList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      HandleSelectionChanged();
    }
    
    private void txtItem_KeyDown(object sender, KeyEventArgs e)
    {
      e.Handled = HandleKeyDown(e.Key);
    }

    private void txtItem_TextChanged(object sender, TextChangedEventArgs e)
    {
      HandleTextChanged();
    }

    private void btnUndo_Click(object sender, RoutedEventArgs e)
    {
      HandleUndoRedo(true);
    }

    private void btnRedo_Click(object sender, RoutedEventArgs e)
    {
      HandleUndoRedo(false);
    }

    private void btnAdd_Click(object sender, RoutedEventArgs e)
    {
      HandleAdd();
    }

    private void btnRemove_Click(object sender, RoutedEventArgs e)
    {
      HandleRemove();
    }

    private static void OnHeaderChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
      ListEditor cle = sender as ListEditor;

      if ((e.Property == HeaderProperty) && ((string)e.NewValue != (string)e.OldValue))
        cle.Header = (string)e.NewValue;
    }

    private void HandleSelectionChanged()
    {
      HandleTextChanged();
    }

    private bool HandleKeyDown(Key key)
    {
      bool isHandled = false;

      switch (key)
      {
        case Key.Escape:
          object selectedItem = this.lstList.SelectedItem;
          this.txtItem.Text = (selectedItem != null) ? selectedItem.ToString() : string.Empty;
          this.txtItem.SelectionStart = this.txtItem.Text.Length;

          isHandled = true;
          break;

        case Key.Return:
          if (this.btnAdd.IsEnabled)
            this.btnAdd.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, this.btnAdd));
          else if (this.btnRemove.IsEnabled)
            this.btnRemove.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, this.btnRemove));

          isHandled = true;
          break;

        default:
          HandleTextChanged();
          break;
      } //switch

      return (isHandled);
    }

    private void HandleTextChanged()
    {
      string text       = this.txtItem.Text;
      bool   hasText    = !string.IsNullOrEmpty(text),
             isExisting = hasText && this.lstList.Items.Contains(text);

      this.btnAdd.IsEnabled    = !isExisting && hasText;
      this.btnRemove.IsEnabled =  isExisting;
    }

    private void HandleUndoRedo(bool isUndo)
    {
      List<tAction> sourceBuffer      = isUndo ? m_UndoBuffer : m_RedoBuffer,
                    destinationBuffer = isUndo ? m_RedoBuffer : m_UndoBuffer;

      if (sourceBuffer.Count > 0)
      {
        int index      = sourceBuffer.Count - 1;
        tAction action = sourceBuffer[index];

        if ((isUndo && (action.Action == eAction.Add)) || (!isUndo && (action.Action == eAction.Delete)))
        {
          int oldIndex = this.lstList.Items.IndexOf(action.Text);

          this.lstList.Items.Remove(action.Text);

          if (this.lstList.Items.Count == 0)
            HandleTextChanged();
          else
            this.lstList.SelectedIndex = (this.lstList.Items.Count > oldIndex) ? oldIndex : this.lstList.Items.Count - 1;
        }
        else
        {
          this.lstList.Items.Add(action.Text);
          this.lstList.SelectedIndex = this.lstList.Items.IndexOf(action.Text);
        } 

        destinationBuffer.Add(action);
        sourceBuffer.RemoveAt(index);
      } 

      this.btnUndo.IsEnabled = (m_UndoBuffer.Count > 0);
      this.btnRedo.IsEnabled = (m_RedoBuffer.Count > 0);
    }

    private void HandleAdd()
    {
      string text = this.txtItem.Text;

      this.lstList.Items.Add(text);
      this.lstList.SelectedIndex = this.lstList.Items.IndexOf(text);

      m_RedoBuffer.Clear();
      m_UndoBuffer.Add(new tAction(eAction.Add, text));

      this.btnUndo.IsEnabled = (m_UndoBuffer.Count > 0);
      this.btnRedo.IsEnabled = (m_RedoBuffer.Count > 0);
    }

    private void HandleRemove()
    {
      int oldIndex = this.lstList.SelectedIndex;

      string text = this.txtItem.Text;
      this.lstList.Items.Remove(text);

      if (this.lstList.Items.Count == 0)
        HandleTextChanged();
      else
        this.lstList.SelectedIndex = (this.lstList.Items.Count > oldIndex) ? oldIndex : this.lstList.Items.Count - 1;

      m_RedoBuffer.Clear();
      m_UndoBuffer.Add(new tAction(eAction.Delete, text));

      this.btnUndo.IsEnabled = (m_UndoBuffer.Count > 0);
      this.btnRedo.IsEnabled = (m_RedoBuffer.Count > 0);
    }

  } 
} 
