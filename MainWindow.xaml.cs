using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace x01.Tetris
{
	public enum BlockType
	{
		Straight, T, Square, Bent
	}

	public struct Square
	{
		public int Row, Col;
	}

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		const int MaxRow = 20;
		const int MaxCol = 11;
		double size = 0;
		static bool isStarted = false;
		int top = 0, down = 0;
		Random rand = new Random();
		DispatcherTimer timer = new DispatcherTimer();
		Rectangle[,] rects = new Rectangle[MaxRow, MaxCol];
		Square[] current = new Square[4];
		List<Square> backup = new List<Square>();
		BlockType blockType = BlockType.T;

		public MainWindow()
		{
			InitializeComponent();
			Init();

			timer.Tick += Timer_Tick;
			timer.Interval = TimeSpan.FromSeconds(0.5);
			timer.Start();
		}

		bool isPressing = false;
		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			if (e.Key == Key.Escape) {
				for (int r = 0; r < MaxRow; r++) {
					for (int c = 0; c < MaxCol; c++) {
						rects[r, c].Visibility = Visibility.Hidden;
					}
				}
				isStarted = false;
				isStarting = false;
			}

			if (!isStarted) return;

			isPressing = true;

			if (e.Key == Key.Left) {
				for (int i = 0; i < 4; i++) {
					var c = current[i];
					if (HasSquare(c.Row, c.Col - 1) || !InRange(c.Row, c.Col - 1)) {
						isPressing = false;
						return;
					}
				}
				backup.AddRange(current);
				for (int i = 0; i < 4; i++) {
					current[i].Col--;
				}
			} else if (e.Key == Key.Right) {
				for (int i = 0; i < 4; i++) {
					var c = current[i];
					if (HasSquare(c.Row, c.Col + 1) || !InRange(c.Row, c.Col + 1)) {
						isPressing = false;
						return;
					}
				}
				backup.AddRange(current);
				for (int i = 0; i < 4; i++) {
					current[i].Col++;
				}
			} else if (e.Key == Key.Up) {
				Rotate();
			} else if (e.Key == Key.Down) {
				for (int i = 0; i < 5; i++) {
					Down();
					ReDraw();
				}
			}

			isPressing = false;
		}

		bool HasSquare(int row, int col)
		{
			return InRange(row, col) && !current.Any(s => s.Row == row && s.Col == col)
				&& rects[row, col].Visibility == Visibility.Visible;
		}

		int rotateCount = 0;
		Square[] rotateBack = null;
		private void Rotate()
		{
			rotateBack = (Square[])current.Clone();

			switch (blockType) {
				case BlockType.Straight:
					if (rotateCount % 4 == 0 || rotateCount % 4 == 2) {
						for (int i = 0; i < 4; i++) {
							rotateBack[i].Row = top;
							rotateBack[i].Col += i;
						}
					} else if (rotateCount % 4 == 1 || rotateCount % 4 == 3) {
						for (int i = 0; i < 4; i++) {
							rotateBack[i].Row += i;
							rotateBack[i].Col = rotateBack[0].Col;
						}
					}
					break;
				case BlockType.T:
					if (rotateCount % 4 == 0) {
						rotateBack[0].Row--;
						rotateBack[0].Col++;
						rotateBack[1].Row++;
						rotateBack[1].Col++;
						rotateBack[3].Row--;
						rotateBack[3].Col--;
					} else if (rotateCount % 4 == 1) {
						rotateBack[0].Row--;
						rotateBack[0].Col--;
						rotateBack[1].Row--;
						rotateBack[1].Col++;
						rotateBack[3].Row++;
						rotateBack[3].Col--;
					} else if (rotateCount % 4 == 2) {
						rotateBack[0].Row++;
						rotateBack[0].Col--;
						rotateBack[1].Row--;
						rotateBack[1].Col--;
						rotateBack[3].Row++;
						rotateBack[3].Col++;
					} else if (rotateCount % 4 == 3) {
						rotateBack[0].Row++;
						rotateBack[0].Col++;
						rotateBack[1].Row++;
						rotateBack[1].Col--;
						rotateBack[3].Row--;
						rotateBack[3].Col++;
					}
					break;
				case BlockType.Square:
					break;
				case BlockType.Bent:
					if (rotateCount % 4 == 0 || rotateCount % 4 == 2) {
						rotateBack[2].Col += 2;
						rotateBack[1].Row -= 2;
					} else if (rotateCount % 4 == 1 || rotateCount % 4 == 3) {
						rotateBack[2].Col -= 2;
						rotateBack[1].Row += 2;
					}
					break;
				default:
					break;
			}

			for (int i = 0; i < 4; i++) {
				var r = rotateBack[i];
				if (HasSquare(r.Row, r.Col) || !InRange(r.Row, r.Col)) {
					return;
				}
			}

			current = (Square[])rotateBack.Clone();
			backup.AddRange(current);
			rotateCount++;
			if (rotateCount == 4) rotateCount = 0;
		}

		private void Timer_Tick(object sender, EventArgs e)
		{
			if (isPressing) return;
			if (isStarting) return;

			if (isStarted == false)
				Start();

			Down();
			ReDraw();
		}

		private void Down()
		{
			if (isStarted == false) return;
			if (current.Any(s => s.Row + 1 == MaxRow)) return;

			foreach (var b in backup) {
				if (InRange(b.Row, b.Col))
					rects[b.Row, b.Col].Visibility = Visibility.Hidden;
			}

			backup.Clear();
			top = down = current[0].Row;
			for (int i = 0; i < 4; i++) {
				int row = ++current[i].Row;
				int col = current[i].Col;
				if (InRange(row, col)) {
					rects[row, col].Visibility = Visibility.Visible;
				}

				if (top > row) top = row;
				if (down < row) down = row;
			}

			backup.AddRange(current);
		}

		bool InRange(int row, int col)
		{
			return row >= 0 && row < MaxRow && col >= 0 && col < MaxCol;
		}

		bool isStarting = false;
		private void Start()
		{
			isStarting = true;

			if (isStarted) return;
			isStarted = true;

			for (int i = 0; i < 4; i++) {
				current[i].Row = current[i].Col = 0;
			}
			rotateCount = 0;

			blockType = (BlockType)rand.Next(4);
			switch (blockType) {
				case BlockType.Straight:
					for (int i = 0; i < 4; i++) {
						current[i].Col = (MaxCol - 1) / 2;
						current[i].Row = -i;
					}
					break;
				case BlockType.T:
					for (int i = 0; i < 4; i++) {
						current[0].Row = 0;
						current[0].Col = (MaxCol - 1) / 2;
						if (i > 0) {
							current[i].Row = -1;
							current[i].Col = (MaxCol - 1) / 2 + (i - 2);
						}
					}
					break;
				case BlockType.Square:
					for (int i = 0; i < 4; i++) {
						if (i <= 1) {
							current[i].Row = 0;
							current[i].Col = (MaxCol - 1) / 2 + i;
						} else {
							current[i].Row = -1;
							current[i].Col = (MaxCol - 1) / 2 + (i - 2);
						}
					}
					break;
				case BlockType.Bent:
					for (int i = 0; i < 4; i++) {
						if (i <= 1) {
							current[i].Row = 0;
							current[i].Col = (MaxCol - 1) / 2 + i;
						} else {
							current[i].Row = -1;
							current[i].Col = (MaxCol - 1) / 2 + (i - 3);
						}
					}
					break;
			}

			isStarting = false;
		}

		private void Init()
		{
			size = (Height - 50) / MaxRow;
			canvas.Width = size * MaxCol;
			canvas.Height = size * MaxRow;

			for (int r = 0; r < MaxRow; r++) {
				for (int c = 0; c < MaxCol; c++) {
					rects[r, c] = new Rectangle();
					rects[r, c].Width = rects[r, c].Height = size;
					rects[r, c].Fill = Brushes.Gray;
					rects[r, c].Stroke = Brushes.LightGray;
					rects[r, c].Visibility = Visibility.Hidden;
					canvas.Children.Add(rects[r, c]);
					Canvas.SetLeft(rects[r, c], c * size);
					Canvas.SetTop(rects[r, c], r * size);
				}
			}

			for (int i = 0; i < 4; i++) {
				current[i].Col = current[i].Row = 0;
			}
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);

			size = (ActualHeight - 50) / MaxRow;
			canvas.Width = size * MaxCol;
			canvas.Height = size * MaxRow;
			ReDraw();
		}

		private void ReDraw()
		{
			if (isStarted == false) return;

			for (int r = 0; r < MaxRow; r++) {
				for (int c = 0; c < MaxCol; c++) {
					rects[r, c].Width = rects[r, c].Height = size;
					Canvas.SetLeft(rects[r, c], c * size);
					Canvas.SetTop(rects[r, c], r * size);

					bool hasSquare = current.Any(s => s.Col == c && s.Row + 1 == r
						&& rects[r, c].Visibility == Visibility.Visible)
						&& !current.Any(s => s.Row == r && s.Col == c);
					if (down == MaxRow - 1 || hasSquare) {
						top = down = 0;
						isStarted = false;
						backup.Clear();
						ClearLines();
						return;
					}
				}
			}
		}

		List<int> cols = new List<int>();
		List<int> rows = new List<int>();
		void ClearLines()
		{
			//if (isStarted != false) return;

			cols.Clear();
			rows.Clear();

			bool isClear;
			for (int r = 0; r < MaxRow; r++) {
				for (int c = 0; c < MaxCol; c++) {
					cols.Add(c);
					if (c == MaxCol - 1) {
						isClear = true;
						foreach (var col in cols) {
							if (rects[r, col].Visibility != Visibility.Visible) {
								isClear = false;
								break;
							}
						}
						cols.Clear();
						if (isClear) rows.Add(r);
					}
				}
			}

			foreach (var r in rows) {
				for (int c = 0; c < MaxCol; c++) {
					rects[r, c].Visibility = Visibility.Hidden;
				}
			}

			foreach (var r in rows) {
				for (int i = r - 1; i >= 0; i--) {
					for (int j = 0; j < MaxCol; j++) {
						rects[i + 1, j].Visibility = rects[i, j].Visibility;
					}
				}
			}
		}
	}
}
