﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenRGB.NET
{
    public class Color : IEquatable<Color>
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        public Color(byte red = 0, byte green = 0, byte blue = 0)
        {
            R = red;
            G = green;
            B = blue;
        }

        public static Color FromHsv(double hue, double saturation, double value)
        {
            if (saturation < 0 || saturation > 1)
                throw new ArgumentOutOfRangeException(nameof(saturation));
            if (value < 0 || value > 1)
                throw new ArgumentOutOfRangeException(nameof(value));

            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = (hue / 60) - Math.Floor(hue / 60);

            value *= 255;
            var v = Convert.ToByte(value);
            var p = Convert.ToByte(value * (1 - saturation));
            var q = Convert.ToByte(value * (1 - (f * saturation)));
            var t = Convert.ToByte(value * (1 - ((1 - f) * saturation)));

            switch (hi)
            {
                case 0:
                    return new Color(v, t, p);
                case 1:
                    return new Color(q, v, p);
                case 2:
                    return new Color(p, v, t);
                case 3:
                    return new Color(p, q, v);
                case 4:
                    return new Color(t, p, v);
                default:
                    return new Color(v, p, q);
            }
        }

        public (double h, double s, double v) ToHsv()
        {
            var max = Math.Max(R, Math.Max(G, B));
            var min = Math.Min(R, Math.Min(G, B));

            var delta = max - min;

            var hue = 0d;
            if (delta != 0)
            {
                if (R == max) hue = (G - B) / (double)delta;
                else if (G == max) hue = 2d + ((B - R) / (double)delta);
                else if (B == max) hue = 4d + ((R - G) / (double)delta);
            }

            hue *= 60;
            if (hue < 0.0) hue += 360;

            var saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            var value = max / 255d;

            return (hue, saturation, value);
        }

        internal static Color[] Decode(byte[] buffer, ref int offset, ushort colorCount)
        {
            var colors = new List<Color>(colorCount);

            for (int i = 0; i < colorCount; i++)
            {
                colors.Add(new Color
                {
                    R = buffer[offset],
                    G = buffer[offset + 1],
                    B = buffer[offset + 2]
                    //Alpha = buffer[offset + 3]
                });
                offset += 4 * sizeof(byte);
            }
            return colors.ToArray();
        }

        internal byte[] Encode()
        {
            return new byte[]
            {
                R,
                G,
                B,
                0
            };
        }

        public static IEnumerable<Color> GetHueRainbow(int amount, double hueStart = 0, double huePercent = 1.0,
                                                                double saturation = 1.0, double value = 1.0) =>
            Enumerable.Range(0, amount)
                      .Select(i => FromHsv(hueStart + (360.0d * huePercent / amount * i), saturation, value));

        public static IEnumerable<Color> GetSinRainbow(int amount, int floor = 127, int width = 128, double range = 1.0, double offset = Math.PI / 2) =>
            Enumerable.Range(0, amount)
                      .Select(i => new Color(
                            (byte)(floor + width * Math.Sin(offset + (2 * Math.PI * range) / amount * i + 0)),
                            (byte)(floor + width * Math.Sin(offset + (2 * Math.PI * range) / amount * i + (2 * Math.PI / 3))),
                            (byte)(floor + width * Math.Sin(offset + (2 * Math.PI * range) / amount * i + (4 * Math.PI / 3)))
                            ));

        public override string ToString()
        {
            return $"R:{R}, G:{G}, B:{B} ";
        }

        public bool Equals(Color other) =>
            this.R == other.R &&
            this.G == other.G &&
            this.B == other.B;

        public Color Clone() => new Color(R, G, B);
    }
}