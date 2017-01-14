using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Voron.AdaptiveWrapPanel
{
	[ContentProperty(nameof(Children))]
	public partial class AdaptiveWrapPanel : ScrollViewer
	{

#if DEBUG
		public static bool Debug { get; set; }
#endif

		#region DPs

		public static readonly DependencyProperty ColumnBreakBehaviorProperty = DependencyProperty.RegisterAttached(
			"ColumnBreakBehavior", typeof(ColumnBreakBehavior), typeof(AdaptiveWrapPanel), new FrameworkPropertyMetadata(UpdateLayout)
			{
				DefaultValue = ColumnBreakBehavior.Default,
				AffectsArrange = true,
				AffectsMeasure = true
			});

		public static void SetColumnBreakBehavior(UIElement element, ColumnBreakBehavior value)
		{
			element.SetValue(ColumnBreakBehaviorProperty, value);
		}

		public static ColumnBreakBehavior GetColumnBreakBehavior(UIElement element)
		{
			return (ColumnBreakBehavior)element.GetValue(ColumnBreakBehaviorProperty);
		}
		#endregion

		private ColumnWrapPanel Panel { get; }

		protected Size MeasureConstraint { get; private set; }

		public AdaptiveWrapPanel()
		{
			Panel = new ColumnWrapPanel(this);
			Content = Panel;
			Children = Panel.Children;
			ColumnDefinitions.CollectionChanged += (sender, args) =>
			{
				InvalidateMeasure();
				InvalidateArrange();
			};
		}

		protected override Size MeasureOverride(Size constraint)
		{
			//if (MeasureConstraint != constraint)
			//{
				MeasureConstraint = constraint;
				Panel.InvalidateMeasure();
				Panel.InvalidateArrange();
			//}
			return base.MeasureOverride(constraint);
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public ObservableCollection<ColumnDefinition> ColumnDefinitions { get; } 
			= new ObservableCollection<ColumnDefinition>();

		[Browsable(false)]
		public new object Content
		{
			get { return GetValue(ContentProperty); }
			private set { SetValue(ContentProperty, value); }
		}

		private static readonly DependencyPropertyKey ChildrenPropertyKey =
			DependencyProperty.RegisterReadOnly(
				nameof(Children),
				typeof(UIElementCollection),
				typeof(AdaptiveWrapPanel),
				new PropertyMetadata());

		private static readonly DependencyProperty ChildrenProperty 
			= ChildrenPropertyKey.DependencyProperty;

		public UIElementCollection Children
		{
			get { return (UIElementCollection)GetValue(ChildrenProperty); }
			private set { SetValue(ChildrenPropertyKey, value); }
		}

		public ColumnBreakBehavior DefaultBreakBehavior
		{
			get { return (ColumnBreakBehavior)GetValue(DefaultBreakBehaviorProperty); }
			set { SetValue(DefaultBreakBehaviorProperty, value); }
		}

		public static readonly DependencyProperty DefaultBreakBehaviorProperty =
			DependencyProperty.Register(nameof(DefaultBreakBehavior), typeof(ColumnBreakBehavior), typeof(AdaptiveWrapPanel),
				new PropertyMetadata(ColumnBreakBehavior.Default, UpdateLayout));

		/// <summary>
		/// In development
		/// </summary>
		public ExpandDirection ChildFlowDirection
		{
			get { return (ExpandDirection)GetValue(ChildFlowDirectionProperty); }
			set { SetValue(FlowDirectionProperty, value); }
		}

		public static readonly DependencyProperty ChildFlowDirectionProperty =
			DependencyProperty.Register(nameof(ChildFlowDirection), typeof(ExpandDirection),
				typeof(AdaptiveWrapPanel), new PropertyMetadata(ExpandDirection.Down, UpdateLayout));

		private static void UpdateLayout(DependencyObject dependencyObject,
			DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{
			var panel = (dependencyObject as AdaptiveWrapPanel)?.Panel ??
			            ((dependencyObject as FrameworkElement)?.Parent as ColumnWrapPanel);
			panel?.InvalidateMeasure();
			panel?.InvalidateArrange();
		}
		
	}

	public enum ColumnBreakBehavior
	{
		Default, DenyBreak, PreferNewColumn, ForceNewColumn
	}
}
