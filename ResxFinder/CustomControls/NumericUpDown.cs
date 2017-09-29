using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ResxFinder.CustomControls
{
  public partial class NumericUpDown : Control
  {

    public NumericUpDown()
    {
      InitializeCommands();
    }

    static NumericUpDown()
    {

    }

    public static readonly DependencyProperty
      ValueProperty = DependencyProperty.Register("Value", typeof(decimal), typeof(NumericUpDown),
                                                  new FrameworkPropertyMetadata(0M,
                                                                                new PropertyChangedCallback(OnValueChanged),
                                                                                new CoerceValueCallback(CoerceValue)));

    public static readonly RoutedEvent
      ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble,
                                                           typeof(RoutedPropertyChangedEventHandler<decimal>),
                                                           typeof(NumericUpDown));

    public static readonly DependencyProperty
      MinValueProperty = DependencyProperty.Register("MinValue", typeof(decimal), typeof(NumericUpDown),
                                                     new FrameworkPropertyMetadata(0M,
                                                                                   new PropertyChangedCallback(OnMinValueChanged),
                                                                                   new CoerceValueCallback(CoerceMinValue)));

    public static readonly RoutedEvent
      MinValueChangedEvent = EventManager.RegisterRoutedEvent("MinValueChanged", RoutingStrategy.Bubble,
                                                              typeof(RoutedPropertyChangedEventHandler<decimal>),
                                                              typeof(NumericUpDown));

    public static readonly DependencyProperty
      MaxValueProperty = DependencyProperty.Register("MaxValue", typeof(decimal), typeof(NumericUpDown),
                                                     new FrameworkPropertyMetadata(100M,
                                                                                   new PropertyChangedCallback(OnMinValueChanged),
                                                                                   new CoerceValueCallback(CoerceMaxValue)));


    public static readonly RoutedEvent
      MaxValueChangedEvent = EventManager.RegisterRoutedEvent("MaxValueChanged", RoutingStrategy.Bubble,
                                                              typeof(RoutedPropertyChangedEventHandler<decimal>),
                                                              typeof(NumericUpDown));

    public decimal Value
    {
      get { return (decimal)GetValue(ValueProperty); }
      set { SetValue(ValueProperty, value); }
    }

    public decimal MinValue
    {
      get { return (decimal)GetValue(MinValueProperty); }
      set { SetValue(MinValueProperty, value); }
    }

    public decimal MaxValue
    {
      get { return (decimal)GetValue(MaxValueProperty); }
      set { SetValue(MaxValueProperty, value); }
    }

    public event RoutedPropertyChangedEventHandler<decimal> ValueChanged
    {
      add { AddHandler(ValueChangedEvent, value); }
      remove { RemoveHandler(ValueChangedEvent, value); }
    }

    public event RoutedPropertyChangedEventHandler<decimal> MinValueChanged
    {
      add { AddHandler(MinValueChangedEvent, value); }
      remove { RemoveHandler(MinValueChangedEvent, value); }
    }

    public event RoutedPropertyChangedEventHandler<decimal> MaxValueChanged
    {
      add { AddHandler(MaxValueChangedEvent, value); }
      remove { RemoveHandler(MaxValueChangedEvent, value); }
    }

    private static object CoerceValue(DependencyObject element,
                                      object value)
    {
      decimal newValue      = (decimal)value;
      NumericUpDown control = (NumericUpDown)element;

      newValue = Math.Max(control.MinValue, Math.Min(control.MaxValue, newValue));

      return (newValue);
    }

    private static void OnValueChanged(DependencyObject obj,
                                       DependencyPropertyChangedEventArgs args)
    {
      NumericUpDown control = (NumericUpDown)obj;

      RoutedPropertyChangedEventArgs<decimal>
        e = new RoutedPropertyChangedEventArgs<decimal>((decimal)args.OldValue,
                                                        (decimal)args.NewValue,
                                                        ValueChangedEvent);
      control.OnValueChanged(e);
    }

    protected virtual void OnValueChanged(RoutedPropertyChangedEventArgs<decimal> args)
    {
      RaiseEvent(args);
    }

    private static object CoerceMinValue(DependencyObject element,
                                         object value)
    {
      decimal newMinValue   = (decimal)value;
      NumericUpDown control = (NumericUpDown)element;

      newMinValue = Math.Min(control.MaxValue, newMinValue);

      if (control.Value < newMinValue)
        control.Value = newMinValue;

      return (newMinValue);
    }

    private static void OnMinValueChanged(DependencyObject obj,
                                          DependencyPropertyChangedEventArgs args)
    {
      NumericUpDown control = (NumericUpDown)obj;

      RoutedPropertyChangedEventArgs<decimal>
        e = new RoutedPropertyChangedEventArgs<decimal>((decimal)args.OldValue,
                                                        (decimal)args.NewValue,
                                                        MinValueChangedEvent);
      control.OnMinValueChanged(e);
    }

    /// <summary>
    /// Raises the MinValueChanged event.
    /// </summary>
    /// <param name="args">Arguments associated with the MinValueChanged event.</param>
    protected virtual void OnMinValueChanged(RoutedPropertyChangedEventArgs<decimal> args)
    {
      RaiseEvent(args);
    }

    private static object CoerceMaxValue(DependencyObject element,
                                         object value)
    {
      decimal newMaxValue   = (decimal)value;
      NumericUpDown control = (NumericUpDown)element;

      newMaxValue = Math.Max(control.MinValue, newMaxValue);

      if (control.Value > newMaxValue)
        control.Value = newMaxValue;

      return (newMaxValue);
    }

    private static void OnMaxValueChanged(DependencyObject obj,
                                          DependencyPropertyChangedEventArgs args)
    {
      NumericUpDown control = (NumericUpDown)obj;

      RoutedPropertyChangedEventArgs<decimal>
        e = new RoutedPropertyChangedEventArgs<decimal>((decimal)args.OldValue,
                                                        (decimal)args.NewValue,
                                                        MaxValueChangedEvent);
      control.OnMaxValueChanged(e);
    }

    /// <summary>
    /// Raises the MaxValueChanged event.
    /// </summary>
    /// <param name="args">Arguments associated with the MaxValueChanged event.</param>
    protected virtual void OnMaxValueChanged(RoutedPropertyChangedEventArgs<decimal> args)
    {
      RaiseEvent(args);
    }

    private static RoutedCommand _increaseCommand;
    private static RoutedCommand _decreaseCommand;

    public static RoutedCommand IncreaseCommand
    {
      get
      {
        return _increaseCommand;
      }
    }
    public static RoutedCommand DecreaseCommand
    {
      get
      {
        return _decreaseCommand;
      }
    }

    private static void InitializeCommands()
    {
      _increaseCommand = new RoutedCommand("IncreaseCommand", typeof(NumericUpDown));
      CommandManager.RegisterClassCommandBinding(typeof(NumericUpDown), new CommandBinding(_increaseCommand, OnIncreaseCommand));
      CommandManager.RegisterClassInputBinding(typeof(NumericUpDown), new InputBinding(_increaseCommand, new KeyGesture(Key.Up)));

      _decreaseCommand = new RoutedCommand("DecreaseCommand", typeof(NumericUpDown));
      CommandManager.RegisterClassCommandBinding(typeof(NumericUpDown), new CommandBinding(_decreaseCommand, OnDecreaseCommand));
      CommandManager.RegisterClassInputBinding(typeof(NumericUpDown), new InputBinding(_decreaseCommand, new KeyGesture(Key.Down)));
    }

    private static void OnIncreaseCommand(object sender, ExecutedRoutedEventArgs e)
    {
      NumericUpDown control = sender as NumericUpDown;

      if (control != null)
        control.OnIncrease();
    }
    private static void OnDecreaseCommand(object sender, ExecutedRoutedEventArgs e)
    {
      NumericUpDown control = sender as NumericUpDown;

      if (control != null)
        control.OnDecrease();
    }

    protected virtual void OnIncrease()
    {
      ++Value;
    }
    protected virtual void OnDecrease()
    {
      --Value;
    }
  }
}