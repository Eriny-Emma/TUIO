/*
	TUIO C# Demo - part of the reacTIVision project
	http://reactivision.sourceforge.net/

	Copyright (c) 2005-2009 Martin Kaltenbrunner <martin@tuio.org>

	This program is free software; you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation; either version 2 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program; if not, write to the Free Software
	Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using TUIO;
using actor;
public class TuioDemoObject : TuioObject
{

	List<drawables> draw = new List<drawables>();
	SolidBrush black = new SolidBrush(Color.Black);
	SolidBrush white = new SolidBrush(Color.White);
	Bitmap img;
	public bool taken = false;

	public TuioDemoObject(long s_id, int f_id, float xpos, float ypos, float angle) : base(s_id, f_id, xpos, ypos, angle)
	{
	}

	public TuioDemoObject(TuioObject o) : base(o)
	{
	}
	public void setimge(Bitmap bitmap)
	{
		img = bitmap;
		img.MakeTransparent(img.GetPixel(1, 1));
	}
	public void getready()
	{
		draw.Clear();
		int Xpos = (int)(xpos * TuioDemo.width);
		int Ypos = (int)(ypos * TuioDemo.height);
		int size = TuioDemo.height / 4;
		Rectangle full = new Rectangle(Xpos - size / 2, Ypos - size / 2, size, size);
		if (symbol_id > 0 && symbol_id < 11)
		{
			int x = full.X;
			int y = full.Y;
			for (int i = 0; i < symbol_id; i++)
			{
				if (i % 4 == 0)
				{
					x = full.X;
					y += size / 4;
				}
				Rectangle oneapple = new Rectangle(x, y, size / 4, size / 4);
				x += size / 4;
				draw.Add(new drawables(img, oneapple));
			}
		}
		else
		{
			draw.Add(new drawables(img, full));
		}
	}
	public void paint(Graphics g)
	{

		int Xpos = (int)(xpos * TuioDemo.width);
		int Ypos = (int)(ypos * TuioDemo.height);
		int size = TuioDemo.height / 4;

		g.TranslateTransform(Xpos, Ypos);
		g.RotateTransform((float)(angle / Math.PI * 180.0f));
		g.TranslateTransform(-1 * Xpos, -1 * Ypos);
		for (int i = 0; i < draw.Count; i++)
		{
			g.DrawImage(draw[i].img, draw[i].rectangle);
		}

		g.TranslateTransform(Xpos, Ypos);
		g.RotateTransform(-1 * (float)(angle / Math.PI * 180.0f));
		g.TranslateTransform(-1 * Xpos, -1 * Ypos);


		Font font = new Font("Arial", 18.0f);
		g.DrawString(symbol_id + "", font, black, new PointF(Xpos - 10, Ypos - 10));
	}

}
