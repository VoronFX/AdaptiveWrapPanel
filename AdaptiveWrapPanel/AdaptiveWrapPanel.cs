using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Voron.AdaptiveWrapPanel
{
	[ContentProperty(nameof(Children))]
	public class AdaptiveWrapPanel : ScrollViewer
	{

#if DEBUG
		public static bool Debug { get; set; }
#endif

		#region DPs

		/// <summary>
		/// Can be used to create a new column with the ColumnedPanel
		/// just before an element
		/// </summary>

		public static readonly DependencyProperty ForceNewColumnProperty = DependencyProperty.RegisterAttached(
			"ForceNewColumn", typeof(bool), typeof(AdaptiveWrapPanel), new FrameworkPropertyMetadata
			{
				DefaultValue = false,
				AffectsArrange = true,
				AffectsMeasure = true
			});

		public static void SetForceNewColumn(UIElement element, Boolean value)
		{
			element.SetValue(ForceNewColumnProperty, value);
		}
		public static Boolean GetForceNewColumn(UIElement element)
		{
			return (bool)element.GetValue(ForceNewColumnProperty);
		}

		public static readonly DependencyProperty FillColumnHeightProperty = DependencyProperty.RegisterAttached(
			"FillColumnHeight", typeof(bool), typeof(AdaptiveWrapPanel), new FrameworkPropertyMetadata
			{
				DefaultValue = false,
				AffectsArrange = true,
				AffectsMeasure = true
			});

		public static void SetFillColumnHeight(UIElement element, Boolean value)
		{
			element.SetValue(FillColumnHeightProperty, value);
		}
		public static Boolean GetFillColumnHeight(UIElement element)
		{
			return (bool)element.GetValue(FillColumnHeightProperty);
		}
		#endregion


		public AdaptiveWrapPanel()
		{
			Content = new ColumnWrapPanel();
			Children = ((ColumnWrapPanel)Content).Children;
		}

		protected override Size MeasureOverride(Size constraint)
		{
			var content = Content as ColumnWrapPanel;
			if (content != null)
			{
				if (content.ParentScrollViewerConstraint != constraint)
					content.InvalidateMeasure();
				content.ParentScrollViewerConstraint = constraint;
			}
			return base.MeasureOverride(constraint);
		}

		public static readonly DependencyPropertyKey ChildrenProperty = DependencyProperty.RegisterReadOnly(
			nameof(Children),  // Prior to C# 6.0, replace nameof(Children) with "Children"
			typeof(UIElementCollection),
			typeof(AdaptiveWrapPanel),
			new PropertyMetadata());

		public UIElementCollection Children
		{
			get { return (UIElementCollection)GetValue(ChildrenProperty.DependencyProperty); }
			private set { SetValue(ChildrenProperty, value); }
		}
	}
}
