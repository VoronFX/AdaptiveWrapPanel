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
			= new AdaptiveWrapPanel.AdaptiveWrapPanel()
			{
				HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
				VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
				//HorizontalContentAlignment = HorizontalAlignment.Stretch,
				//VerticalContentAlignment = VerticalAlignment.Stretch
			};

		public GeneratorSettings GeneratorSettings { get; }
			= new GeneratorSettings();

		public MainWindow()
		{
			AdaptiveWrapPanel.AdaptiveWrapPanel.Debug = true;
			DataContext = this;

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
			try
			{
				var r = new Random();
				for (int i = 0; i < GeneratorSettings.Count; i++)
				{
					var newItem = new DemoItem()
					{
						Text = $"A{index}",
						Background = new SolidColorBrush(
							Color.FromRgb((byte)r.Next(0, 255), (byte)r.Next(0, 255), (byte)r.Next(0, 255))),

						MinWidth = GeneratorSettings.CustomRange(r, GeneratorSettings.MinWidthFrom, GeneratorSettings.MinWidthTo),
						MinHeight = GeneratorSettings.CustomRange(r, GeneratorSettings.MinHeightFrom, GeneratorSettings.MinHeightTo),
						Width = GeneratorSettings.CustomRange(r, GeneratorSettings.WidthFrom, GeneratorSettings.WidthTo),
						Height = GeneratorSettings.CustomRange(r, GeneratorSettings.HeightFrom, GeneratorSettings.HeightTo),

						HorizontalAlignment = (HorizontalAlignment)HorizontalAlignmentList.SelectedItems[r.Next(HorizontalAlignmentList.SelectedItems.Count)],
						VerticalAlignment = (VerticalAlignment)VerticalAlignmentList.SelectedItems[r.Next(VerticalAlignmentList.SelectedItems.Count)],
						ColumnBreakBehavior = (ColumnBreakBehavior)ColumnBreakBehaviorList.SelectedItems[r.Next(ColumnBreakBehaviorList.SelectedItems.Count)]

					};
					Panel.Children.Add(newItem.Item);
					ItemsControl.Items.Add(newItem);
					index++;
				}
			}
			catch (Exception exception)
			{
				MessageBox.Show(this, exception.Message);
				Console.WriteLine(exception);
			}
		}

		private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
		{
			Panel.InvalidateMeasure();
			Panel.InvalidateArrange();
		}

		private void ButtonBase_OnClickClearAll(object sender, RoutedEventArgs e)
		{
			ItemsControl.Items.Clear();
			Panel.Children.Clear();
		}

		private void ButtonBase_OnClickDelColDef(object sender, RoutedEventArgs e)
		{
			Panel.ColumnDefinitions.Remove((ColumnDefinition)((Button)sender).DataContext);
		}

		private void ButtonBase_OnClickAddColDef(object sender, RoutedEventArgs e)
		{
			Panel.ColumnDefinitions.Add(new ColumnDefinition());
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

	public class GeneratorSettings
	{
		public double MinWidthFrom { get; set; } = 100;
		public double MinWidthTo { get; set; } = 250;
		public double MaxWidthFrom { get; set; } = double.PositiveInfinity;
		public double MaxWidthTo { get; set; } = double.PositiveInfinity;
		public double WidthFrom { get; set; } = double.NaN;
		public double WidthTo { get; set; } = double.NaN;
		public double MinHeightFrom { get; set; } = 100;
		public double MinHeightTo { get; set; } = 250;
		public double MaxHeightFrom { get; set; } = double.PositiveInfinity;
		public double MaxHeightTo { get; set; } = double.PositiveInfinity;
		public double HeightFrom { get; set; } = double.NaN;
		public double HeightTo { get; set; } = double.NaN;
		public int Count { get; set; } = 10;

		public static double CustomRange(Random r, double from, double to)
		{
			if (double.IsNaN(from) || double.IsNaN(to))
				return double.NaN;
			if (double.IsPositiveInfinity(from) || double.IsPositiveInfinity(to))
				return double.PositiveInfinity;
			if (double.IsNegativeInfinity(from) || double.IsNegativeInfinity(to))
				return double.NegativeInfinity;
			return from + r.Next((int)(to - from));
		}
	}
}
