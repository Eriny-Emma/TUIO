using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace actor
{
	public class drawables
	{
		public Bitmap img;
		public Rectangle rectangle;
		public drawables(Bitmap im, Rectangle rec)
		{
			img = im;
			rectangle = rec;
		}
	}
}
