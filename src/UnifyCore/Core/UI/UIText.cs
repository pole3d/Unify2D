using Microsoft.Xna.Framework;

namespace Unify2D.Core
{
    /// <summary>
    /// The <see cref="UIText"/> class extends the functionality of
    /// the base <see cref="UIComponent"/> class and introduces specific properties to display text 
    /// </summary>
    public class UIText : UIComponent
    {
        /// Properties
        public string Text { get; set; } = "Lorem Ipsum";
        public int Size { get; set; } = 10;
        public Color VertexColor { get; set; } = Color.White;


        public override void Draw()
        {
            // Draw text
        }
    }
}