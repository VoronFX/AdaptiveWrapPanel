using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Voron.AdaptiveWrapPanel;


namespace Voron.AdaptiveWrapPanelDemo
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{

		public AdaptiveWrapPanel.AdaptiveWrapPanel Panel { get; }
			= new AdaptiveWrapPanel.AdaptiveWrapPanel() { HorizontalScrollBarVisibility = ScrollBarVisibility.Auto};

		public MainWindow()
		{
			AdaptiveWrapPanel.AdaptiveWrapPanel.Debug = true;
			DataContext = Panel;

			InitializeComponent();

			foreach (DemoItem child in ItemsControl.Items)
			{
				//var target = new Rectangle();

				//BindingOperations.SetBinding(target, Rectangle.WidthProperty, new Binding
				//{
				//	Source = child,
				//	Path = new PropertyPath(nameof(child.Width)),
				//	Mode = BindingMode.TwoWay,
				//});

				//ItemsControl.Items.Add(target);
				child.Panel = Panel;
				Panel.Children.Add(child.Item);
			}

			var panelWindow = new Window() { Content = Panel };
			panelWindow.Closed += (sender, args) => Environment.Exit(0);
			Closed += (sender, args) => Environment.Exit(0);
			panelWindow.Show();

			new DispatcherTimer(TimeSpan.FromMilliseconds(10), DispatcherPriority.Background, (sender, args) =>
			{
				foreach (DemoItem child in ItemsControl.Items)
				{
					child.DebugText = child.Item.DataContext?.ToString();
				}
			}, Dispatcher.CurrentDispatcher).Start();
		}

		private void ButtonBase_OnClickRemove(object sender, RoutedEventArgs e)
		{
			Panel.Children.Remove(((DemoItem)((Button)sender).DataContext).Item);
			ItemsControl.Items.Remove(((Button)sender).DataContext);
		}

		private int index = 1;
		private void ButtonBase_OnClickAdd(object sender, RoutedEventArgs e)
		{
			var r = new Random();
			var newItem = new DemoItem()
			{
				Panel = Panel,
				MinWidth = r.Next(50, 150),
				MinHeight = r.Next(50, 150),
				Text = $"A{index}",
				Background = new SolidColorBrush(
					Color.FromRgb((byte)r.Next(0, 255), (byte)r.Next(0, 255), (byte)r.Next(0, 255)))
			};
			Panel.Children.Add(newItem.Item);
			ItemsControl.Items.Add(newItem);
			index++;
		}

		private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
		{
			Panel.InvalidateMeasure();
		}
	}

	public class DemoItem
	{
		public Border Item { get; } = new Border();

		public AdaptiveWrapPanel.AdaptiveWrapPanel Panel
		{
			get { return panel; }
			set
			{
				panel = value;
				UpdateAttached();
			}
		}

		public double Width { get { return Item.Width; } set { Item.Width = value; } }
		public double Height { get { return Item.Height; } set { Item.Height = value; } }
		public double MinWidth { get { return Item.MinWidth; } set { Item.MinWidth = value; } }
		public double MaxWidth { get { return Item.MaxWidth; } set { Item.MaxWidth = value; } }
		public double MinHeight { get { return Item.MinHeight; } set { Item.MinHeight = value; } }
		public double MaxHeight { get { return Item.MaxHeight; } set { Item.MaxHeight = value; } }
		public HorizontalAlignment HorizontalAlignment { get { return Item.HorizontalAlignment; } set { Item.HorizontalAlignment = value; } }
		public VerticalAlignment VerticalAlignment { get { return Item.VerticalAlignment; } set { Item.VerticalAlignment = value; } }
		public Brush Background { get { return Item.Background; } set { Item.Background = value; } }
		//public Brush Foreground { get { return Item.Foreground; } set { Item.Foreground = value; } }

		private string debugText;
		private string text;
		private AdaptiveWrapPanel.AdaptiveWrapPanel panel;
		private ColumnBreakBehavior columnBreakBehavior;

		public string Text
		{
			get
			{
				return text;
			}
			set
			{
				text = value;
				Item.Child = new Label() { Content = text, FontSize = 35 };
				Item.ToolTip = DebugText;
			}
		}

		public string DebugText
		{
			get { return debugText; }
			set
			{
				if (value == debugText)
					return;
				debugText = value;
				Item.Child = new Label() { Content = text, FontSize = 35 };
				//Item.Content = text + "\r\n" + DebugText;
				Item.ToolTip = DebugText;
			}
		}

		public ColumnBreakBehavior ColumnBreakBehavior
		{
			get { return columnBreakBehavior; }
			set
			{
				columnBreakBehavior = value;
				UpdateAttached();
			}
		}

		private void UpdateAttached()
		{
			Item?.SetValue(AdaptiveWrapPanel.AdaptiveWrapPanel.ColumnBreakBehaviorProperty, ColumnBreakBehavior);
		}

	}
}
