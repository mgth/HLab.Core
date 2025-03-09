using System.Composition;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

[Export(typeof(IWpfTextViewCreationListener))]
[ContentType("CSharp")]
[TextViewRole(PredefinedTextViewRoles.Document)]
internal class ColorAdornmentTextViewCreationListener : IWpfTextViewCreationListener
{
   public void TextViewCreated(IWpfTextView textView)
   {
      // Add an adornment manager for the text view
      new ColorAdornmentManager(textView);
   }
}

internal class ColorAdornmentManager
{
   private readonly IWpfTextView _view;

   public ColorAdornmentManager(IWpfTextView view)
   {
      _view = view;
      _view.LayoutChanged += OnLayoutChanged;
   }

   private void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
   {
      // Find the position of color-related text and add adornments
      foreach (var line in e.NewOrReformattedLines)
      {
         var text = line.Extent.GetText();
         if (text.Contains("Red")) // Replace with custom logic
         {
            // Create a colored square
            var rect = new Rectangle
            {
               Width = 10,
               Height = 10,
               Fill = new SolidColorBrush(Colors.Red)
            };

            // Position the square at the text position
            Canvas.SetLeft(rect, line.TextLeft);
            Canvas.SetTop(rect, line.TextTop);

            // Add to the text view's adornment layer
            var adornmentLayer = _view.GetAdornmentLayer("ColorAdornment");
            adornmentLayer.AddAdornment(
               AdornmentPositioningBehavior.TextRelative,
               line.Extent,
               null,
               rect,
               null);
         }
      }
   }
}