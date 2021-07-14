
using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using TUIO;
using actor;

public class TuioDemo : Form, TuioListener
{
	private TuioClient client;
	private Dictionary<long, TuioDemoObject> objectList;
	private Dictionary<long, TuioCursor> cursorList;
	private Dictionary<long, TuioBlob> blobList;
	private object cursorSync = new object();
	private object objectSync = new object();
	private object blobSync = new object();

	public static int width, height;
	private int window_width = 1800;
	private int window_height = 950;
	private int window_left = 0;
	private int window_top = 0;
	private int screen_width = Screen.PrimaryScreen.Bounds.Width;
	private int screen_height = Screen.PrimaryScreen.Bounds.Height;

	private bool fullscreen;
	private bool verbose;
	
	SolidBrush blackBrush = new SolidBrush(Color.Black);
	SolidBrush backgroundbrush = new SolidBrush(Color.White);

	SolidBrush grayBrush = new SolidBrush(Color.Gray);
	Pen fingerPen = new Pen(new SolidBrush(Color.Blue), 1);
	public Bitmap apple,basket,eat;
	public int countapple = 0;
	List<drawables> draw = new List<drawables>();
	bool isbacket = false;
	bool iseat = false;
	long bucketsession = -1;
	long eatsession = -1;

	System.Media.SoundPlayer collectsound = new System.Media.SoundPlayer("collect.wav");
	System.Media.SoundPlayer eatsound = new System.Media.SoundPlayer("eat2.wav");
	System.Media.SoundPlayer fallsound = new System.Media.SoundPlayer("fall.wav");

	string message= "";

	public TuioDemo(int port)
	{

		verbose = true;
		fullscreen = false;
		width = window_width;
		height = window_height;

		this.ClientSize = new System.Drawing.Size(width, height);
		this.Name = "TuioDemo";
		this.Text = "TuioDemo";

		this.Closing += new CancelEventHandler(Form_Closing);
		this.KeyDown += new KeyEventHandler(Form_KeyDown);
		apple = new Bitmap("apple.png");
		eat = new Bitmap("hourse.png");
		basket = new Bitmap("basket.png");
		apple.MakeTransparent(apple.GetPixel(1,1));
		this.SetStyle(ControlStyles.AllPaintingInWmPaint |
						ControlStyles.UserPaint |
						ControlStyles.DoubleBuffer, true);

		objectList = new Dictionary<long, TuioDemoObject>(128);
		cursorList = new Dictionary<long, TuioCursor>(128);

		client = new TuioClient(port);
		client.addTuioListener(this);

		client.connect();
	}
	private void calculate()
    {
		isbacket = false;
		 iseat = false;
		 bucketsession = -1;
		 eatsession = -1;
		int totalapples = 0;
		backgroundbrush = new SolidBrush(Color.White);

		foreach (TuioDemoObject tobject in objectList.Values)
		{
			if(tobject.SymbolID==0)
            {
				isbacket = true;
				bucketsession = tobject.SessionID;
				Console.WriteLine("found a basket");
			}
			else if (tobject.SymbolID == 11)
			{
				iseat = true;
				eatsession = tobject.SessionID;
				Console.WriteLine("found a hourse");
			}
			else if(!tobject.taken && isbacket)
            {
				totalapples += tobject.SymbolID;
				tobject.taken = true;
			}
			

		}

		if (isbacket && !iseat)
        {
			if(totalapples>0)
            {
				collectsound.Play();
            }
			countapple += totalapples;
			message = "" + countapple;
		}
		else if(isbacket && iseat && totalapples <= countapple)
		{
				countapple -= totalapples;
				message = "" + countapple;
        }
		else if(isbacket && iseat && totalapples > countapple)
		{
			backgroundbrush = new SolidBrush(Color.Red);
			int remander = totalapples - countapple;
			message = "collect " + remander + " more";
		}
	

		Console.WriteLine(" 2  countable = " + countapple);
	}
	private void checkupsidedown()
    {
		//1.6 4.5
		if (isbacket)
		{
			if (objectList[bucketsession].Angle >= 1.6 &&
			objectList[bucketsession].Angle <= 4.5)
			{
				if(countapple>0)
                {
					fallsound.Play();
				}
				countapple = 0;
			
			}
		}
	}
	private void getApplesReady()
    {
		draw.Clear();
		int Xpos = width/10;
		int Ypos = height/10;
		int size = height / 4;
		Rectangle full = new Rectangle(Xpos - size / 2, Ypos - size / 2, size, size);
	
			int x = full.X;
			int y = full.Y;
			for (int i = 0; i < countapple; i++)
			{
				if (i % 5 == 0)
				{
					x = full.X;
					y += size / 4;
				}
				Rectangle oneapple = new Rectangle(x, y, size / 4, size / 4);
				x += size / 4;
				draw.Add(new drawables(apple, oneapple));
			}
	}
	private void Form_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
	{

		if (e.KeyData == Keys.F1)
		{
			if (fullscreen == false)
			{

				width = screen_width;
				height = screen_height;

				window_left = this.Left;
				window_top = this.Top;

				this.FormBorderStyle = FormBorderStyle.None;
				this.Left = 0;
				this.Top = 0;
				this.Width = screen_width;
				this.Height = screen_height;

				fullscreen = true;
			}
			else
			{

				width = window_width;
				height = window_height;

				this.FormBorderStyle = FormBorderStyle.Sizable;
				this.Left = window_left;
				this.Top = window_top;
				this.Width = window_width;
				this.Height = window_height;

				fullscreen = false;

			}
		}
		else if (e.KeyData == Keys.Escape)
		{
			this.Close();

		}
		else if (e.KeyData == Keys.V)
		{
			verbose = !verbose;
		}

	}

	private void Form_Closing(object sender, System.ComponentModel.CancelEventArgs e)
	{
		client.removeTuioListener(this);

		client.disconnect();
		System.Environment.Exit(0);
	}

	public void addTuioObject(TuioObject o)
	{
		lock (objectSync)
		{
			TuioDemoObject pnn = new TuioDemoObject(o);
			pnn.setimge(apple);

            if (o.SymbolID == 0)
            {
				pnn.setimge(basket);
            }
            else if (o.SymbolID == 11)
            {
				pnn.setimge(eat);
            }
            else
            {
				pnn.setimge(apple);
            }

			if(pnn.SymbolID<12)
            {
				objectList.Add(o.SessionID, pnn);
			}
			//calculate();
			//checkupsidedown();
			//getApplesReady();
		}
		if (verbose) Console.WriteLine("add obj " + o.SymbolID + " (" + o.SessionID + ") " + o.X + " " + o.Y + " " + o.Angle);
	}

	public void updateTuioObject(TuioObject o)
	{
		lock (objectSync)
		{
			objectList[o.SessionID].update(o);
		
				foreach (TuioDemoObject tobject in objectList.Values)
				{
					tobject.getready();
				}
			calculate();
			checkupsidedown();
			getApplesReady();
		}
		if (verbose)
        {
			//Console.WriteLine("set obj " + o.SymbolID + " " + o.SessionID + " " + o.X + " " + o.Y + " angle " + o.Angle + " " + o.MotionSpeed + " " + o.RotationSpeed + " " + o.MotionAccel + " " + o.RotationAccel);
			Console.WriteLine("set obj " + o.SymbolID + " " + o.SessionID + " angle " + o.Angle );
		}
	}

	public void removeTuioObject(TuioObject o)
	{
		lock (objectSync)
		{
			objectList.Remove(o.SessionID);
			calculate();
			getApplesReady();
		}
		if (verbose) Console.WriteLine("del obj " + o.SymbolID + " (" + o.SessionID + ")");
	}

	public void addTuioCursor(TuioCursor c)
	{
		lock (cursorSync)
		{
			cursorList.Add(c.SessionID, c);
		}
		if (verbose) Console.WriteLine("add cur " + c.CursorID + " (" + c.SessionID + ") " + c.X + " " + c.Y);
	}

	public void updateTuioCursor(TuioCursor c)
	{
		if (verbose) Console.WriteLine("set cur " + c.CursorID + " (" + c.SessionID + ") " + c.X + " " + c.Y + " " + c.MotionSpeed + " " + c.MotionAccel);
	}

	public void removeTuioCursor(TuioCursor c)
	{
		lock (cursorSync)
		{
			cursorList.Remove(c.SessionID);
		}
		if (verbose) Console.WriteLine("del cur " + c.CursorID + " (" + c.SessionID + ")");
	}

	public void addTuioBlob(TuioBlob b)
	{
		lock (blobSync)
		{
			blobList.Add(b.SessionID, b);
		}
		if (verbose) Console.WriteLine("add blb " + b.BlobID + " (" + b.SessionID + ") " + b.X + " " + b.Y + " " + b.Angle + " " + b.Width + " " + b.Height + " " + b.Area);
	}

	public void updateTuioBlob(TuioBlob b)
	{
		if (verbose) Console.WriteLine("set blb " + b.BlobID + " (" + b.SessionID + ") " + b.X + " " + b.Y + " " + b.Angle + " " + b.Width + " " + b.Height + " " + b.Area + " " + b.MotionSpeed + " " + b.RotationSpeed + " " + b.MotionAccel + " " + b.RotationAccel);
	}

	public void removeTuioBlob(TuioBlob b)
	{
		lock (blobSync)
		{
			blobList.Remove(b.SessionID);
		}
		if (verbose) Console.WriteLine("del blb " + b.BlobID + " (" + b.SessionID + ")");
	}

	public void refresh(TuioTime frameTime)
	{
		Invalidate();
	}

	protected override void OnPaintBackground(PaintEventArgs pevent)
	{
		// Getting the graphics object
		Graphics g = pevent.Graphics;
		g.FillRectangle(backgroundbrush, new Rectangle(0, 0, width, height));
		Font font = new Font("Arial", 18.0f);
		// draw the cursor path
		if (cursorList.Count > 0)
		{
			lock (cursorSync)
			{
				foreach (TuioCursor tcur in cursorList.Values)
				{
					List<TuioPoint> path = tcur.Path;
					TuioPoint current_point = path[0];

					for (int i = 0; i < path.Count; i++)
					{
						TuioPoint next_point = path[i];
						g.DrawLine(fingerPen, current_point.getScreenX(width), current_point.getScreenY(height), next_point.getScreenX(width), next_point.getScreenY(height));
						current_point = next_point;
					}
					g.FillEllipse(grayBrush, current_point.getScreenX(width) - height / 100, current_point.getScreenY(height) - height / 100, height / 50, height / 50);
					
					g.DrawString(tcur.CursorID + "", font, blackBrush, new PointF(tcur.getScreenX(width) - 10, tcur.getScreenY(height) - 10));
				}
			}
		}
		//////
		if(isbacket)
        {
			for (int i = 0; i < draw.Count; i++)
			{
				g.DrawImage(draw[i].img, draw[i].rectangle);
			}
			g.DrawString(message, font, Brushes.Black, 5, 5);
		}
		
		//////
		// draw the objects
		if (objectList.Count > 0)
		{
			lock (objectSync)
			{
				foreach (TuioDemoObject tobject in objectList.Values)
				{
					
					tobject.paint(g);
				}
			}
		}
	}

	public static void Main(String[] argv)
	{
		int port = 0;
		switch (argv.Length)
		{
			case 1:
				port = int.Parse(argv[0], null);
				if (port == 0) goto default;
				break;
			case 0:
				port = 3333;
				break;
			default:
				Console.WriteLine("usage: java TuioDemo [port]");
				System.Environment.Exit(0);
				break;
		}

		TuioDemo app = new TuioDemo(port);
		Application.Run(app);
	}
}
