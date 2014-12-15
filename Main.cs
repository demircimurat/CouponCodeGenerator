using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace CouponCodeGenerator
{
	class MainClass
	{

		public static void Main (string[] args)
		{
			var g = new CouponGenerator ();

			for (int i = 0; i < args.Length; i++) {
				switch (args [i]) {
				case "-o": 
					g.OutputFile = args [i + 1]; 
					break;

				case "-i":
					g.InputFile = args [i + 1];
					break;

				case "-l":
					g.Length = int.Parse (args [i + 1]);
					if (g.Length < 4 || g.Length > 12) {
						Console.WriteLine ("code length should be between 4 and 16 characters");
						return;
					}
					break;

				case "-c":
					g.Count = int.Parse (args [i + 1]);
					if (g.Count < 1) {
						Console.WriteLine ("code count should be more than zero");
						return;
					}
					break;
				}
			}

			g.Generate ();
		}
	}

	public class CouponGenerator
	{
		public int Length = 6;
		public int Count = 100;
		public string InputFile = null;
		public string OutputFile = null;
		private Hashtable codes = new Hashtable ();
		private int[] shiftTable = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

		public CouponGenerator ()
		{
		}

		public void Generate ()
		{
			if (InputFile != null)
			if (!LoadInputCodes ()) {
				return;
			}

			TextWriter of = null;
			try {
				of = OutputFile == null ? Console.Out : File.CreateText (OutputFile);
			} catch (Exception ex) {
				Console.WriteLine (ex.Message);
				return;
			}

			if (of == null) {
				return;
			}


			var rand = new Random (DateTime.Now.Millisecond);
			shiftTable = RandomShiftTable (rand);

			uint key = 1;
			int count = 0;
			while (true) {
				key = (uint)rand.Next (1000, (int)0x7fffffff);
				string code = CouponCode2 (key);
				if (!codes.ContainsKey (code)) {
					codes.Add (code, null);
					of.WriteLine (code);
					count++;
				}

				if (count >= Count) {
					break;
				}
			}

			if (OutputFile != null) {
				of.Flush ();
				of.Close ();
			}
		}

		private int RandomInt (int min, int max, Hashtable ht, Random rand)
		{
			while (true) {
				int i = rand.Next (min, max + 1);

				if (ht.ContainsKey (i)) {
					continue;
				}

				ht.Add (i, null);
				return i;
			}
		}

		private int[] RandomShiftTable (Random rand)
		{
			Hashtable ht = new Hashtable ();
			var table = new List<int> ();
			//int max = Math.Min(Length, 10);
			for (int i = 0; i < Length; i++) {
				table.Add (RandomInt (1, Length, ht, rand));
			}

			return table.ToArray ();
		}

		private string CouponCode (uint number)
		{
			var sb = new StringBuilder ();
			var rand = new Random (DateTime.Now.Millisecond);
			for (int i = 0; i < Length; i++) {
				int index = rand.Next (0, ALPHABET.Length);
				sb.Append (ALPHABET [index]);
			}
			return sb.ToString ();
		}

		const string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

		private string CouponCode2 (uint onumber)
		{
			uint number = onumber;
			StringBuilder b = new StringBuilder ();
			for (int i = 0; i < 6; ++i) {
				b.Append (ALPHABET [(int)number & ((1 << 5) - 1)]);
				number = number >> 5;
			}
			return b.ToString ();
		}

		private uint CodeFromCoupon (string coupon)
		{
			uint n = 0;

			for (int i = 0; i < 6; ++i) {
				n = n | (((uint)ALPHABET.IndexOf (coupon [i])) << (5 * i));
			}

			return n;
		}

		private bool LoadInputCodes ()
		{
			try {
				var file = File.OpenText (InputFile);
				while (true) {
					string line = file.ReadLine ();
					if (line == null) {
						break;
					}

					if (!codes.ContainsKey (line)) {
						codes.Add (line, null);
					}
				}
			} catch (Exception ex) {
				Console.WriteLine (ex.Message);
				return false;
			}

			return true;
		}
	}
}
