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


namespace Voron.AdaptiveWrapPanelDemo
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{

		public AdaptiveWrapPanel.AdaptiveWrapPanel Panel { get; }
			= new AdaptiveWrapPanel.AdaptiveWrapPanel();

		public MainWindow()
		{
			AdaptiveWrapPanel.AdaptiveWrapPanel.Debug = true;

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

		public double Width { get { return Item.Width; } set { Item.Width = value; } }
		public double Height { get { return Item.Height; } set { Item.Height = value; } }
		public double MinWidth { get { return Item.MinWidth; } set { Item.MinWidth = value; } }
		public double MaxWidth { get { return Item.MaxWidth; } set { Item.MaxWidth = value; } }
		public double MinHeight { get { return Item.MinHeight; } set { Item.MinHeight = value; } }
		public double MaxHeight { get { return Item.MaxHeight; } set { Item.MaxHeight = value; } }
		public Brush Background { get { return Item.Background; } set { Item.Background = value; } }
		//public Brush Foreground { get { return Item.Foreground; } set { Item.Foreground = value; } }

		private string debugText;
		private string text;

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

		public bool ForceNewColumn
		{
			get { return (bool)Item.GetValue(AdaptiveWrapPanel.AdaptiveWrapPanel.ForceNewColumnProperty); }
			set { Item.SetValue(AdaptiveWrapPanel.AdaptiveWrapPanel.ForceNewColumnProperty, value); }
		}

		public bool FillColumnHeight
		{
			get { return (bool)Item.GetValue(AdaptiveWrapPanel.AdaptiveWrapPanel.FillColumnHeightProperty); }
			set { Item.SetValue(AdaptiveWrapPanel.AdaptiveWrapPanel.FillColumnHeightProperty, value); }
		}


	}
}
